using UnityEngine;

public enum GraphFunctionName
{
    Sine,
    Sine2D,
    MultiSine,
    MultiSine2D,
    Ripple,
    Cylinder,
    Sphere,
    Thorus
}

public delegate Vector3 GraphFunction(float x, float v, float t);

public struct GraphFunctions
{
    const float pi = Mathf.PI;
    static public GraphFunction[] functions = 
    {
        SineFunction,
        Sine2DFunction,
        MultiSineFunction,
        MultiSine2DFunction,
        Ripple,
        Cylinder,
        Sphere,
        Thorus
    };

    static public Vector3 SineFunction(float u, float v, float t)
    {
        Vector3 returnValue = Vector3.zero;

        returnValue.x = u;
        returnValue.y = Mathf.Sin(pi * (u + t));
        returnValue.z = v;

        return returnValue;
    }

    static public Vector3 Sine2DFunction(float u, float v, float t)
    {
        Vector3 returnValue = Vector3.zero;

        returnValue.x = u;
        returnValue.y = Mathf.Sin(pi * (u + t));
        returnValue.y += Mathf.Sin(pi * (v + t));
        returnValue.y *= 0.5f;
        returnValue.z = v;

        return returnValue;
    }

    static public Vector3 MultiSineFunction(float u, float v, float t)
    {
        Vector3 returnValue = Vector3.zero;

        returnValue.x = u;
        returnValue.y = Mathf.Sin(pi * (u + t));
        returnValue.y += Mathf.Sin(2f * pi * (u + 2f * t)) / 2f;
        returnValue.y *= 2f / 3f;
        returnValue.z = v;

        return returnValue;
    }

    static public Vector3 MultiSine2DFunction(float u, float v, float t)
    {
        Vector3 returnValue = Vector3.zero;

        returnValue.x = u;
        returnValue.y = Mathf.Sin(pi * (u + t));
        returnValue.y += Mathf.Sin(2f * pi * (u + 2f * t)) / 2f;
        returnValue.y += Mathf.Sin(pi * (v + t));
        returnValue.y += Mathf.Sin(2f * pi * (v + 2f * t)) / 2f;
        returnValue.y *= 1f/ 3f;
        returnValue.z = v;

        return returnValue;
    }

    static Vector3 Ripple(float u, float v, float t)
    {
        Vector3 returnValue = Vector3.zero;

        returnValue.x = u;
        float d = Mathf.Sqrt(u * u + v * v);
        returnValue.y = Mathf.Sin(pi * (4f * d - t));
        returnValue.y *= 1f / (1f + 10f * d);
        returnValue.z = v;

        return returnValue;
    }

    //static Vector3 PlaneY(float u, float v, float t)
    //{
    //    Vector3 returnValue = Vector3.zero;

    //    returnValue.x = u;
    //    returnValue.y = 0;
    //    returnValue.z = v;

    //    return returnValue;
    //}

    //static Vector3 Circle(float u, float v, float t)
    //{
    //    Vector3 returnValue = Vector3.zero;

    //    returnValue.x = Mathf.Sin(pi * u);
    //    returnValue.y = 0;
    //    returnValue.z = Mathf.Cos(pi * u);

    //    return returnValue;
    //}

    //static Vector3 Cylinder(float u, float v, float t)
    //{
    //    float r = 1f;

    //    Vector3 returnValue = Vector3.zero;

    //    returnValue.x = r * Mathf.Sin(pi * u);
    //    returnValue.y = v;
    //    returnValue.z = r * Mathf.Cos(pi * u);

    //    return returnValue;
    //}

    //static Vector3 Sphere(float u, float v, float t)
    //{
    //    float r = Mathf.Cos(pi * 0.5f * v);

    //    Vector3 returnValue = Vector3.zero;

    //    returnValue.x = r * Mathf.Sin(pi * u);
    //    returnValue.y = Mathf.Sin(pi * 0.5f * v);
    //    returnValue.z = r * Mathf.Cos(pi * u);

    //    return returnValue;
    //}

    //static Vector3 Thorus(float u, float v, float t)
    //{
    //    float r1 = 0.60f;
    //    float r2 = 0.40f;
    //    float s = r2 * Mathf.Cos(pi * v) + r1;

    //    Vector3 returnValue = Vector3.zero;

    //    returnValue.x = s * Mathf.Sin(pi * u);
    //    returnValue.y = r2 * Mathf.Sin(pi * v);
    //    returnValue.z = s * Mathf.Cos(pi * u);

    //    return returnValue;
    //}

    static Vector3 Cylinder(float u, float v, float t)
    {
        float r = 0.8f + Mathf.Sin(pi * (6f * u + 2f*v + t)) * 0.2f;

        Vector3 returnValue = Vector3.zero;

        returnValue.x = r * Mathf.Sin(pi * u);
        returnValue.y = v;
        returnValue.z = r * Mathf.Cos(pi * u);

        return returnValue;
    }

    static Vector3 Sphere(float u, float v, float t)
    {
        float r = 0.8f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
        r += Mathf.Sin(pi * (4f * v + t)) * 0.1f;
        float s = r * Mathf.Cos(pi * 0.5f * v);

        Vector3 returnValue = Vector3.zero;

        returnValue.x = s * Mathf.Sin(pi * u);
        returnValue.y = r * Mathf.Sin(pi * 0.5f * v);
        returnValue.z = s * Mathf.Cos(pi * u);

        return returnValue;
    }

    static Vector3 Thorus(float u, float v, float t)
    {
        float r1 = 0.65f + Mathf.Sin(pi * (6f * u + t)) * 0.1f;
        float r2 = 0.2f + Mathf.Sin(pi * (4f * v + t)) * 0.05f;
        float s = r2 * Mathf.Cos(pi * v) + r1;

        Vector3 returnValue = Vector3.zero;

        returnValue.x = s * Mathf.Sin(pi * u);
        returnValue.y = r2 * Mathf.Sin(pi * v);
        returnValue.z = s * Mathf.Cos(pi * u);

        return returnValue;
    }
}

