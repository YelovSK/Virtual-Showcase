using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowThresholdValue : MonoBehaviour
{
    public TMP_Text thresholdText;
    public Slider thresholdSlider;
    // Start is called before the first frame update
    void Start()
    {
        thresholdText.text = Mathf.RoundToInt(GlobalVars.threshold * 100) + "%";
        thresholdSlider.value = GlobalVars.threshold;
    }

    // Update is called once per frame
    public void SliderUpdate(float value)
    {
        thresholdText.text = Mathf.RoundToInt(value * 100) + "%";
        GlobalVars.threshold = value;
    }
}
