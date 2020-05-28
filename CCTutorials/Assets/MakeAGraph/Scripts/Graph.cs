using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    public Transform pointPrefab;
    [Range(10, 100)]
    public int resolution;
    Transform[] points;
    public float pointOffset = 0.5f;

    private void Awake()
    {
        points = new Transform[resolution];
        float step = 2f / resolution;
        Vector3 localScale = Vector3.one * step;
        Vector3 localPosition = Vector3.zero;
        for (int i = 0; i < points.Length; i++)
        {
            points[i] = Instantiate(pointPrefab);

            localPosition.x = (i + pointOffset) * step - 1f;

            points[i].localPosition = localPosition;
            points[i].localScale = localScale;
            points[i].SetParent(transform, false);
        }
    }

    private void Update()
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 localPosition = points[i].localPosition;
            localPosition.y = Mathf.Sin(Mathf.PI*(localPosition.x + Time.time));
            points[i].localPosition = localPosition;
        }
    }
}
