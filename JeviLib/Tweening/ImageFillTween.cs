using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Jevil.Tweening;

/// <summary>
/// Tween that acts on an <see cref="Image"/>. An instance of this is returned by <see cref="TweenExtensions.TweenFillAmount(Image, float, float)"/>
/// </summary>
public sealed class ImageFillTween : Tween<float>
{
    (float start, float end) interp;

    internal ImageFillTween(Image player, float vol, float length)
        : base(vol,
               length,
               player,
               () => player.fillAmount, // getter
               (v) => player.fillAmount = v) // setter
    {
        interp = (getter(), vol);
    }

    /// <inheritdoc/>
    protected override void Update(float completion)
    {
        float newVal = interp.Interpolate(completion);
        setter(newVal);
    }
}
