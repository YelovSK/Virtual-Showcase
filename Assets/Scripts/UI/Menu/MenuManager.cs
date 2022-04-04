using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualVitrine.UI.Menu
{
    public class MenuManager : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private TMP_Dropdown webcamDropdown;
        [SerializeField] private TMP_Text currentModelText;
        
        [Header("Smoothing elements")]
        [SerializeField] private TMP_Dropdown smoothingDropdown;
        // average
        [SerializeField] private Slider averageSlider;
        [SerializeField] private TMP_Text averageValue;
        // kalman
        [SerializeField] private Slider qSlider;
        [SerializeField] private Slider rSlider;
        [SerializeField] private TMP_Text qValue;
        [SerializeField] private TMP_Text rValue;
        
        [Header("Tracking threshold")]
        [SerializeField] private Slider thresholdSlider;
        [SerializeField] private TMP_Text thresholdText;
        
        [Header("Hue")]
        [SerializeField] private Slider hueSlider;
        [SerializeField] private TMP_Text hueText;
        [SerializeField] private Slider hueThreshSlider;
        [SerializeField] private TMP_Text hueThreshText;
        
        [Header("Glasses")]
        [SerializeField] private Toggle glassesCheck;
        
        [Header("Face tracking object")]
        [SerializeField] private GameObject faceTracking;
        #endregion
        
        #region Private Fields
        private GameObject _faceTrackingInstance;
        #endregion
        
        #region Public Methods
        public void ResetSettings()
        {
            if (GlobalManager.loadedObject != null)
                Destroy(GlobalManager.loadedObject);
            GlobalManager.loadedObject = null;
            GlobalManager.ResetPlayerPrefs();
            SetElementsToPlayerPrefs();
        }
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            GlobalManager.CheckPlayerPrefs();
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
            if (File.Exists(PlayerPrefs.GetString("modelPath")))
                currentModelText.text = "Current model: " + PlayerPrefs.GetString("modelPath").Split('\\').Last();
            else
                currentModelText.text = "Current model: ";
            SetCamName(PlayerPrefs.GetString("cam"));
            SetSmoothingOption(PlayerPrefs.GetString("smoothing"));
            SetAvgSliderAndText(PlayerPrefs.GetInt("framesSmoothed"));
            qSlider.value = PlayerPrefs.GetFloat("kalmanQ");
            rSlider.value = PlayerPrefs.GetFloat("kalmanR");
            thresholdSlider.value = PlayerPrefs.GetFloat("threshold");
            hueSlider.value = PlayerPrefs.GetInt("hue");
            hueThreshSlider.value = PlayerPrefs.GetInt("hueThresh");
            glassesCheck.isOn = PlayerPrefs.GetInt("glassesCheck") == 1;
        }
        
        private void SetAvgSliderAndText(int framesSmoothed)
        {
            averageSlider.value = framesSmoothed;
            averageValue.text = framesSmoothed + " frames";
        }
        
        private void SetCamName(string camName)
        {
            // add WebCam devices to dropdown options
            if (webcamDropdown.options.Count == 0)
            {
                var options = WebCamTexture.devices.Select(device => new TMP_Dropdown.OptionData(device.name)).ToList();
                webcamDropdown.AddOptions(options);
            }

            webcamDropdown.value = webcamDropdown.options.FindIndex(x => x.text == camName);
        }

        private void SetSmoothingOption(string smoothingOption)
        {
            if (smoothingDropdown.options.Count == 0)
                smoothingDropdown.AddOptions(Enum.GetNames(typeof(GlobalManager.SmoothType)).ToList());
            smoothingDropdown.value = smoothingDropdown.options.FindIndex(option => option.text == smoothingOption);
        }

        private void SetDelegates()
        {
            ChangeCamPreview(webcamDropdown);
            webcamDropdown.onValueChanged.AddListener(delegate { ChangeCamPreview(webcamDropdown); });

            ChangeSmoothing(smoothingDropdown);
            smoothingDropdown.onValueChanged.AddListener(delegate { ChangeSmoothing(smoothingDropdown); });

            ChangeAvgFrames(averageSlider);
            averageSlider.onValueChanged.AddListener(delegate { ChangeAvgFrames(averageSlider); });

            ChangeQslider(qSlider);
            qSlider.onValueChanged.AddListener(delegate { ChangeQslider(qSlider); });

            ChangeRslider(rSlider);
            rSlider.onValueChanged.AddListener(delegate { ChangeRslider(rSlider); });

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
            var RGBcolor = Color.HSVToRGB((float) hue / 360, 1, 1);
            // fill hueSlider with given colour
            hueSlider.GetComponentInChildren<Image>().color = RGBcolor;
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
            rValue.text = slider.value.ToString("0.0000");
        }

        private void ChangeQslider(Slider slider)
        {
            PlayerPrefs.SetFloat("kalmanQ", slider.value);
            qValue.text = slider.value.ToString("0.00000");
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
            var averageActive = PlayerPrefs.GetString("smoothing") == GlobalManager.SmoothType.Average.ToString();
            averageSlider.transform.parent.gameObject.SetActive(averageActive);
            var kalmanActive = PlayerPrefs.GetString("smoothing") == GlobalManager.SmoothType.Kalman.ToString();
            qSlider.transform.parent.gameObject.SetActive(kalmanActive);
        }

        private void ChangeAvgFrames(Slider slider)
        {
            PlayerPrefs.SetInt("framesSmoothed", Convert.ToInt16(slider.value));
            averageValue.text = slider.value + " frames";
        }
        #endregion
    }
}