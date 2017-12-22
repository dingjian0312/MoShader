using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateGI : MonoBehaviour 
{
    [Range(1, 10)]
    public int Frames = 1; 
    private Renderer r;
	private int curFrame = 0;

    void OnEnable()
    {
        r = GetComponent<Renderer>();
    }

	void Update () 
    {
        curFrame++;
        if (curFrame >= Frames)
        {
            RendererExtensions.UpdateGIMaterials(r);
            curFrame = 0;
        }
	}
}
