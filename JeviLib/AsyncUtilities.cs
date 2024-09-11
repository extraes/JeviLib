using Il2CppCysharp.Threading.Tasks;
using Il2CppSystem.Runtime.CompilerServices;
using Il2CppSystem.Threading;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Jevil;

/// <summary>
/// Contains utility methods for doing things asynchronously. Does not contain extension methods. <see cref="AsyncExtensions"/> does.
/// </summary>
public static class AsyncUtilities
{
    internal struct MainThreadExecutor
    {
        public Action execute;
        public UniTaskCompletionSource completer;
    }

    internal static ConcurrentQueue<MainThreadExecutor> mainThreadCallbacks = new();

    /// <summary>
    /// Allows a <see cref="ForEachTimeSlice{T}(IEnumerable{T}, Action{T}, float, PlayerLoopTiming)"/> to modify
    /// </summary>
    /// <typeparam name="T">Any type</typeparam>
    /// <param name="param">A parameter that, when modified by your callback, will modify the value stored in the array.</param>
    public delegate void ForEachRefCallback<T>(ref T param);

    /// <summary>
    /// Iterates through the given collection, pausing for a frame when <paramref name="whatToDo"/> has executed more than <paramref name="msPerTick"/> milliseconds on the current frame.
    /// </summary>
    /// <param name="collection">A collection of items.</param>
    /// <param name="whatToDo">Any action that accepts an input of type <typeparamref name="T"/></param>
    /// <param name="msPerTick">The number of milliseconds per frame that <paramref name="whatToDo"/> should be allowed to execute every time its execution begins.</param>
    /// <param name="timing">The <see cref="PlayerLoopTiming"/> to use when yielding execution. It is recommended to keep this to <see cref="PlayerLoopTiming.Update"/> as <see cref="PlayerLoopTiming.FixedUpdate"/> may cause spikes on frames where physics objects are updated.</param>
    /// <returns>An awaitable <see cref="Task"/></returns>
    public static async Task ForEachTimeSlice<T>(IEnumerable<T> collection, Action<T> whatToDo, float msPerTick = 1, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        Stopwatch sw = Stopwatch.StartNew();
        TimeSpan ms = TimeSpan.FromMilliseconds(msPerTick);
        foreach (T item in collection)
        {
            whatToDo(item);

            if (sw.Elapsed > ms)
            {
                await UniTask.Yield(timing);
                sw.Restart();
            }
        }
    }

    /// <summary>
    /// Iterates through the given collection, pausing for a frame when <paramref name="whatToDo"/> has executed more than <paramref name="msPerTick"/> milliseconds on the current frame.
    /// </summary>
    /// <param name="array">An array of items.</param>
    /// <param name="whatToDo">Any action that accepts an input of type <typeparamref name="T"/>, passed a reference to each element of the given array.</param>
    /// <param name="msPerTick">The number of milliseconds per frame that <paramref name="whatToDo"/> should be allowed to execute every time its execution begins.</param>
    /// <param name="timing">The <see cref="PlayerLoopTiming"/> to use when yielding execution. It is recommended to keep this to <see cref="PlayerLoopTiming.Update"/> as <see cref="PlayerLoopTiming.FixedUpdate"/> may cause spikes on frames where physics objects are updated.</param>
    /// <returns>An awaitable <see cref="Task"/></returns>
    public static async Task RefForEachTimeSlice<T>(T[] array, ForEachRefCallback<T> whatToDo, float msPerTick = 1, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        Stopwatch sw = Stopwatch.StartNew();
        TimeSpan ms = TimeSpan.FromMilliseconds(msPerTick);
        for (int i = 0; i < array.Length; i++)
        {
            whatToDo(ref array[i]);

            if (sw.Elapsed > ms)
            {
                await UniTask.Yield(timing);
                sw.Restart();
            }
        }
    }

    /// <summary>
    /// <see cref="ForEachTimeSlice{T}(IEnumerable{T}, Action{T}, float, PlayerLoopTiming)"/>, except it gives you the index.
    /// </summary>
    /// <param name="start">The index to start at.</param>
    /// <param name="approach">The index to approach, exclusive. Can be higher, lower, or equal to <paramref name="start"/>.</param>
    /// <param name="whatToDo">What to do with the index.</param>
    /// <param name="msPerFrame">The number of milliseconds per frame that <paramref name="whatToDo"/> should be allowed to execute.</param>
    /// <param name="timing">The <see cref="PlayerLoopTiming"/> to use when yielding execution. It is recommended to keep this to <see cref="PlayerLoopTiming.Update"/> as <see cref="PlayerLoopTiming.FixedUpdate"/> may cause spikes on frames where physics objects are updated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    public static async Task ForTimeSlice(int start, int approach, Action<int> whatToDo, float msPerFrame = 1, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        if (start == approach) return;

