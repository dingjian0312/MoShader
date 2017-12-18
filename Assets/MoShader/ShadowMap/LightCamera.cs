using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class LightCamera : MonoBehaviour 
{
    public Shader depthShader;
    public Camera renderCamera;
    public Camera lightCamera;
    public Transform shadowLight;
    public float zEps = 1f; //光源相机z方向的偏移
    public float ShadowDistance = 20f;


    public RenderTexture ShadowMap = null;
    private int ShadowMapSize = 2048;
    

    void OnEnable()
    {
        if (ShadowMap.width != ShadowMapSize || ShadowMap.height != ShadowMapSize)
        {
            DestroyImmediate(ShadowMap);
        }

        if (!ShadowMap)
        {
            ShadowMap = new RenderTexture(ShadowMapSize, ShadowMapSize, 16);
            lightCamera.targetTexture = ShadowMap;
        }
    }

    void OnWillRenderObject()
    {
        Camera curCamera = renderCamera;
        float near = curCamera.nearClipPlane;
        float far = ShadowDistance;
        float halfFov = 0.5f * curCamera.fieldOfView * Mathf.Deg2Rad;
        float aspect = curCamera.aspect;

        float halfNearHeight = near * Mathf.Tan(halfFov);
        float halfNearWidth = halfNearHeight * aspect;
        float halfFarHeight = far * Mathf.Tan(halfFov);
        float halfFarWidth = halfFarHeight * aspect;

        Vector3[] frustumPoint = new Vector3[8];
        frustumPoint[0] = new Vector3(-halfNearWidth, -halfNearHeight, near);
        frustumPoint[1] = new Vector3(-halfNearWidth, halfNearHeight, near);
        frustumPoint[2] = new Vector3(halfNearWidth, -halfNearHeight, near);
        frustumPoint[3] = new Vector3(halfNearWidth, halfNearHeight, near);
        frustumPoint[4] = new Vector3(-halfFarWidth, -halfFarHeight, far);
        frustumPoint[5] = new Vector3(-halfFarWidth, halfFarHeight, far);
        frustumPoint[6] = new Vector3(halfFarWidth, -halfFarHeight, far);
        frustumPoint[7] = new Vector3(halfFarWidth, halfFarHeight, far);

        Vector3 frustumCenter = Vector3.zero;
        for (int i = 0; i < 8; ++i)
        {
            frustumPoint[i] = curCamera.transform.TransformPoint(frustumPoint[i]);
            frustumCenter += frustumPoint[i];
        }
        frustumCenter /= 8;

        Vector3 lightForward = shadowLight.forward;
        Vector3 lightUp = shadowLight.up;
        Vector3 lightRight = Vector3.Normalize(Vector3.Cross(lightUp, lightForward));
        lightUp = Vector3.Normalize(Vector3.Cross(lightForward, lightRight));

        /*
         * 求光源相机矩阵
         * http://blog.csdn.net/popy007/article/details/5120158
         *  思路：先逆平移小人和相机，然后再逆旋转小人和相机，此时相机空间和世界空间重合，小人的世界坐标即为在相机空间中的坐标，所以矩阵是 逆旋转*逆平移
         */
        float tx = Vector3.Dot(-frustumCenter, lightRight);
        float ty = Vector3.Dot(-frustumCenter, lightUp);
        float tz = Vector3.Dot(-frustumCenter, lightForward);
        Matrix4x4 lightMatrix = new Matrix4x4();
        lightMatrix.m00 = lightRight.x;     lightMatrix.m01 = lightRight.y;     lightMatrix.m02 = lightRight.z;     lightMatrix.m03 = tx;
        lightMatrix.m10 = lightUp.x;        lightMatrix.m11 = lightUp.y;        lightMatrix.m12 = lightUp.z;        lightMatrix.m13 = ty;
        lightMatrix.m20 = lightForward.x;   lightMatrix.m21 = lightForward.y;   lightMatrix.m22 = lightForward.z;   lightMatrix.m23 = tz;
        lightMatrix.m30 = 0;                lightMatrix.m31 = 0;                lightMatrix.m32 = 0;                lightMatrix.m33 = 1;

        //光源相机原点frustumCenter， 旋转light.transform.rotation
        //lightMatrix = Matrix4x4.TRS(frustumCenter, shadowLight.transform.rotation, Vector3.one);  //光源相机空间到世界空间的转换矩阵
        //lightMatrix = lightMatrix.inverse;

        lightCamera.transform.position = frustumCenter;
        lightCamera.transform.rotation = shadowLight.transform.rotation;
        lightCamera.transform.localScale = Vector3.one;

        float lightCameraSize = 0;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;
        //将8个顶点转换到光源相机空间
        for (int i = 0; i < 8; ++i)
        {
            frustumPoint[i] = lightMatrix.MultiplyPoint(frustumPoint[i]);
            //frustumPoint[i] = lightCamera.transform.worldToLocalMatrix.MultiplyPoint(frustumPoint[i]);
            //frustumPoint[i] = lightCamera.transform.InverseTransformPoint(frustumPoint[i]);

            lightCameraSize = Mathf.Max(lightCameraSize, Mathf.Abs(frustumPoint[i].x));
            lightCameraSize = Mathf.Max(lightCameraSize, Mathf.Abs(frustumPoint[i].y));
            minZ = Mathf.Min(minZ, frustumPoint[i].z);
            maxZ = Mathf.Max(maxZ, frustumPoint[i].z);
        }

        lightCamera.orthographicSize = lightCameraSize;
       
        lightCamera.transform.position = frustumCenter + lightForward * (minZ - zEps);
        lightCamera.nearClipPlane = 0.01f;
        lightCamera.farClipPlane = maxZ - minZ + zEps;

        Graphics.SetRenderTarget(ShadowMap);
        GL.Clear(false, true, Color.white);

        lightCamera.RenderWithShader(depthShader, "RenderType");

        Shader.SetGlobalTexture("_ShadowMap", ShadowMap);
        Shader.SetGlobalFloat("_lightCameraFar", lightCamera.farClipPlane);
        Shader.SetGlobalFloat("_ShadowMapSize", ShadowMapSize);
        Shader.SetGlobalFloat("_lightCameraSize", lightCamera.orthographicSize);
        Shader.SetGlobalMatrix("_LightMatrixV", lightCamera.worldToCameraMatrix);
        Shader.SetGlobalMatrix("_LightMatrixP", lightCamera.projectionMatrix);
    }
}
