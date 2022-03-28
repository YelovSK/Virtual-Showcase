using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.Core;

namespace VirtualVitrine.UI.Menu
{
    public class MenuManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private TMP_Dropdown webcamDropdown;
        [SerializeField] private TMP_Dropdown smoothingDropdown;
        [SerializeField] private TMP_Text currentModelText;
        [SerializeField] private GameObject averageElements;
        [SerializeField] private GameObject kalmanElements;
        [SerializeField] private Slider thresholdSlider;
        [SerializeField] private TMP_Text thresholdText;
        [SerializeField] private Slider hueSlider;
        [SerializeField] private TMP_Text hueText;
        [SerializeField] private Slider hueThreshSlider;
        [SerializeField] private TMP_Text hueThreshText;
        [SerializeField] private GameObject faceTracking;
        [SerializeField] private Toggle glassesCheck;
        #endregion
        
        #region Private Fields
        private Slider _avgSlider;
        private TMP_Text _avgText;
        private GameObject _faceTrackingInstance;
        private Slider _qSlider;
        private TMP_Text _qValue;
        private Slider _rSlider;
        private TMP_Text _rValue;
        #endregion
        
        #region Public Methods
        public void ResetSettings()
        {
            Destroy(StaticVars.loadedObject);
            StaticVars.loadedObject = null;
            StaticVars.ResetPlayerPrefs();
            SetElementsToPlayerPrefs();
        }
        
        public void SetCamName(string camName)
        {
            // add WebCam devices to dropdown options
            if (webcamDropdown.options.Count == 0)
            {
                var options = WebCamTexture.devices.Select(device => new TMP_Dropdown.OptionData(device.name)).ToList();
                webcamDropdown.AddOptions(options);
            }

            webcamDropdown.value = webcamDropdown.options.FindIndex(x => x.text == camName);
        }

        public void SetSmoothingOption(string smoothingOption)
        {
            if (smoothingDropdown.options.Count == 0)
                smoothingDropdown.AddOptions(new List<string> {"Kalman", "Average", "Off"});
            smoothingDropdown.value = smoothingDropdown.options.FindIndex(option => option.text == smoothingOption);
        }
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            StaticVars.CheckPlayerPrefs();
            SetElementsToPlayerPrefs();
            SetDelegates();
        }

        private void OnDestroy()
        {
            if (_faceTrackingInstance != null)
                Destroy(_faceTrackingInstance);
        }
        #endregion
        
        #region Private Methods
        private void SetElementsToPlayerPrefs()
        {
            // find components in children
            _avgSlider = averageElements.GetComponentInChildren<Slider>(true);
            _avgText = averageElements.GetComponentInChildren<TMP_Text>(true);
            var kalmanSliders = kalmanElements.GetComponentsInChildren<Slider>(true);
            _qSlider = kalmanSliders[0];
            _rSlider = kalmanSliders[1];
            var kalmanValues = kalmanElements.GetComponentsInChildren<TMP_Text>(true);
            _qValue = kalmanValues[0];
            _rValue = kalmanValues[1];
            // set to current player prefs
            if (File.Exists(PlayerPrefs.GetString("modelPath")))
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
            glassesCheck.isOn = PlayerPrefs.GetInt("glassesCheck") == 1;
        }

        private void SetDelegates()
        {
            ChangeCamPreview(webcamDropdown);
            webcamDropdown.onValueChanged.AddListener(delegate { ChangeCamPreview(webcamDropdown); });

            ChangeSmoothing(smoothingDropdown);
            smoothingDropdown.onValueChanged.AddListener(delegate { ChangeSmoothing(smoothingDropdown); });

            ChangeAvgFrames(_avgSlider);
            _avgSlider.onValueChanged.AddListener(delegate { ChangeAvgFrames(_avgSlider); });

            ChangeQslider(_qSlider);
            _qSlider.onValueChanged.AddListener(delegate { ChangeQslider(_qSlider); });

            ChangeRslider(_rSlider);
            _rSlider.onValueChanged.AddListener(delegate { ChangeRslider(_rSlider); });

            ChangeThreshold(thresholdSlider);
            thresholdSlider.onValueChanged.AddListener(delegate { ChangeThreshold(thresholdSlider); });

            ChangeHue(hueSlider);
            hueSlider.onValueChanged.AddListener(delegate { ChangeHue(hueSlider); });

            ChangeHueThresh(hueThreshSlider);
            hueThreshSlider.onValueChanged.AddListener(delegate { ChangeHueThresh(hueThreshSlider); });

            ChangeGlassesCheck(glassesCheck);
            glassesCheck.onValueChanged.AddListener(delegate { ChangeGlassesCheck(glassesCheck); });
        }

        private void ChangeThreshold(Slider sender)
        {
            thresholdText.text = Mathf.RoundToInt(sender.value * 100) + "%";
            PlayerPrefs.SetFloat("threshold", sender.value);
        }

        private void ChangeHue(Slider sender)
        {
            var hue = (int) sender.value;
            hueText.text = hue.ToString();
            var rgBcolor = Color.HSVToRGB((float) hue / 360, 1, 1);
            // fill hueSlider with given colour
            hueSlider.GetComponentInChildren<Image>().color = rgBcolor;
            PlayerPrefs.SetInt("hue", hue);
        }

        private void ChangeHueThresh(Slider sender)
        {
            var thresh = (int) sender.value;
            hueThreshText.text = thresh.ToString();
            PlayerPrefs.SetInt("hueThresh", thresh);
        }

        private void ChangeGlassesCheck(Toggle sender)
        {
            PlayerPrefs.SetInt("glassesCheck", sender.isOn ? 1 : 0);
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

        private void ChangeCamPreview(TMP_Dropdown sender)
        {
            PlayerPrefs.SetString("cam", webcamDropdown.options[sender.value].text);
            if (_faceTrackingInstance != null)
                Destroy(_faceTrackingInstance);
            _faceTrackingInstance = Instantiate(faceTracking);
            _faceTrackingInstance.SetActive(true);
        }

        private void ChangeSmoothing(TMP_Dropdown sender)
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
        #endregion
    }
}