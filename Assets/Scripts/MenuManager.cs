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
    [SerializeField] Slider thresholdSlider;
    [SerializeField] TMP_Text thresholdText;
    [SerializeField] Slider hueSlider;
    [SerializeField] TMP_Text hueText;
    [SerializeField] Slider hueThreshSlider;
    [SerializeField] TMP_Text hueThreshText;
    [SerializeField] GameObject faceTracking;
    Slider _avgSlider;
    TMP_Text _avgText;
    Slider _qSlider;
    Slider _rSlider;
    TMP_Text _qValue;
    TMP_Text _rValue;
    GameObject _faceTrackingInstance;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetDefaultPrefs();
        SetElementsToPlayerPrefs();
        SetDelegates();
    }

    private void SetDefaultPrefs()
    {
        if (!PlayerPrefs.HasKey("smoothing"))
            PlayerPrefs.SetString("smoothing", "Kalman");   // Kalman, Average, Off
        if (!PlayerPrefs.HasKey("cam"))
            PlayerPrefs.SetString("cam", WebCamTexture.devices.First().name);
        if (!PlayerPrefs.HasKey("threshold"))
            PlayerPrefs.SetFloat("threshold", 0.5f);    // 0.0 - 1.0
        if (!PlayerPrefs.HasKey("hue"))
            PlayerPrefs.SetInt("hue", 240);    // 0 - 360
        if (!PlayerPrefs.HasKey("hueThresh"))
            PlayerPrefs.SetInt("hueThresh", 20);    // 1 - 100
        if (!PlayerPrefs.HasKey("modelPath"))
            PlayerPrefs.SetString("modelPath", "");
        if (!PlayerPrefs.HasKey("previewIx"))
            PlayerPrefs.SetInt("previewIx", 0); // 0, 1, 2
        if (!PlayerPrefs.HasKey("framesSmoothed"))
            PlayerPrefs.SetInt("framesSmoothed", 8);   // 1-200
        if (!PlayerPrefs.HasKey("kalmanQ"))
            PlayerPrefs.SetFloat("kalmanQ", 0.002f);   // 1e-08 - 1e-02
        if (!PlayerPrefs.HasKey("kalmanR"))
            PlayerPrefs.SetFloat("kalmanR", 0.04f);  // 0.0001 - 0.5
        if (!PlayerPrefs.HasKey("stereo"))
            PlayerPrefs.SetInt("stereo", 0);
    }
    
    private void SetElementsToPlayerPrefs()
    {
        _avgSlider = averageElements.GetComponentInChildren<Slider>(true);
        _avgText = averageElements.GetComponentInChildren<TMP_Text>(true);
        var kalmanSliders = kalmanElements.GetComponentsInChildren<Slider>(true);
        _qSlider = kalmanSliders[0];
        _rSlider = kalmanSliders[1];
        var kalmanValues = kalmanElements.GetComponentsInChildren<TMP_Text>(true);
        _qValue = kalmanValues[0];
        _rValue = kalmanValues[1];
        if (System.IO.File.Exists(PlayerPrefs.GetString("modelPath")))
            currentModelText.text = "Current model: " + PlayerPrefs.GetString("modelPath").Split('\\').Last();
        else
            currentModelText.text = "Current model: ";
        SetCamName(PlayerPrefs.GetString("cam"));
        SetSmoothingOption(PlayerPrefs.GetString("smoothing"));
        SetAvgSliderAndText(PlayerPrefs.GetInt("framesSmoothed"));
        _qSlider.value = PlayerPrefs.GetFloat("kalmanQ");
        _rSlider.value = PlayerPrefs.GetFloat("kalmanR");
        thresholdSlider.value = PlayerPrefs.GetFloat("threshold");
        hueSlider.value = PlayerPrefs.GetInt("hue");
        hueThreshSlider.value = PlayerPrefs.GetInt("hueThresh");
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
        
        ChangeAvgFrames(_avgSlider);
        _avgSlider.onValueChanged.AddListener(delegate
        {
            ChangeAvgFrames(_avgSlider);
        });
        
        ChangeQslider(_qSlider);
        _qSlider.onValueChanged.AddListener(delegate
        {
            ChangeQslider(_qSlider);
        });
        
        ChangeRslider(_rSlider);
        _rSlider.onValueChanged.AddListener(delegate
        {
            ChangeRslider(_rSlider);
        });
        
        ChangeThreshold(thresholdSlider);
        thresholdSlider.onValueChanged.AddListener(delegate
        {
            ChangeThreshold(thresholdSlider);
        });
        
        ChangeHue(hueSlider);
        hueSlider.onValueChanged.AddListener(delegate
        {
            ChangeHue(hueSlider);
        });
        
        ChangeHueThresh(hueThreshSlider);
        hueThreshSlider.onValueChanged.AddListener(delegate
        {
            ChangeHueThresh(hueThreshSlider);
        });
    }
    
    private void ChangeThreshold(Slider sender)
    {
        thresholdText.text = Mathf.RoundToInt(sender.value * 100) + "%";
        PlayerPrefs.SetFloat("threshold", sender.value);
    }

    private void ChangeHue(Slider sender)
    {
        int hue = (int) sender.value;
        hueText.text = hue.ToString();
        var RGBcolor = Color.HSVToRGB((float) hue / 360, 1, 1);
        hueSlider.gameObject.transform.Find("Fill Area").Find("Fill").GetComponent<Image>().color =
            RGBcolor;
        PlayerPrefs.SetInt("hue", hue);
    }
    
    private void ChangeHueThresh(Slider sender)
    {
        int thresh = (int) sender.value;
        hueThreshText.text = thresh.ToString();
        PlayerPrefs.SetInt("hueThresh", thresh);
    }

    private void ChangeRslider(Slider slider)
    {
        PlayerPrefs.SetFloat("kalmanR", slider.value);
        _rValue.text = slider.value.ToString();
    }

    private void ChangeQslider(Slider slider)
    {
        PlayerPrefs.SetFloat("kalmanQ", slider.value);
        _qValue.text = slider.value.ToString();

    }

    private void SetAvgSliderAndText(int framesSmoothed)
    {
        _avgSlider.value = framesSmoothed;
        _avgText.text = framesSmoothed + " frames";
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
        _avgText.text = slider.value + " frames";
    }
    
    public void ResetSettings()
    {
        Destroy(StaticVars.loadedObject);
        StaticVars.loadedObject = null;
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
