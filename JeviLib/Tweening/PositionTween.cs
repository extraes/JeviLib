using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// Positional tween returned by <see cref="TweenExtensions.TweenPosition(Transform, Vector3, float)"/>.
/// </summary>
public sealed class PositionTween : Tween<Vector3>
{
    /// <summary>
    /// Whether or not a PositionTween is acting in local space or world space.
    /// </summary>
    public bool IsLocal { get; internal set; }

    internal PositionTween(Transform transform, Vector3 target, float length, bool isLocal)
        : base(target,
               length,
               transform,
               isLocal ? () => transform.localPosition : () => transform.position, // getter
               isLocal ? (pos) => transform.localPosition = pos : (pos) => transform.position = pos) // setter
    {
        this.IsLocal = isLocal;
    }

    /// <inheritdoc/>
    protected override void Update(float completion)
    {
        float lerpComplete = interpolator(completion);
        Vector3 newVal = Vector3.Lerp(startValue, endValue, lerpComplete);
        setter(newVal);
    }
}
