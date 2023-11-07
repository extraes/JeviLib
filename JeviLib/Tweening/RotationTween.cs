using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// Positional tween returned by <see cref="TweenExtensions.TweenRotation(Transform, Quaternion, float)"/>.
/// </summary>
public sealed class RotationTween : Tween<Quaternion>
{
    /// <summary>
    /// Whether or not a PositionTween is acting in local space or world space.
    /// </summary>
    public bool IsLocal { get; internal set; }

    internal RotationTween(Transform transform, Quaternion target, float length, bool isLocal)
        : base(target,
               length,
               transform,
               isLocal ? () => transform.localRotation : () => transform.rotation, // getter
               isLocal ? (rot) => transform.localRotation = rot : (rot) => transform.rotation = rot) // setter
    {
        this.IsLocal = isLocal;
    }

    /// <inheritdoc/>
    protected override void Update(float completion)
    {
        float lerpComplete = interpolator(completion);
        // use quaterntion.lerp because aint no fuckin way im gonna dare to touch the bullfuckery that is quaternions
        Quaternion current = Quaternion.Lerp(startValue, endValue, lerpComplete);
        setter(current);
    }
}
