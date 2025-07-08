using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if !SELFCONTAINED
using BoneLib.BoneMenu;
#endif

namespace Jevil.Prefs;

internal static class PrefsInternal
{
    // like a dictionary, but doesnt stop GC of its PrefEntries values
    static readonly ConditionalWeakTable<string, PrefEntries> nameToEntries = new();
    static readonly Dictionary<Type, Type> genericActions = new();
    static readonly Dictionary<Type, MethodInfo> genericCreateEnumElements = new();
    static readonly object[] NoParameters = Array.Empty<object>();
    static readonly string GenericActionTypeName = typeof(Action<string>).FullName;
    //public static void RegisterPreferences<T>(string categoryName, bool prefSubcategory, Color categoryColor, string filePath) => RegisterPreferences(typeof(T), categoryName, prefSubcategory, categoryColor, filePath);

//#if !SELFCONTAINED
//    static readonly MethodInfo baseCreateEnumElement = typeof(MenuCategory).GetMethodEasy(nameof(MenuCategory.CreateEnumElement));
//#endif

    public static PrefEntries RegisterPreferences(Type type, string categoryName, bool prefSubcategory, Color categoryColor, string filePath)
    {
        // interally first checks for GetCategory
#if SELFCONTAINED
        PrefEntries ret = nameToEntries.GetValue(categoryName, (key) => new PrefEntries(MelonPreferences.CreateCategory(key)));
        MelonPreferences_Category mpCat = ret.MelonPrefsCategory;
#else
        PrefEntries ret = nameToEntries.GetValue(categoryName, (key) => new PrefEntries(MelonPreferences.CreateCategory(key), Page.Root.CreatePage(key, categoryColor)));
        MelonPreferences_Category mpCat = ret.MelonPrefsCategory;
        ret.methodCategory = ret.BoneMenuPage;
        ret.fieldCategory = prefSubcategory ? ret.BoneMenuPage.CreatePage(Preferences.prefSubcategoryName, categoryColor) : ret.BoneMenuPage;
#endif

        if (!filePath.EndsWith("MelonPreferences.cfg")) // only set file path if its not MP.cfg
            mpCat.SetFilePath(filePath, true, false); // actually get the values

        RegisterPreferences(type, ret);
        
        return ret;
    }

    public static void RegisterPreferences(Type type, PrefEntries pentries)
    {
        FieldInfo[] staticFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);
        MethodInfo[] staticMethods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

#if DEBUG
        FieldInfo[] instanceFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        FieldInfo[] instancePrefs = instanceFields.Where(f => f.GetCustomAttribute<Pref>() != null).ToArray();
        FieldInfo[] instanceRanges = instanceFields.Where(f => f.GetCustomAttribute<RangePref>() != null).ToArray();
        MethodInfo[] instancedMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(m => m.GetCustomAttribute<Pref>() != null || m.GetCustomAttribute<PrefWatch>() != null).ToArray();

        if (instancePrefs.Length != 0)
        {
            JeviLib.Warn($"The type {type.FullName} declares instanced preferences, this is not allowed!");
            JeviLib.Warn($"These preferences are: ");
            foreach (var field in instancePrefs) JeviLib.Warn(" - " + field.Name);
        }

        if (instanceRanges.Length != 0)
        {
            JeviLib.Warn($"The type {type.FullName} declares instanced range preferences, this is not allowed!");
            JeviLib.Warn($"These preferences are: ");
            foreach (var field in instanceRanges) JeviLib.Warn(" - " + field.Name);
        }

        if (instancedMethods.Length != 0)
        {
            JeviLib.Warn($"The type {type.Namespace}.{type.Name} declares instanced method preferences or watchers, this is not allowed!");
            JeviLib.Warn($"These methods are: ");
            foreach (MethodInfo method in instancedMethods) JeviLib.Warn(" - " + method.Name);
        }
#endif

