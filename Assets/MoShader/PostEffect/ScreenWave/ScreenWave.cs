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

   [Range(1, 10)]
    public float speed = 1;

    [Range(0.001f, 0.01f)]
    public float amplitude = 0.005f;

    [Range(10, 40)]
    public float density = 10;

    public float spread = 1;
    public float width = 1;

    private float startTime = 0;
    private Vector4 startPos;
    void OnEnable()
    {
        startTime = Time.time;
        startPos = new Vector4(0.5f, 0.5f, 0, 0);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            material.SetFloat("_density", density);
            material.SetFloat("_amplitude", amplitude);
            material.SetFloat("_speed", speed);
            material.SetFloat("_radius", spread * (Time.time - startTime));
            material.SetFloat("_width", width);
            material.SetVector("_startPos", startPos);

            Graphics.Blit(src, dst, material);
        }
        else
        {
            Graphics.Blit(src, dst);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            //将mousePos转化为（0，1）区间  
            startPos = new Vector4(mousePos.x / Screen.width, mousePos.y / Screen.height, 0, 0);
            startTime = Time.time;
        }
    }  
}
