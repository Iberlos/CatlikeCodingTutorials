using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : GameTileContent
{
    [SerializeField]
    private GameObject tower = default;
    [SerializeField]
    private GameObject northWall = default, eastWall = default, southWall = default, westWall = default;

    public override void Adapt(GameTile tile)
    {
        northWall.SetActive(tile.north.Content.Type == Type? true : false);
        eastWall.SetActive(tile.east.Content.Type == Type ? true : false);
        southWall.SetActive(tile.south.Content.Type == Type ? true : false);
        westWall.SetActive(tile.west.Content.Type == Type ? true : false);
    }
}
