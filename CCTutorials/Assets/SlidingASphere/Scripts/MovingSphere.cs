using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{
    [SerializeField]
    private BoxCollider groundBounds;

    [SerializeField, Range(0f, 1f)]
    private float bounciness = 0.5f;

    [SerializeField, Range(0f, 100f)]
    private float maxAcceleration = 10f;

    [SerializeField, Range(0f, 100f)]
    private float maxSpeed = 10f;

    private Vector3 velocity;
    private Rect allowedArea;

    private void Start()
    {
        float sphereRadius = GetComponent<SphereCollider>().radius;
        allowedArea = new Rect(groundBounds.bounds.min.x + sphereRadius, groundBounds.bounds.min.z + sphereRadius, groundBounds.bounds.size.x - sphereRadius*2f, groundBounds.bounds.size.z - sphereRadius*2f);
    }

    void Update()
    {
        //Take Input
        Vector2 playerImput;
        playerImput.x = Input.GetAxis("Horizontal");
        playerImput.y = Input.GetAxis("Vertical");
        //Limit input to unit circle area
        playerImput = Vector2.ClampMagnitude(playerImput, 1); ;
        //set the desired velocity
        Vector3 desiredVelocity = new Vector3(playerImput.x, 0f, playerImput.y) * maxSpeed;
        //Calculate how much you can change the velocity this frame
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        //modify the velocity to "Lerp" twards the desired velocity by the maxVelocityChange
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        //Calculate how much the position changed this frame
        Vector3 displacement = velocity * Time.deltaTime;
        //Clamp the movement to area
        Vector3 newPosition = transform.localPosition + displacement;
        if(newPosition.x < allowedArea.xMin)
        {
            newPosition.x = allowedArea.xMin;
            velocity.x = -velocity.x * bounciness;
        }else if(newPosition.x > allowedArea.xMax)
        {
            newPosition.x = allowedArea.xMax;
            velocity.x = -velocity.x * bounciness;
        }else if(newPosition.z < allowedArea.yMin)
        {
            newPosition.z = allowedArea.yMin;
            velocity.z = -velocity.z * bounciness;
        } else if(newPosition.z > allowedArea.yMax)
        {
            newPosition.z = allowedArea.yMax;
            velocity.z = -velocity.z * bounciness;
        }
        //modify the position
        transform.localPosition = newPosition;
    }
}
