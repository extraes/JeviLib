using HarmonyLib;
using Il2CppCysharp.Threading.Tasks;
using Il2CppInterop.Runtime.Injection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jevil.Internal.Patching;

internal static class DisableMethodResolution
{
    internal static bool Initialized { get; private set; }
    static readonly MethodInfo isManagedTypeInjected = typeof(ClassInjector).GetMethod("IsManagedTypeInjected", BindingFlags.Static | BindingFlags.NonPublic) ?? throw new MissingMethodException("IsManagedTypeInjected");
    internal static readonly Func<Type, bool> IsManagedTypeInjected = (Func<Type, bool>)Delegate.CreateDelegate(typeof(Func<Type, bool>), isManagedTypeInjected);

    // this isnt synchronized because it's only ever modified during patching, and its unlikely two threads will patch the same type at the same time
    internal static HashSet<Type> disableResolutionFor = new();

    internal static void Init()
    {
        HarmonyMethod earlyOutTriggerer = Jevil.Utilities.ToHarmony(DisableProxyResolution);

        JeviLib.instance.HarmonyInstance.Patch(isManagedTypeInjected, prefix: earlyOutTriggerer);
        Initialized = true;
    }

    static bool DisableProxyResolution(ref bool __result, Type type)
    {
#if DEBUG
        JeviLib.Log("IsManagedTypeInjected called during patching with parameter: " + type);
#endif

        if (!disableResolutionFor.Contains(type))
            return true;

#if DEBUG
        JeviLib.Log("Disabling method resolution. The interop-generated proxy method is going to be patched instead.");
#endif
        __result = true;
        return false;
    }
}
