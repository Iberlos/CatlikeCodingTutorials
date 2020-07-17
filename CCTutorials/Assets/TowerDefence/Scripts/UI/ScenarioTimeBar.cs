﻿using System.Collections;
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
        scale.x = percentageTimeRemaining;
        fill.transform.localScale = scale;
        fill.color = Color.white;
        counterText.text = text;
    }
}
