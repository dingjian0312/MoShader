using System;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Text))]
public class MoFPSCounter : MonoBehaviour
{
    [Range(1, 60)]
    public int frames = 10;
    private int curFrame = 0;
    private float lastTime = 0;
    const string display = "FPS:{0}";
    private Text text;


    private void Start()
    {
        text = GetComponent<Text>();
    }


    private void Update()
    {
        curFrame++;
        if (curFrame >= frames)
        {
            int fps = (int)(frames / (Time.time - lastTime));
            curFrame = 0;
            lastTime = Time.time;
            text.text = string.Format(display, fps);
        }
    }
}