        RegisterPrefAttr(type, staticFields, pentries);
        RegisterRangeAttr(type, staticFields, pentries);
        RegisterMethods(type, staticMethods, pentries); // methods arent saved to melonprefs

#if DEBUG && !SELFCONTAINED
        if (pentries.methodCategory == pentries.fieldCategory) 
            JeviLib.Log($"Created {pentries.MelonPrefsCategory.Entries.Count} entries in the MelonPrefs '{pentries.MelonPrefsCategory.DisplayName}' category and {pentries.fieldCategory.Elements.Count} elements in the BoneMenu '{pentries.fieldCategory.Name}' category");
        else
            JeviLib.Log($"Created {pentries.MelonPrefsCategory.Entries.Count} entries in the MelonPrefs '{pentries.MelonPrefsCategory.DisplayName}' category, {pentries.fieldCategory.Elements.Count} field elements in the BoneMenu '{pentries.fieldCategory.Name}' category, and {pentries.methodCategory.Elements.Count} method elements in '{pentries.methodCategory.Name}'");

#endif

        if (pentries.MelonPrefsCategory.Entries.Count != 0) pentries.MelonPrefsCategory.SaveToFile(false);

        //nameToEntries.Add(categoryName, ret); dont need, GetOrCreate works fine
    }

    private static MelonPreferences_Entry<T> SetEntry<T>(MelonPreferences_Category mpCategory, FieldInfo field, out T toSet, string desc = "")
    {
        T defaultValue = (T)field.GetValue(null)!;
        desc = string.IsNullOrEmpty(desc) ? "Default: " + defaultValue.ToString() : string.Join(", ", "Default: " + defaultValue.ToString(), desc);
        var entry = mpCategory.HasEntry(field.Name) ? mpCategory.GetEntry<T>(field.Name) : mpCategory.CreateEntry<T>(field.Name, defaultValue, description: desc);
        T entryValue = entry.Value;
        toSet = defaultValue.Equals(entryValue) ? entryValue : entryValue; // if the two are different
        field.SetValue(null, defaultValue);
        return entry;
    }

    internal static Color EnumToColor(UnityDefaultColor udc) => udc switch
        {
            UnityDefaultColor.RED => Color.red,
            UnityDefaultColor.GREEN => Color.green,
            UnityDefaultColor.BLUE => Color.blue,
            UnityDefaultColor.WHITE => Color.white,
            UnityDefaultColor.BLACK => Color.black,
            UnityDefaultColor.YELLOW => Color.yellow,
            UnityDefaultColor.CYAN => Color.cyan,
            UnityDefaultColor.MAGENTA => Color.magenta,
            UnityDefaultColor.GRAY => Color.gray,
            _ => throw new ArgumentException($"Unrecognized {nameof(UnityDefaultColor)} value: {udc}"),
        };
    

    private static void RegisterPrefAttr(Type type, FieldInfo[] fields, PrefEntries pentries)
    {
        foreach (FieldInfo field in fields)
        {
            var ep = field.GetCustomAttribute<Pref>();
            if (ep == null) continue;
#if DEBUG
            JeviLib.Log($"Found preference on {type.Name}.{field.Name}");
#endif

            var fieldType = field.FieldType;
            var readableName = Utilities.GenerateFriendlyMemberName(field.Name);
            if (fieldType == typeof(string))
            {
                pentries.mappedFields[GetFieldKey(type, field.Name)] = field;
                var entry = SetEntry(pentries.MelonPrefsCategory, field, out string toSet, ep.desc);
                field.SetValue(null, toSet);
#if DEBUG
                JeviLib.Warn($"BoneMenu does not support string elements. Preference field is {type.FullName}.{type.FullDescription()}");
#endif
                //fieldCategory.CreateStringElement(readableName, ep.color, toSet, val =>
                //{

                //    field.SetValue(null, val);
                //    InvokePrefWatchers(pentries.GetHook(field), val);
                //    entry.Value = val;
                //    pentries.fieldCategory.SaveToFile(false);
                //});
            }
            else if (fieldType == typeof(bool))
            {
                pentries.mappedFields[GetFieldKey(type, field.Name)] = field;
                var entry = SetEntry(pentries.MelonPrefsCategory, field, out bool toSet, ep.desc);
                field.SetValue(null, toSet);
#if !SELFCONTAINED
                pentries.fieldCategory.CreateBool(readableName, ep.color, toSet, val =>
                {
                    field.SetValue(null, val);
                    InvokePrefWatchers(pentries.GetHook(field), val);
                    entry.Value = val;
                    pentries.MelonPrefsCategory.SaveToFile(false);
                });
#endif
            }
            else if (fieldType == typeof(Color))
            {
                pentries.mappedFields[GetFieldKey(type, field.Name)] = field;
                var entry = SetEntry(pentries.MelonPrefsCategory, field, out Color toSet, ep.desc);
                field.SetValue(null, toSet);
#if DEBUG
                JeviLib.Warn($"BoneMenu does not support color elements. Preference field is {type.FullName}.{type.FullDescription()}");
#endif
                //fieldCategory.CreateColorElement(readableName, toSet, val =>
                //{
                //    field.SetValue(null, val);
                //    InvokePrefWatchers(pentries.GetHook(field), val);
                //    entry.Value = val;
                //    pentries.MelonPrefsCategory.SaveToFile(false);
                //});
            }
            else if (fieldType.IsEnum)
            {
#if DEBUG
                if (!string.IsNullOrEmpty(ep.desc)) JeviLib.Warn($"Descriptions are not allowed for enum preferences - the description '{ep.desc}' on {type.FullName}.{field.Name} will not be put in MelonPreferences.cfg");
#endif
                pentries.mappedFields[GetFieldKey(type, field.Name)] = field;

                Enum dv = (Enum)field.GetValue(null)!;
                var entry = pentries.MelonPrefsCategory.CreateEntry<string>(field.Name, dv.ToString(), description: $"Default: {dv}; Options: {string.Join(", ", Enum.GetNames(fieldType))}");
                Enum toSet = dv; // if the two are different
                try
                {
                    toSet = (Enum)Enum.Parse(fieldType, entry.Value);
                }
                catch
                {
                    JeviLib.Warn($"Failed to parse '{entry.Value}' as a value in the enum {fieldType.Name} for the class {type.FullName}'s preference {field.Name}");
                    JeviLib.Warn("Replacing it with its default value of " + dv);
                    entry.Value = dv.ToString();
                }
                object[] parameters = { readableName, ep.color, (dynamic val) => { field.SetValue(null, val); InvokePrefWatchers(pentries.GetHook(field), val);entry.Value = val.ToString(); pentries.MelonPrefsCategory.SaveToFile(false); } };
                field.SetValue(null, toSet);

#if !SELFCONTAINED
                try
                {

                    pentries.fieldCategory.CreateEnum(readableName, Color.white, toSet, val =>
                    {
                        field.SetValue(null, val);
                        InvokePrefWatchers(pentries.GetHook(field), val);
                        entry.Value = val.ToString();
                        pentries.MelonPrefsCategory.SaveToFile(false);
                    });
                }
#if DEBUG
                catch (Exception ex)
                {
                    JeviLib.Warn($"It seems that JeviLib is unable to create an enum element for {type.FullName}.{field.Name}; See more: {ex}");
                }
#else
                catch { }
#endif
#endif
            }
#if DEBUG
            else
            {
                JeviLib.Error($"{type.FullName}.{field.Name} is of un-bonemenu-able (or un-melonpreferences-able) type {fieldType.Name}! This is no good!");
            }
            JeviLib.Log($"Set {type.FullName}.{field.Name} to " + field.GetValue(null));
#endif
        }
    }

    private static void RegisterRangeAttr(Type type, FieldInfo[] fields, PrefEntries pentries)
    {
        foreach (var field in fields)
        {
            var rp = field.GetCustomAttribute<RangePref>();
            if (rp == null) continue;
#if DEBUG
            JeviLib.Log($"Found effect range preference on {type.Name}.{field.Name}");
#endif

            var readableName = Utilities.GenerateFriendlyMemberName(field.Name);
            var defaultValue = field.GetValue(null);
            if (field.FieldType == typeof(int))
            {
                pentries.mappedFields[GetFieldKey(type, field.Name)] = field;
                var entry = SetEntry(pentries.MelonPrefsCategory, field, out int toSet, $"{rp.low} to {rp.high}");
                field.SetValue(null, toSet);
#if !SELFCONTAINED
                pentries.fieldCategory.CreateInt(readableName, Color.white, toSet, (int)rp.inc, (int)rp.low, (int)rp.high, val =>
                {
                    field.SetValue(null, val);
                    InvokePrefWatchers(pentries.GetHook(field), val);
                    entry.Value = val;
                    pentries.MelonPrefsCategory.SaveToFile(false);
                });
#endif
            }
            else if (field.FieldType == typeof(float))
            {
                pentries.mappedFields[GetFieldKey(type, field.Name)] = field;
                var entry = SetEntry(pentries.MelonPrefsCategory, field, out float toSet, $"{rp.low} to {rp.high}");
                field.SetValue(null, toSet);
#if !SELFCONTAINED
                pentries.fieldCategory.CreateFloat(readableName, Color.white, toSet, rp.inc, rp.low, rp.high, val =>
                {
                    field.SetValue(null, val);
                    InvokePrefWatchers(pentries.GetHook(field), val);
                    entry.Value = val;
                    pentries.MelonPrefsCategory.SaveToFile(false);
                });
#endif
            }
#if DEBUG
            else
            {
                JeviLib.Error($"{type.Name}.{field.Name} is of un-range-able type {field.FieldType.Name}! This is no good!");
            }
            JeviLib.Log($"Set {type.FullName}.{field.Name} to " + field.GetValue(null));
            JeviLib.Log("Successfully created range preference");
#endif
        }
    }

    private static void RegisterMethods(Type type, MethodInfo[] methods, PrefEntries pentries)
    {
        foreach (MethodInfo method in methods)
        {
#if DEBUG
            string name = $"{type.FullName}.{method.Name}";
            ParameterInfo[] parameters = method.GetParameters();
            Pref pref = method.GetCustomAttribute<Pref>();
            PrefWatch prefWatch = method.GetCustomAttribute<PrefWatch>();
            if (pref is not null && prefWatch is not null)
            {
                JeviLib.Warn($"A method is trying to be a pref AND watch a pref! This isn't allowed and will throw an exception! The offending method: {name}");
            }
            if (pref is not null && prefWatch is null && parameters.Length != 0)
            {
                JeviLib.Warn($"A method is trying to be a pref and accept parameters! This isn't allowed and will throw an exception! (Did you mean to use PrefWatch?) The offending method: {name}");
            }
            if (pref is null && prefWatch is not null)
            {
                if (!pentries.mappedFields.TryGetValue(GetFieldKey(type, prefWatch.fieldName), out FieldInfo boundField))
                    JeviLib.Warn($"A method is trying watch a nonexistent pref named '{prefWatch.fieldName}'! This isn't allowed and will throw an exception! (Did you forget to mark that field as a Pref? Or did you rename the field? Consider using nameof) The offending method: {name}");
                if (parameters.Length != 1)
                    JeviLib.Warn($"A method is trying watch a pref but doesn't take a single parameter! This isn't allowed and will throw an exception! (Did you mean to use Pref?) The offending method: {name}");
                else if (parameters[0].ParameterType == boundField.FieldType)
                    JeviLib.Warn($"A method is trying watch a pref but doesn't take a single parameter! This isn't allowed and will throw an exception! (Did you mean to use Pref?) The offending method: {name}");
            }
#endif

            RegisterMethodPref(type, method, pentries);

            RegisterMethodPrefWatcher(type, method, pentries);
        }
    }

