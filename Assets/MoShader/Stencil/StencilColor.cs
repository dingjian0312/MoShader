using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilColor : MonoBehaviour
{
    public Material stencilColor;


    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture buffer = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.SetRenderTarget(buffer.colorBuffer, src.depthBuffer);
        GL.Clear(false, true, Color.black);
        for (int i = 0; i < 8; ++i)
        {
            Graphics.Blit(src, stencilColor, i);
        }
        Graphics.Blit(buffer, dst);
        RenderTexture.ReleaseTemporary(buffer);
    }
}
