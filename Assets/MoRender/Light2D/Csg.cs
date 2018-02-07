using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Csg : MonoBehaviour
{
    const int SAMPLE_COUNT = 64;
    const int MAX_STEP = 10;
    const float MAX_DISTANCE = 2f;
    const float EPSILON = 1e-6f;

    class Result
    {
        public float sd;
        public float emissive;
        public Result(float s, float e)
        {
            sd = s;
            emissive = e;
        }
    }

    float CircleSDF(Vector2 p, Vector2 c, float r)
    {
        return Vector2.Distance(p, c) - r;
    }

    Result UnionOp(Result a, Result b)
    {
        return a.sd < b.sd ? a : b;
    }

    Result IntersectOp(Result a, Result b)
    {
        Result r = a.sd < b.sd ? a : b; //颜色取近的
        r.sd = a.sd > b.sd ? a.sd : b.sd; //距离取远的
        return r;
    }

    Result SubtractOp(Result a, Result b)
    {
        Result r = a;
        r.sd = (a.sd > -b.sd) ? a.sd : -b.sd;
        return r;
    }

    Result Scene(Vector2 p)
    {
#if false
        Result r1 = new Result(CircleSDF(p, new Vector2(0.3f, 0.3f), 0.10f), 2.0f);
        Result r2 = new Result(CircleSDF(p, new Vector2(0.3f, 0.7f), 0.05f), 0.8f);
        Result r3 = new Result(CircleSDF(p, new Vector2(0.7f, 0.5f), 0.10f), 0.0f);
        return UnionOp(UnionOp(r1, r2), r3);
        //Result r1 = new Result(CircleSDF(p, new Vector2(0.5f, 0.5f), 0.05f), 2f);
        //return r1;
#else
        Result a = new Result(CircleSDF(p, new Vector2(0.4f, 0.5f), 0.20f), 1.0f );
        Result b = new Result(CircleSDF(p, new Vector2(0.6f, 0.5f), 0.20f), 0.8f );
        //return UnionOp(a, b);
        return IntersectOp(a, b);
        //return SubtractOp(a, b);
        //return SubtractOp(b, a);
#endif
    }

    float Trace(Vector2 p, Vector2 d)
    {
        float t = 0.0f;
        //Vector2 c = new Vector2(0.5f, 0.5f);
        for (int i = 0; i < MAX_STEP && t < MAX_DISTANCE; i++)
        {
            Result r = Scene(p + d * t);
            if (r.sd < EPSILON) return r.emissive;
            t += r.sd;
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

    void Start()
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