#if !SELFCONTAINED
    private static Page CreateSubmenu(Page bmRoot, Pref pref)
    {
        if (string.IsNullOrWhiteSpace(pref.desc)) return bmRoot;
        string[] pathParts = pref.desc.Split('/');

        Page ret = bmRoot;
        for (int i = 0; i < pathParts.Length; i++)
        {
            Page? target = ret.Elements.OfType<PageLinkElement>().FirstOrDefault(mc => mc.ElementName == pathParts[i])?.LinkedPage;
            if (target is null)
            {
#if DEBUG
                JeviLib.Log($"Creating submenu '{pathParts[i]}' on Menu '{bmRoot.Name}' for part {i + 1} of submenu path {pref.desc}");
#endif
                target = bmRoot.CreatePage(pathParts[i], pref.color);
            }

            ret = target;
        }

        return ret;
    }
#endif

    static void RegisterMethodPref(Type declaringType, MethodInfo method, PrefEntries pentries)
    {
        Pref? pref = method.GetCustomAttribute<Pref>();
        if (pref is null) return;

#if DEBUG
        if (method.GetParameters().Length != 0)
        {
            JeviLib.Warn($"Method {declaringType.FullName}.{method.Name} takes parameters! This makes it ineligible to become a FunctionElement!");
            return;
        }
#endif

        Action deleg8; // "delegate" is a keyword
        try
        {
            deleg8 = (Action)method.CreateDelegate(typeof(Action));
        }
        catch
        {
#if DEBUG
            JeviLib.Warn($"Failed to create FunctionElement delegate for {declaringType.FullName}.{method.Name}, this is likely because it returns a value, which is fine. Falling back to the slower MethodInfo.Invoke");
#endif
            deleg8 = () => { method.Invoke(null, NoParameters); };
        }

#if !SELFCONTAINED
        Page targetCategory = CreateSubmenu(pentries.methodCategory, pref);

        targetCategory.CreateFunction(Utilities.GenerateFriendlyMemberName(method.Name), pref.color, deleg8);
#if DEBUG
        JeviLib.Log($"Successfully created FunctionElement for {declaringType.FullName}.{method.Name}");
#endif
#endif
    }

    static void RegisterMethodPrefWatcher(Type declaringType, MethodInfo method, PrefEntries pentries)
    {
        PrefWatch prefWatch = method.GetCustomAttribute<PrefWatch>();
        if (prefWatch is null) return;

        string key = GetFieldKey(declaringType, prefWatch.fieldName);

#if DEBUG
        if (!pentries.mappedFields.ContainsKey(key))
        {
            JeviLib.Error($"The field {key} isn't mapped (it isn't registered as a preference!), yet is referenced as a pref to be watched by {declaringType.Name}.{method.Name}! An exception WILL be thrown!");
        }
#endif

        FieldInfo field = pentries.mappedFields[prefWatch.fieldName];
        Type actionGeneric = GetOrCreateParameterActionType(field.FieldType);

        // create delegate for quick execution 👍
        Delegate methodDelegate = method.CreateDelegate(actionGeneric);
        if (pentries.prefHooks.TryGetValue(field, out Delegate preexistingHooks))
        {
            methodDelegate = Delegate.Combine(preexistingHooks, methodDelegate);
        }
        pentries.prefHooks[field] = methodDelegate;
    }

    private static string GetFieldKey(Type declaringType, string fieldName) => $"{declaringType.FullName}.{fieldName}";

    private static Type GetOrCreateParameterActionType(Type parameterType)
    {
        if (genericActions.TryGetValue(parameterType, out var actionType))
        {
            return actionType;
        }

        Type unmadeGenericAction = typeof(Type).Assembly.GetType(GenericActionTypeName);
        actionType = unmadeGenericAction.MakeGenericType(parameterType);

        genericActions.Add(parameterType, actionType);
        return actionType;
    }

    // this will only work so long as JeviLib maintains a list of supported field types that are pref-able, because doing this from reflection will require dynamic method creation, lol
    private static void InvokePrefWatchers<T>(Delegate? hook, T value)
    {
        if (hook is null) return;
        Action<T> action = (Action<T>)hook;
        action.InvokeSafeSync(value);
    }
}
