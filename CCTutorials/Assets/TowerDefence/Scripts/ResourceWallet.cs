using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceWallet : GameBehavior
{
    [SerializeField]
    int startingFood = 0;
    [SerializeField]
    int startingWood = 0;
    [SerializeField]
    int startingMetal = 0;
    [SerializeField]
    int startingCrystal = 0;
    [SerializeField]
    int startingGold = 0;

    [Header("UI")]
    [SerializeField]
    Text foodCounter = default;
    [SerializeField]
    Text woodCounter = default;
    [SerializeField]
    Text metalCounter = default;
    [SerializeField]
    Text crystalCounter = default;
    [SerializeField]
    Text goldCounter = default;

    public float Food { get; private set; }
    public float Wood { get; private set; }
    public float Metal { get; private set; }
    public float Crystal { get; private set; }
    public float Gold { get; private set; }

    private float foodGeneratedPerSecond = 0;
    private float woodGeneratedPerSecond = 0;
    private float metalGeneratedPerSecond = 0;
    private float crystalGeneratedPerSecond = 0;
    private float goldGeneratedPerSecond = 0;

    public override bool GameUpdate()
    {
        Food += foodGeneratedPerSecond * Time.deltaTime;
        Wood += woodGeneratedPerSecond * Time.deltaTime;
        Metal += metalGeneratedPerSecond * Time.deltaTime;
        Crystal += crystalGeneratedPerSecond * Time.deltaTime;
        Gold += goldGeneratedPerSecond * Time.deltaTime;
        foodCounter.text = ((int)Food).ToString();
        woodCounter.text = ((int)Wood).ToString();
        metalCounter.text = ((int)Metal).ToString();
        crystalCounter.text = ((int)Crystal).ToString();
        goldCounter.text = ((int)Gold).ToString();
        return true;
    }

    public void UpdateResourceGeneration(ResourceType previous, ResourceType current, float previousGeneration, float currentGeneration)
    {
        switch (previous)
        {
            case ResourceType.Food:
                {
                    foodGeneratedPerSecond -= previousGeneration;
                    break;
                }
            case ResourceType.Forest:
                {
                    woodGeneratedPerSecond -= previousGeneration;
                    break;
                }
            case ResourceType.Metal:
                {
                    metalGeneratedPerSecond -= previousGeneration;
                    break;
                }
            case ResourceType.Crystal:
                {
                    crystalGeneratedPerSecond -= previousGeneration;
                    break;
                }
            case ResourceType.Gold:
                {
                    goldGeneratedPerSecond -= previousGeneration;
                    break;
                }
        }
        switch (current)
        {
            case ResourceType.Food:
                {
                    foodGeneratedPerSecond += currentGeneration;
                    break;
                }
            case ResourceType.Forest:
                {
                    woodGeneratedPerSecond += currentGeneration;
                    break;
                }
            case ResourceType.Metal:
                {
                    metalGeneratedPerSecond += currentGeneration;
                    break;
                }
            case ResourceType.Crystal:
                {
                    crystalGeneratedPerSecond += currentGeneration;
                    break;
                }
            case ResourceType.Gold:
                {
                    goldGeneratedPerSecond += currentGeneration;
                    break;
                }
        }
    }

    public override void Recycle()
    {
        Food = 0; 
        Wood = 0; 
        Metal = 0; 
        Crystal = 0; 
        Gold = 0; 
        foodGeneratedPerSecond = 0;
        woodGeneratedPerSecond = 0;
        metalGeneratedPerSecond = 0;
        crystalGeneratedPerSecond = 0;
        goldGeneratedPerSecond = 0;
        foodCounter.text = ((int)Food).ToString();
        woodCounter.text = ((int)Wood).ToString();
        metalCounter.text = ((int)Metal).ToString();
        crystalCounter.text = ((int)Crystal).ToString();
        goldCounter.text = ((int)Gold).ToString();
    }
}
