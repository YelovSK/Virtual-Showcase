using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowThresholdValue : MonoBehaviour
{
    public TMP_Text thresholdText;
    public Slider thresholdSlider;

    void Start()
    {
        thresholdText.text = Mathf.RoundToInt(GlobalVars.threshold * 100) + "%";
        thresholdSlider.value = GlobalVars.threshold;
    }

    public void SliderUpdate(float value)
    {
        thresholdText.text = Mathf.RoundToInt(value * 100) + "%";
        GlobalVars.threshold = value;
    }
}
