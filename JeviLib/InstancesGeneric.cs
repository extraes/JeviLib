using HarmonyLib;
using Jevil.Patching;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Jevil;

/// <summary>
/// ComponentCache, but static, in the Mono domain, with more helper methods, and, of course, documentation.
/// </summary>
/// <typeparam name="T">Any component, can be injected or native to the IL2CPP domain.</typeparam>
public static class Instances<T> where T : Component
{
    /// <summary>
    /// Returns the most recently cached component. This may return null, but return a destroyed object.
    /// </summary>
    public static T MostRecentlyCached => mostRecent.INOC() ? null : mostRecent;
    static T mostRecent;
    static bool triedAutocache = false;
    static bool isAutocaching = false;
    static readonly Dictionary<GameObject, T> cache = new(new UnityObjectComparer<GameObject>());
    static readonly HarmonyMethod autoCacheHMethod = typeof(Instances<T>).GetMethod(nameof(AutoCachePatch), BindingFlags.Static | BindingFlags.NonPublic).ToNewHarmonyMethod();

    static Instances()
    {
#if DEBUG
        JeviLib.Log("Initialized Instances Cache of " + typeof(T).Name);
#endif
        Instances.instanceCachesToClear.Add(cache);
    }

    /// <summary>
    /// Determines whether the cache should be cleared whenever <see cref="MelonMod.OnSceneWasInitialized(int, string)"/> is called. Defaults to true.
    /// </summary>
    public static bool ClearOnSceneInit
    {
        get
        {
            return Instances.instanceCachesToClear.Contains(cache);
        }
        set
        {
            bool cosi = ClearOnSceneInit;
            if (value && !cosi) Instances.instanceCachesToClear.Add(cache);
            else if (cosi) Instances.instanceCachesToClear.Remove(cache);
        }
    }

    /// <summary>
    /// Removes a key from the cache.
    /// <para>I'm not sure why you'd want to do this, but you have the option to.</para>
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> to be deregistered from the cache.</param>
    /// <returns><see langword="true"/> if the value was successfully removed, otherwise <see langword="false"/>. Returns <see langword="false"/> if the value was not found in the cache.</returns>
    public static bool Remove(GameObject go) => cache.Remove(go);
    
    /// <summary>
    /// Attempts to get the component from the cache, get it using <see cref="GameObject.GetComponent{T}"/>, and if all else fails, add the component to the GameObject and add it to the cache.
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static T GetOrAdd(GameObject go)
    {
        T ret = Get(go);

        if (ret == null)
        {
            ret = go.AddComponent<T>();
            cache[go] = ret;
            mostRecent = ret;
        }

        return ret;
    }

    /// <summary>
    /// Add the component as attached to the GameObject <paramref name="go"/>.
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> that the cache should check for.</param>
    /// <param name="component">The value that should should be remembered with <paramref name="go"/>.</param>
    /// <returns>Whether there was already a component of type <typeparamref name="T"/> on the GameObject <paramref name="go"/>.
    /// <br><see langword="true"/> if <paramref name="go"/> already existed in the cache (and was overwritten), <see langword="false"/> if not or if <paramref name="go"/> was <see langword="null"/>.</br>
    /// </returns>
    public static bool AddManual(GameObject go, T component)
    {
        if (go == null) return false;

        bool ret = cache.ContainsKey(go);
        cache[go] = component;
        return ret;
    }

    /// <summary>
    /// Gets the <typeparamref name="T"/> component on <typeparamref name="T"/>, first trying the cache, otherwise getting it via <see cref="GameObject.GetComponent{T}"/>.
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> to be checked for a component of type <typeparamref name="T"/>.</param>
    /// <returns>The component, or <see langword="null"/> if it wasn't on the object.</returns>
    public static T Get(GameObject go)
    {
        if (cache.TryGetValue(go, out T obj)) return obj;

        T val = go.GetComponent<T>();
        if (val != null)
        {
            cache[go] = val;
            mostRecent = val;
        }
        

        return val;
    }

    /// <summary>
    /// Get the component from the cache using the Transform's <see cref="Component.gameObject"/>.
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static T Get(Transform t) => Get(t.gameObject);

    /// <summary>
    /// Whether or not the cache has <paramref name="go"/> in it.
    /// </summary>
    /// <param name="go">The <see cref="GameObject"/> to check the cache for.</param>
    /// <returns><see langword="true"/> if cache[<paramref name="go"/>] exists, otherwise <see langword="false"/>.</returns>
    public static bool Has(GameObject go) => cache.ContainsKey(go);

