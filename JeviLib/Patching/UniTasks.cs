using Il2CppCysharp.Threading.Tasks;
using HarmonyLib;
using Il2CppTimeSpan = Il2CppSystem.TimeSpan;
using Il2CppCancellationToken = Il2CppSystem.Threading.CancellationToken;
using Il2CppSystem.Threading;
using System;

namespace Jevil.Patching;

#if false

// Any UniTask.Delay other than the fourth one crashes the game outright upon being called with a null cancellationtoken and has no method body (as confirmed by CPP2IL)
// new CancellationToken(bool) crashes the game outright, as Unhollower tries to IL2CPP unbox the struct in its constructor, when its existence is unknown to IL2CPP. Funny.
internal static class UniTasks
{
    // public static UniTask Delay(int millisecondsDelay, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, [Optional] CancellationToken cancellationToken)
    [HarmonyPatch(typeof(UniTask), nameof(UniTask.Delay), new Type[] { typeof(int), typeof(bool), typeof(PlayerLoopTiming), typeof(Il2CppCancellationToken) })]
    public static class Delay_1
    {
        public static bool Prefix(ref UniTask __result, int millisecondsDelay, bool ignoreTimeScale, PlayerLoopTiming delayTiming, Il2CppCancellationToken cancellationToken)
        {
            __result = UniTask.Delay(Il2CppTimeSpan.FromMilliseconds(millisecondsDelay), ignoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime, delayTiming, cancellationToken ?? new Il2CppCancellationToken());
            return false;
        }
    }

    // public static UniTask Delay(TimeSpan delayTimeSpan, bool ignoreTimeScale = false, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, [Optional] Il2CppCancellationToken cancellationToken)
    [HarmonyPatch(typeof(UniTask), nameof(UniTask.Delay), new Type[] { typeof(Il2CppTimeSpan), typeof(bool), typeof(PlayerLoopTiming), typeof(Il2CppCancellationToken) })]
    public static class Delay_2
    {
        public static bool Prefix(ref UniTask __result, Il2CppTimeSpan delayTimeSpan, bool ignoreTimeScale, PlayerLoopTiming delayTiming, Il2CppCancellationToken cancellationToken)
        {
            __result = UniTask.Delay(delayTimeSpan, ignoreTimeScale ? DelayType.UnscaledDeltaTime : DelayType.DeltaTime, delayTiming, cancellationToken ?? new Il2CppCancellationToken());
            return false;
        }
    }

    // public static UniTask Delay(int millisecondsDelay, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, [Optional] Il2CppCancellationToken cancellationToken)
    [HarmonyPatch(typeof(UniTask), nameof(UniTask.Delay), new Type[] { typeof(int), typeof(DelayType), typeof(PlayerLoopTiming), typeof(Il2CppCancellationToken) })]
    public static class Delay_3 
    {
        public static bool Prefix(ref UniTask __result, int millisecondsDelay, DelayType delayType, PlayerLoopTiming delayTiming, Il2CppCancellationToken cancellationToken)
        {
            __result = UniTask.Delay(Il2CppTimeSpan.FromMilliseconds(millisecondsDelay), delayType, delayTiming, cancellationToken ?? new Il2CppCancellationToken());
            return false;
        }
    }

    [HarmonyPatch(typeof(UniTask), nameof(UniTask.Delay), new Type[] { typeof(int), typeof(DelayType), typeof(PlayerLoopTiming), typeof(Il2CppCancellationToken) })]
    public static class Delay_4
    {
        // patch the main Delay to make sure the cancellationtoken is never null, as that will cause a trace to appear for one frame before hardcrashing
        public static void Prefix(ref Il2CppCancellationToken cancellationToken)
        {
            cancellationToken ??= new Il2CppCancellationToken();
        }
    }

    // ONLY USE VVVVV
    // ONLY USE public unsafe static UniTask Delay(TimeSpan delayTimeSpan, DelayType delayType, PlayerLoopTiming delayTiming = PlayerLoopTiming.Update, [Optional] CancellationToken cancellationToken)
    // ONLY USE ^^^^^
}

#endif