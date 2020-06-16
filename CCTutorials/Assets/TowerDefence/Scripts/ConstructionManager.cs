using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstructionManager : MonoBehaviour
{
    public GameTile LastConstructedTile
    {
        get => lastConstructedTile;
        set
        {
            if(value != null)
            {
                lastConstructedTile = value;
            }
            else
            {
                Debug.LogError("Use the ClearConstruction function to reset the LastConstructedTile.");
            }
        }
    }
    private GameTile lastConstructedTile;

    public GameTileContentType ConstructionType
    {
        get => constructionType;
    }
    private GameTileContentType constructionType = GameTileContentType.Invalid;

    public void SetContentTypeForConstruction(int type)
    {
        constructionType = (GameTileContentType)type;
    }

    public void ClearConsnstruction()
    {
        lastConstructedTile = null;
    }
}
