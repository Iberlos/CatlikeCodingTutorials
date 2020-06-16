using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField, Range(0.1f,2.0f)]
    private float panSensitivity = 1.0f;
    [SerializeField, Range(0.1f, 2.0f)]
    private float scrollSensitivity = 1.0f;

    private Vector3 prePanPosition;

    public void ApplyZoom(float zoomAmmount)
    {
        Vector3 p = transform.localPosition;
        p.y -= zoomAmmount * scrollSensitivity;
        p.y = Mathf.Clamp(p.y, 1f, 100f);
        transform.localPosition = p;
    }

    public void InitiatePan()
    {
        prePanPosition = transform.localPosition;
    }

    public void ApplyPan(Vector2 panAmmount)
    {
        Vector3 p = prePanPosition;
        p.x -= panAmmount.x * panSensitivity;
        p.y = transform.localPosition.y;
        p.z -= panAmmount.y * panSensitivity;
        transform.localPosition = p;
    }
}
