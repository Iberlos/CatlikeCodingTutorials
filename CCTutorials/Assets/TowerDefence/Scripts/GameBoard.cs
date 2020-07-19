﻿using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    [SerializeField]
    private Transform ground = default;
    [SerializeField]
    private GameTile tilePrefab = default;
    [SerializeField]
    private Texture2D gridTexture = default;

    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if(showPaths)
            {
                foreach(GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach(GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }

    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;
            Material m = ground.GetComponent<MeshRenderer>().material;
            if(showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex", size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }

    public int spawnPointCount => spawnPoints.Count;

    private Vector2Int size;
    private GameTile[] tiles;
    private Queue<GameTile> searchFrontier = new Queue<GameTile>();
    private GameTileContentFactory contentFactory;
    private bool showPaths, showGrid;
    private List<GameTile> spawnPoints = new List<GameTile>();
    private List<GameTileContent> updatingContent = new List<GameTileContent>();

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory, ref GeneratorParams generatorParams)
    {
        //initialize fields
        this.size = size;
        this.contentFactory = contentFactory;
        tiles = new GameTile[size.x * size.y];

        //Size Ground
        ground.localScale = new Vector3(size.x, size.y, 1f);

        //Instantiate tiles
        Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f); //shift to keep board centered
        for (int i = 0, y = 0; y < size.y; y++)//i is defined here
        {
            for (int x = 0; x < size.x; x++, i++)//i is incremented here
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);

                if (x > 0)
                    GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
                if (y > 0)
                    GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
                if (x > 0 && y > 0)
                    GameTile.MakeDiagonalNeighbours(tile, tiles[i - size.x]);

                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }
            }
        }

        GenerateMap(ref generatorParams);
    }

    public GeneratorParams GenerateMap(ref GeneratorParams generatorParams)
    {
        Clear();
        MapGenerator generator = new MapGenerator();
        generator.Initialize(ref generatorParams);
        Populate(generator.GenerateMap(size.x, size.y));
        return generatorParams;
    }

    public void Clear()
    {
        foreach(GameTile tile in tiles)
        {
            tile.Content = contentFactory.Get<Ground>(GameTileContentType.Ground);
        }
        spawnPoints.Clear();
        updatingContent.Clear();
    }

    private void Populate(MapData[] typeMap)
    {
        for(int i =0; i< tiles.Length; i++)
        {
            if(typeMap[i].tileType == GameTileContentType.Resource)
            {
                tiles[i].Content = contentFactory.Get<Resource>(GameTileContentType.Resource, typeMap[i].variation); //probably needs to be changed to be specifically of type resource
            }
            else if(typeMap[i].tileType == GameTileContentType.Ground)
            {
                tiles[i].Content = contentFactory.Get<Ground>(GameTileContentType.Ground,typeMap[i].variation);
            }
            else
            {
                tiles[i].Content = contentFactory.Get<GameTileContent>(typeMap[i].tileType);
            }
        }
        int destinationTile = tiles.Length / 2;
        for (int i = 0; i < tiles.Length; i++)
        {
            int x = i % size.x;
            int y = i / size.x;
            if (x == 0 || x == size.x - 1 || y == 0 || y == size.y - 1)
                PlaceBuilding(tiles[i], GameTileContentType.SpawnPoint, 0);
        }
        PlaceBuilding(tiles[destinationTile], GameTileContentType.Destination, 0);
    }

    public void GameUpdate()
    {
        for(int i = 0; i< updatingContent.Count; i++)
        {
            updatingContent[i].GameUpdate();
        }
    }

    private bool FindPaths(bool crossHardTerrain = false)
    {
        foreach(GameTile tile in tiles)
        {
            if(tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        if(searchFrontier.Count == 0)
        {
            return false;
        }

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();
            if (tile != null)
            {
                int before = searchFrontier.Count;
                if (tile.IsAlternative)
                {
                    searchFrontier.Enqueue(tile.GrowPathNorth(crossHardTerrain));
                    searchFrontier.Enqueue(tile.GrowPathSouth(crossHardTerrain));
                    searchFrontier.Enqueue(tile.GrowPathEast(crossHardTerrain));
                    searchFrontier.Enqueue(tile.GrowPathWest(crossHardTerrain));
                }
                else
                {
                    searchFrontier.Enqueue(tile.GrowPathWest(crossHardTerrain));
                    searchFrontier.Enqueue(tile.GrowPathEast(crossHardTerrain));
                    searchFrontier.Enqueue(tile.GrowPathSouth(crossHardTerrain));
                    searchFrontier.Enqueue(tile.GrowPathNorth(crossHardTerrain));
                }
            }
        }

        bool searchAgain = false;
        foreach(GameTile tile in tiles)
        {
            tile.Content.Adapt(tile);
            Wall w = tile.Content as Wall;
            if (w)
                w.Adapt(tile);
            if (!tile.HasPath && tile.Content.Type != GameTileContentType.Water && tile.Content.Type != GameTileContentType.Mountain)
            {
                if (!crossHardTerrain)
                {
                    tile.CausedSecondSearch = true;
                    searchAgain = true;
                }
                else
                {
                    return false;
                }
            }else if(!crossHardTerrain)
            {
                tile.CausedSecondSearch = false;
            }
        }

        if(searchAgain)
        {
            return FindPaths(true);
        }

        if(showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }

        return true;
    }

    public GameTile GetTile(Ray ray)
    {
        if(Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, layerMask: 1))
        {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if(x >= 0 && x < size.x && y >= 0 && y < size.y)
            {
                return tiles[x + y * size.x];
            }
        }
        return null;
    }

    public bool Demolish(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.Destination)
        {
            Destination d = tile.Content as Destination;
            if (d != null && d.destinationType == DestinationType.Capital)
                return false;
        }
        if(tile.Content.Type != GameTileContentType.Ground && tile.Content.Type != GameTileContentType.Water && tile.Content.Type != GameTileContentType.Mountain)
        {
            if (tile.Content.Type == GameTileContentType.Tower) //Fix for tower removal causing issues in update
            {
                updatingContent.Remove(tile.Content);
            }
            tile.Content = contentFactory.Get<Ground>(GameTileContentType.Ground);
            FindPaths();
            return true;
        }
        return false;
    }

    public bool PlaceBuilding(GameTile tile, GameTileContentType type, int variation)
    {
        ResourceType resourceType = ResourceType.Last;
        if (tile.Content.Type == GameTileContentType.Resource)
        {
            resourceType = ((Resource)(tile.Content)).resourceType;
            Demolish(tile);
        }
        if (tile.Content.Type == GameTileContentType.Ground)
        {
            tile.Content = contentFactory.Get<GameTileContent>(type, variation);
            if (!FindPaths())
            {
                if(resourceType == ResourceType.Last)
                    tile.Content = contentFactory.Get<GameTileContent>(GameTileContentType.Ground);
                else
                    tile.Content = contentFactory.Get<GameTileContent>(GameTileContentType.Resource, (int)resourceType);
                FindPaths();
                return false;
            }
            if (type == GameTileContentType.Tower)
                updatingContent.Add(tile.Content);
            if (type == GameTileContentType.SpawnPoint)
                spawnPoints.Add(tile);
            return true;
        }
        return false;
    }

    public GameTile GetSpawnPoint(int index)
    {
        return spawnPoints[index];
    }
}
