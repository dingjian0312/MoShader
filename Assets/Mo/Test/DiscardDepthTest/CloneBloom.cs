using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneBloom : MonoBehaviour {
    public int bloomCount = 20;
	
	void Start () 
    {
		for (int i = 0; i < bloomCount; ++i)
        {
            gameObject.AddComponent<Bloom>();
        }
	}
	
	
	void Update () {
		
	}
}