        Stopwatch sw = Stopwatch.StartNew();
        TimeSpan ms = TimeSpan.FromMilliseconds(msPerFrame);
        int incOrDecBy = Math.Sign(approach - start);

        for (int i = 0; i != approach; i += incOrDecBy)
        {
            whatToDo(i);

            if (sw.Elapsed > ms)
            {
                await UniTask.Yield(timing);
                sw.Restart();
            }
        }
    }

    /// <summary>
    /// <see cref="ForEachTimeSlice{T}(IEnumerable{T}, Action{T}, float, PlayerLoopTiming)"/>, except it gives you the index and awaits the result.
    /// <para>This is kinda pointless but its a nicety.</para>
    /// </summary>
    /// <param name="start">The index to start at.</param>
    /// <param name="approach">The index to approach, exclusive. Can be higher, lower, or equal to <paramref name="start"/>.</param>
    /// <param name="whatToDo">What to do with the index.</param>
    /// <param name="msPerFrame">The number of milliseconds per frame that <paramref name="whatToDo"/> should be allowed to execute.</param>
    /// <param name="timing">The <see cref="PlayerLoopTiming"/> to use when yielding execution. It is recommended to keep this to <see cref="PlayerLoopTiming.Update"/> as <see cref="PlayerLoopTiming.FixedUpdate"/> may cause spikes on frames where physics objects are updated.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    public static async Task ForTimeSlice(int start, int approach, Func<int, Task> whatToDo, float msPerFrame = 1, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        if (start == approach) return;

        Stopwatch sw = Stopwatch.StartNew();
        TimeSpan ms = TimeSpan.FromMilliseconds(msPerFrame);
        int incOrDecBy = Math.Sign(approach - start);

        for (int i = 0; i != approach; i += incOrDecBy)
        {
            await whatToDo(i);

            if (sw.Elapsed > ms)
            {
                await UniTask.Yield(timing);
                sw.Restart();
            }
        }
    }

    /// <summary>
    /// Allows you to await a call to <see cref="UnityWebRequest.BeginWebRequest"/> or other <see cref="AsyncOperation"/> not covered by <see cref="AsyncExtensions"/>.
    /// <para>You will still need to keep a copy of its return result, as this method does not allow you to retrieve asset after the fact.</para>
    /// </summary>
    /// <param name="ao">Any <see cref="AsyncOperation"/>.</param>
    /// <param name="timing">See: <see cref="PlayerLoopTiming"/></param>
    /// <returns>An awaitable <see cref="UniTask"/></returns>
    public static UniTask ToUniTask(AsyncOperation ao, PlayerLoopTiming timing = PlayerLoopTiming.Update)
    {
        return UnityAsyncExtensions.ToUniTask(ao, null, timing, new Il2CppSystem.Threading.CancellationToken());
    }

    /// <summary>
    /// Executes an action on the main thread, possibly returning to the main thread as well.
    /// </summary>
    /// <param name="fun">The action to execute on the main thread.</param>
    /// <returns>A UniTask that will be completed when JeviLib executes your callback on the main thread</returns>
    public static UniTask RunOnMainThread(Action fun)
    {
        MainThreadExecutor mte = new()
        {
            completer = new(),
            execute = fun
        };
        mainThreadCallbacks.Enqueue(mte);
        return mte.completer.Task;
    }

    #region WrapNoThrow

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// </summary>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <returns><see langword="null"/> if <c><see langword="await"/> <paramref name="taskReturner"/>();</c> was successful. An <see cref="Exception"/></returns>
    public static async Task<Exception?> WrapNoThrow(Func<Task> taskReturner)
    {
        try
        {
            await taskReturner();
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// </summary>
    /// <typeparam name="TParam1"/>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <param name="param1">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <returns><see langword="null"/> if <c><see langword="await"/> <paramref name="taskReturner"/>();</c> was successful. An <see cref="Exception"/></returns>
    public static async Task<Exception?> WrapNoThrow<TParam1>(Func<TParam1, Task> taskReturner, TParam1 param1)
    {
        try
        {
            await taskReturner(param1);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// </summary>
    /// <typeparam name="TParam1"/> <typeparam name="TParam2"/>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <param name="param1">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <param name="param2">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <returns><see langword="null"/> if <c><see langword="await"/> <paramref name="taskReturner"/>();</c> was successful. An <see cref="Exception"/></returns>
    public static async Task<Exception?> WrapNoThrow<TParam1, TParam2>(Func<TParam1, TParam2, Task> taskReturner, TParam1 param1, TParam2 param2)
    {
        try
        {
            await taskReturner(param1, param2);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// </summary>
    /// <typeparam name="TParam1"/> <typeparam name="TParam2"/> <typeparam name="TParam3"/>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <param name="param1">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <param name="param2">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <param name="param3">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <returns><see langword="null"/> if <c><see langword="await"/> <paramref name="taskReturner"/>();</c> was successful. An <see cref="Exception"/></returns>
    public static async Task<Exception?> WrapNoThrow<TParam1, TParam2, TParam3>(Func<TParam1, TParam2, TParam3, Task> taskReturner, TParam1 param1, TParam2 param2, TParam3 param3)
    {
        try
        {
            await taskReturner(param1, param2, param3);
            return null;
        }
        catch (Exception ex)
        {
            return ex;
        }
    }



    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// <para>See also: <seealso cref="OneOf{TRes, TExc}"/>, <seealso cref="OneOf{TRes, TExc}.HasException"/></para>
    /// </summary>
    /// <typeparam name="TRes">The original return type of <paramref name="taskReturner"/></typeparam>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <returns>A wrapper that will either contain an exception or the result of <c><see langword="await"/> <paramref name="taskReturner"/>();</c></returns>
    public static async Task<OneOf<TRes, Exception>> WrapNoThrowWithResult<TRes>(Func<Task<TRes>> taskReturner)
    {
        try
        {
            return new OneOf<TRes, Exception>(await taskReturner());
        }
        catch (Exception ex)
        {
#if DEBUG
            return new OneOf<TRes, Exception>(ex, taskReturner.Method);
#else
            return new OneOf<TRes, Exception>(ex);
#endif
        }
    }

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// <para>See also: <seealso cref="OneOf{TRes, TExc}"/>, <seealso cref="OneOf{TRes, TExc}.HasException"/></para>
    /// </summary>
    /// <typeparam name="TParam1"/>
    /// <typeparam name="TRes">The original return type of <paramref name="taskReturner"/></typeparam>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <param name="param1">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <returns>A wrapper that will either contain an exception or the result of <c><see langword="await"/> <paramref name="taskReturner"/>();</c></returns>
    public static async Task<OneOf<TRes, Exception>> WrapNoThrowWithResult<TParam1, TRes>(Func<TParam1, Task<TRes>> taskReturner, TParam1 param1)
    {
        try
        {
            return new OneOf<TRes, Exception>(await taskReturner(param1));
        }
        catch (Exception ex)
        {
#if DEBUG
            return new OneOf<TRes, Exception>(ex, taskReturner.Method);
#else
            return new OneOf<TRes, Exception>(ex);
#endif
        }
    }

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// <para>See also: <seealso cref="OneOf{TRes, TExc}"/>, <seealso cref="OneOf{TRes, TExc}.HasException"/></para>
    /// </summary>
    /// <typeparam name="TParam1"/> <typeparam name="TParam2"/>
    /// <typeparam name="TRes">The original return type of <paramref name="taskReturner"/></typeparam>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <param name="param1">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <param name="param2">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <returns>A wrapper that will either contain an exception or the result of <c><see langword="await"/> <paramref name="taskReturner"/>();</c></returns>
    public static async Task<OneOf<TRes, Exception>> WrapNoThrowWithResult<TParam1, TParam2, TRes>(Func<TParam1, TParam2, Task<TRes>> taskReturner, TParam1 param1, TParam2 param2)
    {
        try
        {
            return new OneOf<TRes, Exception>(await taskReturner(param1, param2));
        }
        catch (Exception ex)
        {
#if DEBUG
            return new OneOf<TRes, Exception>(ex, taskReturner.Method);
#else
            return new OneOf<TRes, Exception>(ex);
#endif
        }
    }

    /// <summary>
    /// Wraps the awaiting of a task so it doesn't throw an exception.
    /// <para>See also: <seealso cref="OneOf{TRes, TExc}"/>, <seealso cref="OneOf{TRes, TExc}.HasException"/></para>
    /// </summary>
    /// <typeparam name="TParam1"/> <typeparam name="TParam2"/> <typeparam name="TParam3"/>
    /// <typeparam name="TRes">The original return type of <paramref name="taskReturner"/></typeparam>
    /// <param name="taskReturner">Any parameterless method/lambda that returns an awaitable <see cref="Task{TResult}"/></param>
    /// <param name="param1">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <param name="param2">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <param name="param3">Parameter to pass into <paramref name="taskReturner"/>.</param>
    /// <returns>A wrapper that will either contain an exception or the result of <c><see langword="await"/> <paramref name="taskReturner"/>();</c></returns>
    public static async Task<OneOf<TRes, Exception>> WrapNoThrowWithResult<TParam1, TParam2, TParam3, TRes>(Func<TParam1, TParam2, TParam3, Task<TRes>> taskReturner, TParam1 param1, TParam2 param2, TParam3 param3)
    {
        try
        {
            return new OneOf<TRes, Exception>(await taskReturner(param1, param2, param3));
        }
        catch (Exception ex)
        {
#if DEBUG
            return new OneOf<TRes, Exception>(ex, taskReturner.Method);
#else
            return new OneOf<TRes, Exception>(ex);
#endif
        }
    }

    #endregion
}
