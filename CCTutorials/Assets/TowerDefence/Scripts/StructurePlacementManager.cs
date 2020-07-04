﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePlacementManager : GameBehavior
{
    private enum PlacementMode { Single = 0, Line, Square};

    private GameTileContentType constructionType;
    private int variationConstructionType;
    private GameTile lastTileConstructed;

    public override bool GameUpdate()
    {
        //Sets the display mesh position and color to show placement and if it is possible to build.
        return true;
    }

    public void InitiatePlacement(GameTile tile, GameBoard board)
    {
        if (tile != null && tile != lastTileConstructed)
        {
            bool placedSomething = false;
            switch (constructionType)
            {
                case GameTileContentType.Empty:
                    {
                        placedSomething = board.Demolish(tile);
                        break;
                    }
                case GameTileContentType.Wall:
                    {
                        placedSomething = board.PlaceWall(tile);
                        break;
                    }
                case GameTileContentType.Destination:
                    {
                        placedSomething = board.PlaceDestination(tile);
                        break;
                    }
                case GameTileContentType.Tower:
                    {
                        placedSomething = board.PlaceTower(tile, (TowerType)variationConstructionType);
                        break;
                    }
            }
            if (placedSomething) lastTileConstructed = tile;
        }
    }

    public void ConfirmPlacement()
    {
        lastTileConstructed = null;
    }

    public void CancelPlacement()
    {
        constructionType = GameTileContentType.Last;
        variationConstructionType = 0;
        lastTileConstructed = null;
    }
    public void ClearConstructionType()
    {
        CancelPlacement();
    }

    public void SetConstructionTypeAs(int value)
    {
        constructionType = (GameTileContentType)value;
    }

    public void SetConstructionVariationAs(int value)
    {
        variationConstructionType = value;
    }
    
    public override void Recycle()
    {
        throw new System.NotImplementedException();
    }
}
