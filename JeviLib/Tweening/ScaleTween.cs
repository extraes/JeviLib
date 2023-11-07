using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// Tween that acts on a Transform's local scale. An instance of this is returned by <see cref="TweenExtensions.TweenLocalScale(Transform, Vector3, float)"/>.
/// </summary>
public sealed class ScaleTween : Tween<Vector3>
{

    internal ScaleTween(Transform transform, Vector3 target, float length) 
        : base(target,
               length,
               transform,
               () => transform.localScale, // getter
               (scale) => transform.localScale = scale) // setter
    {

    }

    /// <inheritdoc/>
    protected override void Update(float completion)
    {
        float lerpComplete = interpolator(completion);
        Vector3 newVal = Vector3.Lerp(startValue, endValue, lerpComplete);
        setter(newVal); 
    }
}
