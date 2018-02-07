using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSR : PostEffectBase
{
    public Shader ssrShader;
    private Material ssrMaterial;

    [Range(0, 8)]
    public int backDepthDownsample = 0;

    [Range(0, 5)]
    public float thickness = 1;

    [Range(0, 1)]
    public float stepLength = 0.2f;


    private Camera ssrCamera;
    private Camera backCamera;
    private RenderTexture backDepthTexture;

    public Material material
    {
        get
        {
            if (!ssrMaterial)
            {
                ssrMaterial = CreateMaterial(ssrShader);
            }
            return ssrMaterial;
        }
    }

    void OnEnable()
    {
        ssrCamera = GetComponent<Camera>();
        //ssrCamera.depthTextureMode = DepthTextureMode.Depth;
    }

    void OnPreCull()
    {
        //渲染背面深度
        int downsample = backDepthDownsample + 1;
		int width = ssrCamera.pixelWidth / downsample;
		int height = ssrCamera.pixelHeight / downsample;

        backDepthTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
        if (!backCamera)
        {
            GameObject go = new GameObject("BackDepthCamera");
            go.hideFlags = HideFlags.HideAndDontSave;
            backCamera = go.AddComponent<Camera>();
        }
        backCamera.CopyFrom(ssrCamera);
        backCamera.renderingPath = RenderingPath.Forward;
        backCamera.enabled = false;
        backCamera.backgroundColor = Color.white;
        backCamera.clearFlags = CameraClearFlags.SolidColor;
        backCamera.targetTexture = backDepthTexture;
        backCamera.RenderWithShader(Shader.Find("Mo/BackDepth"), "RenderType");
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (material != null)
        {
            material.SetTexture("_BackFaceDepthTex", backDepthTexture);
            material.SetFloat("_thickness", thickness);
            material.SetFloat("_stepLength", stepLength);
            
            Graphics.Blit(src, dst, material);
        }
        else
        {
            Graphics.Blit(src, dst);
        }

        RenderTexture.ReleaseTemporary(backDepthTexture);
    }
}

