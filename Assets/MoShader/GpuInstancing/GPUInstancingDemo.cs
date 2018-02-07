using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUInstancingDemo : MonoBehaviour
{
    public GameObject instancePrefab;
    public int instanceCount = 50;
    void Awake()
    {

        for (int i = 0; i < instanceCount; i++)
        {
            GameObject father = new GameObject();
            father.name = "father" + i;
            GameObject instancedTemplate = Instantiate(instancePrefab) as GameObject;
            instancedTemplate.transform.SetParent(father.transform);
            father.transform.position = Random.insideUnitSphere * 5;

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            MeshRenderer renderer;
            float r = Random.Range(0.0f, 1.0f);
            float g = Random.Range(0.0f, 1.0f);
            float b = Random.Range(0.0f, 1.0f);
            props.SetFloat("_InstanceID", (float)i);
            props.SetColor("_Color", new Color(r, g, b));

            renderer = instancedTemplate.GetComponent<MeshRenderer>();
            renderer.SetPropertyBlock(props);
        }
    }
}
