using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.Tweening;

/// <summary>
/// A Tween that can act on any value. Use this if there's no extension method for a Tween you want to get.
/// </summary>
public sealed class GenericFloatTween : Tween<float>
{
    /// <summary>
    /// Create a new <see cref="GenericFloatTween"/>. This does not activate the tween, so you will need to call <see cref="Play"/>.
    /// </summary>
    /// <param name="getter">A method or lambda to retrieve the value. This will be used to get the starting value.</param>
    /// <param name="setter">A method or lambda to set the value. This will be used in Update.</param>
    /// <param name="endingValue">The ending value you want to Tween to.</param>
    /// <param name="length">How long (in seconds) you want the Tween to take.</param>
    /// <param name="cancelWith">What the Tween should cancel with. Defaults to <see cref="Instances.NeverCancel"/>.</param>
#pragma warning disable UNT0007 // Null coalescing on Unity objects
    public GenericFloatTween(Func<float> getter,
                             Action<float> setter,
                             float endingValue,
                             float length,
                             UnityEngine.Object cancelWith = null) 
        : base(endingValue,
               length,
               cancelWith ?? Instances.NeverCancel,
               () => getter(),
               (val) => setter(val))
    {

    }
#pragma warning restore UNT0007 // Null coalescing on Unity objects

    /// <summary>
    /// Start the created tween.
    /// </summary>
    /// <returns>The tween</returns>
    public GenericFloatTween Play()
    {
        Tweener.AddTween(this);
        return this;
    }

    /// <summary>
    /// Call the setter with the new value
    /// </summary>
    /// <param name="completion">How far along the tween is, from 0 to 1.</param>
    protected override void Update(float completion)
    {
        float completionInterpolated = interpolator(completion);
        float valueToSet = Mathf.Lerp(startValue, endValue, completionInterpolated);
        setter(valueToSet);
    }
}
