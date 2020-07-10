using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct TileTypeSet
{
    public Mesh north_south;
    public Mesh north_west;
    public Mesh north_east_west;
    public Mesh north_east_south_west;
    public Mesh south_west;
    public Mesh north_south_southWest_west_northWest;
    public Mesh north_west_northWest;
    public Mesh north_northEast_east_south_west_northWest;
    public Mesh north_northEast_east_south_southWest_west_northWest;
    public Mesh north_northEast_east_west;
    public Mesh north_east_west_northWest;
    public Mesh defaultMesh;

    public bool Valid => north_south != null && north_west != null && north_east_west != null && north_east_south_west != null && south_west != null && north_south_southWest_west_northWest != null && north_west_northWest != null && north_northEast_east_south_west_northWest != null && north_northEast_east_south_southWest_west_northWest != null && north_northEast_east_west != null && north_east_west_northWest != null && defaultMesh != null;
}

[SelectionBase]
public class GameTileContent : MonoBehaviour
{
    [SerializeField]
    private GameTileContentType type = default;
    [SerializeField]
    private bool staticAdaptation = false;
    [SerializeField]
    private TileTypeSet tileTypeSet;
    [SerializeField]
    private MeshFilter terrainMeshFilter = default;

    public GameTileContentFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory");
            originFactory = value;
        }
    }

    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower || Type == GameTileContentType.Water || Type == GameTileContentType.Mountain;

    private GameTileContentFactory originFactory;

    public GameTileContentType Type => type;

    public virtual void GameUpdate() { }

    public void Recycle()
    {
        originFactory.Reclaim(this);
    }

    enum cartographicDirection { North = 0b0000001, NorthEast = 0b00000010, East = 0b00000100, SouthEast = 0b00001000, South = 0b00010000, SouthWest = 0b00100000, West = 0b01000000, NorthWest = 0b10000000 };
    public virtual void Adapt(GameTile tile)
    {
        if (staticAdaptation || terrainMeshFilter == null || !tileTypeSet.Valid)
            return;

        int flags = 0;
        if (tile.north != null && tile.north.Content.type == type)
            flags |= (int)cartographicDirection.North;
        if (tile.northEast != null && tile.northEast.Content.type == type)
            flags |= (int)cartographicDirection.NorthEast;
        if (tile.east != null && tile.east.Content.type == type)
            flags |= (int)cartographicDirection.East;
        if (tile.southEast != null && tile.southEast.Content.type == type)
            flags |= (int)cartographicDirection.SouthEast;
        if (tile.south != null && tile.south.Content.type == type)
            flags |= (int)cartographicDirection.South;
        if (tile.southWest != null && tile.southWest.Content.type == type)
            flags |= (int)cartographicDirection.SouthWest;
        if (tile.west != null && tile.west.Content.type == type)
            flags |= (int)cartographicDirection.West;
        if (tile.northWest != null && tile.northWest.Content.type == type)
            flags |= (int)cartographicDirection.NorthWest;


        float yAngle = 0;
        bool meshFound = false;
        for(int i = 0; i < 4 && !meshFound; i++)
        {
            switch (flags)
            {
                case 0b11111111:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.defaultMesh;
                        terrainMeshFilter.transform.position = new Vector3(terrainMeshFilter.transform.position.x, -0.1f, terrainMeshFilter.transform.position.z);
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.South:
                //oneCorner
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South:
                case (int)cartographicDirection.North | (int)cartographicDirection.SouthEast | (int)cartographicDirection.South:
                case (int)cartographicDirection.North | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthWest | (int)cartographicDirection.South:
                //two corners
                //northEast varying
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthEast:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.NorthWest:
                //SouthEast varying
                case (int)cartographicDirection.North | (int)cartographicDirection.SouthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.SouthEast | (int)cartographicDirection.South | (int)cartographicDirection.NorthWest:
                //southWest varying
                case (int)cartographicDirection.North | (int)cartographicDirection.SouthWest | (int)cartographicDirection.South | (int)cartographicDirection.NorthWest:
                //three corners
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthEast | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthEast | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest | (int)cartographicDirection.NorthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_south;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.West:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.West:
                case (int)cartographicDirection.North | (int)cartographicDirection.West | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.West | (int)cartographicDirection.SouthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_west;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.West:
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.SouthEast | (int)cartographicDirection.West:
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.West | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.SouthEast | (int)cartographicDirection.West | (int)cartographicDirection.SouthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_east_west;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.South | (int)cartographicDirection.West:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_east_south_west;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.South | (int)cartographicDirection.West:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.south_west;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.South | (int)cartographicDirection.SouthEast | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.South | (int)cartographicDirection.SouthEast | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_south_southWest_west_northWest;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest :
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_west_northWest;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.East | (int)cartographicDirection.South | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_northEast_east_south_west_northWest;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.East | (int)cartographicDirection.South | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_northEast_east_south_southWest_west_northWest;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.East | (int)cartographicDirection.West:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.East | (int)cartographicDirection.SouthEast | (int)cartographicDirection.West:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.East | (int)cartographicDirection.West | (int)cartographicDirection.SouthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.NorthEast | (int)cartographicDirection.East | (int)cartographicDirection.SouthEast | (int)cartographicDirection.West | (int)cartographicDirection.SouthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_northEast_east_west;
                        meshFound = true;
                        break;
                    }
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.SouthEast | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                case (int)cartographicDirection.North | (int)cartographicDirection.East | (int)cartographicDirection.SouthEast | (int)cartographicDirection.SouthWest | (int)cartographicDirection.West | (int)cartographicDirection.NorthWest:
                    {
                        terrainMeshFilter.mesh = tileTypeSet.north_east_west_northWest;
                        meshFound = true;
                        break;
                    }
                default: //Tile unsuported
                    {
                        if(i == 3)
                        {
                            Debug.LogWarning("Unsuported flag combination " + ((flags & (int)cartographicDirection.North) == (int)cartographicDirection.North ? "North_" : "") + ((flags & (int)cartographicDirection.NorthEast) == (int)cartographicDirection.NorthEast ? "NorthEast_" : "") + ((flags & (int)cartographicDirection.East) == (int)cartographicDirection.East ? "East_" : "") + ((flags & (int)cartographicDirection.SouthEast) == (int)cartographicDirection.SouthEast ? "SouthEast_" : "") + ((flags & (int)cartographicDirection.South) == (int)cartographicDirection.South ? "South_" : "") + ((flags & (int)cartographicDirection.SouthWest) == (int)cartographicDirection.SouthWest ? "SouthWest_" : "") + ((flags & (int)cartographicDirection.West) == (int)cartographicDirection.West ? "West_" : "") + ((flags & (int)cartographicDirection.NorthWest) == (int)cartographicDirection.NorthWest ? "NorthWest_" : ""), this.gameObject);
                            terrainMeshFilter.mesh = tileTypeSet.defaultMesh;
                        }
                        break;
                    }
            }

            int first = flags & 0b00000011;
            first = first << 6;
            int mask =  flags & 0b11111100;
            mask = mask >> 2;
            flags = first | mask;//shift Rotate flags by 2
            yAngle += 90;//increse the yAngle by 90

            terrainMeshFilter.transform.rotation = Quaternion.Euler(0f, yAngle, 0f);
        }
    }
}
