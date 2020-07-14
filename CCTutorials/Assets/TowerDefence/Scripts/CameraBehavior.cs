using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehavior : GameBehavior
{
    [SerializeField]
    private float cameraSpeed = 3f;
    [SerializeField]
    private float cameraAngularSpeed = 15f;
    [SerializeField]
    private float cameraZoomSpeed = 2f;
    [SerializeField]
    private Transform target = default;
    [SerializeField]
    private float offsetDistance = 5;
    [SerializeField]
    private float minOffsetDistance = 1;
    [SerializeField]
    private float maxOffsetDistance = 15;
    [SerializeField]
    private float cameraDegreesX = 45f;
    [SerializeField]
    private float cameraDegreesY = 0f;

    private Vector3 position = Vector3.zero;

    public override bool GameUpdate()
    {
        if(target == null)
        {
            transform.rotation = Quaternion.Euler(cameraDegreesX, cameraDegreesY, 0f);
            transform.position = position - transform.forward * offsetDistance;
        }
        else
        {
            Vector3 pos = target.position;
            transform.rotation = Quaternion.Euler(cameraDegreesX, cameraDegreesY, 0f);
            transform.position = pos - transform.forward * offsetDistance;
        }
        return true;
    }

    public void ApplyInput(Vector3 movementInput, float rotationInput, float zoomInput)
    {
        //Rotate and zoom the camera
        cameraDegreesY += rotationInput * cameraAngularSpeed * Time.deltaTime;
        cameraDegreesY = cameraDegreesY>360?cameraDegreesY-360:cameraDegreesY<0?360f-cameraDegreesY:cameraDegreesY; //loop arround 360 degrees
        offsetDistance += zoomInput * cameraZoomSpeed * Time.deltaTime;
        offsetDistance = Mathf.Clamp(offsetDistance, minOffsetDistance, maxOffsetDistance);
        if (target != null) return; //if you are focusing on an object you can't move the camera freely
        //move the camera
        movementInput.Normalize();
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = transform.right; //does not need to be projected because the camra can never roll
        position += (forward * movementInput.z + right * movementInput.x) * cameraSpeed * Time.deltaTime;
    }

    public override void Recycle()
    {
        throw new System.NotImplementedException();
    }

    public void SetPositionAndRotation(Vector3 positionOnGrid, Vector3 eulerAngles = default)
    {
        Vector3 pos = positionOnGrid;
        transform.rotation = Quaternion.Euler(cameraDegreesX, cameraDegreesY, 0f);
        transform.position = pos - transform.forward * offsetDistance;
        position = positionOnGrid;
    }
}
