﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour
{
    [SerializeField]
    private GameTileContentType type = default;

    public GameTileContentFactory OriginFactory
    {
        get => originFactory;
        set
        {
            Debug.Assert(originFactory == null, "Redefined origin factory");
            originFactory = value;
        }
    }

    public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;

    private GameTileContentFactory originFactory;

    public GameTileContentType Type => type;

    public virtual void GameUpdate() { }

    public void Recycle()
    {
        originFactory.Reclaim(this);
    }

    public virtual void Adapt(GameTile tile)
    {
        //Basic tiles do nothing for now;
        Debug.LogError("GTC.Adapt");
    }
}
