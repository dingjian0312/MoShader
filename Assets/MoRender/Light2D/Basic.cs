using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Basic : MonoBehaviour
{
    const int SAMPLE_COUNT = 64;
    const int MAX_STEP = 10;
    const float MAX_DISTANCE = 2f;
    const float EPSILON = 1e-6f;

    float CircleSDF(Vector2 p, Vector2 c, float r)
    {
        return Vector2.Distance(p, c) - r;
    }

    float Trace(Vector2 p, Vector2 d)
    {
        float t = 0.0f;
        Vector2 c = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < MAX_STEP && t < MAX_DISTANCE; i++)
        {
            float sd = CircleSDF(p + d * t , c, 0.1f);
            if (sd < EPSILON) return 2.0f;
            t += sd;
        }
        return 0.0f;
    }

   
    float Sample(Vector2 p)
    {
        float sum = 0.0f;
        for (int i = 0; i < SAMPLE_COUNT; ++i)
        {
            //float a = 2 * Mathf.PI * Random.Range(0.0f, 1.0f);
            //float a = 2 *Mathf.PI * i / SAMPLE_COUNT;
            float a = 2 * Mathf.PI * (i + Random.Range(0.0f, 1.0f)) / SAMPLE_COUNT;
            sum += Trace(p, new Vector2(Mathf.Cos(a), Mathf.Sin(a)));
        }
        return sum / SAMPLE_COUNT;
    }

    void Start ()
    {
        MoDraw.Instance.ClearScreen(Color.black);
        int w = MoDraw.Instance.ScreenWidth;
        int h = MoDraw.Instance.ScreenHeight;
        for (int i = 0; i < h; ++i)
        {
            for (int j = 0; j < w; ++j)
            {
                Vector2 p = new Vector2(1.0f * j / w, 1.0f * i / h);
                float l = Sample(p);
                MoDraw.Instance.DrawPoint(new Vector2Int(j, i), new Color(l, l, l));
            }
        }
         
        MoDraw.Instance.Flush();
	}
}
