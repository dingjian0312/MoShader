using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSAO : PostEffectBase 
{
    public Shader ssaoShader;
    private Material ssaoMaterial;

    [Range(4, 128)]
    public int sampleCount = 12;

    [Range(0.001f, 1)]
    public float radius = 1;

    [Range(0.1f, 4)]
    public float intensity = 1;

    public bool enableBlur;

    [Range(0.1f, 4)]
    public float blurSize = 1;

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

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            material.SetFloat("_Radius", radius);
            material.SetInt("_SampleCount", sampleCount);
            material.SetFloat("_Intensity", intensity);
            RenderTexture aoBuffer = RenderTexture.GetTemporary(src.width, src.height, 0);
            RenderTexture aoBlurBuffer = RenderTexture.GetTemporary(src.width, src.height, 0);
            Graphics.Blit(src, aoBuffer, material, 0);

            if (enableBlur)
            {
                material.SetFloat("_BlurSize", blurSize);
                Graphics.Blit(aoBuffer, aoBlurBuffer, material, 1);
                Graphics.Blit(aoBlurBuffer, aoBuffer, material, 2);
            }

            material.SetTexture("_AOTex", aoBuffer);
            Graphics.Blit(src, dst, material, 3);

            RenderTexture.ReleaseTemporary(aoBuffer);
            RenderTexture.ReleaseTemporary(aoBlurBuffer);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }
}

