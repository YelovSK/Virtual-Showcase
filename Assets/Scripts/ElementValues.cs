using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementValues : MonoBehaviour
{
    public TMP_Dropdown webcamDropdown;
    public TMP_Dropdown smoothingDropdown;
    public GameObject faceTracking;
    public TMP_Text currentModelText;
    public GameObject averageElements;
    private Slider avgSlider;
    private TMP_Text avgText;
    public GameObject kalmanElements;
    private Slider qSlider;
    private Slider rSlider;
    private TMP_Text qValue;
    private TMP_Text rValue;
    private GameObject _faceTrackingInstance;

    void Start()
    {
        avgSlider = averageElements.GetComponentInChildren<Slider>(true);
        avgText = averageElements.GetComponentInChildren<TMP_Text>(true);
        var kalmanSliders = kalmanElements.GetComponentsInChildren<Slider>(true);
        qSlider = kalmanSliders[0];
        rSlider = kalmanSliders[1];
        var kalmanValues = kalmanElements.GetComponentsInChildren<TMP_Text>(true);
        qValue = kalmanValues[0];
        rValue = kalmanValues[1];
        // set to values in GlobalVars
        currentModelText.text = "Current model: " + GlobalVars.modelPath;
        SetCamName(GlobalVars.cam.name);
        SetSmoothingOption(GlobalVars.smoothing);
        SetAvgSliderAndText(GlobalVars.framesSmoothed);
        qSlider.value = GlobalVars.kalmanQ;
        rSlider.value = GlobalVars.kalmanR;
        
        ChangeCamPreview(webcamDropdown);
        webcamDropdown.onValueChanged.AddListener(delegate
        {
            ChangeCamPreview(webcamDropdown);
        });
        
        ChangeSmoothing(smoothingDropdown);
        smoothingDropdown.onValueChanged.AddListener(delegate
        {
            ChangeSmoothing(smoothingDropdown);
        });
        
        ChangeAvgFrames(avgSlider);
        avgSlider.onValueChanged.AddListener(delegate
        {
            ChangeAvgFrames(avgSlider);
        });
        
        ChangeQslider(qSlider);
        qSlider.onValueChanged.AddListener(delegate
        {
            ChangeQslider(qSlider);
        });
        
        ChangeRslider(rSlider);
        rSlider.onValueChanged.AddListener(delegate
        {
            ChangeRslider(rSlider);
        });
    }

    private void ChangeRslider(Slider slider)
    {
        GlobalVars.kalmanR = slider.value;
        rValue.text = slider.value.ToString();
    }

    private void ChangeQslider(Slider slider)
    {
        GlobalVars.kalmanQ = slider.value;
        qValue.text = slider.value.ToString();

    }

    private void SetAvgSliderAndText(int framesSmoothed)
    {
        avgSlider.value = framesSmoothed;
        avgText.text = framesSmoothed + " frames";
    }

    public void SetCamName(string camName)
    {
        webcamDropdown.options = GlobalVars.webcams
            .Select(cam => new TMP_Dropdown.OptionData() {text = cam.name})
            .ToList();
        webcamDropdown.value = webcamDropdown.options.FindIndex(option => option.text == camName);
    }

    public void SetSmoothingOption(string smoothingOption)
    {
        smoothingDropdown.AddOptions(GlobalVars.SmoothingOptionsArr.ToList());
        smoothingDropdown.value = smoothingDropdown.options.FindIndex(option => option.text == smoothingOption);
    }

    void ChangeCamPreview(TMP_Dropdown sender)
    {
        GlobalVars.cam = GlobalVars.webcams.Find(cam => cam.name == webcamDropdown.options[sender.value].text);
        if (_faceTrackingInstance != null)
            Destroy(_faceTrackingInstance);
        _faceTrackingInstance = Instantiate(faceTracking);
        _faceTrackingInstance.SetActive(true);
    }

    void ChangeSmoothing(TMP_Dropdown sender)
    {
        GlobalVars.smoothing = smoothingDropdown.options[sender.value].text;
        averageElements.SetActive(GlobalVars.smoothing == "Average");
        kalmanElements.SetActive(GlobalVars.smoothing == "Kalman");
    }
    
    private void ChangeAvgFrames(Slider slider)
    {
        GlobalVars.framesSmoothed = Convert.ToInt16(slider.value);
        avgText.text = slider.value + " frames";
    }

    private void OnDestroy()
    {
        if (_faceTrackingInstance != null)
            Destroy(_faceTrackingInstance);
    }
}
