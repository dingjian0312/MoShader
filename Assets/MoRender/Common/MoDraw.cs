using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[ExecuteInEditMode]
[RequireComponent(typeof(RawImage))]
public class MoDraw : MonoBehaviour
{
    public static MoDraw Instance { get; set; }
    public int ScreenWidth;
    public int ScreenHeight;
    private Texture2D displayTexture;

    void OnEnable()
    {
        Instance = this;
        ScreenWidth = Screen.width;
        ScreenHeight = Screen.height;

        RawImage displayImage = GetComponent<RawImage>();
        if (displayTexture == null)
        {
            displayTexture = new Texture2D(ScreenWidth, ScreenHeight, TextureFormat.ARGB32, false);
            displayTexture.wrapMode = TextureWrapMode.Clamp;
            displayImage.texture = displayTexture;
        }
    }

    public void ClearScreen(Color clearColor)
    {
        for (int i = 0; i < ScreenWidth; ++i)
        {
            for (int j = 0; j < ScreenHeight; ++j)
            {
                displayTexture.SetPixel(i, j, clearColor);
            }
        }
    }

    public void DrawLine(Vector2Int start, Vector2Int end, Color color)
    {

        Vector2Int diff = end - start;
        int x = Mathf.Abs(diff.x);
        int y = Mathf.Abs(diff.y);
        int step = x > y ? x : y;
        for (int i = 0; i <= step; ++i)
        {
            displayTexture.SetPixel((int)(start.x + i * diff.x / step), (int)(start.y + i * diff.y / step), color);
        }
    }

    public void DrawPoint(Vector2Int p, Color color)
    {
        displayTexture.SetPixel(p.x, p.y, color);
    }

    public void Flush()
    {
        displayTexture.Apply();
    }
}
