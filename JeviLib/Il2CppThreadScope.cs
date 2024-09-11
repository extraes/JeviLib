using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Il2CppInterop.Runtime;

namespace Jevil;

/// <summary>
/// <para><b>It's recommended you use <see cref="Jevil.Patching.UngovernableAttribute"/></b> instead.</para>
/// Makes IL2CPP aware of the current thread, (so it can GC on it?).
/// <para>This allows you to call (thread-safe) IL2CPP methods from off-main threads.</para>
/// <para><b>The code this is based off was found by Lakatrazz! I'm not <i>that</i> good!</b></para>
/// <see href="https://github.com/djkaty/Il2CppInspector/issues/155#issuecomment-821991119"/>
/// <see href="https://forum.unity.com/threads/when-are-callback-for-il2cpp_thread_attach-processed.1095472/"/>
/// </summary>
[Obsolete(nameof(Il2CppThreadScope) + " is no longer recommended. If calling allocating native methods from an async method, use [assembly: Ungovernable(...)]. If explicitly creating threads, just call IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get()). It's not an expensive call.")]
public class Il2CppThreadScope : IDisposable
{
    [ThreadStatic] internal static int usesOfThisThreadInIl2Cpp;
    [ThreadStatic] internal static IntPtr thisThreadInIl2Cpp;
    [ThreadStatic] static int? managedThreadId;
    int startingThread;

    /// <summary>
    /// Creates a new Il2CPP thread scope. This should not cross thread boundaries, as if it does there is no use in its existence.
    /// </summary>
    public Il2CppThreadScope()
    {
#if DEBUG
        using var ps = new ProfilingScope(JeviLib.instance.LoggerInstance, ProfilingScope.ProfilingType.STOPWATCH_EXECUTION_TIME, "Thread attach");
#endif

        Enter();
    }

    /// <summary>
    /// Detaches the current thread from IL2CPP, awaits the task, and then attaches IL2CPP again.
    /// </summary>
    /// <param name="task">Any awaitable task.</param>
    /// <returns>Task representing the actions Il2CppThreadScope will perform, as well as your task.</returns>
    public async Task Await(Task task)
    {
#if DEBUG
        if (startingThread != managedThreadId) throw new ThreadStateException("Il2CppThreadScope should stay on the same thread! This may be due to something being 'await'ed without using Il2CppThreadScope.Await.");
#endif
        
        Exit();
        await task;
        Enter();
    }

    /// <summary>
    /// Detaches the current thread from IL2CPP, awaits the task, and then attaches IL2CPP again.
    /// </summary>
    /// <param name="task">Any awaitable task.</param>
    /// <returns>Task representing the actions Il2CppThreadScope will perform, as well as your task.</returns>
    public async Task<T> Await<T>(Task<T> task)
    {
#if DEBUG
        if (startingThread != managedThreadId) throw new ThreadStateException("Il2CppThreadScope should stay on the same thread! This may be due to something being 'await'ed without using Il2CppThreadScope.Await.");
#endif

        Exit();
        await task;
        Enter();

        return task.Result;
    }

    /// <summary>
    /// Detaches the IL2CPP GC from the current thread (?)
    /// <para>Calls <see cref="IL2CPP.il2cpp_thread_detach(IntPtr)"/>, suggesting it detaches IL2CPP from the current thread, making it blissfully unaware of the chicanery ongoing in the Mono domain.</para>
    /// </summary>
    public void Dispose()
    {
#if DEBUG
        if (startingThread != managedThreadId) throw new ThreadStateException("Il2CppThreadScope should stay on the same thread! This may be due to something being 'await'ed.");
#endif

        if (managedThreadId == JeviLib.unityMainThread) return;
        Exit();
    }

    void Enter()
    {
        ThisThreadInUse();
        // micro-optimization: avoid calling extern (possibly expensive) getters get_CurrentThread and get_ManagedThreadId
        managedThreadId ??= Environment.CurrentManagedThreadId;
        startingThread = managedThreadId.Value;
    }

    void Exit()
    {
        //ThisThreadNoLongerInUse();
    }

    static void ThisThreadInUse()
    {
        if (managedThreadId == JeviLib.unityMainThread) return; // dont dare do shit with the main thread

        usesOfThisThreadInIl2Cpp++;

        if (usesOfThisThreadInIl2Cpp == 1)
        {
#if DEBUG
            JeviLib.Log("Attaching IL2CPP to current thread");
#endif
            thisThreadInIl2Cpp = IL2CPP.il2cpp_thread_attach(IL2CPP.il2cpp_domain_get());
        }
    }

    static void ThisThreadNoLongerInUse()
    {
        if (managedThreadId == JeviLib.unityMainThread) return; // dont dare do shit with the main thread

        usesOfThisThreadInIl2Cpp--;

        if (usesOfThisThreadInIl2Cpp == 0)
        {
#if DEBUG
            JeviLib.Log("Detaching IL2CPP from current thread");
#endif
            IL2CPP.il2cpp_thread_detach(thisThreadInIl2Cpp);
            thisThreadInIl2Cpp = IntPtr.Zero;
        }
    }
}
