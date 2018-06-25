using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class StencilMSAAExample : MonoBehaviour
{
    public Material postStencilRed;

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
    }
}
