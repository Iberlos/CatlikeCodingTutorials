using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GeneratorParams
{
    [Header("Random Fill Parameters")]
    public bool useRanomSeed;
    public string seed;
    public int groundFilPercent;
    public int mountainFilPercent;
    public int resourceFilPercent;
    [Header("Smooth Settings")]
    [Range(0, 10)]
    public int iterations;
    [Range(0, 10)]
    public int mountainIterations;
    [Range(0, 10)]
    public int resourceIterations;
    public int waterCountThreshold;
    public int mountainCountThreshold;
    public int resourceCountThreshold;
    public int moytainDistanceToRivers;

    public bool useMapBacklog;
}

public struct MapData
{
    public GameTileContentType tileType;
    public int variation;
}

public class MapGenerator
{
    private int width;
    private int height;
    private int borderSize = 2;

    GeneratorParams generatorParameters;

    int[,] map;
    int[,] mapBackLog;

    System.Random pseudoRandom;
    public void Initialize(ref GeneratorParams generatorParameters)
    {
        this.generatorParameters = generatorParameters;
    }

    public MapData[] GenerateMap(int xSize, int ySize)
    {
        width = xSize - borderSize * 2;
        height = ySize- borderSize * 2;

        map = new int[width, height];
        if (generatorParameters.useMapBacklog)
        {
            mapBackLog = new int[width, height];
        }

        RanodmFillMap();
        SmoothMap(generatorParameters.iterations);
        SimplifyMap();
        if (generatorParameters.useMapBacklog) SaveBacklog();
        RandomFilMapWithMountains();
        SmoothMountains(generatorParameters.mountainIterations);
        if (generatorParameters.useMapBacklog) SaveBacklog();
        RandomFillMapWithResources();
        SmoothResources(generatorParameters.resourceIterations);

        MapData[,] typeMap = new MapData[width, width];
        for (int x = 0; x < map.GetLength(0); x++)
        {
            for (int y = 0; y < map.GetLength(1); y++)
            {
                if (map[x, y] == 1)
                {
                    typeMap[x,y].tileType = GameTileContentType.Ground;
                }
                else if (map[x, y] == 0)
                {
                    typeMap[x, y].tileType = GameTileContentType.Water;
                }
                else if (map[x, y] == 2)
                {
                    typeMap[x, y].tileType = GameTileContentType.Mountain;
                }
                else if (map[x, y] == 3)
                {
                    typeMap[x, y].tileType = GameTileContentType.Resource;
                }
            }
        }
        for (int x = 0; x < typeMap.GetLength(0); x++)
        {
            for (int y = 0; y < typeMap.GetLength(1); y++)
            {
                if(typeMap[x,y].tileType == GameTileContentType.Resource)
                {
                    SetVariation(x,y,typeMap);
                }
            }
        }

        MapData[,] borderedMap = new MapData[width + borderSize * 2, height + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = typeMap[x - borderSize, y - borderSize];
                }
                else
                {
                    if(x <= borderSize)
                        borderedMap[x, y].tileType = GameTileContentType.Water;
                    else
                        borderedMap[x, y].tileType = GameTileContentType.Ground;
                }
            }
        }
        typeMap = borderedMap;

        MapData[] returnMap = new MapData[typeMap.GetLength(0) * typeMap.GetLength(1)];
        for (int i=0, x = 0; x < typeMap.GetLength(0); x++)
        {
            for (int y = 0; y < typeMap.GetLength(1); y++, i++)
            {
                returnMap[i] = typeMap[x, y];
            }
        }
        return returnMap;
    }

    List<List<Coord>> GetLakes(int a_tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == a_tileType)
                {
                    List<Coord> newRegion = GetLakeTiles(x, y);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    void SimplifyMap()
    {
        List<List<Coord>> wallRegions = GetLakes(1);
        int wallThreshold = 50;

        foreach (List<Coord> wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallThreshold)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetLakes(0);
        int roomThresholdSize = 50;
        List<Lake> survivingRooms = new List<Lake>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Lake(roomRegion, map));
            }
        }

        ConnectClosestlakes(survivingRooms);
        ConnectAllLakes(survivingRooms);
    }

    void ConnectClosestlakes(List<Lake> a_roomList)
    {
        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();

        Lake bestRoomA = new Lake();
        Lake bestRoomB = new Lake();
        bool possibleConnectionFound = false;


        foreach (Lake roomA in a_roomList)
        {
            possibleConnectionFound = false;

            foreach (Lake roomB in a_roomList)
            {
                if (roomA == roomB) continue;
                if (roomA.IsConnected(roomB))
                {
                    possibleConnectionFound = false;
                    break;
                }

                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];

                        int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound)
            {
                CreateRiver(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }
    }

    void ConnectAllLakes(List<Lake> a_roomList)
    {
        List<Lake> allRooms = a_roomList;
        List<List<Lake>> regions;

        do
        {
            regions = new List<List<Lake>>();

            foreach (Lake room in allRooms)//clean flags from previous iterations
            {
                room.flag = false;
            }

            foreach (Lake room in allRooms)
            {
                if (room.flag == false)
                {
                    regions.Add(new List<Lake>());
                    regions[regions.Count - 1].Add(room);
                    for (int i = 0; i < regions[regions.Count - 1].Count; i++)
                    {
                        regions[regions.Count - 1][i].flag = true;
                        foreach (Lake conectedRoom in regions[regions.Count - 1][i].conectedLakes)
                        {
                            if (conectedRoom.flag == false)
                            {
                                conectedRoom.flag = true;
                                regions[regions.Count - 1].Add(conectedRoom);
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < regions.Count; i++) //for each region
            {
                int bestDistance = 0;
                Coord bestTileA = new Coord();
                Coord bestTileB = new Coord();

                Lake bestRoomA = new Lake();
                Lake bestRoomB = new Lake();
                bool possibleConnectionFound = false;

                for (int j = 0; j < regions.Count; j++) //for each remaining region
                {
                    if (j == i) continue;
                    foreach (Lake roomA in regions[i]) //for each room in the region being tested
                    {
                        foreach (Lake roomB in regions[j]) //for each room in the other region being considered
                        {
                            for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                            {
                                for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                                {
                                    Coord tileA = roomA.edgeTiles[tileIndexA];
                                    Coord tileB = roomB.edgeTiles[tileIndexB];

                                    int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

                                    if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                                    {
                                        bestDistance = distanceBetweenRooms;
                                        possibleConnectionFound = true;
                                        bestTileA = tileA;
                                        bestTileB = tileB;
                                        bestRoomA = roomA;
                                        bestRoomB = roomB;
                                    }
                                }
                            }
                        }
                    }
                }

                if (possibleConnectionFound)
                {
                    CreateRiver(bestRoomA, bestRoomB, bestTileA, bestTileB);
                }
            }
        } while (regions.Count > 1);
    }

    void CreateRiver(Lake a_lakeA, Lake a_LakeB, Coord a_tileA, Coord a_tileB)
    {
        Lake.ConectLakes(a_lakeA, a_LakeB);
        List<Coord> line = GetLine(a_tileA, a_tileB);
        bool bridgeableTilePresent = false;
        for (int i = 0; i < line.Count; i++) //find out if there is a straight line of three tiles in this line
        {
            if(i > 0 && i < line.Count-2)
            {
                int tileX = line[i].tileX;
                int tileY = line[i].tileY;
                if (line[i - 1].tileX == tileX && line[i + 1].tileX == tileX || line[i - 1].tileY == tileY && line[i + 1].tileY == tileY)
                {
                    bridgeableTilePresent = true;
                }
            }
        }
        for (int i = 0; i < line.Count; i++) 
        {
            int tileX = line[i].tileX;
            int tileY = line[i].tileY;
            map[tileX, tileY] = 0; //become a river
            if(i > 0 && (line[i-1].tileX != tileX || line[i-1].tileY != tileY)) //if your are not the first index and the previous tile was in a diagonal direction
            {
                int x = line[i].tileX;
                int y = line[i - 1].tileY;
                map[x, y] = 0; //place a river tile to make the corner
            }
            if (i > 0 && i < line.Count - 2) //if you are not the first or the last
            {
                if (!bridgeableTilePresent && i < line.Count - 2 && line[i + 1].tileX != tileX || line[i + 1].tileY != tileY) //if there are no brigeable tiles yet, you are not the last in line and the next tile is in a diagonal
                {
                    line.Insert(i + 1, new Coord(line[i].tileX, line[i + 1].tileY)); //insert a tile betwen you to form a straight line of three tiles
                    bridgeableTilePresent = true;
                }
            }
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x < r; x++)
        {
            for (int y = -r; y < r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord a_from, Coord a_to)
    {
        List<Coord> line = new List<Coord>();

        int x = a_from.tileX;
        int y = a_from.tileY;

        int dx = a_to.tileX - x;
        int dy = a_to.tileY - y;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;

            //switch values
            int temp = longest;
            longest = shortest;
            shortest = temp;

            temp = step;
            step = gradientStep;
            gradientStep = temp;
        }

        int gradientAccumulation = longest / 2;

        for (int i = 0; i < longest + 1; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted) y += step;
            else x += step;

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted) x += gradientStep;
                else y += gradientStep;
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorld(Coord a_tile)
    {
        return new Vector3(-width / 2 + 0.5f + a_tile.tileX, 2, -height / 2 + 0.5f + a_tile.tileY);
    }

    List<Coord> GetLakeTiles(int a_startX, int a_startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[a_startX, a_startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(a_startX, a_startY));
        mapFlags[a_startX, a_startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    bool IsInMapRange(int a_x, int a_y)
    {
        return a_x >= 0 && a_x < width && a_y >= 0 && a_y < height;
    }

    private void RanodmFillMap()
    {
        if (generatorParameters.useRanomSeed)
        {
            generatorParameters.seed = Time.time.ToString();
        }

        pseudoRandom = new System.Random(generatorParameters.seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < generatorParameters.groundFilPercent) ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap(int iterations)
    {
        if (iterations == 0) return;

        iterations--;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbourWallTiles = CountSurroundingWaterTiles(x, y);

                if (neighbourWallTiles > generatorParameters.waterCountThreshold)
                {
                    if (generatorParameters.useMapBacklog)
                    {
                        mapBackLog[x, y] = 1;
                    }
                    else
                    {
                        map[x, y] = 1;
                    }
                }
                else if (neighbourWallTiles < generatorParameters.waterCountThreshold)
                {
                    if (generatorParameters.useMapBacklog)
                    {
                        mapBackLog[x, y] = 0;
                    }
                    else
                    {
                        map[x, y] = 0;
                    }
                }
            }
        }

        if (generatorParameters.useMapBacklog)
        {
            ApplyBacklog();
        }
        SmoothMap(iterations);
    }

    private void ApplyBacklog()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                map[x, y] = mapBackLog[x, y];
            }
        }
    }

    int CountSurroundingWaterTiles(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        wallCount += map[neighborX, neighborY];
                    }
                }
                else
                {
                    if (neighborX == gridX || neighborY == gridY)
                    {
                        wallCount++;
                    }
                }
            }
        }
        return wallCount;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int a_x, int a_y)
        {
            tileX = a_x;
            tileY = a_y;
        }
    }

    class Lake
    {
        public bool flag = false; //Used in outside iterations
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Lake> conectedLakes;
        public int lakeSize;

        public Lake()
        {
        }

        public Lake(List<Coord> a_tiles, int[,] map)
        {
            tiles = a_tiles;
            lakeSize = tiles.Count;
            conectedLakes = new List<Lake>();

            edgeTiles = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y < tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }
        public static void ConectLakes(Lake a_roomA, Lake a_roomB)
        {
            if (!a_roomA.IsConnected(a_roomB) && !a_roomB.IsConnected(a_roomA))
            {
                a_roomA.conectedLakes.Add(a_roomB);
                a_roomB.conectedLakes.Add(a_roomA);
            }
        }

        public bool IsConnected(Lake a_otherRoom)
        {
            return conectedLakes.Contains(a_otherRoom);
        }
    }

    private void RandomFilMapWithMountains()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(map[x,y]==1)
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < (int)(generatorParameters.mountainFilPercent * (float)x/width)) ? 2 : 1;
                }
            }
        }
    }

    void SmoothMountains(int iterations)
    {
        if (iterations == 0) return;

        iterations--;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] != 0)
                {
                    int neighbourMountainTiles = CountSurroundingMountainTiles(x, y);

                    if (neighbourMountainTiles > generatorParameters.mountainCountThreshold)
                    {
                        if (generatorParameters.useMapBacklog)
                        {
                            mapBackLog[x, y] = 2;
                        }
                        else
                        {
                            map[x, y] = 2;
                        }
                    }
                    else if (neighbourMountainTiles < generatorParameters.mountainCountThreshold)
                    {
                        if (generatorParameters.useMapBacklog)
                        {
                            mapBackLog[x, y] = 1;
                        }
                        else
                        {
                            map[x, y] = 1;
                        }
                    }
                }
            }
        }

        if (generatorParameters.useMapBacklog)
        {
            ApplyBacklog();
        }
        SmoothMountains(iterations);
    }

    int CountSurroundingMountainTiles(int gridX, int gridY)
    {
        for (int neighborX = gridX - generatorParameters.moytainDistanceToRivers; neighborX <= gridX + generatorParameters.moytainDistanceToRivers; neighborX++)
        {
            for (int neighborY = gridY - generatorParameters.moytainDistanceToRivers; neighborY <= gridY + generatorParameters.moytainDistanceToRivers; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (map[neighborX, neighborY] == 0) return 0;
                }
            }
        }

        int wallCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        if(map[neighborX,neighborY] == 2)
                            wallCount += 1;
                    }
                }
            }
        }
        return wallCount;
    }

    private void SaveBacklog()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                mapBackLog[x, y] = map[x, y];
            }
        }
    }

    private void RandomFillMapWithResources()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    map[x, y] = (pseudoRandom.Next(0, 100) < (int)(generatorParameters.mountainFilPercent * (float)x / width)) ? 3 : 1;
                }
            }
        }
    }

    void SmoothResources(int iterations)
    {
        if (iterations == 0) return;

        iterations--;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1 || map[x, y] == 3)
                {

                    int neighbourMountainTiles = CountSurroundingResources(x, y);

                    if (neighbourMountainTiles > generatorParameters.resourceCountThreshold)
                    {
                        if (generatorParameters.useMapBacklog)
                        {
                            mapBackLog[x, y] = 3;
                        }
                        else
                        {
                            map[x, y] = 3;
                        }
                    }
                    else if (neighbourMountainTiles < generatorParameters.resourceCountThreshold)
                    {
                        if (generatorParameters.useMapBacklog)
                        {
                            mapBackLog[x, y] = 1;
                        }
                        else
                        {
                            map[x, y] = 1;
                        }
                    }


                }
            }
        }

        if (generatorParameters.useMapBacklog)
        {
            ApplyBacklog();
        }
        SmoothResources(iterations);
    }

    private int CountSurroundingResources(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX != gridX || neighborY != gridY)
                    {
                        if (map[neighborX, neighborY] == 0 || /*map[neighborX, neighborY] == 2 ||*/ map[neighborX, neighborY] == 3)
                            wallCount += 1;
                    }
                }
            }
        }
        return wallCount;
    }

    private void SetVariation(int gridX, int gridY, MapData[,] typeMap)
    {
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (typeMap[neighborX, neighborY].tileType == GameTileContentType.Water)
                    {
                        typeMap[gridX, gridY].variation = (int)ResourceType.Forest;
                        return;
                    }
                    if (typeMap[neighborX, neighborY].tileType == GameTileContentType.Mountain)
                    {
                        typeMap[gridX, gridY].variation = (int)ResourceType.Metal;
                        return;
                    }
                }
            }
        }
        for (int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++)
        {
            for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++)
            {
                if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
                {
                    if (neighborX == gridX && neighborY == gridY) continue;
                    if (typeMap[neighborX, neighborY].tileType == GameTileContentType.Resource)
                    {
                        typeMap[gridX, gridY].variation = typeMap[neighborX, neighborY].variation;
                        return;
                    }
                }
            }
        }
        typeMap[gridX, gridY].variation = (int)ResourceType.Crystal;
    }
}
