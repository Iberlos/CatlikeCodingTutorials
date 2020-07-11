using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : GameTileContent
{
    [SerializeField]
    public GroundType groundType = default;

    public override void Adapt(GameTile tile)
    {
        if(groundType == GroundType.Bridge)
        {
            if(tile.north.Content.Type == GameTileContentType.Water)
            {
                terrainMeshFilter.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            }
        }
    }
}
