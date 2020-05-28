using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;
    [Range(10, 100)]
    public int resolution;
    Transform[] points;
    public GraphFunctionName function;


    private void Awake()
    {
        points = new Transform[resolution * resolution];
        float step = 2f / resolution;
        Vector3 localScale = Vector3.one * step;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Instantiate(pointPrefab);
            points[i].localScale = localScale;
            points[i].SetParent(transform);
        }
    }

    private void Update()
    {
        float t = Time.time;
        GraphFunction f = GraphFunctions.functions[(int)function];
        float step = 2f / resolution;
        for (int i = 0, z =0; z < resolution; z++)
        {
            float v = (z + 0.5f) * step - 1f;
            for(int x = 0; x < resolution; x++, i++)
            {
                float u = (x + 0.5f) * step - 1f;
                points[i].localPosition = f(u, v, t);
            }
        }
    }
}
