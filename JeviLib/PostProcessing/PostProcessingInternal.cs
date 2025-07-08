using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Reflection;
using UnityEngine.Rendering.Universal;
using Il2CppInterop.Runtime;
using MelonLoader.NativeUtils;
using Il2CppInterop.Common;

namespace Jevil.PostProcessing;

internal static class PostProcessingInternal
{
    internal static AssetBundle postProcessingBundle;
    static readonly List<object> neverCollect = new();
    internal static event Action<ScriptableRenderContext, RenderingData>? ExecuteRenderPass;

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void Patch_ScriptableRenderer_ExecuteRenderPass(
        IntPtr _this,                       //ScriptableRenderer _this, 
        ScriptableRenderContext context,    //ScriptableRenderContext context, 
        IntPtr renderPass,                  //ref ScriptableRenderPass renderPass,
        IntPtr renderingData,               //ref RenderingData renderingData,
        IntPtr nativeMethodInfo             //ref MethodInfo nativeMethodInfo,
        );

    private delegate void NativeSignature_ScriptableRenderer_ExecuteRenderPass(
        IntPtr _this,                       //ScriptableRenderer _this, 
        ScriptableRenderContext context,    //ScriptableRenderContext context, 
        IntPtr renderPass,                  //ref ScriptableRenderPass renderPass,
        IntPtr renderingData,               //ref RenderingData renderingData,
        IntPtr nativeMethodInfo             //ref MethodInfo nativeMethodInfo,
        );

    private static NativeHook<NativeSignature_ScriptableRenderer_ExecuteRenderPass> hook;
    private static NativeSignature_ScriptableRenderer_ExecuteRenderPass _original_ExecuteRenderPass;

    private static Material _blitMat;
    private static Material _depthMat;
    private static Dictionary<int, int> lastRenderedFrames = new(); // instanceid, framecount
    static readonly int _MainTex = Shader.PropertyToID("_MainTex");

    internal static void Init()
    {
        byte[] bytes = null!;
        string bundleName = Utilities.IsPlatformQuest() ? "PostProcessingQuest.bundle" : "PostProcessing.bundle";
        JeviLib.instance.Assembly.UseEmbeddedResource("Jevil.Resources." + bundleName, b => bytes = b);
        postProcessingBundle = AssetBundle.LoadFromMemory(bytes);
        postProcessingBundle.Persist();

        if (Utilities.IsPlatformQuest())
        {
            // if we dont do this, ssl (subsampled layout) will make shit look like THIS: https://cdn.discordapp.com/attachments/1167200124926181386/1167211068125351956/b2546f1a5cdd5912ce1f797c3c49cad5.mov?ex=654d4d04&is=653ad804&hm=6d4a0b5d999da54b43cced8cc1f4c924887b6557b9da0b9c4a82ea4d3c9a7dd9&
            // and that is ASSSSSS
            Unity.XR.Oculus.OculusSettings.s_Settings.SubsampledLayout = false;

            EnsureDepthMat();
        }

        EnsureBlitMat();
        PerformNativeHook();
    }

    private static unsafe void PerformNativeHook()
    {
        Patch_ScriptableRenderer_ExecuteRenderPass patch = NativeMethodPatch_ScriptableRenderer_ExecuteRenderPass;
        neverCollect.Add(patch); // prevent "A callback was made on a garbage collected delegate of type 'JeviLib!Jevil.PostProcessing.PostProcessingInternal+Patch_ScriptableRenderer_ExecuteRenderPass::Invoke'."

        // hardcoding, so hype. possible todo: look for the field name by iterating getfields?
        //string nativeName = Utilities.IsPlatformQuest()
        //                  ? throw new NotImplementedException("PostProcessing on quest needs method pointer name") /*"NativeMethodInfoPtr_ExecuteRenderPass_Private_Void_ScriptableRenderContext_ScriptableRenderPass_RenderingData_0"*/
        //                  : "NativeMethodInfoPtr_ExecuteRenderPass_Private_Void_ScriptableRenderContext_ScriptableRenderPass_byref_RenderingData_0";
        //var nativeMethodPtr = *(IntPtr*)(IntPtr)typeof(ScriptableRenderer).GetField(nativeName, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        var nativeMethodPtr = *(IntPtr*)(IntPtr)Il2CppInteropUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(typeof(ScriptableRenderer).GetMethod(nameof(ScriptableRenderer.ExecuteRenderPass))).GetValue(null)!;

        //Patching.Hook.OntoMethod(typeof(ScriptableRenderer).GetMethod(nameof(ScriptableRenderer.ExecuteRenderPass))!, () => JeviLib.Log("uwe bole"));

        //var managedPatchPtr = patch.Method.MethodHandle.GetFunctionPointer();
        IntPtr managedPatchPtr = Marshal.GetFunctionPointerForDelegate(patch);

        //hook = new((IntPtr)(&nativeMethodPtr), managedPatchPtr);
        //hook.Attach();
        //hook = new()
#pragma warning disable CS0618 // Type or member is obsolete
        typeof(MelonUtils).Assembly.GetType("MelonLoader.InternalUtils.BootstrapInterop")!.GetMethod("NativeHookAttach", Const.AllBindingFlags)!.Invoke(null, new object[] { (IntPtr)(&nativeMethodPtr), managedPatchPtr });
        //MelonUtils.NativeHookAttach();
#pragma warning restore CS0618 // Type or member is obsolete

        _original_ExecuteRenderPass = Marshal.GetDelegateForFunctionPointer<NativeSignature_ScriptableRenderer_ExecuteRenderPass>(nativeMethodPtr);
        //_original_ExecuteRenderPass = Marshal.GetDelegateForFunctionPointer<NativeSignature_ScriptableRenderer_ExecuteRenderPass>();
    }

