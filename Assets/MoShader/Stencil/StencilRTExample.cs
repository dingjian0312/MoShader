using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StencilRTExample : MonoBehaviour
{

    public Material postStencilRed;
    public Material rtCopy;

    private Camera cameraComponent;
    private RenderTexture cameraRenderTexture;

    void Awake()
    {
        cameraComponent = GetComponent<Camera>();
        cameraRenderTexture = new RenderTexture(Screen.width, Screen.height, 24);
        cameraRenderTexture.antiAliasing = 1; //2、4、8 则Stencil无效
    }

    void OnPreRender()
    {
        cameraComponent.targetTexture = cameraRenderTexture;
    }

    void OnPostRender()
    {
        //cameraComponent.targetTexture = null;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        RenderTexture buffer = RenderTexture.GetTemporary(src.width, src.height);
        Graphics.SetRenderTarget(buffer.colorBuffer, src.depthBuffer);
        Graphics.Blit(src, rtCopy);
        Graphics.Blit(src, postStencilRed);

        Graphics.Blit(buffer, null as RenderTexture);
        RenderTexture.ReleaseTemporary(buffer);
    }
}
