using HarmonyLib;
using Il2CppCysharp.Threading.Tasks;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Patching;

internal static class UnitaskDelays
{
    static bool currentlyPatching = false;

    internal static void Init()
    {
        HarmonyMethod cancellationTokenDefaulter = Utilities.ToHarmony(DefaultCancellationToken);

        foreach (MethodInfo delayOverride in typeof(UniTask).GetMethods(nameof(UniTask.Delay)))
        {
            if (delayOverride.GetParameters().Any(p => p.ParameterType == typeof(Il2CppSystem.Threading.CancellationToken)))
                JeviLib.instance.HarmonyInstance.PatchProxyMethod(delayOverride, prefix: cancellationTokenDefaulter);
        }
    }

    static void DefaultCancellationToken(ref Il2CppSystem.Threading.CancellationToken cancellationToken)
    {
        // literal.
        if (cancellationToken is null)
        {
            JeviLib.Log("Interecepted call to a UniTask method with a default (null) cancellationtoken. Setting value to avoid exception.");
            cancellationToken = new Il2CppSystem.Threading.CancellationToken();
        }
    }
}
