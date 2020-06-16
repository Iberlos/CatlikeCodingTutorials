﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameTileContentType
{
    Invalid = -1,
    Empty = 0,
    Wall,
    Destination,
    Last
}

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

    private GameTileContentFactory originFactory;

    public GameTileContentType Type => type;

    public void Recycle()
    {
        originFactory.Reclaim(this);
    }
}
