using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Jevil;

/// <summary>
/// A structure representing a result of a task call.
/// <para>Attempting to access the result if the task threw an exception will throw an exception in response. Debug builds inform you what method call was wrapped.</para>
/// </summary>
/// <typeparam name="TRes"></typeparam>
/// <typeparam name="TExc"></typeparam>
public readonly struct OneOf<TRes, TExc> where TExc : Exception
{
    private readonly TRes result;
    private readonly TExc exception;
#if DEBUG
    private readonly MethodInfo originalMethod;
#endif

    /// <summary>
    /// Gets the result of your original task call. This will throw an exception if <see cref="HasException"/> is <see langword="true"/>.
    /// </summary>
    public TRes Result => HasResult 
                            ? result
#if DEBUG
                            : throw new InvalidOperationException($"OneOf<{typeof(TRes).FullName}, {typeof(TExc).FullName}> (wrapping method call to {originalMethod?.FullDescription() ?? "<User Code>"}) errored, so there is no result to get.", exception);
#else
                            : throw new InvalidOperationException($"OneOf<{typeof(TRes).FullName}, {typeof(TExc).FullName}> errored, so there is no result to get.", exception);
#endif

    /// <summary>
    /// Whether <see cref="Result"/> is available.
    /// </summary>
    public bool HasResult => !HasException;

    /// <summary>
    /// Whether your wrapped <see cref="Task"/>/<see cref="Task{TResult}"/> errored.
    /// </summary>
    public bool HasException => exception != null;

#if DEBUG
    /// <summary>
    /// Creates a new wrapper for your value, similar to <see cref="Task.FromResult{TResult}(TResult)"/>
    /// </summary>
    /// <param name="result"></param>
    public OneOf(TRes result)
    {
        this.result = result;
        this.exception = null;
        this.originalMethod = null; // not necessary if a result was actually gotten
    }

    internal OneOf(TExc exception, MethodInfo ogMethod)
    {
        this.result = default;
        this.exception = exception;
        this.originalMethod = ogMethod;
    }
#else

    internal OneOf(TRes result)
    {
        this.result = result;
        this.exception = null;
    }

    internal OneOf(TExc exception)
    {
        this.result = default;
        this.exception = (TExc)exception;
    }

#endif

    // add methods to explicitly call in case people don't like using the implicit conversions
    /// <summary>
    /// Creates a successful <see cref="OneOf{TRes, TExc}"/> from a value.
    /// </summary>
    public static OneOf<TRes, TExc> FromSuccess(TRes result)
    {
        return result;
    }

    /// <summary>
    /// Creates a failed <see cref="OneOf{TRes, TExc}"/> from an exception.
    /// </summary>
    public static OneOf<TRes, TExc> FromFailure(TExc ex)
    {
        return ex;
    }
    
    /// <summary>
    /// Retrieves the <see cref="Result"/> from <paramref name="o"/>.
    /// </summary>
    /// <param name="o"></param>
    public static implicit operator TRes(OneOf<TRes, TExc> o) => o.Result;

    /// <summary>
    /// Converts a exception into itself wrapped into a <see cref="OneOf{TRes, TExc}"/> struct.
    /// </summary>
    /// <param name="value">The exception to be wrapped.</param>
    public static implicit operator OneOf<TRes, TExc> (TRes value)
    {
        return new OneOf<TRes, TExc>(value);
    }

    /// <summary>
    /// Converts a exception into itself wrapped into a <see cref="OneOf{TRes, TExc}"/> struct.
    /// </summary>
    /// <param name="exception">The exception to be wrapped.</param>
    public static implicit operator OneOf<TRes, TExc>(Exception exception)
    {
#if DEBUG
        return new OneOf<TRes, TExc>((TExc)exception, null);
#else
        return new OneOf<TRes, TExc>((TExc)exception);
#endif
    }
}
