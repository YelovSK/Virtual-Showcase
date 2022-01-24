using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ElementValues : MonoBehaviour
{
    public TMP_Dropdown webcamDropdown;
    public TMP_Dropdown smoothingDropdown;
    public GameObject faceTracking;
    private GameObject _faceTrackingInstance;

    void Start()
    {
        // set to values in GlobalVars
        SetCamName(GlobalVars.cam.name);
        SetSmoothingOption(GlobalVars.smoothing);

        ChangeCamPreview(webcamDropdown);
        webcamDropdown.onValueChanged.AddListener(delegate
        {
            ChangeCamPreview(webcamDropdown);
        });
        smoothingDropdown.onValueChanged.AddListener(delegate
        {
            ChangeSmoothing(smoothingDropdown);
        });
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
    }

    private void OnDestroy()
    {
        if (_faceTrackingInstance != null)
            Destroy(_faceTrackingInstance);
    }
}
