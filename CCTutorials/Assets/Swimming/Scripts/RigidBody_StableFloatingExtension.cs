using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RigidBody_StableFloatingExtension : MonoBehaviour
{
    [SerializeField, Range(0.00000001f, 0.001f)]
    private float sleepSqrVelocityThreshold = 0.0001f;
    [SerializeField]
    bool safeFloating = false;
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
    private Vector3[] buoyancyOffsets = default;

    private Rigidbody body;
    private float floatDelay;
    private float[] submergences;
    private Vector3 gravity;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        submergences = new float[buoyancyOffsets.Length];
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
        float dragFactor = waterDrag * Time.deltaTime / buoyancyOffsets.Length;
        float buoyancyFactor = -buoyancy / buoyancyOffsets.Length;
        for(int i = 0; i < buoyancyOffsets.Length; i++)
        {
            if (submergences[i] > 0f)
            {
                float drag = Mathf.Max(0f, 1f - dragFactor * submergences[i] * Time.deltaTime);
                body.velocity *= drag;
                body.angularVelocity *= drag;
                body.AddForceAtPosition(gravity * (buoyancyFactor * submergences[i]), transform.TransformPoint(buoyancyOffsets[i]), ForceMode.Acceleration);
                submergences[i] = 0f;
            }
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
        Vector3 down = gravity.normalized;
        Vector3 offset = down * - submergenceOffset;
        for(int i = 0; i < buoyancyOffsets.Length; i++)
        {
            Vector3 p = offset + transform.TransformPoint(buoyancyOffsets[i]);
            if (Physics.Raycast(p, down, out RaycastHit hit, submergenceRange + 1, waterMask, QueryTriggerInteraction.Collide))
            {
                submergences[i] = 1f - hit.distance / submergenceRange;
            }
            else if ( !safeFloating || Physics.CheckSphere(p, 0.01f, waterMask, QueryTriggerInteraction.Collide))
            {
                submergences[i] = 1f;
            }
        }
    }
}
