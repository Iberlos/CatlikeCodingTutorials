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
        UpdateVisuals();
        return true;
    }

    private void UpdateVisuals()
    {
        foodCounter.text = ((int)Food).ToString();
        woodCounter.text = ((int)Wood).ToString();
        metalCounter.text = ((int)Metal).ToString();
        crystalCounter.text = ((int)Crystal).ToString();
        goldCounter.text = ((int)Gold).ToString();
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
        Food = startingFood; 
        Wood = startingWood; 
        Metal = startingMetal; 
        Crystal = startingCrystal; 
        Gold = startingGold; 
        foodGeneratedPerSecond = 0;
        woodGeneratedPerSecond = 0;
        metalGeneratedPerSecond = 0;
        crystalGeneratedPerSecond = 0;
        goldGeneratedPerSecond = 0;
        UpdateVisuals();
    }

    public bool RequestResource(SpendingSum spendingSum)
    {
        if(Food >= spendingSum.food && Wood >= spendingSum.wood && Metal >= spendingSum.metal && Crystal >= spendingSum.crystal && Gold >= spendingSum.gold)
        {
            return true;
        }
        return false;
    }

    public void SpendResources(SpendingSum spendingSum)
    {
        Food -= spendingSum.food;
        Wood -= spendingSum.wood;
        Metal -= spendingSum.metal;
        Crystal -= spendingSum.crystal;
        Gold -= spendingSum.gold;
        UpdateVisuals();
    }

    [System.Serializable]
    public struct SpendingSum
    {
        public int food, wood, metal, crystal, gold;
        public SpendingSum(int food, int wood, int metal, int crystal, int gold)
        {
            this.food = food;
            this.wood = wood;
            this.metal = metal;
            this.crystal = crystal;
            this.gold = gold;
        }

        public SpendingSum(ResourceType type, int amount)
        {
            switch (type)
            {
                case ResourceType.Food:
                    {
                        food = amount;
                        wood = metal = crystal = gold = 0;
                        break;
                    }
                case ResourceType.Forest:
                    {
                        wood = amount;
                        food = metal = crystal = gold = 0;
                        break;
                    }
                case ResourceType.Metal:
                    {
                        metal = amount;
                        wood = food = crystal = gold = 0;
                        break;
                    }
                case ResourceType.Crystal:
                    {
                        crystal = amount;
                        wood = metal = food = gold = 0;
                        break;
                    }
                case ResourceType.Gold:
                    {
                        gold = amount;
                        wood = metal = crystal = food = 0;
                        break;
                    }
                default:
                    {
                        food = wood = metal = crystal = gold = 0;
                        break;
                    }
            }
        }

        public SpendingSum RecycledByPercentage(float recycleFraction)
        {
            return new SpendingSum((int)(-food * recycleFraction), (int)(-wood * recycleFraction), (int)(-metal * recycleFraction), (int)(-crystal * recycleFraction), 0);
        }
    }
}
