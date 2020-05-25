﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravitySphere : GravitySource
{
    [SerializeField]
    private float gravity = 9.18f;
    [SerializeField, Min(0f)]
    private float outerRadius = 10f, outerFalloffRadius = 15f;
    [SerializeField]
    private float innerRadius = 5f, innerFalloffRadius = 1f;

    private float innerFalloffFactor, outerFalloffFactor;

    private void OnDrawGizmos()
    {
        Vector3 p = transform.position;
        if(innerFalloffRadius > 0f && innerFalloffRadius < innerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, innerFalloffRadius);
        }
        Gizmos.color = Color.yellow;
        if(innerRadius > 0f && innerRadius < outerRadius)
        {
            Gizmos.DrawWireSphere(p, innerRadius);
        }
        Gizmos.DrawWireSphere(p, outerRadius);
        if (outerFalloffRadius > outerRadius)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, outerFalloffRadius);
        }
    }

    private void OnValidate()
    {
        innerFalloffRadius = Mathf.Max(innerFalloffRadius, 0f);
        innerRadius = Mathf.Max(innerRadius, innerFalloffRadius);
        outerRadius = Mathf.Max(outerRadius, innerRadius);
        outerFalloffRadius = Mathf.Max(outerFalloffRadius, outerRadius);

        innerFalloffFactor = 1f / (innerRadius - innerFalloffRadius);
        outerFalloffFactor = 1f / (outerFalloffRadius - outerRadius);
    }

    public override Vector3 GetGravity(Vector3 position)
    {
        Vector3 vector = transform.position - position;
        float distance = vector.magnitude;
        if (distance > outerFalloffRadius || distance < innerFalloffRadius)
        {
            return Vector3.zero;
        }
        float g = gravity / distance;
        if(distance > outerRadius)
        {
            g *= 1f - (distance - outerRadius) * outerFalloffFactor;
        }
        else if(distance < innerRadius)
        {
            g *= 1f - (innerRadius - distance) * innerFalloffFactor;
        }
        return g * vector;
    }
}
