using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using MelonLoader;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace Jevil.Patching;

// all the patches are commented out because they didnt actually end up doing anything
internal static class ThreadRegistrar
{
    //[HarmonyPatch(typeof(Thread))]
    //[HarmonyPatch(MethodType.Constructor, new Type[] { typeof(ThreadStart) })]
    //[HarmonyPatch(MethodType.Constructor, new Type[] { typeof(ThreadStart), typeof(int) })]
    //[HarmonyPatch(MethodType.Constructor, new Type[] { typeof(ParameterizedThreadStart) })]
    //[HarmonyPatch(MethodType.Constructor, new Type[] { typeof(ParameterizedThreadStart), typeof(int) })]
    //internal static class ThreadCreation
    //{
    //    public static void Postfix(Thread __instance)
    //    {
    //        JeviLib.Log($"Thread (id {__instance.ManagedThreadId}) created @ " + new System.Diagnostics.StackTrace());
    //    }
    //}

    //[HarmonyPatch(typeof(Thread), "Finalize")]
    //internal static class ThreadFinalize
    //{
    //    public static void Prefix(Thread __instance)
    //    {
    //        JeviLib.Log($"Thread (id {__instance.ManagedThreadId}) is being finalized @ " + new System.Diagnostics.StackTrace());
    //    }
    //}

    //[HarmonyPatch(typeof(Thread))]
    //[HarmonyPatch("Create", new Type[] { })]
    //internal static class ThreadCreateMethod
    //{
    //    public static void Prefix(Thread __result)
    //    {
    //        Thread.
    //        JeviLib.Log($"Thread (id {__instance.ManagedThreadId}) is being finalized @ " + new System.Diagnostics.StackTrace());
    //    }
    //}
    public static void AttachThread()
    {
        IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
    }

    //[HarmonyPatch(typeof(Thread), MethodType.Constructor, new System.Type[] { typeof(ThreadStart) })]
    //private static class PatchOne
    //{
    //    public static void Prefix(ref ThreadStart start)
    //    {
    //        MelonLogger.Msg("Changing ThreadStart");
    //        var copy = (ThreadStart)start.Clone();

    //        start = () =>
    //        {
    //            AttachThread();
    //            copy.Invoke();
    //        };
    //    }
    //}

    //[HarmonyPatch(typeof(Thread), MethodType.Constructor, new System.Type[] { typeof(ThreadStart), typeof(int) })]
    //private static class PatchTwo
    //{
    //    public static void Prefix(ref ThreadStart start, int maxStackSize)
    //    {
    //        MelonLogger.Msg("Changing ThreadStart (w/ maxstack)");

    //        var copy = (ThreadStart)start.Clone();

    //        start = new(() =>
    //        {
    //            AttachThread();
    //            copy.Invoke();
    //        });
    //    }
    //}

}
