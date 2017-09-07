using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeButton : MonoBehaviour 
{
    public GameObject cubeRoot1;
    public GameObject cubeRoot2;


    void Awake()
    {
        cubeRoot2.SetActive(false);
    }

	public void OnCube1Click()
    {
        cubeRoot1.SetActive(true);
        cubeRoot2.SetActive(false);
    }

    public void OnCube2Click()
    {
        cubeRoot1.SetActive(false);
        cubeRoot2.SetActive(true);
    }
}
