using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidBody_ComplexGravityExtension : MonoBehaviour
{
    [SerializeField, Range(0.00000001f, 0.001f)]
    private float sleepSqrVelocityThreshold = 0.0001f;

    private Rigidbody body;
    private float floatDelay;

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
        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
    }
}
