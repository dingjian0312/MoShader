using UnityEngine;
using System.Collections;

public class ScreenWave : PostEffectBase
{
    public Shader waveShader;
    private Material waveMaterial;

    public Material material
    {
        get
        {
            if (!waveMaterial)
            {
                waveMaterial = CreateMaterial(waveShader);
            }
            return waveMaterial;
        }
    }

   
    public float speed = 1;
    public float amplitude = 1;
    public float distanceFactor = 1;


    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            material.SetFloat("_distanceFactor", distanceFactor);
            material.SetFloat("_amplitude", amplitude);
            material.SetFloat("_speed", speed);
            Graphics.Blit(src, dst, material);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }
}
