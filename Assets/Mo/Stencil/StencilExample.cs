using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilExample : MonoBehaviour
{

    public Material postStencilRed;
    public Material rtCopy;



    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture buffer = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.SetRenderTarget(buffer.colorBuffer, src.depthBuffer);
        Graphics.Blit(src, rtCopy);
        Graphics.Blit(src, postStencilRed);

        Graphics.Blit(buffer, dst);
        RenderTexture.ReleaseTemporary(buffer);
    }
}
