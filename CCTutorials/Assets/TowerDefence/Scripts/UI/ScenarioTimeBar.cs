using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioTimeBar : MonoBehaviour
{
    [SerializeField]
    private Image fill;
    [SerializeField]
    private Text counterText;

    public void UpdateTimeBar(float percentageTimeRemaining, string text)
    {
        Vector3 scale = fill.transform.localScale;
        if (percentageTimeRemaining == -1f)
        {
            text = "Wave in Progress!";
            scale.x = 1f;
            fill.transform.localScale = scale;
            fill.color = Color.red;
        }
        scale.x = percentageTimeRemaining;
        fill.transform.localScale = scale;
        fill.color = Color.white;
        counterText.text = text;
    }
}
