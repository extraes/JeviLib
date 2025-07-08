using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Il2CppInterop.Runtime;

namespace Jevil.Internal.Patching;

internal static partial class Ungovernable
{
    static readonly HarmonyMethod AttachThreadPrefix = Jevil.Utilities.ToHarmony(AttachThread);


    private static void PatchAsyncStateMachines(Assembly asm)
    {
#if DEBUG
        Stopwatch sw = Stopwatch.StartNew();
        int count = 0;
#endif
        try
        {
            foreach (var type in asm.GetTypes())
            {
                Type[] interfaces = type.GetInterfaces();
                MethodInfo? method;
                if (!interfaces.Any(i => i == typeof(IAsyncStateMachine)))
                    continue;

                method = type.GetMethod("MoveNext", BindingFlags.Instance | BindingFlags.NonPublic);
                if (method is null)
                    continue;

                if (method.IsGenericMethod || type.IsGenericTypeDefinition)
                {
#if DEBUG
                    JeviLib.Warn("Unable to patch method " + method.FullDescription() + " - it (or the type that declares it) is generic!");
#endif
                    continue;
                }

                //try
                //{
                Harmony.Patch(method, prefix: AttachThreadPrefix);
                //}
                //catch (Exception e)
                //{
                //    JeviLib.Error($"Exception when patching async state machine ({method.FullDescription()}): " + e);
                //    continue;
                //}
#if DEBUG
                count++;
#endif
            }
        }
        catch (Exception ex)
        {
            JeviLib.Log($"Unable to fully all async state machines in {asm.GetName().Name}. See: {ex}");
        }
#if DEBUG
        JeviLib.Log($"Patched {count} method(s) in {sw.ElapsedMilliseconds}ms");
#endif
    }

    static void AttachThread()
    {
#if DEBUG
        // timing code not needed - every operation was less than 1ms and Thread::Attach checks to see if the thread is already attached.
        // See Unity\Hub\Editor\2021.3.5f1\Editor\Data\il2cpp\libil2cpp\vm\Thread.cpp line 104 

        //Stopwatch sw = Stopwatch.StartNew();
        //IntPtr currThread = IL2CPP.il2cpp_thread_current();
        //if (currThread == IntPtr.Zero)
        //    JeviLib.Log($"Thread not currently attached (checking took {sw.ElapsedTicks / 100 / 100.0}ms)");
        //else
        //    JeviLib.Log("Thread was already attached");
        //sw.Restart();
#endif
        IL2CPP.il2cpp_thread_attach(il2cppDomain);
#if DEBUG
        //JeviLib.Log($"Thread attach call took {sw.ElapsedTicks / 100 / 100.0}ms");
#endif
    }
}
