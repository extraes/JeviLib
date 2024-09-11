using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Jevil.Patching;

/// <summary>
/// You can use these if you want, but they're only here to allow <see cref="UngovernableType.PLAYER_PREFS_REDIRECT"/> to work.
/// </summary>
public static class PlayerPrefsReplacements
{
    internal static MethodInfo SetInt = Utilities.AsInfo(SetIntButBetter);
    internal static MethodInfo SetFloat = Utilities.AsInfo(SetFloatButBetter);
    internal static MethodInfo SetString = Utilities.AsInfo(SetStringButBetter);

    // The PlayerPrefs.Set[Type] methods reference PlayerPrefsException, but that was stripped, so they don't work, but the TrySet[Type] ones don't so they work fine lol

    /// <summary>
    /// Does what <see cref="PlayerPrefs.SetInt(string, int)"/> does, but doesn't break.
    /// </summary>
    public static void SetIntButBetter(string key, int value)
    {
        if (!PlayerPrefs.TrySetInt(key, value))
        {
            throw new Exception("PlayerPrefsException: Could not store preference value");
        }
    }


    /// <summary>
    /// Does what <see cref="PlayerPrefs.SetFloat(string, float)"/> does, but doesn't break.
    /// </summary>
    public static void SetFloatButBetter(string key, float value)
    {
        if (!PlayerPrefs.TrySetFloat(key, value))
        {
            throw new Exception("PlayerPrefsException: Could not store preference value");
        }
    }


    /// <summary>
    /// Does what <see cref="PlayerPrefs.SetString(string, string)"/> does, but doesn't break.
    /// </summary>
    public static void SetStringButBetter(string key, string value)
    {
        if (!PlayerPrefs.TrySetSetString(key, value))
        {
            throw new Exception("PlayerPrefsException: Could not store preference value");
        }
    }
}
