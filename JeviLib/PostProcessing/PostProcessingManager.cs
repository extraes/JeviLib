using Jevil.IMGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
using System.Reflection;
using MelonLoader;
using System.Diagnostics;
using static Jevil.PostProcessing.SharedPostProcessingMaterials;

namespace Jevil.PostProcessing;

/// <summary>
/// The class that is primarily responsible for executing Jevil postprocessing render passes.
/// </summary>
public static class PostProcessingManager
{
    static readonly List<Material> postProcessFx = new(4);
    static readonly List<GlobalTextureDescriptor> globalTextures = new(2);

    static readonly RenderTargetHandle tempTexHandle = new();
    static readonly int _MainTex = Shader.PropertyToID("_MainTex");
    static readonly int _DepthTex = Shader.PropertyToID("_DepthTex");

    internal static UniversalRenderPipelineAsset _urpa;
    internal static UniversalRenderPipelineAsset UrpAsset
    {
        get
        {
            if (_urpa == null)
                _urpa = UniversalRenderPipeline.asset;
            return _urpa;
        } 
    }

    /// <summary>
    /// Tells whether the current renderer config will allow a postprocessing shader to access the "opaque texture", AKA the rendering output immediately after all opaque (non-transparent) surfaces, including Alpha Cutout, are drawn.
    /// <para>This will allow for the most basic level of postprocessing, like color correction or "dumb" distortion. This will incur an extra memory cost.</para>
    /// <para><b>This defaults to <see langword="false"/> on Quest 2 and <see langword="true"/> on PC.</b> JeviLib will force it to <see langword="true"/> on startup.</para>
    /// </summary>
    public static bool OpaqueTextureEnabled
    {
        get => UrpAsset.supportsCameraOpaqueTexture;
        set => UrpAsset.supportsCameraOpaqueTexture = value;
    }

    internal static void Init()
    {
        tempTexHandle.Init("Jevil");

        // hook event before recieving any callbacks
        PostProcessingInternal.ExecuteRenderPass += DoRenderPass;

        //UniversalRenderPipelineAsset urpa = (UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset;
        //urpa.supportsCameraOpaqueTexture = true;
#if DEBUG
        JeviLib.Log("Current URP asset supports opaque tex? " + OpaqueTextureEnabled);
        if (!OpaqueTextureEnabled)
            JeviLib.Log("Let's change that.");
#endif

        OpaqueTextureEnabled = true;

#if DEBUG
        JeviLib.Log("OpaqueTexEnabled = " + OpaqueTextureEnabled);
#endif

        // todo: check memory/perf impact for this on quest

        // must init internal before materials
        PostProcessingInternal.Init();
        SharedPostProcessingMaterials.Init();

        //SharedPostProcessingMaterials.Depth.UseColor.SetOn(SharedPostProcessingMaterials.Depth.Material, false);
    }

    //private static bool RendererVisible(Renderer rend, Il2CppStructArray<Plane> planes)
    //{
    //    r.
    //}

    private static void DoRenderPass(ScriptableRenderContext context, RenderingData renderingData)
    {
        if (postProcessFx.Count == 0) return;

        RenderTextureDescriptor cameraDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        RenderTargetIdentifier cameraSource = renderingData.cameraData.renderer.cameraColorTarget;
        CommandBuffer cmd = CommandBufferPool.Get("JevilFX");

        cameraDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
        cameraDescriptor.useMipMap = false;
        cameraDescriptor.autoGenerateMips = false;
        cameraDescriptor.depthBufferBits = 0;
        
        cmd.Clear();
        cmd.GetTemporaryRT(tempTexHandle.id, cameraDescriptor);
        cmd.SetGlobalTexture(_DepthTex, renderingData.cameraData.renderer.cameraDepth);

        // use a for reverse loop to allow for removing invalid textures without iterating twice.
        for (int i = globalTextures.Count - 1; i >= 0; i--)
        {
            GlobalTextureDescriptor gTex = globalTextures[i];

            // remove invalid (collected?) globaltextures
            if (!gTex.IsValid)
            {
                globalTextures.RemoveAt(i);
                JeviLib.Warn($"Removing global texture {(string.IsNullOrEmpty(gTex.name) ? "<untitled>" : gTex.name)}. It is not valid at the start of the current frame. (IL2CPP collected tex or 0 propertyid)");
                continue;
            }

            RenderTargetIdentifier ident = new(gTex.tex);
            cmd.SetGlobalTexture(gTex.propertyId, ident);
        }
        
        // todo: add a way to pass in compute buffer. possibly change GlobalTextureDescriptor to GlobalShaderResourceDescriptor? and switch on a ResourceType enum
        //cmd.SetGlobalBuffer()

        //////////// TEMP POSTPROCESS LOOP ////////////
        // i wrote this section in unity, and it worked, so i moved it to the melonmod. it ended up working so i just used it for debugging
        // im keeping it here in case i ever need to use it for last resort debugging.

        //cmd.SetGlobalTexture("_MainTex", cameraSource);

        //cmd.SetRenderTarget(tempTexHandle.Identifier());

        //cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, postProcessFx[0], 0, 0);

        //////////// END TEMP POSTPROCESS LOOP ////////////

        PostProcessLoop(cmd, cameraSource, tempTexHandle.Identifier(), out RenderTargetIdentifier finalDst);

        if (finalDst != cameraSource)
            PostProcessingInternal.Blitlike(cmd, finalDst, cameraSource);

        cmd.ReleaseTemporaryRT(tempTexHandle.id);
        //todo: REMOVE
        //long preExec = sw.ElapsedTicks;
        context.ExecuteCommandBuffer(cmd);

        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }

