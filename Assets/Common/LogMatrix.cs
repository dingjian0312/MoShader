using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LogMatrix : MonoBehaviour 
{

    void Update()
    {
        Debug.Log(transform.localToWorldMatrix);
        Matrix4x4 trs = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
        Debug.Log(trs);
    }
}
