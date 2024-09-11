using HarmonyLib;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#if false

namespace Jevil.Patching;

/// <summary>
/// The class with
/// </summary>
public static class Disable
{
    static readonly HarmonyMethod skipHMethod = Utilities.ToHarmony(Disable.SkipInternal);
    private static bool SkipInternal() => false;
    static int disableWhenCount = 0;
    internal static List<Func<bool>> disableWhenDelegates = new();

    /// <summary>
    /// Get all methods dynamically created from <see cref="When(Func{bool}, MethodInfo)"/>.
    /// </summary>
    public static IEnumerable<MethodInfo> GetConditionalDisableMethods()
        => DynTools.DynamicModuleBuilder.GetTypes()
                                        .Select(t => t.GetMethods(Const.AllBindingFlags))
                                        .Flatten()
                                        .Where(m => m.Name.Contains("Disable_"));

    /// <summary>
    /// <para>Attempts to disable a method by getting the type via <see cref="Utilities.GetTypeFromString(string, string)"/> and attempting to best-match the parameter types, or use <see cref="Extensions.GetMethodEasy(Type, string, Type[])"/>.</para>
    /// <para>If <see cref="JeviLib.DoneMappingNamespacesToAssemblies"/> is false, this will return <see langword="false"/> but will queue a patch for when the namespace mapper is done.</para>
    /// <para><b>THIS IS IRREVERSIBLE!!!</b> MelonLoader does not support unpatching on IL2CPP games!</para>
    /// </summary>
    /// <param name="namezpaze">The Type's namespace. This will be used to look through assemblies with this namespace defined.</param>
    /// <param name="clazz">The Type's name.</param>
    /// <param name="method">The method's name.</param>
    /// <param name="paramTypes">The names of types of the parameters. <i>Do NOT include the namespaces in the names.</i></param>
    /// <returns>Whether or not the method was successfully patched out. Debug builds also include log statements when it fails to patch.</returns>
    public static bool TryFromName(string namezpaze, string clazz, string method, string[] paramTypes = null)
    {
        if (!JeviLib.DoneMappingNamespacesToAssemblies)
        {
            JeviLib.onNamespaceAssembliesCompleted += () => { TryFromName(namezpaze, clazz, method, paramTypes); };
            return false;
        }

        MethodInfo? minf = Utilities.GetMethodFromString(namezpaze, clazz, method, paramTypes);
        if (minf == null) return false;
        FromMethod(minf);

        return true;
    }

    /// <summary>
    /// Disables the method behind the given delegate. I suppose you <i>can</i> put in a lambda, but like, why?
    /// </summary>
    /// <typeparam name="TDelegate">Any method.</typeparam>
    /// <param name="dele">The method.</param>
    public static void FromDelegate<TDelegate>(TDelegate dele) where TDelegate : Delegate
        => FromMethod(dele.Method);

    /// <summary>
    /// Disables a method by getting the method using <see cref="Extensions.GetMethodEasy(Type, string, Type[])"/>.
    /// </summary>
    /// <param name="hostType">The <see cref="Type"/> which holds the given method.</param>
    /// <param name="methodName">The name of the method. Should it be overridden and <paramref name="paramTypes"/> is null, the first method with the least parameters will be chosen.</param>
    /// <param name="paramTypes">The types of the parameters. You should use this if the method is overridden.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FromMethod(Type hostType, string methodName, Type[]? paramTypes = null) => FromMethod(hostType.GetMethodEasy(methodName, paramTypes));

    /// <summary>
    /// Disables a method. That is, using Harmony to patch it out using a Prefix that always returns <see langword="false"/> (which Harmony sees as "skip the patched method").
    /// </summary>
    /// <param name="toDisable"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void FromMethod(MethodInfo? toDisable)
    {
        if (toDisable is null)
            return;
        JeviLib.instance.HarmonyInstance.Patch(toDisable, prefix: skipHMethod);
    }

