using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    private int pointNumber = 0;
    const int MAX_POINT_NUMBER = 20;
    private Vector4[] startPos;
    private float[] startTime;
    private float[] radius;
   
    void OnEnable()
    {
        pointNumber = 0;
        startPos = new Vector4[MAX_POINT_NUMBER];
        startTime = new float[MAX_POINT_NUMBER];
        radius = new float[MAX_POINT_NUMBER];
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            material.SetFloat("_density", density);
            material.SetFloat("_amplitude", amplitude);
            material.SetFloat("_speed", speed);
            material.SetFloat("_width", width);
            material.SetFloat("_pointNumber", pointNumber);
            material.SetVectorArray("_startPos", startPos);
            for (int i = 0; i < pointNumber; ++i)
            {
                radius[i] = spread * (Time.time - startTime[i]);
            }
            material.SetFloatArray("_radius", radius);

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
            if (pointNumber == MAX_POINT_NUMBER)
            {
                for (int i = MAX_POINT_NUMBER-1; i > 0; --i)
                {
                    startPos[i] = startPos[i - 1];
                    startTime[i] = startTime[i - 1];
                }
            }
            else
            {
                for (int i = pointNumber; i > 0; --i)
                {
                    startPos[i] = startPos[i - 1];
                    startTime[i] = startTime[i - 1];
                }
                pointNumber++;
            }
            Vector2 mousePos = Input.mousePosition;
            //将mousePos转化为（0，1）区间  
            mousePos = new Vector4(mousePos.x / Screen.width, mousePos.y / Screen.height, 0, 0);
            startPos[0] = mousePos;
            startTime[0] = Time.time;
        }
    }  
}
