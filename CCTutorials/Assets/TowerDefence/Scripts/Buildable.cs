using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildable : Demolishable
{
    [SerializeField]
    private ResourceWallet.SpendingSum spendingSum;
    [SerializeField, Range(0f, 100f)]
    private float recyclingPercentage = 50;

    public override void Demolish()
    {
        Game.instance.wallet.SpendResources(spendingSum.RecycledByPercentage(recyclingPercentage/100f));
    }

    public bool Build()
    {
        if(Game.instance.wallet.RequestResource(spendingSum))
        {
            Game.instance.wallet.SpendResources(spendingSum);
            return true;
        }
        return false;
    }
}
