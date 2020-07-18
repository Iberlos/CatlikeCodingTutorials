using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buildable : Demolishable
{
    [SerializeField]
    private int health = 1;
    [SerializeField]
    private ResourceWallet.SpendingSum spendingSum;
    [SerializeField, Range(0f, 100f)]
    private float recyclingPercentage = 50;
    [SerializeField]
    private AudioClip buildSound = default;
    [SerializeField]
    private AudioClip damageSound = default;
    [SerializeField]
    private AudioClip destroySound = default;

    protected AudioSource audioSource;

    public override void Demolish()
    {
        Game.instance.wallet.SpendResources(spendingSum.RecycledByPercentage(recyclingPercentage/100f));
        PlayBuildableSound(destroySound);
    }

    public bool Build()
    {
        if(Game.instance.wallet.RequestResource(spendingSum))
        {
            Game.instance.wallet.SpendResources(spendingSum);
            PlayBuildableSound(buildSound);
            return true;
        }
        return false;
    }

    public void TakeDamage(GameBoard board, GameTile tile)
    {
        health--;
        PlayBuildableSound(damageSound);
        Destination d = this as Destination;
        if (d != null && d.destinationType == DestinationType.Capital)
            Game.instance.playerHealth--;
        else if (health <=0)
        {
            PlayBuildableSound(destroySound);
            Game.instance.board.Demolish(tile);
        }
    }

    private void PlayBuildableSound(AudioClip clip)
    {
        if(audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.clip = clip;
        audioSource.loop = false;
        audioSource.Play();
    }

    public virtual void Clicked()
    {

    }
}