    ///// <summary>
    ///// Uses <see cref="Redirect"/> and <see cref="Hook"/> to patch out a method when <paramref name="whileThisIsExecuting"/> is executing, but before it has finished.
    ///// </summary>
    ///// <param name="methodToBeDisabled">The method to be disabled.</param>
    ///// <param name="whileThisIsExecuting"></param>
    //[Obsolete("WhenCalledFrom does not work; Use Redirect.FromMethod, Hook.OntoMethod, and Disable.When instead.", true)]
    //public static void WhenCalledFrom(MethodInfo methodToBeDisabled, MethodInfo whileThisIsExecuting)
    //{
    //    //bool isExecuting = false;
    //    //Redirect.FromMethod(whileThisIsExecuting, () => { isExecuting = true; });
    //    //Hook.OntoMethod(whileThisIsExecuting, () => { isExecuting = false; });
    //    //When(() => isExecuting, methodToBeDisabled);
    //    throw new NotImplementedException("WhenCalledFrom does not work; Use Redirect.FromMethod, Hook.OntoMethod, and Disable.When instead.");
    //}

//    /// <summary>
//    /// Skips a method when <paramref name="predicate"/> returns <see langword="true"/>.
//    /// </summary>
//    /// <param name="predicate">A method, lambda, or other delegate that returns a boolean value.</param>
//    /// <param name="toBeDisabled">A delegate of the method to be disabled.</param>
//    /// <exception cref="ArgumentNullException">Either of the parameters are <see langword="null"/></exception>
//    public static void When(Func<bool> predicate, Delegate toBeDisabled)
//    {
//        When(predicate, toBeDisabled.Method);
//    }

//    /// <summary>
//    /// Skips a method when <paramref name="predicate"/> returns <see langword="true"/>.
//    /// </summary>
//    /// <param name="predicate">A method, lambda, or other delegate that returns a boolean value.</param>
//    /// <param name="toBeDisabled">The method to be disabled.</param>
//    /// <exception cref="ArgumentNullException">Either of the parameters are <see langword="null"/></exception>
//    public static void When(Func<bool> predicate, MethodInfo toBeDisabled)
//    {
//        // cover my ass to make sure shit doesnt break while messing around in such a critical field
//        if (predicate == null) throw new ArgumentNullException(nameof(predicate));
//        if (toBeDisabled == null) throw new ArgumentNullException(nameof(toBeDisabled));
//        Type delegateType = typeof(Func<bool>);

//        //if (predicate.Method.IsStatic)
//        //{
//        //    Log("Predicate is static, skipping dynamic method creation and directly patching.");
//        //    JeviLib.instance.HarmonyInstance.Patch(toBeDisabled, predicate.Method.ToNewHarmonyMethod());
//        //    return;
//        //}

//        // have a redirection-specific identifier
//        int thisRedirNum = disableWhenCount++;
//        int thisDelegateIdx = disableWhenDelegates.Count;
//        disableWhenDelegates.Add(predicate);

//        // get it, bob the builder, har har
//        TypeBuilder tb = DynTools.GetTypeBuilder("Disable_" + thisRedirNum);
//        FieldBuilder funcHolder = tb.DefineField("forwardTo", delegateType, FieldAttributes.Private | FieldAttributes.Static);
//        MethodBuilder bob = tb.DefineMethod(toBeDisabled.Name + " _Disable_" + thisRedirNum,
//                                            MethodAttributes.Public | MethodAttributes.Static,
//                                            CallingConventions.Any,
//                                            typeof(bool),
//                                            Array.Empty<Type>());

//        MethodInfo invokeMethod = DynTools.GetInvokeMethod(delegateType);
//        const bool INJECT_LOGGING = true;

//        ILGenerator ilGen = bob.GetILGenerator();
//        if (INJECT_LOGGING)
//            ilGen.DeclareLocal(typeof(bool));

//        ilGen.Emit(OpCodes.Ldsfld, funcHolder);
//        ilGen.Emit(OpCodes.Callvirt, invokeMethod);
//        ilGen.Emit(OpCodes.Ldc_I4_0); // ldc.i4.0 & Ceq are the ! in "!predicate()"
//        ilGen.Emit(OpCodes.Ceq);
//        if (INJECT_LOGGING)
//        {
//            ilGen.Emit(OpCodes.Stloc_0);
//            ilGen.Emit(OpCodes.Ldstr, bob.Name + " - predicate returned ");
//            ilGen.Emit(OpCodes.Ldloca_S, 0);
//            ilGen.Emit(OpCodes.Call, typeof(bool).GetMethod("ToString", BindingFlags.Instance | BindingFlags.Public, Array.Empty<Type>())!);
//            ilGen.Emit(OpCodes.Call, Utilities.AsInfo(LogExtern));
//            ilGen.Emit(OpCodes.Ldloc_0);
//        }
//        ilGen.Emit(OpCodes.Ret);

//        Type createdType = tb.CreateType() ?? throw new NullReferenceException("Created type is null");
//        FieldInfo actionHolderBuilt = createdType.GetField(funcHolder.Name, BindingFlags.NonPublic | BindingFlags.Static) ?? throw new NullReferenceException("Delegate-holding field is null");
//        actionHolderBuilt.SetValue(null, predicate);
//        Log($"Compiled patch method and created runtime type!");
//        Log($"Created: <asm={createdType.Assembly.GetName().Name}> <module={createdType.Module.Name}> {createdType.FullName}");

//        //MethodBase dynInfo = MethodBase.GetMethodFromHandle(bob.MethodHandle);
//        //SymbolExtensions.GetMethodInfo(dynInfo);
//        HarmonyMethod hPrefix = new(bob.GetMethodInfo());

//        MethodInfo patch = JeviLib.instance.HarmonyInstance.Patch(toBeDisabled, prefix: hPrefix);
//        Log($"Resulting method: " + patch.FullDescription());
//    }

//    /// <summary>
//    /// A combination of <see cref="Utilities.GetMethodFromString(string, string, string, string[])"/> and <see cref="When(Func{bool}, MethodInfo)"/>.
//    /// </summary>
//    /// <param name="predicate">The method or lambda to determine whether the method will be disabled.</param>
//    /// <param name="namezpaze">The Type's namespace. This will be used to look through assemblies with this namespace defined.</param>
//    /// <param name="clazz">The Type's name.</param>
//    /// <param name="method">The method's name.</param>
//    /// <param name="paramTypes">The names of types of the parameters. <i>Do NOT include the namespaces in the names.</i></param>
//    /// <returns>Whether the patch was successful</returns>
//    public static bool When(Func<bool> predicate, string namezpaze, string clazz, string method, string[]? paramTypes = null)
//    {
//        MethodInfo? minf = Utilities.GetMethodFromString(namezpaze, clazz, method, paramTypes);

//        if (minf != null)
//        {
//            When(predicate, minf);
//            return true;
//        }
//        else
//        {
//#if DEBUG  
//            Log($"Unable to find {namezpaze}.{clazz}::{method} in loaded assemblies. This is likely fine and intended.");
//#endif
//            return false;
//        }
//    }

//    /// <summary>
//    /// 
//    /// </summary>
//    /// <param name="predicate">The method or lambda to determine whether the method will be disabled.</param>
//    /// <param name="type">The type with a potentially overridden method named <paramref name="methodName"/>.</param>
//    /// <param name="methodName">The name of the method. Can be overridden.</param>
//    /// <param name="paramTypes">In case you want more specificity, you can specify an array of types correspoding to the parameter types.</param>
//    public static void When(Func<bool> predicate, Type type, string methodName, Type[] paramTypes = null) => When(predicate, type.GetMethodEasy(methodName, paramTypes));

    #region Logging
    /// <summary>
    /// Whether to allow the (frankly copious amounts of) log statements in <see cref="When(Func{bool}, MethodInfo)"/> to output to the log file.
    /// <para>Debug builds default to false, release builds default to true.</para>
    /// </summary>
    public static bool DisableLogging { get => DynTools.disableLogging; set { DynTools.disableLogging = value; } }
    private static void Log(object obj) => Log(obj?.ToString() ?? "<null>");
    private static void Log(string str)
    {
        if (!DynTools.disableLogging) JeviLib.Log("DISABLE -> " + str, ConsoleColor.DarkGray);
    }
    internal static void LogExtern(string str) => Log("DynMethod -> " + str);
    #endregion
}

#endif