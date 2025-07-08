using Jevil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Jevil.PostProcessing;

/// <summary>
/// You shouldn't use these and expect to have exclusive access to them. Your fault if you use these and another mod changes settings you don't like, really.
/// <para>More just useful for quickly testing out what Jevil Postprocessing has to offer.</para>
/// </summary>
public static class SharedPostProcessingMaterials
{
    internal static void Init()
    {
        // initialize all materials by calling their getters
        foreach (Type t in typeof(SharedPostProcessingMaterials).GetNestedTypes())
        {
            PropertyInfo pInf = t.GetProperty(nameof(ClipFar.Material));
#if DEBUG
            if (pInf is null)
                throw new HarmonyLib.MemberNotFoundException($"{nameof(ClipFar.Material)} property not found on type {t.FullName}");
#endif
            pInf.GetValue(null);
        }
    }

    /// <summary>
    /// Will 'clip' anything that's too far from the camera. This will cause the camera to not clear that area, resembling what happens when you go <i>way</i> far out of bounds in Source games.
    /// </summary>
    public static class ClipFar
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/ClipFar.shader";
        /// <summary>
        /// The material made from the ClipFar Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// Controls the value the depth is multiplied by.
        /// </summary>
        public static readonly ShaderProperty<float> DepthMult = new("_DepthMult", Material);
        /// <summary>
        /// Controls the value below which the screen gets clipped. Depth values are 1 closest to the camera and 0 furthest away.
        /// </summary>
        public static readonly ShaderProperty<float> Cutoff = new("_Cutoff", Material);
    }

    /// <summary>
    /// Multiplies the color values by themselves.
    /// </summary>
    public static class ColorSqr
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/ColorSqr.shader";
        /// <summary>
        /// The material made from the ColorSqr Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// A value that is added to the sampled color value before squaring.
        /// </summary>
        public static readonly ShaderProperty<float> Brightness = new("_BrightnessMod", Material);
    }

    /// <summary>
    /// Displays an alternative texture in a square in the corner of the screen. Use a <see cref="GlobalTextureDescriptor"/> with the name "_AltTex" to define that alternative view.
    /// </summary>
    public static class CornerScreen
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/CornerScreen.shader";
        /// <summary>
        /// The material made from the CornerScreen Shader.
        /// </summary>
        public static Material Material
        {
            get
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// The size of the corner, from 0 to 1, with 1 being the entire screen.
        /// </summary>
        public static readonly ShaderProperty<float> CornerSize = new("_CornerSize", Material);
    }

    /// <summary>
    /// Shows an alternative view on the right hand side of the screen. Use a <see cref="GlobalTextureDescriptor"/> with the name "_AltTex" to define that alternative view.
    /// </summary>
    public static class SideScreen
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/SideScreen.shader";
        /// <summary>
        /// The material made from the SideScreen Shader.
        /// </summary>
        public static Material Material
        {
            get
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// The size, from 0 to 1, of the alternative screen.
        /// </summary>
        public static readonly ShaderProperty<float> SideSize = new("_SideSize", Material);
    }

    /// <summary>
    /// Displays the depth values. Optionally can be used to display a darkened version of the main texture, looking similar to Minecraft's Blindess potion effect.
    /// </summary>
    public static class Depth
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/Depth.shader";
        /// <summary>
        /// The material made from the Depth Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// Multiplies the (processed) depth values by the color values.
        /// </summary>
        public static readonly ShaderProperty<bool> UseColor = new("_UseMainTex", Material);
        /// <summary>
        /// Multiplies the depth values by this number. This will be done after the DepthPow calculation.
        /// </summary>
        public static readonly ShaderProperty<float> DepthMult = new("_DepthMult", Material);
        /// <summary>
        /// Raises the depth values to this number.
        /// </summary>
        public static readonly ShaderProperty<float> DepthPow = new("_DepthPow", Material);
    }

    /// <summary>
    /// A really trippy effect. Mirrors the bottom of the screen onto the top if the depth value is beneath a certain threshold.
    /// </summary>
    public static class DepthFlip
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/DepthFlip.shader";
        /// <summary>
        /// The material made from the DepthFlip Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// The depth value below which the world will appear flipped.
        /// </summary>
        public static readonly ShaderProperty<float> FlipAt = new("_TargetDepth", Material);
    }

    /// <summary>
    /// Makes things "warble" according to their distance to the camera. Can be inverted.
    /// </summary>
    public static class DepthRefract
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/DepthRefract.shader";
        /// <summary>
        /// The material made from the DepthRefract Shader.
        /// </summary>
        public static Material Material
        {
            get
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// The horizontal scale of the warble effect.
        /// </summary>
        public static readonly ShaderProperty<float> ScaleX = new("_RefractionScaleX", Material);
        /// <summary>
        /// The vertical scale of the warble effect.
        /// </summary>
        public static readonly ShaderProperty<float> ScaleY = new("_RefractionScaleY", Material);
        /// <summary>
        /// The strength of the warble. That is, the maximal value the warble can move the screenspace position from what it should be.
        /// </summary>
        public static readonly ShaderProperty<float> Strength = new("_Strength", Material);
        /// <summary>
        /// The value to raise the depth values to.
        /// </summary>
        public static readonly ShaderProperty<float> DepthPow = new("_DepthPow", Material);
        /// <summary>
        /// Whether to reverse the depth or not. By default, Unity maxes out at 1 when things are close to the camera. Using this option will invert that.
        /// </summary>
        public static readonly ShaderProperty<bool> ReverseDepth = new("_OneMinusDepth", Material);
    }

    /// <summary>
    /// Makes things get more saturated as they get closer, beyond their original saturations.
    /// </summary>
    public static class DepthSaturation
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/DepthSaturation.shader";
        /// <summary>
        /// The material made from the DepthSaturation Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// The value to multiply the depth by.
        /// </summary>
        public static readonly ShaderProperty<float> SaturationMult = new("_DepthMult", Material);
    }

    /// <summary>
    /// Pretty trippy at closer distances. Makes it almost look like light is bending around surfaces. Try it for yourself, it's pretty cool!
    /// </summary>
    public static class DepthShift
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/DepthWarble.shader";
        /// <summary>
        /// The material made from the DepthShift Shader ('DepthWarble').
        /// </summary>
        public static Material Material
        {
            get
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);
        
        /// <summary>
        /// The number that the depth values will be raised to.
        /// </summary>
        public static readonly ShaderProperty<float> DepthPow = new("_DepthPow", Material);
        /// <summary>
        /// The base that the depth values will be logarithm'd by.
        /// </summary>
        public static readonly ShaderProperty<float> DepthLog = new("_DepthLog", Material);
        /// <summary>
        /// Reverses depth values, transforming them from 1 being close to 1 being far, making things far away bend light more than closer objects.
        /// </summary>
        public static readonly ShaderProperty<bool> ReverseDepth = new("_OneMinusDepth", Material);
    }

    /// <summary>
    /// Does a fisheye-like effect. At higher strengths, it makes the view look round, and the corners will loop and mirror the screen for a trippier effect.
    /// </summary>
    public static class Fisheye
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/Fisheye.shader";
        /// <summary>
        /// The material made from the Fisheye Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// Controls how strong the effect is.
        /// </summary>
        public static readonly ShaderProperty<float> Strength = new("_SpherizeStrength", Material);
        /// <summary>
        /// A (0,0) to (1,1) value representing where on the screen the center is.
        /// </summary>
        public static readonly ShaderProperty<Vector2> Center = new("_SpherizeCenter", Material);
    }

    /// <summary>
    /// Flips the screen along the vertical axis, so that left will be shown as right, and right shown as left. A funny trick to be sure.
    /// </summary>
    public static class HorizontalMirror
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/HorizontalMirror.shader";
        /// <summary>
        /// The material made from the HorizontalMirror Shader.
        /// </summary>
        public static Material Material
        {
            get
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);
    }


    /// <summary>
    /// Splits the screen up into a bunch of vertical strips, alternating between showing the normal view and showing a horizontally mirrored view.
    /// </summary>
    public static class InterleavedMirror
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/InterleavedMirror.shader";
        /// <summary>
        /// The material made from the InterleavedMirror Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);


        /// <summary>
        /// The scale of the interleaving.
        /// </summary>
        public static readonly ShaderProperty<float> Scale = new("_InterleaveScale", Material);
    }

    /// <summary>
    /// Does a pixelation effect on the screen. Not a 'blur' pixelation, it's closer to the SNES's 'mosaic' effect. See: <see href="https://www.sneslab.net/wiki/Mosaic"/>
    /// </summary>
    public static class Pixelate
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/Pixelate.shader";
        /// <summary>
        /// The material made from the Pixelate Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// Controls the number, and therefore the scale, of pixels on each axis.
        /// </summary>
        public static readonly ShaderProperty<float> PixelsPerAxis = new("_Pixels", Material);
        /// <summary>
        /// Tints the screen by this color. Defaults to white, AKA no tint.
        /// </summary>
        public static readonly ShaderProperty<Color> Color = new("_BaseColor", Material);
    }

    /// <summary>
    /// Just warbles the screen indiscriminantly. Adds a sin/cosine value to the sampled position.
    /// </summary>
    public static class Refract
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/Refract.shader";
        /// <summary>
        /// The material made from the Refract Shader.
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// The strength of the warbling on the horizontal axis.
        /// </summary>
        public static readonly ShaderProperty<float> ScaleX = new("_RefractionScaleX", Material);
        /// <summary>
        /// The strength of the warbling on the vertical axis.
        /// </summary>
        public static readonly ShaderProperty<float> ScaleY = new("_RefractionScaleY", Material);
    }

    /// <summary>
    /// Produces a rainbow rolling tint that moves with time and is positioned via depth values.
    /// </summary>
    public static class Tint
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/Tint.shader";
        /// <summary>
        /// The m
        /// </summary>
        public static Material Material 
        {
            get 
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// Controls the speed of the rolling effect.
        /// </summary>
        public static readonly ShaderProperty<float> TimeScale = new("_TimeMult", Material);
        /// <summary>
        /// Controls how strong the depth value is in the calculations. Will likely allow for more 'bands' of rainbow color.
        /// </summary>
        public static readonly ShaderProperty<float> DepthMult = new("_DepthMult", Material);
        /// <summary>
        /// Controls how much the base color's sautration will be multiplied by.
        /// </summary>
        public static readonly ShaderProperty<float> SaturationMult = new("_SaturationMult", Material);
        /// <summary>
        /// The minimum value the saturation must be after being multiplied by SaturationMult
        /// </summary>
        public static readonly ShaderProperty<float> MinSaturation = new("_MinSaturation_PostMult", Material);
    }

    /// <summary>
    /// Keys out a color with a certain tolerance. This is useful for green screens, but can be used for any color.
    /// </summary>
    public static class GreenScreen
    {
        private static Material? material;
        const string ASSET_PATH = "Assets/PostProcess/GreenScreen.shader";
        /// <summary>
        /// The m
        /// </summary>
        public static Material Material
        {
            get
            {
                if (material == null)
                    material = PostProcessingInternal.CreateMaterialFromShader(ASSET_PATH);
                return material;
            }
        }

        /// <summary>
        /// Adds the postprocessing effect to the Jevil Postprocessing stack.
        /// </summary>
        public static void Enable() => PostProcessingManager.AddToStack(Material);

        /// <summary>
        /// Removes the postprocessing effect from the Jevil Postprocessing stack.
        /// </summary>
        public static void Disable() => PostProcessingManager.RemoveFromStack(Material);

        /// <summary>
        /// Controls the maximum summed difference between the components of the keyed color and the sampled color from _AltTex.
        /// </summary>
        public static readonly ShaderProperty<float> Tolerance = new("_MaxDifference", Material);
        /// <summary>
        /// The color to key out.
        /// </summary>
        public static readonly ShaderProperty<Color> KeyColor = new("_ClipColor", Material);
    }
}
