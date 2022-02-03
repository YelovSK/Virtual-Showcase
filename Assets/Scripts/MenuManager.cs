using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TMP_Dropdown webcamDropdown;
    [SerializeField] TMP_Dropdown smoothingDropdown;
    [SerializeField] TMP_Text currentModelText;
    [SerializeField] GameObject averageElements;
    [SerializeField] GameObject kalmanElements;
    [SerializeField] TMP_Text thresholdText;
    [SerializeField] Slider thresholdSlider;
    [SerializeField] GameObject faceTracking;
    Slider avgSlider;
    TMP_Text avgText;
    Slider qSlider;
    Slider rSlider;
    TMP_Text qValue;
    TMP_Text rValue;
    GameObject _faceTrackingInstance;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetDefaultPrefs();
        SetElementsToPlayerPrefs();
        SetDelegates();
    }

    private void ChangeThreshold(Slider sender)
    {
        thresholdText.text = Mathf.RoundToInt(sender.value * 100) + "%";
        PlayerPrefs.SetFloat("threshold", sender.value);
    }

    private void SetDefaultPrefs()
    {
        if (!PlayerPrefs.HasKey("smoothing"))
            PlayerPrefs.SetString("smoothing", "Kalman");
        if (!PlayerPrefs.HasKey("cam"))
            PlayerPrefs.SetString("cam", WebCamTexture.devices.ToList().First().name);
        if (!PlayerPrefs.HasKey("threshold"))
            PlayerPrefs.SetFloat("threshold", 0.5f);
        if (!PlayerPrefs.HasKey("modelPath"))
            PlayerPrefs.SetString("modelPath", "");
        if (!PlayerPrefs.HasKey("previewIx"))
            PlayerPrefs.SetInt("previewIx", 0);
        if (!PlayerPrefs.HasKey("framesSmoothed"))
            PlayerPrefs.SetInt("framesSmoothed", 30);
        if (!PlayerPrefs.HasKey("kalmanQ"))
            PlayerPrefs.SetFloat("kalmanQ", 0.0001f);
        if (!PlayerPrefs.HasKey("kalmanR"))
            PlayerPrefs.SetFloat("kalmanR", 0.1f);
    }
    
    private void SetElementsToPlayerPrefs()
    {
        avgSlider = averageElements.GetComponentInChildren<Slider>(true);
        avgText = averageElements.GetComponentInChildren<TMP_Text>(true);
        var kalmanSliders = kalmanElements.GetComponentsInChildren<Slider>(true);
        qSlider = kalmanSliders[0];
        rSlider = kalmanSliders[1];
        var kalmanValues = kalmanElements.GetComponentsInChildren<TMP_Text>(true);
        qValue = kalmanValues[0];
        rValue = kalmanValues[1];
        if (System.IO.File.Exists(PlayerPrefs.GetString("modelPath")))
            currentModelText.text = "Current model: " + PlayerPrefs.GetString("modelPath").Split('\\').Last();
        else
            currentModelText.text = "Current model: ";
        SetCamName(PlayerPrefs.GetString("cam"));
        SetSmoothingOption(PlayerPrefs.GetString("smoothing"));
        SetAvgSliderAndText(PlayerPrefs.GetInt("framesSmoothed"));
        qSlider.value = PlayerPrefs.GetFloat("kalmanQ");
        rSlider.value = PlayerPrefs.GetFloat("kalmanR");
        thresholdText.text = Mathf.RoundToInt(PlayerPrefs.GetFloat("threshold") * 100) + "%";
        thresholdSlider.value = PlayerPrefs.GetFloat("threshold");
    }
    
    private void SetDelegates()
    {
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
        
        ChangeThreshold(thresholdSlider);
        thresholdSlider.onValueChanged.AddListener(delegate
        {
            ChangeThreshold(thresholdSlider);
        });
    }

    private void ChangeRslider(Slider slider)
    {
        PlayerPrefs.SetFloat("kalmanR", slider.value);
        rValue.text = slider.value.ToString();
    }

    private void ChangeQslider(Slider slider)
    {
        PlayerPrefs.SetFloat("kalmanQ", slider.value);
        qValue.text = slider.value.ToString();

    }

    private void SetAvgSliderAndText(int framesSmoothed)
    {
        avgSlider.value = framesSmoothed;
        avgText.text = framesSmoothed + " frames";
    }

    public void SetCamName(string camName)
    {
        if (webcamDropdown.options.Count == 0)
            webcamDropdown.options = WebCamTexture.devices.ToList()
                .Select(cam => new TMP_Dropdown.OptionData() {text = cam.name})
                .ToList();
        webcamDropdown.value = webcamDropdown.options.FindIndex(option => option.text == camName);
    }

    public void SetSmoothingOption(string smoothingOption)
    {
        if (smoothingDropdown.options.Count == 0)
            smoothingDropdown.AddOptions(new List<string>{"Kalman", "Average", "Off"});
        smoothingDropdown.value = smoothingDropdown.options.FindIndex(option => option.text == smoothingOption);
    }

    void ChangeCamPreview(TMP_Dropdown sender)
    {
        PlayerPrefs.SetString("cam", webcamDropdown.options[sender.value].text);
        if (_faceTrackingInstance != null)
            Destroy(_faceTrackingInstance);
        _faceTrackingInstance = Instantiate(faceTracking);
        _faceTrackingInstance.SetActive(true);
    }

    void ChangeSmoothing(TMP_Dropdown sender)
    {
        PlayerPrefs.SetString("smoothing", smoothingDropdown.options[sender.value].text);
        averageElements.SetActive(PlayerPrefs.GetString("smoothing") == "Average");
        kalmanElements.SetActive(PlayerPrefs.GetString("smoothing") == "Kalman");
    }
    
    private void ChangeAvgFrames(Slider slider)
    {
        PlayerPrefs.SetInt("framesSmoothed", Convert.ToInt16(slider.value));
        avgText.text = slider.value + " frames";
    }
    
    public void ResetSettings()
    {
        PlayerPrefs.DeleteAll();
        SetDefaultPrefs();
        SetElementsToPlayerPrefs();
    }

    private void OnDestroy()
    {
        if (_faceTrackingInstance != null)
            Destroy(_faceTrackingInstance);
    }
}
