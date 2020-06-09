using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloatRange {
    public float min = 0f, max = 1f;

    public float RandomValueInRange
    {
        get
        {
            return Random.Range(min, max);
        }
    }
}
