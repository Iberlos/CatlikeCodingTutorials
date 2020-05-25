﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere_OrbitalCamera : MonoBehaviour
{
    //Movement Related Serialized Fields
    [Header("Movement")]
    [SerializeField]
    Transform playerInputSpace = default;
    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f;
    [SerializeField, Range(0f, 100f)]
    private float maxAirAcceleration = 1f;
    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 10f;
    //Movement and Jump related Serialized Fields
    [Header("Ground Contact")]
    [SerializeField, Range(0, 90)]
    private float maxGroundAngle = 25f;
    [SerializeField, Range(0, 90)]
    private float maxStairsAngle = 50f;
    [SerializeField, Range(0f, 100f)]
    private float maxSnapSpeed = 100f;
    [SerializeField, Min(0)]
    private float probeDistance = 1f;
    [SerializeField]
    private LayerMask probeMask = -1;
    [SerializeField]
    private LayerMask stairMask = -1;
    //Jump Related Serialized Fields
    [Header("Jump")]
    [SerializeField, Range(0f, 10f)]
    private float jumpHeight = 2f;
    [SerializeField, Range(0, 5)]
    private int maxAirJumps;

    //General private variables
    private Rigidbody body;
    //Movement related private variables
    private Vector3 velocity, desiredVelocity;
    //Movement and Jump related private variables
    private bool OnGround => groundContactCount > 0;
    private bool OnSteep => steepContactCount > 0;
    private float minGroundDotProduct, minStairDotProduct;
    private Vector3 contactNormal, steepNormal;
    private int groundContactCount, steepContactCount;
    private int stepsSinceLastGrounded;
    //Jump related private variables
    private bool desiredJump;
    private int jumpPhase;
    private int stepsSinceLastJump;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update()
    {
        //Take Input
        desiredJump |= Input.GetButtonDown("Jump");
        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        //Limit input to unit circle area
        playerInput = Vector2.ClampMagnitude(playerInput, 1); ;
        //set the desired velocity
        if(playerInputSpace) //if an input space was defined use it
        {
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            desiredVelocity = (forward * playerInput.y + right * playerInput.x) * maxSpeed;
        }
        else //otherwise use the global space like always
        {
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }
        
    }

    private void FixedUpdate()
    {
        UpdateState();
        AdjustVelocity();
        //Deal with jump request
        if(desiredJump)
        {
            desiredJump = false;
            Jump();
        }
        //Modify rigid body velocity
        body.velocity = velocity;
        ClearState();
    }
    private void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
    }

    void UpdateState()
    {
        //Increase the counts
        stepsSinceLastGrounded++;
        stepsSinceLastJump++;
        //Retrieve velocity from the body for adjustment
        velocity = body.velocity;
        if(OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if(stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if(groundContactCount > 1)
            {
                contactNormal.Normalize();
            }
        }
        else
        {
            contactNormal = Vector3.up;
        }
    }

    bool CheckSteepContacts()
    {
        if(steepContactCount > 1)
        {
            steepNormal.Normalize();
            if(steepNormal.y >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormal = steepNormal;
                return true;
            }
        }
        return false;
    }

    bool SnapToGround()
    {
        //Abort if this is not the first step after ungrounded
        if(stepsSinceLastGrounded > 1)
        {
            return false;
        }
        //Abort if you jumped up to two steps before
        if(stepsSinceLastJump <= 2)
        {
            return false;
        }
        //Abort if above the max snap speed
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        //abort if there is no surface under you
        if(!Physics.Raycast(body.position, Vector3.down, out RaycastHit hit, probeDistance, probeMask)) //NOTE: hit is defined in this line
        {
            return false;
        }
        //abort if the surface under you is not considered ground
        if(hit.normal.y < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }
        //Consider yourself grounded
        groundContactCount = 1;
        contactNormal = hit.normal;
        //align velocity to the new ground, but keep the current speed.
        float dot = Vector3.Dot(velocity, hit.normal);
        if(dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;
    }

    void Jump()
    {
        Vector3 jumpDirection;
        if(OnGround) //If on ground use the ground normal to jump
        {
            jumpDirection = contactNormal;
        } else if(OnSteep) //If not on ground check if close to a steep wall or a crevace and jum using the resulting steep normal
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        } else if(maxAirJumps > 0 && jumpPhase <= maxAirJumps) //if on air check if you can still air jump and use the ground normal
        {
            if(jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormal;
        } else
        {
            return;
        }
        //Reset step counter
        stepsSinceLastJump = 0;
        //Incrment the phase (zero is ground jump)
        jumpPhase++;
        //Modify the velocity based on the jump settings
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        //Give the wall jump an up bias
        jumpDirection = (jumpDirection + Vector3.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;

    }

    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormal = steepNormal = Vector3.zero;
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for(int i =0; i<collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if(normal.y >= minDot)
            {
                groundContactCount++;
                contactNormal += normal;
            }
            else if(normal.y > -0.01f)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    float GetMinDot(int layer)
    {
        return (stairMask &(1 << layer)) == 0 ? minGroundDotProduct: minStairDotProduct;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        //Calculate how much you can change the velocity this frame
        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.fixedDeltaTime;
        //modify the velocity to "Lerp" twards the desired velocity by the maxVelocityChange
        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
    }
}