    static void PostProcessLoop(CommandBuffer cmd, RenderTargetIdentifier src, RenderTargetIdentifier dst, out RenderTargetIdentifier finalDestination) // like the movie (get it)
    {

        foreach (Material mat in postProcessFx)
        {
#if DEBUG
            //JeviLib.Log($"Postprocessloop: processing from tex type {src.m_Type} to tex type {src.m_Type} with material {mat.name}");
#endif

            cmd.SetGlobalTexture(_MainTex, src);
            cmd.SetRenderTarget(dst);
            cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, mat, 0, 0);

            PostProcessingInternal.Blitlike(cmd, dst, src);
            (src, dst) = (dst, src);
        }

        finalDestination = src; // because the vars get flipped at the end
    }

    /// <summary>
    /// Adds a material with a post processing shader to the end of the stack.
    /// <para>It will recieve the color buffer output by the layers before it, but the depth buffer will be unmodified.</para>
    /// </summary>
    /// <param name="postProcessMat"></param>
    public static void AddToStack(Material postProcessMat)
    {
        if (postProcessMat == null) throw new NullReferenceException("Cannot add a null/collected material to the postprocessing stack.");

        postProcessFx.Add(postProcessMat);
    }

    /// <summary>
    /// Removes a material with a post processing shader from the top of the stack.
    /// </summary>
    /// <param name="postProcessMat">A material that was already added to the stack.</param>
    public static void RemoveFromStack(Material postProcessMat)
    {
        // make sure we're not gonna try calling GetInstanceID from a null/collected IL2CPP object
        RemoveCollectedMaterials();
        int idx = postProcessFx.FindIndex(m => m.GetInstanceID() == postProcessMat.GetInstanceID());

        if (idx != -1)
            postProcessFx.Remove(postProcessMat);
    }

    /// <summary>
    /// Sets a texture to be used when render passes are executed. Make sure your textures and PropertyID's aren't invalid.
    /// </summary>
    /// <param name="descriptor"></param>
    /// <exception cref="ArgumentException"></exception>
    public static void SetGlobalTexture(GlobalTextureDescriptor descriptor)
    {
        if (descriptor.propertyId == default) throw new ArgumentException("Descriptor must have a valid propertyID! Try Shader.ToPropertyID.", nameof(descriptor));
        if (descriptor.tex == null) throw new ArgumentException("Descriptor must have a valid Texture! Try checking .WasCollected when checking for null and/or using .Persist().", nameof(descriptor));

        foreach(GlobalTextureDescriptor gTex in globalTextures)
        {
            if (gTex.propertyId == descriptor.propertyId)
            {
                MelonBase melon = MelonUtils.GetMelonFromStackTrace();
                if (melon == null) 
                    continue;

                JeviLib.Warn($"Mod '{melon.MelonTypeName}' is trying to set a global texture that's already in use!");
                if (!string.IsNullOrEmpty(gTex.name)) JeviLib.Warn($"\tTexture name: {gTex.name}");
            }

        }

        globalTextures.Add(descriptor);
    }

    /// <summary>
    /// Removes a texture from the list of <see cref="GlobalTextureDescriptor"/>s that are passed into postprocessing shaders.
    /// </summary>
    /// <param name="descriptor"></param>
    /// <returns></returns>
    public static bool RemoveGlobalTexture(GlobalTextureDescriptor descriptor)
    {
        return globalTextures.Remove(descriptor);
    }

    /// <summary>
    /// Returns whether a shader takes in the the depth texture (<c>_DepthTex</c>) as an input.
    /// <br/>This is useful for determining whether a postprocessing shader will appear properly on Quest.
    /// </summary>
    /// <param name="shader">Any shader. Doesn't <i>have</i> to be a postprocessing shader, but this is most useful for them.</param>
    /// <returns>Whether or not there's a texture property named "_DepthTex"</returns>
    public static bool UsesDepthTexture(Shader shader)
    {
        int depthTexIdx = shader.FindPropertyIndex("_DepthTex");
        return depthTexIdx != -1 && shader.GetPropertyType(depthTexIdx) == ShaderPropertyType.Texture;
    }

    private static void RemoveCollectedMaterials()
    {
        for (int i = postProcessFx.Count - 1; i >= 0; i--)
        {
            if (postProcessFx[i] == null)
                postProcessFx.RemoveAt(i);
        }

        for (int i = 0; i < globalTextures.Count; i++)
        {
            if (!globalTextures[i].IsValid)
                globalTextures.RemoveAt(i);
        }
    }
}
