using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : Demolishable
{
    public ResourceType resourceType;
    public override int Variation => (int)resourceType;

    public override void Demolish()
    {
        Game.instance.wallet.SpendResources((new ResourceWallet.SpendingSum(resourceType, 5)).RecycledByPercentage(1));
    }
}
