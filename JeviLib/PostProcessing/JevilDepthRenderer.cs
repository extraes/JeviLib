using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;
using UnityEngine.Rendering;

namespace Jevil.PostProcessing;

[RegisterTypeInIl2Cpp]
internal class JevilDepthRenderer : MonoBehaviour
{
    public JevilDepthRenderer(IntPtr peter) : base(peter) { }


    public RenderTexture rTex;
    public Material mat;
    public MeshRenderer rend;
    MeshFilter mf;

    Action<ScriptableRenderContext, Camera> render;

    void Start()
    {
        render = EndCameraRendering;

        RenderPipelineManager.add_endCameraRendering(render);
        mf = rend.GetComponent<MeshFilter>();
    }


    void OnDestroy()
    {
        RenderPipelineManager.remove_endCameraRendering(render);
    }

    void EndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (!mf) return;

        CommandBuffer cmd = CommandBufferPool.Get("ChaosPostTest");
        cmd.Clear();
        //cmd.SetViewMatrix();

        //cmd.SetRenderTarget(cam.activeTexture);
        //cmd.SetRenderTarget(rTex);
        //cmd.ClearRenderTarget(true, true, Color.clear);
        
        cmd.SetViewMatrix(Camera.main.worldToCameraMatrix);
        cmd.SetProjectionMatrix(Camera.main.projectionMatrix);
        cmd.DrawMesh(mf.sharedMesh, rend.transform.localToWorldMatrix, mat, rend.subMeshStartIndex, 0);
        ctx.ExecuteCommandBuffer(cmd);
        Debug.Log("Drew jdr!");
        //Blitter.BlitTexture(cmd)
    }
}
