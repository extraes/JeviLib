using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Jevil.Tweening;

/// <summary>
/// The class that holds various extension methods related to initiaiting a Tween. See also <seealso cref="Tweener"/>
/// </summary>
public static class TweenExtensions
{
    /// <summary>
    /// Tweens the given transform's world space position.
    /// <br>If it has a <see cref="Rigidbody"/>, it's recommended to set <see cref="Rigidbody.isKinematic"/> to <see langword="true"/>, as this will not respect physics.</br>
    /// </summary>
    /// <param name="t">The Transform to be moved.</param>
    /// <param name="target">Where the Transform will be moving to, in workd space.</param>
    /// <param name="length">The duration, in seconds, that the Tween wll take.</param>
    /// <returns>A <see cref="PositionTween"/> instance representing the active tween.</returns>
    public static PositionTween TweenPosition(this Transform t, Vector3 target, float length)
    {
        PositionTween tween = new(t, target, length, false);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Tweens the given transform's position in local space.
    /// <br>If it has a <see cref="Rigidbody"/>, it's recommended to set <see cref="Rigidbody.isKinematic"/> to <see langword="true"/>, as this will not respect physics.</br>
    /// </summary>
    /// <param name="t">The Transform to be moved.</param>
    /// <param name="target">Where the Transform will be moving to, in workd space.</param>
    /// <param name="length">The duration, in seconds, that the Tween wll take.</param>
    /// <returns>A <see cref="PositionTween"/> instance representing the active tween.</returns>
    public static PositionTween TweenLocalPosition(this Transform t, Vector3 target, float length)
    {
        PositionTween tween = new(t, target, length, true);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Tweens the given transform's scale in local space.
    /// <br>If it has a <see cref="Rigidbody"/>, it's recommended to set <see cref="Rigidbody.isKinematic"/> to <see langword="true"/>, as this will not respect physics.</br>
    /// </summary>
    /// <param name="t">The Transform to be moved.</param>
    /// <param name="target">Where the Transform will be moving to, in workd space.</param>
    /// <param name="length">The duration, in seconds, that the Tween wll take.</param>
    /// <returns>A <see cref="PositionTween"/> instance representing the active tween.</returns>
    public static ScaleTween TweenLocalScale(this Transform t, Vector3 target, float length)
    {
        ScaleTween tween = new(t, target, length);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Tween an <see cref="AudioSource"/>'s volume. You can use this in combination with <see cref="TweenTweenExtensions.RunOnFinish{T}(T, Action)"/> to call <see cref="AudioSource.Stop()"/> if you want to smoothly stop an AudioPlayer.
    /// </summary>
    /// <param name="a">The AudioSource to be acted upon.</param>
    /// <param name="vol">The target volume.</param>
    /// <param name="length">The duration of the Tween.</param>
    /// <returns>A <see cref="AudioTween"/> instance representing the active tween.</returns>
    public static AudioTween TweenVolume(this AudioSource a, float vol, float length)
    {
        AudioTween tween = new(a, vol, length);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Tween an <see cref="AudioSource"/>'s volume to 0 and then call <see cref="AudioSource.Stop()"/>.
    /// </summary>
    /// <param name="a">The AudioSource to be acted upon.</param>
    /// <param name="length">The duration of the Tween before it gets stopped.</param>
    /// <returns>A <see cref="AudioTween"/> instance representing the active tween.</returns>
    public static AudioTween TweenStopAudio(this AudioSource a, float length)
    {
        AudioTween tween = new(a, 0, length);
        Tweener.AddTween(tween);
        tween.RunOnFinish(a.Stop);
        return tween;
    }

    /// <summary>
    /// Tweens an <see cref="AudioSource"/>'s volume to 0, switches clips, plays, and then tweens its volume back to 1.
    /// </summary>
    /// <param name="a">The AudioSource to be acted upon.</param>
    /// <param name="clip">The clip to switch to.</param>
    /// <param name="lengthStop">The duration of the first Tween, before the AudioSource gets stopped.</param>
    /// <param name="lengthStart">The duration of the second Tween, before the AudioSource's volume hits 1.</param>
    /// <param name="vol">The volume to set. If left as <see langword="null"/>, it will use the <see cref="AudioSource"/> <paramref name="a"/>'s last volume.</param>
    /// <returns>The first AudioTween. The second can't really be grabbed, but you can stop it using <see cref="TweenTweenExtensions.Unique{T}(T)"/> if you need to.</returns>
    public static AudioTween TweenSwitchClips(this AudioSource a, AudioClip clip, float lengthStop, float lengthStart, float? vol = null)
    {
        AudioTween tween = new(a, 0, lengthStop);
        Tweener.AddTween(tween);
        tween.RunOnFinish(() => a.clip = clip);
        tween.RunOnFinish(() => a.time = 0f);
        tween.RunOnFinish(() => a.Play());

        tween.RunOnFinish(() => Tweener.AddTween(new AudioTween(a, vol ?? a.volume, lengthStart)));

        return tween;
    }

    /// <summary>
    /// Smoothly twens between a Transform <paramref name="t"/>'s current rotation and the target rotation in world space.
    /// </summary>
    /// <param name="t">The transform to be acted upon.</param>
    /// <param name="targetRot">The rotation for the Transform to have at the end of the Tween.</param>
    /// <param name="length">The duration of the Tween in seconds.</param>
    /// <returns>A <see cref="RotationTween"/> representing the current tween.</returns>
    public static RotationTween TweenRotation(this Transform t, Quaternion targetRot, float length)
    {
        RotationTween tween = new(t, targetRot, length, false);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// Smoothly twens between a Transform <paramref name="t"/>'s current rotation and the target rotation in local space.
    /// </summary>
    /// <param name="t">The transform to be acted upon.</param>
    /// <param name="targetRot">The local rotation for the Transform to have at the end of the Tween.</param>
    /// <param name="length">The duration of the Tween in seconds.</param>
    /// <returns>A <see cref="RotationTween"/> representing the current tween.</returns>
    public static RotationTween TweenLocalRotation(this Transform t, Quaternion targetRot, float length)
    {
        RotationTween tween = new(t, targetRot, length, true);
        Tweener.AddTween(tween);
        return tween;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="img"></param>
    /// <param name="target"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static ImageFillTween TweenFillAmount(this Image img, float target, float length)
    {
        ImageFillTween tween = new(img, target, length);
        Tweener.AddTween(tween);
        return tween;
    }
}