    private static void NativeMethodPatch_ScriptableRenderer_ExecuteRenderPass(IntPtr _this,                       //ScriptableRenderer _this, 
        ScriptableRenderContext context,    //ScriptableRenderContext context, 
        IntPtr renderPass,                  //ref ScriptableRenderPass renderPass,
        IntPtr renderingData,               //ref RenderingData renderingData,
        IntPtr nativeMethodInfo             //ref MethodInfo nativeMethodInfo, (<- usually null cuz IL2CPP)
        )
    {
        // makes this, effectively, a postfix
        //hook.Trampoline(_this, context, renderPass, renderingData, nativeMethodInfo);
        _original_ExecuteRenderPass(_this, context, renderPass, renderingData, nativeMethodInfo);

        ScriptableRenderPass srp = new(renderPass);
        if (srp.renderPassEvent != RenderPassEvent.BeforeRenderingPostProcessing)
            return;

        // only box (alloc on unmanaged heap) when renderpass is actually postprocess
        IntPtr boxedValue = IL2CPP.il2cpp_value_box(Il2CppClassPointerStore<RenderingData>.NativeClassPtr, renderingData);
        RenderingData rendData = new(boxedValue);
        Camera renderingCamera = rendData.cameraData.camera;
        int camId = renderingCamera.GetInstanceID();

        int currFrame = Time.renderedFrameCount;

        // only call event once per camera
        lastRenderedFrames.TryGetValue(camId, out int lastRenderedFrame);
        if (currFrame <= lastRenderedFrame)
            return;

        lastRenderedFrames[camId] = currFrame;

        //postfix
        ExecuteRenderPass.InvokeSafeSync(context, rendData);
    }

    internal static void Blitlike(CommandBuffer cmd, RenderTargetIdentifier src, RenderTargetIdentifier dst)
    {
        // perform blocking call cuz shit falls apart if the material is missing :)
        EnsureBlitMat();

        cmd.SetGlobalTexture(_MainTex, src);
        cmd.SetRenderTarget(dst);
        cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, _blitMat, 0, 0);
    }

    internal static void Depthlike(CommandBuffer cmd, MeshRenderer rend)
    {
        EnsureDepthMat();

        MeshFilter? mf = Instances<MeshFilter>.Get(rend.transform);
        if (mf == null || mf.sharedMesh == null)
        {
#if DEBUG
            JeviLib.Warn($"MeshRenderer's {rend.transform.GetFullPath()} meshfilter {(mf == null ? "is null!" : "is meshless!")} thanks obama.");
#endif
            return;
        }
        //cmd.DrawRenderer(rend, _depthMat);
        cmd.DrawMesh(mf.sharedMesh, rend.localToWorldMatrix, _depthMat, rend.subMeshStartIndex, 0);
    }

    private static void EnsureBlitMat()
    {
        if (_blitMat == null)
            _blitMat = CreateMaterialFromShader("Assets/PostProcess/BlitWithoutBlit.shader");
    }

    private static void EnsureDepthMat()
    {
        // shadergraph cuz im a bitch
        if (_depthMat == null)
            _depthMat = CreateMaterialFromShader("Assets/PostProcess/CustomDepth.shadergraph");
    }

    internal static Material CreateMaterialFromShader(string shaderAssetPath)
    {
        Shader shader = postProcessingBundle.LoadAsset(shaderAssetPath).Cast<Shader>();
        Material ret = new(shader);

        ret.Persist();

        return ret;
    }
}