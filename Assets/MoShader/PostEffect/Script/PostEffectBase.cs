using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class PostEffectBase : MonoBehaviour
{
    protected Material CreateMaterial(Shader shader)
    {
        if (shader == null || !shader.isSupported)
        {
            Debug.LogError("Shader is error");
            return null;
        }

        Material material = new Material(shader);
        material.hideFlags = HideFlags.DontSave;
        return material;
    }
}
