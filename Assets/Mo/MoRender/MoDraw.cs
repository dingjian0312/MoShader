using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
[ExecuteInEditMode]
public class MoDraw : MonoBehaviour 
{
    const int ScreenWidth = 500;
    const int ScreenHeight = 300;
    private RawImage displayImage;
    private Texture2D displayTexture;
    

	void OnEnable() 
    {
        displayImage = GetComponent<RawImage>();

        if (displayTexture == null)
        {
            displayTexture = new Texture2D(ScreenWidth, ScreenHeight, TextureFormat.ARGB32, false);
            displayTexture.wrapMode = TextureWrapMode.Clamp;
            displayImage.texture = displayTexture;
        }
	}
   

    void ClearScreen(Texture2D screen, Color clearColor)
    {
        for (int i = 0; i < ScreenWidth; ++i)
        {
            for (int j = 0; j < ScreenHeight; ++j)
            {
                screen.SetPixel(i, j, clearColor);
            }
        }
    }

    void DrawLine(Texture2D screen, Vector2 start, Vector2 end, Color color)
    {
        
        Vector2 diff = end - start;
        int x = (int)Mathf.Abs(diff.x);
        int y = (int)Mathf.Abs(diff.y);
        int step = x > y ? x : y;
        for (int i = 0; i <= step; ++i)
        {
            screen.SetPixel((int)(start.x + i * diff.x / step), (int)(start.y + i * diff.y / step), color);
        }
    }
	
    void MainDraw()
    {
        ClearScreen(displayTexture, Color.black);
        DrawLine(displayTexture, new Vector2(100, 100), new Vector2(300, 200), Color.red);
        displayTexture.Apply();
    }


	void Update () 
    {
        MainDraw();
	}
}
