using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShowThresholdValue : MonoBehaviour
{
    TMP_Text thresholdText;
    // Start is called before the first frame update
    void Start()
    {
        thresholdText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    public void textUpdate(float value)
    {
        thresholdText.text = Mathf.RoundToInt(value * 100) + "%";
    }
}
