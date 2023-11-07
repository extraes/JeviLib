using System;

namespace Jevil.Patching;

/// <summary>
/// A bitfield enum representing the kinds of things you want JeviLib to do to your assembly when it's loaded.
/// </summary>
[Flags]
public enum UngovernableType
{
    /// <summary>
    /// JeviLib will prefix every call to your async method's underlying MoveNext.
    /// <para>This will make your async methods always be running on a thread that IL2CPP's garbage collector is aware of.</para>
    /// <para>This will make calls to .ToString on IL2CPP types no longer crash the game, so long as they don't call other single-threaded code, like Unity engine code.</para>
    /// </summary>
    ASYNC_THREAD_ATTACH = 1 << 0,
    /// <summary>
    /// JeviLib will replace all your calls to the broken <see cref="UnityEngine.PlayerPrefs.SetInt(string, int)"/> (and its associated SetFloat and SetString counterparts) to JeviLib replacements.
    /// <para><see cref="UnityEngine.PlayerPrefs.TrySetInt(string, int)"/> (and its associated <see langword="float"/> and <see langword="string"/> counterparts) are not broken, so they won't be touched.</para>
    /// </summary>
    PLAYER_PREFS_REDIRECT = 1 << 1,
}
