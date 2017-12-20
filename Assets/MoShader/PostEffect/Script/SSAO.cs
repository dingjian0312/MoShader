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

    private Camera camera;
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
        camera = GetComponent<Camera>();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            material.SetFloat("_Radius", radius);
            material.SetInt("_SampleCount", sampleCount);
            material.SetFloat("_Intensity", intensity);
            material.SetMatrix("_ViewMatrix", camera.worldToCameraMatrix);
            Graphics.Blit(src, dst, material);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }
}
