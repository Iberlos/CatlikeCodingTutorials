using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : Buildable
{
    [SerializeField]
    private GameObject tower = default;
    [SerializeField]
    private GameObject northWall = default, northEastWall = default, eastWall = default, southEastWall = default, southWall = default, southWestWall = default, westWall = default, northWestWall = default;

    public override void Adapt(GameTile tile)
    {
        northWall.SetActive(tile.north.Content.Type == Type? true : false);
        northEastWall.SetActive(tile.northEast.Content.Type == Type ? true : false);
        eastWall.SetActive(tile.east.Content.Type == Type ? true : false);
        southEastWall.SetActive(tile.southEast.Content.Type == Type ? true : false);
        southWall.SetActive(tile.south.Content.Type == Type ? true : false);
        southWestWall.SetActive(tile.southWest.Content.Type == Type ? true : false);
        westWall.SetActive(tile.west.Content.Type == Type ? true : false);
        northWestWall.SetActive(tile.northWest.Content.Type == Type ? true : false);

        //Should only be active if either three or more walls are active or if the two active walls are not oposed to each other. But I will do that later
        tower.SetActive(true);
    }
}
