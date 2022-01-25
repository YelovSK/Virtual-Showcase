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
    public Slider avgSlider;
    public TMP_Text avgText;
    private GameObject _faceTrackingInstance;

    void Start()
    {
        // set to values in GlobalVars
        currentModelText.text = "Current model: " + GlobalVars.modelPath;
        SetCamName(GlobalVars.cam.name);
        SetSmoothingOption(GlobalVars.smoothing);
        SetAvgSliderAndText(GlobalVars.framesSmoothed);
        
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
        if (GlobalVars.smoothing == "Average")
        {
            avgSlider.gameObject.SetActive(true);
            avgText.gameObject.SetActive(true);
        }
        else
        {
            avgSlider.gameObject.SetActive(false);
            avgText.gameObject.SetActive(false);
        }
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
