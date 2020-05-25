using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere: MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 10f;

    [SerializeField, Range(0f, 10f)]
    private float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    private int maxAirJumps;

    [SerializeField, Range(0, 90)]
    float maxGroundAngle = 25f;

    private Vector3 velocity, desiredVelocity;
    private Rigidbody body;
    private bool desiredJump;
    private int groundContactCount;
    private bool OnGround => groundContactCount > 0;
    private int jumpPhase;
    private float minGroundDotProduct;
    private Vector3 contactNormal;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update()
    {
        //Take Input
        desiredJump |= Input.GetButtonDown("Jump");
        Vector2 playerImput;
        playerImput.x = Input.GetAxis("Horizontal");
        playerImput.y = Input.GetAxis("Vertical");
        //Limit input to unit circle area
        playerImput = Vector2.ClampMagnitude(playerImput, 1); ;
        //set the desired velocity
        desiredVelocity = new Vector3(playerImput.x, 0f, playerImput.y) * maxSpeed;
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
    }

    void UpdateState()
    {
        //Retrieve velocity from the body for adjustment
        velocity = body.velocity;
        if(OnGround)
        {
            jumpPhase = 0;
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

    void Jump()
    {
        if(OnGround || jumpPhase < maxAirJumps)
        {
            jumpPhase++;
            float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
            float alignedSpeed = Vector3.Dot(velocity, contactNormal);
            if(alignedSpeed > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            }
            velocity += contactNormal * jumpSpeed;
        }
    }

    void ClearState()
    {
        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    void EvaluateCollision(Collision collision)
    {
        for(int i =0; i<collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if(normal.y >= minGroundDotProduct)
            {
                groundContactCount++;
                contactNormal += normal;
            }
        }
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
