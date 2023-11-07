using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnhollowerRuntimeLib.XrefScans;

#if !JEVIL_MELONMOD
using JeviLib = JevilPlugin.JevilPlugin;
#endif

namespace Jevil.Unsafe;

/// <summary>
/// Utilities for getting/navigating Unity's game managers. Odds are you don't need this, so you really shouldn't fuck with this, because you'll need to use <see langword="unsafe"/> code to do anything with this.
/// <para>These are only useful if you have a decompilation of UnityPlayer.dll, which, if you are new to decompilation or hell are even of interemediate skill with C#/C# in modding, you probably shouldn't waste time trying to comprehend.</para>
/// <para>Some C++ experience helps.</para>
/// </summary>
public static partial class GlobalGameManagers
{
    internal const int PLAYERSETTINGS_DISABLEDEPTHANDSTENCILBUFFERS_OFFSET = 0x2c4;

    /// <summary>
    /// Retrieves a global game manager from the given index. An enum (<see cref="ManagerIndex"/>) has been provided for a little more definition as to what you're going to be accessing.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public delegate IntPtr GetManagerFromContextDelegate(ManagerIndex index);

    /// <summary>
    /// Returns game managers according to which manager index you pass in. Invoke it like you would a normal method.
    /// </summary>
    public static readonly GetManagerFromContextDelegate GetManagerFromContext;

    // God bless WNP78 https://github.com/WNP78/FieldInjector/blob/bonelab/SerialisationHandler.cs#L192
    static unsafe GlobalGameManagers()
    {
        IntPtr layerToNameCodePtr = IL2CPP.il2cpp_resolve_icall("UnityEngine.LayerMask::LayerToName");

#if DEBUG
        var jt = XrefScannerLowLevel.JumpTargets(layerToNameCodePtr);
        JeviLib.Log($"LayerToName targets {jt.Count()} jump locations: {string.Join(", ", jt.Select(ptr => ptr.ToString("X")))}");
#endif
        IntPtr getTagManagerCodePtr = XrefScannerLowLevel.JumpTargets(layerToNameCodePtr).First();

#if DEBUG
        jt = XrefScannerLowLevel.JumpTargets(getTagManagerCodePtr);
        JeviLib.Log($"GetTagManager(? i think) targets {jt.Count()} jump locations: {string.Join(", ", jt.Select(ptr => ptr.ToString("X")))}");
#endif
        IntPtr getManagerFromContext = XrefScannerLowLevel.JumpTargets(getTagManagerCodePtr).First(); // changed from WNP's .Single() because it thinks theres 2 in the jump table, but the second ptr is null. lol.
#if DEBUG
        JeviLib.Log($"Found GetManagerFromContext @ ptr {getManagerFromContext:X}");
#endif
        GetManagerFromContext = Marshal.GetDelegateForFunctionPointer<GetManagerFromContextDelegate>(getManagerFromContext);
    }
}
