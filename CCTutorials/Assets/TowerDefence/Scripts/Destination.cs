using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destination : Buildable
{
    public DestinationType destinationType;
    public override int Variation => (int)destinationType;

    private ResourceType gatheringResource = ResourceType.Forest;
    private int gatheringResourceCount = 0;
    private float eficiency = 0.1f;
    private int reach = 1;

    public override void Adapt(GameTile tile)
    {
        int groundCount = 0, forestCount = 0, metalCount = 0, crystalCount = 0, waterCount = 0;
        List<GameTile> neighbours = new List<GameTile>();
        int lastAdded = AddNeighbours(neighbours, tile);
        for(int r = reach; r >1; r--)
        {
            int currentLastIndex = neighbours.Count - 1;
            int addedThisLoop = 0;
            for (int i = currentLastIndex; i > currentLastIndex - lastAdded; i --)
            {
                addedThisLoop += AddNeighbours(neighbours, neighbours[i]);
            }
            lastAdded = addedThisLoop;
        }
        for(int i = 0; i < neighbours.Count; i++)
        {
            if(neighbours[i].Content.Type == GameTileContentType.Ground)
            {
                groundCount++;
            } else if(neighbours[i].Content.Type == GameTileContentType.Water)
            {
                waterCount++;
            }
            else if (neighbours[i].Content.Type == GameTileContentType.Resource)
            {
                switch(((Resource)(neighbours[i].Content)).resourceType)
                {
                    case ResourceType.Forest:
                        {
                            forestCount++;
                            break;
                        }
                    case ResourceType.Metal:
                        {
                            metalCount++;
                            break;
                        }
                    case ResourceType.Crystal:
                        {
                            crystalCount++;
                            break;
                        }
                }
            }
        }
        int gatheringResourceCount;
        ResourceType gatheringResource;
        if (destinationType == DestinationType.Farm)
        {
            gatheringResourceCount = groundCount;
            gatheringResource = ResourceType.Food;
        }
        else
        {
            gatheringResourceCount = waterCount;
            gatheringResource = ResourceType.Food;
            if (forestCount > gatheringResourceCount)
            {
                gatheringResourceCount = forestCount;
                gatheringResource = ResourceType.Forest;
            }
            if (metalCount > gatheringResourceCount)
            {
                gatheringResourceCount = metalCount;
                gatheringResource = ResourceType.Metal;
            }
            if (crystalCount > gatheringResourceCount)
            {
                gatheringResourceCount = crystalCount;
                gatheringResource = ResourceType.Crystal;
            }
        }

        Game.instance.wallet.UpdateResourceGeneration(this.gatheringResource, gatheringResource, this.gatheringResourceCount*eficiency, gatheringResourceCount*eficiency);

        this.gatheringResourceCount = gatheringResourceCount;
        this.gatheringResource = gatheringResource;
    }

    private int AddNeighbours(List<GameTile> neighbourList, GameTile tile)
    {
        int added = 0;
        if(!neighbourList.Contains(tile.north))
        {
            added++;
            neighbourList.Add(tile.north);
        }
        if (!neighbourList.Contains(tile.northEast))
        {
            added++;
            neighbourList.Add(tile.northEast);
        }
        if (!neighbourList.Contains(tile.east))
        {
            added++;
            neighbourList.Add(tile.east);
        }
        if (!neighbourList.Contains(tile.southEast))
        {
            added++;
            neighbourList.Add(tile.southEast);
        }
        if (!neighbourList.Contains(tile.south))
        {
            added++;
            neighbourList.Add(tile.south);
        }
        if (!neighbourList.Contains(tile.southWest))
        {
            added++;
            neighbourList.Add(tile.southWest);
        }
        if (!neighbourList.Contains(tile.west))
        {
            added++;
            neighbourList.Add(tile.west);
        }
        if (!neighbourList.Contains(tile.northWest))
        {
            added++;
            neighbourList.Add(tile.northWest);
        }
        return added;
    }

    public override void Clicked()
    {
        Game.EnableTradeMenu(transform);
    }
}
