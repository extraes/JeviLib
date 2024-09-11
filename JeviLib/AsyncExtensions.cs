using Il2CppCysharp.Threading.Tasks;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Jevil;

/// <summary>
/// Extension method class for fitting things into an asynchronus method workflow.
/// </summary>
public static class AsyncExtensions
{

    /// <summary>
    /// Allows you to await a call to <see cref="AssetBundle.LoadFromMemoryAsync(Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray{byte})"/> or <see cref="AssetBundle.LoadFromFileAsync(string)"/>.
    /// </summary>
    /// <param name="abcr">interior crocodile alligator i drive a chevrolet movie theater</param>
    /// <param name="timing">See: <see cref="PlayerLoopTiming"/></param>
    /// <returns>An awaitable <see cref="Task{TResult}"/></returns>
    public static async Task<AssetBundle> ToTask(this AssetBundleCreateRequest abcr, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        await UnityAsyncExtensions.ToUniTask(abcr, null, timing, new Il2CppSystem.Threading.CancellationToken());
        return abcr.assetBundle;
    }

    /// <summary>
    /// Allows you to await a call to <see cref="AssetBundle.LoadAssetAsync(string)"/>.
    /// <para>You will still need to keep a copy of its return result, as this method does not allow you to retrieve asset after the fact.</para>
    /// </summary>
    /// <param name="abr">interior crocodile alligator i drive a chevrolet movie theater</param>
    /// <param name="timing">See: <see cref="PlayerLoopTiming"/></param>
    /// <returns>An awaitable <see cref="UniTask"/></returns>
    public static UniTask ToUniTask(this AssetBundleRequest abr, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        return UnityAsyncExtensions.ToUniTask(abr, null, timing, new Il2CppSystem.Threading.CancellationToken());
    }

    /// <summary>
    /// Runs an action upon the successful completion of a <see cref="Task"/>
    /// </summary>
    /// <param name="awaited">The task to await.</param>
    /// <param name="runAfter">The action to run after <paramref name="awaited"/> finishes.</param>
    /// <returns>A Task that will return after <paramref name="runAfter"/> finishes execution.</returns>
    public static async Task RunOnFinish(this Task awaited, Action runAfter)
    {
#if DEBUG
        MethodBase? caller = null;
        StackTrace st = new(1);

        for (int i = 0; i < st.FrameCount; i++)
        {
            StackFrame? frame = st.GetFrame(i);
            if (frame is null)
                continue;
            MethodBase? mb = frame.GetMethod();

            if (mb != null  && mb.DeclaringType != null && mb.DeclaringType.Assembly != typeof(object).Assembly && mb.DeclaringType != typeof(AsyncExtensions))
            {
                caller = mb;
                break;
            }
        }

        try
        {
            await awaited;
            runAfter();
        }
        catch (Exception ex)
        {
            JeviLib.Warn($"(UNCAUGHT IN RELEASE) Exception while invoking method {runAfter.Method.FullDescription()} - RunOnFinish originally invoked from {caller?.FullDescription() ?? "<Unknown method>"}; Details below\n\t" + ex);
        }
#else 
        await awaited;
        runAfter();
#endif
    }

    /// <summary>
    /// Runs an action upon the successful completion of a <see cref="Task"/>
    /// </summary>
    /// <param name="awaited">The task to await.</param>
    /// <param name="runAfter">The action to run after <paramref name="awaited"/> finishes.</param>
    /// <returns>A Task that will return after <paramref name="runAfter"/> finishes execution.</returns>
    public static async Task RunOnFinish<T>(this Task<T> awaited, Action<T> runAfter)
    {
#if DEBUG
        MethodBase? caller = null;
        StackTrace st = new(1);

        for (int i = 0; i < st.FrameCount; i++)
        {
            StackFrame? frame = st.GetFrame(i);
            if (frame is null)
                continue;
            MethodBase? mb = frame.GetMethod();

            if (mb != null && mb.DeclaringType != null && mb.DeclaringType.Assembly != typeof(object).Assembly && mb.DeclaringType != typeof(AsyncExtensions))
            {
                caller = mb;
                break;
            }
        }

        try
        {
            await awaited;
            runAfter(awaited.Result);
        }
        catch (Exception ex)
        {
            JeviLib.Warn($"(UNCAUGHT IN RELEASE) Exception while invoking method {runAfter.Method.FullDescription()} - RunOnFinish originally invoked from {caller?.FullDescription() ?? "<Unknown method>"}; Details below\n\t" + ex);
        }
#else 
        await awaited;
        runAfter(awaited.Result);
#endif
    }
}
