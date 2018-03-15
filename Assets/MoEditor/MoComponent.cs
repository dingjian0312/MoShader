using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorType
{
    Black,
    White,
    Blue
};

public class MoComponent : MonoBehaviour
{
    public int intValue;
    public bool boolValue;
    public float floatValue;
    public Color colorValue;
    //初始化动画曲线
    public AnimationCurve curveValue = new AnimationCurve(new Keyframe(0, 0, 1, 1), new Keyframe(1, 1, 1, 1));
    public GameObject goRefrence;

    [Range(0, 20)]
    public float length = 10;

    public ColorType colorType;
	
	void Start () 
    {
		
	}
	
	
	void Update () 
    {
		
	}
}
