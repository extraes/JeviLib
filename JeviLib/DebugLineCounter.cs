using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Jevil;

/// <summary>
/// In the event that you need something to be print-debugged with a line-by-line of where stuff went wrong, use this as a <see langword="using"/> variable.
/// <para>Use <see cref="UpdateProgress(int)"/> to update progress, and <see cref="Success"/> if the instance you created is no longer needed.</para>
/// </summary>
public sealed class DebugLineCounter : IDisposable
{
    /// <summary>
    /// The kind of <see cref="DebugLineCounter"/> made.
    /// </summary>
    public enum Kind
    {
        /// <summary>
        /// Counts up every time 
        /// </summary>
        CHECKPOINT_COUNTER,
        /// <summary>
        /// Uses the <see cref="CallerLineNumberAttribute"/> to have the compiler track the line number <see cref="UpdateProgress(int)"/> was called from.
        /// </summary>
        LINE_NUMBER
    }

#if DEBUG
    readonly MelonLogger.Instance logger;
    readonly string whatDoing;
    readonly Kind kind;
    bool success;
    int num;
#endif

    /// <summary>
    /// Creates a new line counter for debugging. Be sure to call <see cref="Success"/> when the thing you're tracking finishes successfully.
    /// </summary>
    /// <param name="logger">The MelonLogger instance to use when logging.</param>
    /// <param name="kind">Determines what <see cref="UpdateProgress(int)"/> does, and what <see cref="Dispose"/> will log.</param>
    /// <param name="doingWhat">What are you trying to track? Will be used in the following format: <c>... unexpectedly erroring during {doingWhat}!...</c></param>
    public DebugLineCounter(MelonLogger.Instance logger, Kind kind, string doingWhat)
    {
#if DEBUG
        this.logger = logger;
        this.kind = kind;
        this.whatDoing = doingWhat;
#endif
    }

    /// <summary>
    /// Update this <see cref="DebugLineCounter"/>'s progress.
    /// </summary>
    /// <param name="progressNum">If this is a checkpoint counter, this is unused. As a line number counter, the compiler will automatically place in the line where this was called from.</param>
    public void UpdateProgress([CallerLineNumber] int progressNum = -1)
    {
#if DEBUG
        switch (kind)
        {
            case Kind.CHECKPOINT_COUNTER:
                num++;
                break;
            default:
                num = progressNum;
                break;
        }
#endif
    }

    /// <summary>
    /// Informs this counter that it is no longer needed as the operation it was tracking completed successfully.
    /// </summary>
    public void Success()
    {
#if DEBUG
        success = true;
#endif
    }

    /// <summary>
    /// It's recommended you just let the compiler do this for you.
    /// </summary>
    public void Dispose()
    {
#if DEBUG
        if (success) return;

        switch (kind)
        {
            case Kind.CHECKPOINT_COUNTER:
                logger.Error($"Reached the {num}-th checkpoint before unexpectedly erroring during {whatDoing}! If there was no error thrown, be sure to call {nameof(DebugLineCounter)}.{nameof(Success)}().");
                break;
            case Kind.LINE_NUMBER:
                logger.Error($"Got past line {num} before unexpectedly erroring during ({whatDoing})! If there was no error thrown, be sure to call {nameof(DebugLineCounter)}.{nameof(Success)}().");
                break;
            default:
                break;
        }
#endif
    }
}
