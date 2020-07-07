using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    North = 0, East, South, West
}

public enum DirectionChange
{
    None = 0, TurnRight, TurnLeft, TurnAround
}

public static class DirectionExtensions
{
    static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 270f, 0f)
    };
    static Vector3[] halfVectors = {
        Vector3.forward * 0.5f,
        Vector3.right * 0.5f,
        Vector3.back * 0.5f,
        Vector3.left * 0.5f
    };

    public static Quaternion GetRotation(this Direction direction)
    {
        return rotations[(int)direction];
    }

    public static DirectionChange GetDirectionChangeTo(this Direction current, Direction next)
    {
        if (current == next)
        {
            return DirectionChange.None;
        }
        else if (current + 1 == next || current - 3 == next)
        {
            return DirectionChange.TurnRight;
        }
        else if (current - 1 == next || current + 3 == next)
        {
            return DirectionChange.TurnLeft;
        }
        return DirectionChange.TurnAround;
    }

    public static float GetAngle(this Direction direction)
    {
        return (float)direction * 90f;
    }

    public static Vector3 GetHalfVector(this Direction direction)
    {
        return halfVectors[(int)direction];
    }
}

public class GameTile : MonoBehaviour
{
    [SerializeField]
    private Transform arrow = default;

    public bool HasPath => distance != int.MaxValue;
    public bool IsAlternative {get;set;}
    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content");
            if(content != null)
            {
                content.Recycle();
            }
            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    public GameTile NextTileOnPath => nextOnPath;
    public Direction PathDirection { get; private set; }

    public Vector3 ExitPoint { get; private set; }

    [HideInInspector]
    public GameTile north, northEast, east, southEast, south, southWest, west, northWest, nextOnPath;
    private int distance;
    private static Quaternion
        northRotation = Quaternion.Euler(90f, 0f, 0f),
        eastRotation = Quaternion.Euler(90f, 90f, 0f),
        southRotation = Quaternion.Euler(90f, 180f, 0f),
        westRotation = Quaternion.Euler(90f, 270f, 0f);
    private GameTileContent content;

    public static void MakeEastWestNeighbors(GameTile tile, GameTile west)
    {
        Debug.Assert(west.east == null && tile.west == null, "Redefined East/West neighbors!");
        west.east = tile;
        tile.west = west;
    }

    public static void MakeNorthSouthNeighbors(GameTile tile, GameTile south)
    {
        Debug.Assert(south.north == null && tile.south == null, "Redefined North/Sounth neighbors!");
        south.north = tile;
        tile.south = south;
    }

    public static void MakeDiagonalNeighbours(GameTile tile, GameTile south)
    {
        if(south.east != null)
        {
            Debug.Assert(south.east.northWest == null && tile.southEast == null, "Redefined Diagonal neighbors!");
            south.east.northWest = tile;
            tile.southEast = south.east;
        }
        if(south.west!= null)
        {
            Debug.Assert(south.west.northEast == null && tile.southWest == null, "Redefined Diagonal neighbors!");
            south.west.northEast = tile;
            tile.southWest = south.west;
        }
    }

    public void ClearPath()
    {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        if (!HasPath || neighbor == null || neighbor.HasPath)
        {
            return null;
        }
        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;
        neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();
        neighbor.PathDirection = direction;
        return neighbor.content.BlocksPath ? null : neighbor;
    }

    public GameTile GrowPathNorth() => GrowPathTo(north, Direction.South);
    public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);
    public GameTile GrowPathSouth() => GrowPathTo(south, Direction.North);
    public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);

    public void ShowPath()
    {
        if (distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == north ? northRotation :
            nextOnPath == east ? eastRotation :
            nextOnPath == south ? southRotation :
            westRotation;
    }

    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }
}
