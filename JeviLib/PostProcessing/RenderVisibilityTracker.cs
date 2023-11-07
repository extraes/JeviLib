using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;

namespace Jevil.PostProcessing;

[RegisterTypeInIl2Cpp]
internal class RenderVisibilityTracker : MonoBehaviour
{
    public RenderVisibilityTracker(IntPtr peter) : base(peter) { } // hey lois

    public static IEnumerable<Renderer> VisibleRenderers => visibleTrackers.SelectMany(rvt => rvt.renderers).NoUNull();
    static readonly HashSet<RenderVisibilityTracker> visibleTrackers = new();
    static readonly List<RenderVisibilityTracker> allInstances = new();

    // technically it'd be more efficient to declare this as an IL2CPPArrayBase but i dont like my shit being GC'd by the wicked bitch of the runtime 
    Renderer[] renderers;

    void Awake()
    {
        allInstances.Add(this);
        renderers = GetComponents<Renderer>();
        visibleTrackers.Add(this);
    }

    void OnDestroy()
    {
        allInstances.Remove(this);
    }

    void OnBecameVisible()
    {
        visibleTrackers.Add(this);
    }

    void OnBecameInvisible()
    {
        visibleTrackers.Remove(this);
    }
}
