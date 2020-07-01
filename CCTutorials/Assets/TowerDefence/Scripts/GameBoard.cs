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

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
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

                tile.IsAlternative = (x & 1) == 0;
                if((y&1) ==0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

                
            }
        }

        Clear();
    }

    public void Clear()
    {
        foreach(GameTile tile in tiles)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
        }
        spawnPoints.Clear();
        int destinationTile = tiles.Length / 2;
        ToggleDestination(tiles[destinationTile]);
        for (int i = 0; i < tiles.Length; i++)
        {
            int x = i % size.x;
            int y = i / size.x;
            if(x==0 || x==size.x-1 || y == 0 || y == size.y-1)
                ToggleSpawnPoint(tiles[i]);
        }
    }

    public void GameUpdate()
    {
        for(int i = 0; i< updatingContent.Count; i++)
        {
            updatingContent[i].GameUpdate();
        }
    }

    private bool FindPaths()
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
                if (tile.IsAlternative)
                {
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathWest());
                }
                else
                {
                    searchFrontier.Enqueue(tile.GrowPathWest());
                    searchFrontier.Enqueue(tile.GrowPathEast());
                    searchFrontier.Enqueue(tile.GrowPathSouth());
                    searchFrontier.Enqueue(tile.GrowPathNorth());
                }
            }
        }

        foreach(GameTile tile in tiles)
        {
            if (!tilePrefab.HasPath)
                return false;
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

    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if(!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Destination);
                FindPaths();
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if(!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
    }

    public void ToggleTower(GameTile tile, TowerType towerType)
    {
        if (tile.Content.Type == GameTileContentType.Tower)
        {
            updatingContent.Remove(tile.Content);
            if(((Tower)tile.Content).TowerType == towerType)
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
            else
            {
                tile.Content = contentFactory.Get(towerType);
                updatingContent.Add(tile.Content);
            }
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(towerType);
            if (FindPaths())
            {
                updatingContent.Add(tile.Content);
            }
            else
            { 
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
        else if(tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(towerType);
            updatingContent.Add(tile.Content);
        }
    }

    public void ToggleSpawnPoint(GameTile tile)
    {
        if(tile.Content.Type == GameTileContentType.SpawnPoint)
        {
            if(spawnPoints.Count >1)
            {
                spawnPoints.Remove(tile);
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
        else if(tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
            spawnPoints.Add(tile);
        }
    }

    public GameTile GetSpawnPoint(int index)
    {
        return spawnPoints[index];
    }
}
