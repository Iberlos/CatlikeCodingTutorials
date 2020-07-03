using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePlacementManager : GameBehavior
{
    private enum PlacementMode { Single = 0, Line, Square};

    private GameTileContentType constructionType;
    private int variationConstructionType;
    private List<int> tilesToConstructOn;

    public override bool GameUpdate()
    {
        //Sets the display mesh position and color to show placement and if it is possible to build.
        return true;
    }

    public void InitiateBuild()
    {

    }

    public void ConfirmBuild()
    {

    }

    public void CancelBuild()
    {

    }

    public void SetConstructionTypeAs()
    {

    }

    public void ClearConstructionType()
    {
        CancelBuild();
    }

    public override void Recycle()
    {
        throw new System.NotImplementedException();
    }
}
