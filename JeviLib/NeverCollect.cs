using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil;

/// <summary>
/// A self-referencing IL2CPP type. This has a check to make sure there is only one of itself ever in existence.
/// <para>This is technically a memory leak. Don't think too hard about it, because it's only leaking one MonoBehaviour's worth of RAM.</para>
/// </summary>
[MelonLoader.RegisterTypeInIl2Cpp]
public class NeverCollect : MonoBehaviour
{
    /// <summary>
    /// An IL2CPP object that holds a reference to another IL2CPP object, preventing the IL2 obj's from being collected.
    /// <para>This <see cref="NeverCollect"/> holds a reference to itself, preventing it from being colelcted.</para>
    /// </summary>
    public Il2CppSystem.Collections.Generic.List<Il2CppSystem.Object> neverCollect = new();

    static bool instantiated = false;

    /// <summary>
    /// Instantiate a new instance of <see cref="NeverCollect"/>. This should only be done once per game load.
    /// </summary>
    /// <param name="ptr">Pointer. This should only be called by IL2CPP because the pointer gets immediately passed back into the IL2 domain via the <see cref="MonoBehaviour"/> constructor.</param>
    /// <exception cref="ObjectDisposedException"></exception>
    public NeverCollect(IntPtr ptr) : base(ptr)
    {
#if DEBUG
        JeviLib.Log($"New {nameof(NeverCollect)} created. Inital check says this is{(instantiated ? "" : " not")} the first.");
#endif
        if (instantiated) throw new ObjectDisposedException($"{nameof(NeverCollect)} should never be instantiated twice!");
        neverCollect.Add(this);
        neverCollect.Add(neverCollect); // memory leak cuz fuck you
        instantiated = true;
        JeviLib.unityMainThread = Thread.CurrentThread.ManagedThreadId;
    }
}
