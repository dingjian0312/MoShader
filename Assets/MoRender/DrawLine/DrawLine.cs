using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class DrawLine : MonoBehaviour
{
	void Start ()
    {
		
	}
	
	
	void Update ()
    {
        MoDraw.Instance.ClearScreen(Color.black);
        MoDraw.Instance.DrawLine(new Vector2Int(200, 100), new Vector2Int(1000, 500), Color.red);
        MoDraw.Instance.Flush();
    }
}
