using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StencilMSAAExample : MonoBehaviour
{
    public Material postStencilRed;
    public Material postStencilRedQuad;
    public Material rtCopy;
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

    //方案1
    /*private void OnEnable()
    {
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "commandBuffer";
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, postStencilRed);
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }
    }*/

    //方案2
    /*private void OnEnable()
    {
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "commandBuffer";

            int cachedScreenImageID = Shader.PropertyToID("_Temp");
            commandBuffer.GetTemporaryRT(cachedScreenImageID, -1, -1, 0);
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cachedScreenImageID);
            commandBuffer.Blit(cachedScreenImageID, BuiltinRenderTextureType.CameraTarget, postStencilRed);
            commandBuffer.SetGlobalTexture("_CachedScreenImage", cachedScreenImageID);
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }
    }*/


    //方案3
    /*private void OnEnable()
    {
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "commandBuffer";

            int cachedScreenImageID = Shader.PropertyToID("_Temp");
            commandBuffer.GetTemporaryRT(cachedScreenImageID, -1, -1, 0);

            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget, postStencilRed);
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cachedScreenImageID);
            commandBuffer.SetGlobalTexture("_CachedScreenImage", cachedScreenImageID);
            camera.AddCommandBuffer(CameraEvent.AfterForwardAlpha, commandBuffer);
        }
    }



    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture cachedScreenRT = Shader.GetGlobalTexture("_CachedScreenImage") as RenderTexture;
        Graphics.Blit(cachedScreenRT, dst);
    }*/


    //方案4
    private void OnEnable()
    {
        if (commandBuffer == null)
        {
            commandBuffer = new CommandBuffer();
            commandBuffer.name = "commandBuffer";

            int cachedScreenImageID = Shader.PropertyToID("_Temp");
            commandBuffer.GetTemporaryRT(cachedScreenImageID, -1, -1, 0);

            //通过CommandBuffer的SetRenderTarget方式
            commandBuffer.Blit(BuiltinRenderTextureType.CameraTarget, cachedScreenImageID);
            commandBuffer.SetRenderTarget(cachedScreenImageID, BuiltinRenderTextureType.CameraTarget);
            commandBuffer.DrawMesh(Quad, Matrix4x4.identity, postStencilRedQuad);

            commandBuffer.SetGlobalTexture("_CachedScreenImage", cachedScreenImageID);
            camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, commandBuffer);
        }
    }



    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture cachedScreenRT = Shader.GetGlobalTexture("_CachedScreenImage") as RenderTexture;
        Graphics.Blit(cachedScreenRT, dst);
    }
}
