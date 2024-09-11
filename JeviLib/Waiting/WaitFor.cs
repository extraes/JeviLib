using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Waiting;

/// <summary>
/// Replacement for <see cref="WaitUntil"/>, because that was tragically stripped during the IL2CPP battle (build) of 2019-2020.
/// <para><i>Original docs: Suspends the coroutine execution until the supplied delegate evaluates to true.</i></para>
/// </summary>
public class WaitFor : IEnumerator
{
    readonly Func<bool> waiter;

    /// <summary>
    /// literally <see langword="null"/>
    /// </summary>
    public object Current => null!;

    /// <summary>
    /// yeah basically
    /// </summary>
    /// <returns>The value of <c>waiter()</c></returns>
    public bool MoveNext()
    {
        return !waiter();
    }

    /// <summary>
    /// throws, https://cdn.discordapp.com/attachments/587792632986730507/985659335088697454/trim.5C5AD44D-3A16-467D-8B16-2DBB397C205D.mov
    /// </summary>
    public void Reset() => throw new NotImplementedException("Cannot reset something intended to be a coroutine yield instruction.");

    /// <summary>
    /// Create a new instance of <see cref="WaitFor"/>
    /// </summary>
    /// <param name="waiter">The method (or lambda) that will be ran to determine if your coroutine should resume execution.</param>
    public WaitFor(Func<bool> waiter)
    {
        this.waiter = waiter;
    }
}
