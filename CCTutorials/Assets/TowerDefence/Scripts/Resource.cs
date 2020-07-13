using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : GameTileContent
{
    public ResourceType resourceType;
    public override int Variation => (int)resourceType;
}
