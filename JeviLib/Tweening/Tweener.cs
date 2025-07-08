using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// The class that controls Tweens. 
/// <para>Tweens are initiated via extenison methods. The only thing that may be of interest is <see cref="CancelTweensOn(UnityEngine.Object)"/>.</para>
/// </summary>
public static class Tweener
{
    internal static Func<float, float> EasingInterpolator = inVal => (0f, 1f).Interpolate(inVal, true);
    internal static Func<float, float> LinearInterpolator = inVal => (0f, 1f).Interpolate(inVal, false);

    internal static readonly List<TweenBase> tweens = new(32);

    internal static void UpdateAll()
    {
        if (tweens.Count == 0)
            return;

        for (int i = tweens.Count - 1; i >= 0; i--)
        {
            TweenBase tween = tweens[i];
            IntPtr ptr = IntPtr.Zero;
            try
            {
                ptr = tween.CancelWith.Pointer;
            }
            catch { }
            // check using the equality operator because then it checks if its destroyed.
            if (tween.CancelWith == null || ptr == IntPtr.Zero)
            {
#if DEBUG
                JeviLib.Log($"Cancelling {tween.name}; Its corresponding UnityEngine.Object was destroyed or null.");
#endif
                tween.Active = false;
                tween.IsCancelled = true;
                tweens.RemoveAt(i);
                continue;
            }
            else if (tween.UnscaledTimeRemaining == 0)
            {
#if DEBUG
                JeviLib.Log($"Removing {tween.name}; It is done.");
#endif
                tweens.RemoveAt(i);
                tween.FinishInternal();
                continue;
            }

            float increment = tween.IsRealtime ? Time.unscaledDeltaTime : Time.deltaTime;
            
            try
            {
                tween.UpdateInternal(increment);
            }
            catch(Exception ex)
            {
#if DEBUG
                JeviLib.Log($"Removing {tween.name}; There was an exception whilst updating it.");
                JeviLib.Log($"Tween exception: {ex}", ConsoleColor.DarkRed);
#endif
                tweens.RemoveAt(i);
            }
        }
    }

    internal static void AddTween(TweenBase tween)
    {
#if DEBUG
        if (tween.IsCancelled || tween.CancelWith == null)
        {
            JeviLib.Warn("Don't try to add a cancelled Tween or a Tween with a null CancelWith into the list of Tweens! FYI, the Tween was acting on " + tween.name);
            JeviLib.Warn("This check is exclusive to debug builds, do not rely on this check in your production code!");
            return;
        }
#endif
        if (tweens.Contains(tween))
        {
#if DEBUG
            JeviLib.Warn("Can't double-add a Tween to the list of acive Tweens! FYI, the Tween was acting on " + tween.name);
#endif
            return;
        }

        tweens.Add(tween);

#if DEBUG
        JeviLib.Log("Adding Tween " + tween.name + " to the list of active Tweens.");
#endif
    }

    internal static void RemoveTween(TweenBase tween)
    {
#if DEBUG
        JeviLib.Log("Removing tween " + tween.name);
#endif

        tweens.Remove(tween);
    }

    /// <summary>
    /// Cancels all Tweens that are acting upon the Object <paramref name="cancelWith"/>.
    /// </summary>
    /// <param name="cancelWith">The object that Tweens cancel with. Can be a <see cref="Component"/> (like <see cref="AudioSource"/>), <see cref="GameObject"/>, or <see cref="Transform"/>. Likely a Transform.</param>
    /// <returns>The number of Tweenbs that were cancelled.</returns>
    public static int CancelTweensOn(UnityEngine.Object cancelWith)
    {
        TweenBase[] tweensToCancel = tweens.Where(tween => tween.CancelWith == cancelWith).ToArray();
#if DEBUG
        JeviLib.Log("Cancelling " + tweensToCancel.Length + " Tweens that act on the Unity object " + cancelWith.name);
#endif
        foreach (TweenBase tween in tweensToCancel)
        {
            RemoveTween(tween);
        }
        return tweensToCancel.Length;
    }
}
