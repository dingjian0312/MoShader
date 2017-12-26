using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LightDepthRender : MonoBehaviour 
{
    public Light light;
    const int SHADOW_MAP_SIZE = 4096;
    public Shader depthShader;
    private RenderTexture shadowmap;

	void OnPreRender()
    {
        shadowmap = RenderTexture.GetTemporary(SHADOW_MAP_SIZE, SHADOW_MAP_SIZE, 16);
        Camera camera = light.GetComponent<Camera>();
        camera.targetTexture = shadowmap;
        camera.RenderWithShader(depthShader, "RenderType");
        Shader.SetGlobalTexture("_ShadowMap", shadowmap);
        Shader.SetGlobalMatrix("_LightMatrixV", camera.worldToCameraMatrix);
        Shader.SetGlobalMatrix("_LightMatrixP", camera.projectionMatrix);
        //Shader.SetGlobalFloat("_lightCameraFar", camera.farClipPlane);
    }
	
	
	void OnPostRender () 
    {
        RenderTexture.ReleaseTemporary(shadowmap);
	}
}
