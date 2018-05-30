using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ray
{
    public Vector3 origin;
    public Vector3 direction;
    public Ray(Vector3 o, Vector3 d)
    {
        origin = o;
        direction = d;
    }

    public Vector3 PointAtParameter(float t)
    {
        return origin + direction * t;
    }
}

public class HitRecord
{
    public float t;
    public Vector3 normal;
    public Vector3 p;
}

public interface Hitable
{
    bool Hit(Ray r, float tMin, float tMax, HitRecord hrc);
}

public class Sphere : Hitable
{
    public Vector3 center;
    public float radius;

    public Sphere(Vector3 c, float r)
    {
        center = c;
        radius = r;
    }

    public bool Hit(Ray r, float tMin, float tMax, HitRecord hrc)
    {
        Vector3 oc = r.origin - center;
        float a = Vector3.Dot(r.direction, r.direction);
        float b = 2.0f * Vector3.Dot(oc, r.direction);
        float c = Vector3.Dot(oc, oc) - radius * radius;
        float discriminant = b * b - 4 * a * c;

        if (discriminant > 0)
        {
            float temp = (-b - Mathf.Sqrt(b * b - a * c)) / a;
            if (temp < tMax && temp > tMin)
            {
                hrc.t = temp;
                hrc.p = r.PointAtParameter(hrc.t);
                hrc.normal = (hrc.p - center) / radius;

                return true;
            }

            temp = (-b + Mathf.Sqrt(b * b - a * c)) / a;
            if (temp < tMax && temp > tMin)
            {
                hrc.t = temp;
                hrc.p = r.PointAtParameter(hrc.t);
                hrc.normal = (hrc.p - center) / radius;

                return true;
            }
        }

        return false;
    }
}

public class HitableList : Hitable
{
    public List<Hitable> hitList;

    public HitableList(List<Hitable> l)
    {
        hitList = l;
    }

    public bool Hit(Ray r, float tMin, float tMax, HitRecord hrc)
    {
        bool hitAny = false;
        double closeSoFar = tMax;
        for (int i = 0; i < hitList.Count; ++i)
        {
            if (hitList[i].Hit(r, tMin, tMax, hrc))
            {
                hitAny = true;
                closeSoFar = hrc.t;
            }
        }
        return hitAny;
    }
}

public class CameraRay
{
    Vector3 leftBottom;
    Vector3 horizontalDir;
    Vector3 verticalDir;
    Vector3 origin;

    public CameraRay()
    {
        leftBottom = new Vector3(-2.0f, -1.0f, -1.0f);
        horizontalDir = new Vector3(4.0f, 0, 0);
        verticalDir = new Vector3(0, 2.0f, 0);
        origin = Vector3.zero;
    }

    public Ray GetRay(float u, float v)
    {
        return new Ray(origin, leftBottom + u * horizontalDir + v * verticalDir - origin);
    }
}


[ExecuteInEditMode]
public class RayTracing : MonoBehaviour
{
    int screenWidth = 200;
    int screenHeight = 100;

    void Start()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(Screen.width, Screen.height);
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        MoDraw.Instance.ClearScreen(Color.black);
        DrawPicture();
        MoDraw.Instance.Flush();
    }

    void DrawPicture()
    {
        int sampleCount = 1;
        Hitable sphere1 = new Sphere(new Vector3(0, 0, -1), 0.5f);
        Hitable sphere2 = new Sphere(new Vector3(0, -100.5f, -1), 100);
        List<Hitable> hitList = new List<Hitable>();
        hitList.Add(sphere1);
        hitList.Add(sphere2);
        Hitable world = new HitableList(hitList);
        CameraRay camRay = new CameraRay();
        for (int i = 0; i < screenWidth; ++i)
        {
            for (int j = 0; j < screenHeight; ++j)
            {
                Color c = Color.black;
                for (int k = 0; k < sampleCount; ++k)
                {
                    float t1 = Random.Range(0.0f, 1.0f);
                    float t2 = Random.Range(0.0f, 1.0f);
                    Ray r = camRay.GetRay((i + t1) / screenWidth, (j + t2) / screenHeight);
                    c += GetColor(r, world);
                }
                c = c / sampleCount;
                MoDraw.Instance.DrawPoint(new Vector2Int(i, j), c);
            }
        }
    }



    Color GetColor(Ray r, Hitable world)
    {
        HitRecord hrc = new HitRecord();
        if (world.Hit(r, 0, float.MaxValue, hrc))
        {
            Vector3 normal = r.PointAtParameter(hrc.t) - new Vector3(0, 0, -1);
            normal.Normalize();
            Vector3 c = (normal + Vector3.one) * 0.5f;
            return new Color(c.x, c.y, c.z);
        }
        else
        {
            Vector3 unitDirection = r.direction;
            unitDirection.Normalize();
            float c = 0.5f * (unitDirection.y + 1);
            Vector3 col = (1 - c) * Vector3.one + c * new Vector3(0.5f, 0.7f, 1.0f);
            return new Color(col.x, col.y, col.z);
        }
    }
}
