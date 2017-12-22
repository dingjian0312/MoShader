using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SSR : PostEffectBase
{
    public Shader ssrShader;
    private Material ssrMaterial;

    [Range(0, 8)]
    public int backDepthDownsample = 0;

    


    private Camera camera;
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
        camera = GetComponent<Camera>();
    }

    void OnPreCull()
    {
        //渲染背面深度
        int downsample = backDepthDownsample + 1;
		int width = camera.pixelWidth / downsample;
		int height = camera.pixelHeight / downsample;

        backDepthTexture = RenderTexture.GetTemporary(width, height, 16, RenderTextureFormat.ARGB32);
        if (!backCamera)
        {
            GameObject go = new GameObject("BackDepthCamera");
            go.hideFlags = HideFlags.HideAndDontSave;
            backCamera = go.AddComponent<Camera>();
        }
        backCamera.CopyFrom(camera);
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
            Graphics.Blit(src, dst, material);
        }
        else
        {
            Graphics.Blit(src, dst);
        }

        RenderTexture.ReleaseTemporary(backDepthTexture);
    }
}

