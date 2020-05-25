using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGravity
{
    public static Vector3 GetGravity (Vector3 position)
    {
        return position.normalized * Physics.gravity.y;
    }
    
    public static Vector3 GetUpAxis(Vector3 position)
    {
        return position.normalized;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        upAxis = GetUpAxis(position);
        return GetGravity(position);
    }
}
