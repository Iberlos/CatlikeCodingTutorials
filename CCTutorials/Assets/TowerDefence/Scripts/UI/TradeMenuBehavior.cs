using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeMenuBehavior : MonoBehaviour
{
    [SerializeField]
    private Slider slider = default;
    [SerializeField]
    private InputField resourceInputField = default;
    [SerializeField]
    private InputField goldInputField = default;
    [SerializeField]
    private Image resourceIcon = default;
    [SerializeField]
    private Text conversionText = default;
    [SerializeField]
    private Text resourceResultText = default;

    [Header("Source Images")]
    [SerializeField]
    private Sprite foodIcon = default;
    [SerializeField]
    private Sprite woodIcon = default;
    [SerializeField]
    private Sprite metalIcon = default;
    [SerializeField]
    private Sprite crystalIcon = default;

    private Transform target;
    private Destination destinationReference;

    public void Toggle(Transform target = null, bool state = true)
    {
        if (target != null) 
        {
            this.target = target;
            destinationReference = target.GetComponent<Destination>();
            UpdateVisuals(destinationReference.GoldConvesionPercentage);
        }
        gameObject.SetActive(state);
        Vector3 pos = Camera.main.WorldToScreenPoint(target != null ? target.position + Vector3.up : Vector3.zero);
        transform.position = pos;
    }

    private void Update()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(target != null? target.position + Vector3.up: Vector3.zero);
        transform.position = pos;
    }

    public void ModifyPercentage()
    {
        destinationReference.UpdateGoldConversionPercentage(slider.value);
        UpdateVisuals(slider.value);
    }

    public void ModifyResourceProduction()
    {
        float requestedValue;
        if(ParseStringToFloat(out requestedValue, resourceInputField.text))
        {
            float max = destinationReference.MaxResourceProduction;
            float newPercent = 1f - requestedValue / max;
            newPercent = newPercent > 1f ? 1f : (newPercent < -1 ? -1 : newPercent);
            UpdateVisuals(newPercent);
            ModifyPercentage();
        }
    }

    public void ModifyGoldProduction()
    {
        float requestedValue;
        if (ParseStringToFloat(out requestedValue, goldInputField.text))
        {
            float max = destinationReference.MaxResourceProduction;
            float newPercent = requestedValue / max;
            newPercent = newPercent > 1f ? 1f : (newPercent < -1 ? -1 : newPercent);
            UpdateVisuals(newPercent);
            ModifyPercentage();
        }
    }

    private bool ParseStringToFloat(out float value, string text)
    {
        bool negative = false;
        int periodIndex = -1;
        value = 0f;
        //check all the indexes for values not equal to numbers or a period or dash if you find it return false; Record the index in which the period was found
        for(int i =0; i < text.Length; i++)
        {
            if(text[i] != '0' && text[i] != '1' && text[i] != '2' && text[i] != '3' && text[i] != '4' && text[i] != '5' && text[i] != '6' && text[i] != '7' && text[i] != '8' && text[i] != '9')
            {
                if(text[i] == '.' && periodIndex == -1)
                {
                    periodIndex = i;
                    continue;
                }
                if (text[i] == '-' && !negative)
                {
                    negative = true;
                    continue;
                }
                return false;
            }
        }
        if (periodIndex == -1) periodIndex = text.Length - 1;
        for(int i = 0; i < text.Length; i++)
        {
            if (text[i] == '-' || text[i] == '.') continue;
            int power = periodIndex - i;
            power -= power >0 ? 1 : 0;
            float v = text[i] - '0';
            value += v * Mathf.Pow(10f, power);
        }
        if (negative) value *= -1;
        return true;
    }

    private void UpdateVisuals(float newPercent)
    {
        slider.value = newPercent;
        float max = destinationReference.MaxResourceProduction;
        (resourceInputField.placeholder as Text).text = ((1f - newPercent) * max).ToString();
        resourceInputField.text = "";
        (goldInputField.placeholder as Text).text = ((newPercent) * max * destinationReference.GatheringResource.GoldValue()).ToString();
        goldInputField.text = "";

        switch (destinationReference.GatheringResource)
        {
            case ResourceType.Food:
                {
                    resourceIcon.sprite = foodIcon;
                    conversionText.text = (ResourceType.Food.GoldValue()*100).ToString() + " Food/s = 1 Gold/s";
                    resourceResultText.text = "Food/s = ";
                    break;
                }
            case ResourceType.Wood:
                {
                    resourceIcon.sprite = woodIcon;
                    conversionText.text = (ResourceType.Wood.GoldValue() * 100).ToString() + " Wood/s = 1 Gold/s";
                    resourceResultText.text = "Wood/s = ";
                    break;
                }
            case ResourceType.Metal:
                {
                    resourceIcon.sprite = metalIcon;
                    conversionText.text = (ResourceType.Metal.GoldValue() * 100).ToString() + " Metal/s = 1 Gold/s";
                    resourceResultText.text = "Metal/s = ";
                    break;
                }
            case ResourceType.Crystal:
                {
                    resourceIcon.sprite = crystalIcon;
                    conversionText.text = (ResourceType.Crystal.GoldValue() * 100).ToString() + " Crystal/s = 1 Gold/s";
                    resourceResultText.text = "Crystal/s = ";
                    break;
                }
        }
    }
}
