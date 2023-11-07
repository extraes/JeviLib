using MelonLoader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Jevil.Waiting;

/// <summary>
/// Use to call an action or a unity event.
/// <para>It's recommended that you look at <see cref="CallToken"/> to know how you can configure the result.</para>
/// </summary>
public static class CallDelayed
{
    /// <summary>
    /// Call an action after <paramref name="time"/>.
    /// </summary>
    /// <param name="fun">The Action to invoke when the waiter is finished.</param>
    /// <param name="time">The time, in seconds, to wait before execution.</param>
    /// <param name="runAsync">Defaults to <see langword="false"/>, meaning it'll run your action from a coroutine. Set to <see langword="true"/> to run from the thread pool, separately from the game's code, so it will run at the right time even if the game starts lagging.
    /// <para>If you call any Unity methods from off the main thread it will immediately crash the game, and log statements may interrupt other in-progress log statements, causing one part of the log statement to have another log statement jammed in the middle.</para>
    /// </param>
    /// <returns>An instance of <see cref="CallToken"/>.</returns>
    public static CallToken CallAction(Action fun, float time, bool runAsync = false)
    {
        CallToken ct = new(fun, runAsync);

        if (runAsync)
        {
            // prevent async errors from crashing main (mono) thread (closing game w/o stack trace)
            try { _ = WaiterAsync(ct, time); }
#if DEBUG
            catch (Exception ex)
            {
                JeviLib.Error("Exception occurred in async method call " + ex);
            }
#else
            catch { } 
#endif
        }
        else MelonCoroutines.Start(WaiterUnity(ct, time));

        return ct;
    }

    /// <summary>
    /// Call an action after <paramref name="time"/>.
    /// </summary>
    /// <param name="unityEvent">The Unity Event to invoke when the waiter is finished.</param>
    /// <param name="time">The time, in seconds, to wait before execution.</param>
    /// <returns>An instance of <see cref="CallToken"/>.</returns>
    public static CallToken CallEvent(UnityEvent unityEvent, float time) => CallAction(unityEvent.Invoke, time, false);

    private static IEnumerator WaiterUnity(CallToken token, float time)
    {
        float elapsed = 0;
        while (elapsed < time)
        {
            if (token.waitInRealtime) elapsed += Time.unscaledDeltaTime;
            else elapsed += Time.deltaTime;

            yield return null;
        }
        if (!token.doNotCall) token.call();
    }

    private static async Task WaiterAsync(CallToken token, float time)
    {
        await Task.Delay((int)(time * 1000));
        if (!token.doNotCall) token.call();
    }
}
