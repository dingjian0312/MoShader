using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneObject : MonoBehaviour 
{
    public GameObject cube;
    public int Count = 100;

	// Use this for initialization
	void Start () 
    {
		for (int i = 0; i < Count; ++i)
        {
            GameObject go = GameObject.Instantiate(cube);
            go.transform.SetParent(transform);
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
		
	}
}