    /// <summary>
    /// Whether or not the cache has <paramref name="t"/> in it.
    /// </summary>
    /// <param name="t">The <see cref="Transform"/> whose <see cref="Component.gameObject"/> to check the cache for.</param>
    /// <returns><see langword="true"/> if cache[<paramref name="t"/>.gameObject] exists, otherwise <see langword="false"/>.</returns>
    public static bool Has(Transform t) => Has(t.gameObject);

    /// <summary>
    /// Finds the first Transform that is a parent of <paramref name="t"/> that has a component of the given type.
    /// </summary>
    /// <param name="t">The transform down the hierarchy to look upwards from</param>
    /// <returns>Whether any transform in the upward hierarchy has a <typeparamref name="T"/> component.</returns>
    public static Transform HasUpwards(Transform t)
    {
        bool has = Has(t.gameObject);

        if (has)
            return t;

        if (t.parent == null)
            return null;

        return HasUpwards(t.parent);
    }

    private static T GetUpwardsImpl(Transform t)
    {
        T ret = Get(t);

        if (ret != null)
            return ret;

        if (t.parent == null)
            return null;

        return GetUpwardsImpl(t.parent);
    }

    /// <summary>
    /// Recursively checks if any of Transform <paramref name="t"/>'s parents have a component of type <typeparamref name="T"/>.
    /// <para>Does a first pass to see if anything above has a cached component using <see cref="HasUpwards(Transform)"/>.</para>
    /// <br>Because of this, a cached component will be given priority over one that is <i>technically</i> closer in the hierarchy to the given Transform.</br>
    /// </summary>
    /// <param name="t">The Transforms whose parents to check.</param>
    /// <returns>The first component found going upwards from <paramref name="t"/>, or null if there are none.</returns>
    public static T GetUpwards(Transform t)
    {
        Transform hup = HasUpwards(t);
        if (hup != null) return Get(hup.gameObject);
        else return GetUpwardsImpl(t);
    }


    /// <summary>
    /// Recursively checks if any of GameObject <paramref name="go"/>'s parents have a component of type <typeparamref name="T"/>.
    /// </summary>
    /// <param name="go">The GameObject whose parents to check.</param>
    /// <returns>The first component found going upwards from <paramref name="go"/>, or null if there are none.</returns>
    public static T GetUpwards(GameObject go) => GetUpwards(go.transform);

    /// <summary>
    /// Get the <typeparamref name="T"/> in immediate children.
    /// </summary>
    /// <param name="go"></param>
    /// <param name="cacheOnly">Whether or not to only check the cache.</param>
    /// <returns>The component if it was found, or <see langword="null"/> if it wasn't.</returns>
    public static T GetInImmediateChildren(GameObject go, bool cacheOnly = true)
    {
        T comp = null;
        foreach (Transform child in go)
        {
            if (cacheOnly && Has(child)) comp = Get(child);
            else comp = Get(child);

            if (comp != null) break;
        }
        return comp;
    }

    /// <summary>
    /// Attempts to patch <typeparamref name="T"/>'s Awake method with a Postfix that caches the type. If there's no Awake, it tries the Start method.
    /// </summary>
    /// <returns>Whether a patch was successful.</returns>
    public static bool TryAutoCache()
    {
        if (triedAutocache) return isAutocaching;
        triedAutocache = true;

#if DEBUG
        string asmName = typeof(T).Assembly.GetName().Name;
        if (asmName.StartsWith("UnityEngine"))
            JeviLib.Warn($"The component {typeof(T).FullName} from the engine assembly {asmName} is likely implemented exclusively in native code! It likely will not be able to be autocached as a result.");
#endif

        BindingFlags methodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = typeof(T);
        MethodInfo patcho = type.GetMethod("Awake", methodFlags) ?? type.GetMethod("Start", methodFlags);
        if (patcho == null) return false;

        try
        {
            Hook.OntoMethod(patcho, AutoCachePatch);
#if DEBUG
            JeviLib.Log($"Successfully hooked {type.Name}'s {patcho.Name}() method for autocaching.");
#endif
            isAutocaching = true;
            return true;
        }
        catch 
        {
#if DEBUG
            JeviLib.Log($"Failed to patch {type.Name} to autocache.");
#endif
            return false;
        } 
    }

    private static void AutoCachePatch(T instance)
    {
#if DEBUG
        JeviLib.Log($"Autocache for {typeof(T).Name} called, caching now.");
#endif
        cache[instance.gameObject] = instance;
        mostRecent = instance;
    }
}
