using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Jevil.Patching;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Il2CppInterop.Runtime;
using System.Diagnostics;

namespace Jevil.Internal.Patching;

internal static partial class Ungovernable
{
    static readonly HarmonyLib.Harmony Harmony = new("extraes.jevil.ungovernable");
    static IntPtr il2cppDomain;

    internal static void Init()
    {
        // perform in Init() because it's thread-dependent, and this way we ensure we get it from the main thread.
        il2cppDomain = IL2CPP.il2cpp_domain_get();
        AppDomain.CurrentDomain.AssemblyLoad += AssemblyLoad;

        foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            ProcessAssembly(asm);
    }

    private static void AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
    {
        Assembly asm = args.LoadedAssembly;
        ProcessAssembly(asm);
    }

    private static void ProcessAssembly(Assembly asm)
    {
        UngovernableAttribute? ungov = asm.GetCustomAttribute<UngovernableAttribute>();

        if (ungov is null) return;

#if DEBUG
        JeviLib.Log($"Assembly is marked as ungovernable! (Types: {ungov.type}) - {asm.FullName}");
#endif

        try
        {
            if (ungov.type.HasFlag(UngovernableType.ASYNC_THREAD_ATTACH))
                PatchAsyncStateMachines(asm);
            if (ungov.type.HasFlag(UngovernableType.PLAYER_PREFS_REDIRECT))
                TranspilePlayerPrefs(asm);
        }
        catch (Exception ex)
        {
            JeviLib.Warn("Exception whilst performing Ungovernable patches for " + ungov.type + ":" + ex);
        }
    }
}
