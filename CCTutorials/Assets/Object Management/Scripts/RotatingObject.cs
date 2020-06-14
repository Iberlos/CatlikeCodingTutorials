using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : GameLevelObject
{
    [SerializeField]
    private Vector3 angularVelocity;

    public override void GameUpdate()
    {
        transform.Rotate(angularVelocity * Time.fixedDeltaTime);
    }
}
