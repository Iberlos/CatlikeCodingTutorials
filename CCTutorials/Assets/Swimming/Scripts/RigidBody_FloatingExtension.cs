﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidBody_FloatingExtension : MonoBehaviour
{
    [SerializeField, Range(0.00000001f, 0.001f)]
    private float sleepSqrVelocityThreshold = 0.0001f;
    [SerializeField]
    private float submergenceOffset = 0.5f;
    [SerializeField, Min(0.1f)]
    private float submergenceRange = 1f;
    [SerializeField, Min(0f)]
    private float buoyancy = 1f;
    [SerializeField, Range(0f, 10f)]
    private float waterDrag = 1f;
    [SerializeField]
    private LayerMask waterMask = 5;
    [SerializeField]
    private Vector3 buoyancyOffset = Vector3.zero;

    private Rigidbody body;
    private float floatDelay;
    float submergence;
    Vector3 gravity;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    private void FixedUpdate()
    {
        if(body.IsSleeping())
        {
            GetComponent<Renderer>().material.SetColor("_Color", Color.grey);
            floatDelay = 0f;
            return;
        }

        if(body.velocity.sqrMagnitude < sleepSqrVelocityThreshold)
        {
            GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
            floatDelay += Time.deltaTime;
            if(floatDelay >= 1f)
            {
                return;
            }
        }
        else
        {
            floatDelay = 0f;
        }

        GetComponent<Renderer>().material.SetColor("_Color", Color.red);

        gravity = CustomGravity.GetGravity(body.position);
        if (submergence > 0f)
        {
            float drag = Mathf.Max(0f, 1f - waterDrag * submergence * Time.deltaTime);
            body.velocity *= drag;
            body.angularVelocity *= drag;
            body.AddForceAtPosition(gravity * -(buoyancy * submergence), transform.TransformPoint(buoyancyOffset), ForceMode.Acceleration);
            submergence = 0f;
        }

        body.AddForce(gravity, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if((waterMask & (1<<other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!body.IsSleeping() && (waterMask & (1 << other.gameObject.layer)) != 0)
        {
            EvaluateSubmergence();
        }
    }

    private void EvaluateSubmergence()
    {
        Vector3 upAxis = -gravity.normalized;
        if(Physics.Raycast(body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit, submergenceRange + 1, waterMask, QueryTriggerInteraction.Collide))
        {
            submergence = 1f - hit.distance / submergenceRange;
        }
        else
        {
            submergence = 1f;
        }
    }
}
