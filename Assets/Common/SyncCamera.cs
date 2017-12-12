using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SyncCamera : MonoBehaviour 
{
    public bool sync = true;

    private Vector3 lastPosition;
    private Quaternion lastRotation;


    void Start()
    {
        if (Application.isPlaying)
        {
            sync = false;
        }
    }


    void OnRenderObject()
    {
        if (!Application.isPlaying)
        {
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }

    void OnPreCull()
    {
        if (sync)
        {
            UnityEditor.SceneView sceneView = UnityEditor.SceneView.lastActiveSceneView;
            if (sceneView)
            {
                Transform t = sceneView.camera.transform;
                lastPosition = transform.position;
                lastRotation = transform.rotation;
                transform.position = t.position;
                transform.rotation = t.rotation;
            }
        }
    }

    void OnPostRender()
    {
        if (sync)
        {
            transform.position = lastPosition;
            transform.rotation = lastRotation;
        }
    }
	
}
