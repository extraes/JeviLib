using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Waiting;

/// <summary>
/// Configure whether <see cref="CallDelayed"/>'s methods will cancel their calls or wait in scaled time or in realtime.
/// </summary>
public class CallToken
{
    /// <summary>
    /// Cancels the call so it doesn't call. You can change your mind at any time so long as <see cref="TimesUp"/> is <see langword="false"/>.
    /// </summary>
    public bool doNotCall = false;
    /// <summary>
    /// Switches from using <see cref="Time.deltaTime"/> to <see cref="Time.unscaledTime"/>. Defaults to <see langword="true"/>
    /// <para>If the waiter is on the Mono domain's C# thread pool, this will be ignored and will always be in realtime.</para>
    /// </summary>
    public bool waitInRealtime = true;

    /// <summary>
    /// Whether the waiter will be running in a Unity coroutine or if it'll be running in the Mono domain's thread pool.
    /// <para>If running on the thread pool, the waiter cannot use scaled time. That is, it will always be realtime, so it won't be hindered by Unity's main thread.</para>
    /// <para><see langword="true"/> if on thread pool, <see langword="false"/> if coroutine.</para>
    /// </summary>
    public bool IsAsync { get; init; }
    /// <summary>
    /// Whether or not the time to execute your call has passed/whether the call has already been invoked.
    /// </summary>
    public bool TimesUp { get; internal set; }

    internal Action call;
    internal CallToken(Action call, bool isAsnyc) 
    {
        this.call = call;
        this.IsAsync = isAsnyc;
    }

    /// <summary>
    /// Add another call to be invoked at the same time.
    /// </summary>
    /// <param name="fun"></param>
    public void AddCall(Action fun) => call += fun;
}
