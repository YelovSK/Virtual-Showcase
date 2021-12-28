using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ElementValues : MonoBehaviour
{
    public TMP_Dropdown webcamDropdown;
    public TMP_Dropdown smoothingDropdown;
    public Slider thresholdSlider;
    public AspectRatioFitter fitter;
    public GameObject faceTracking;
    GameObject faceTrackingInstance;

    void Start()
    {
        webcamDropdown.options.Clear();

        WebCamDevice[] webcams = WebCamTexture.devices;
        List<string> webcamNames = new List<string>();
        foreach (WebCamDevice webcam in webcams)
        {
            webcamNames.Add(webcam.name);
        }
        foreach (var webcamName in webcamNames)
        {
            webcamDropdown.options.Add(new TMP_Dropdown.OptionData() { text = webcamName });
        }
        // set to values in GlobalVars
        setCamName();
        setSmoothingOption();
        setThreshold();

        changeCamPreview(webcamDropdown);
        webcamDropdown.onValueChanged.AddListener(delegate
        {
            changeCamPreview(webcamDropdown);
        });
        smoothingDropdown.onValueChanged.AddListener(delegate
        {
            changeSmoothing(smoothingDropdown);
        });
        thresholdSlider.onValueChanged.AddListener(delegate
        {
            changeThreshold(thresholdSlider);
        });
    }

    public string getCamName()
    {
        return webcamDropdown.options[webcamDropdown.value].text;
    }

    public void setCamName()
    {
        webcamDropdown.value = webcamDropdown.options.FindIndex(option => option.text == GlobalVars.camName);
    }

    public string getSmoothingOption()
    {
        return smoothingDropdown.options[smoothingDropdown.value].text;
    }

    public void setSmoothingOption()
    {
        smoothingDropdown.value = smoothingDropdown.options.FindIndex(option => option.text == GlobalVars.smoothing);
    }

    public float getThreshold()
    {
        return thresholdSlider.value;
    }

    public void setThreshold()
    {
        thresholdSlider.value = GlobalVars.threshold;
    }

    void changeCamPreview(TMP_Dropdown sender)
    {
        GlobalVars.camName = webcamDropdown.options[sender.value].text;
        if (faceTrackingInstance != null)
        {
            Destroy(faceTrackingInstance);
        }
        faceTrackingInstance = Instantiate(faceTracking);
        faceTrackingInstance.SetActive(true);
    }

    void changeSmoothing(TMP_Dropdown sender)
    {
        GlobalVars.smoothing = smoothingDropdown.options[sender.value].text;
    }

    void changeThreshold(Slider sender)
    {
        GlobalVars.threshold = sender.value;
    }

    private void OnDestroy()
    {
        if (faceTrackingInstance != null)
            Destroy(faceTrackingInstance);
    }
}
