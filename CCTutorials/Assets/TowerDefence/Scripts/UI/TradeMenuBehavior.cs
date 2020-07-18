using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TradeMenuBehavior : MonoBehaviour
{
    private Transform target;

    public void Activate(Transform target = null)
    {
        if (target != null) this.target = target;
        gameObject.SetActive(true);
        Vector3 pos = Camera.main.WorldToScreenPoint(target != null ? target.position + Vector3.up : Vector3.zero);
        transform.position = pos;
    }

    private void Update()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(target != null? target.position + Vector3.up: Vector3.zero);
        transform.position = pos;
    }
}
