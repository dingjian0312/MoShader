using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSAO : PostEffectBase 
{
    public Shader ssaoShader;
    private Material ssaoMaterial;

    public Material material
    {
        get
        {
            if (!ssaoMaterial)
            {
                ssaoMaterial = CreateMaterial(ssaoShader);
            }
            return ssaoMaterial;
        }
    }

    void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            Graphics.Blit(src, dst, material);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }
}
