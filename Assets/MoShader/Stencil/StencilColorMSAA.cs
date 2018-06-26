using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StencilColorMSAA : MonoBehaviour
{
    public Material stencilColorQuad;
    public Mesh Quad;

    private CommandBuffer commandBuffer;
    private Camera m_Camera;


    public Camera camera
    {
        get
        {
            if (m_Camera == null)
            {
                m_Camera = GetComponent<Camera>();
            }

            return m_Camera;
        }
    }


    private void OnEnable()
    {
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "commandBuffer";

            int cachedScreenImageID = Shader.PropertyToID("_Temp");
            commandBuffer.GetTemporaryRT(cachedScreenImageID, -1, -1, 0);

            //通过CommandBuffer的SetRenderTarget方式
            commandBuffer.SetRenderTarget(cachedScreenImageID, BuiltinRenderTextureType.CameraTarget);
            commandBuffer.ClearRenderTarget(false, true, Color.black);

            for (int i = 0; i < 8; ++i)
            {
                commandBuffer.DrawMesh(Quad, Matrix4x4.identity, stencilColorQuad, 0, i);
            }
            commandBuffer.SetGlobalTexture("_CachedScreenImage", cachedScreenImageID);
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }
    }



    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture cachedScreenRT = Shader.GetGlobalTexture("_CachedScreenImage") as RenderTexture;
        Graphics.Blit(cachedScreenRT, dst);
    }
   
}
