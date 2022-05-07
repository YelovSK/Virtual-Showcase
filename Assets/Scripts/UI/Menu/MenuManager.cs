using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.FaceTracking;

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
        [SerializeField] private WebcamInput faceTracking;

        #endregion

        #region Event Functions

        private void Start()
        {
            MyPrefs.CheckPlayerPrefs();
            SetElementsToPlayerPrefs();
            SetDelegates();
        }

        #endregion


        public void ResetSettings()
        {
            if (ModelLoader.Model != null)
                Destroy(ModelLoader.Model);
            MyPrefs.ResetPlayerPrefsExceptKeyBinds();
            SetElementsToPlayerPrefs();
        }


        private void SetElementsToPlayerPrefs()
        {
            currentModelText.text = "Current model: " + MyPrefs.ModelPath.Split('\\').Last();
            SetCamName(MyPrefs.CameraName);
            SetSmoothingOption(MyPrefs.SmoothingType);
            SetAvgSliderAndText(MyPrefs.FramesSmoothed);
            qSlider.value = MyPrefs.KalmanQ;
            rSlider.value = MyPrefs.KalmanR;
            thresholdSlider.value = MyPrefs.DetectionThreshold;
            hueSlider.value = MyPrefs.Hue;
            hueThreshSlider.value = MyPrefs.HueThreshold;
            glassesCheck.isOn = MyPrefs.GlassesCheck == 1;
        }

        private void SetAvgSliderAndText(int framesSmoothed)
        {
            averageSlider.value = framesSmoothed;
            averageValue.text = framesSmoothed + " frames";
        }

        private void SetCamName(string camName)
        {
            // Add WebCam devices to dropdown options.
            if (webcamDropdown.options.Count == 0)
            {
                List<TMP_Dropdown.OptionData> options = WebCamTexture.devices.Select(device => new TMP_Dropdown.OptionData(device.name)).ToList();
                webcamDropdown.AddOptions(options);
            }

            webcamDropdown.value = webcamDropdown.options.FindIndex(x => x.text == camName);
        }

        private void SetSmoothingOption(string smoothingOption)
        {
            if (smoothingDropdown.options.Count == 0)
                smoothingDropdown.AddOptions(Enum.GetNames(typeof(MyPrefs.SmoothingTypeEnum)).ToList());
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
            MyPrefs.DetectionThreshold = sender.value;
        }

        private void ChangeHue(Slider sender)
        {
            var hue = (int) sender.value;
            hueText.text = hue.ToString();
            Color rgBcolor = Color.HSVToRGB((float) hue / 360, 1, 1);
            // Fill hueSlider with given colour.
            hueSlider.GetComponentInChildren<Image>().color = rgBcolor;
            MyPrefs.Hue = hue;
        }

        private void ChangeHueThresh(Slider sender)
        {
            var thresh = (int) sender.value;
            hueThreshText.text = thresh.ToString();
            MyPrefs.HueThreshold = thresh;
        }

        private void ChangeGlassesCheck(Toggle sender)
        {
            MyPrefs.GlassesCheck = sender.isOn ? 1 : 0;
        }

        private void ChangeRslider(Slider slider)
        {
            MyPrefs.KalmanR = slider.value;
            rValue.text = slider.value.ToString("0.0000");
        }

        private void ChangeQslider(Slider slider)
        {
            MyPrefs.KalmanQ = slider.value;
            qValue.text = slider.value.ToString("0.00000");
        }

        private void ChangeCamPreview(TMP_Dropdown sender)
        {
            MyPrefs.CameraName = sender.options[sender.value].text;
            if (faceTracking.WebCamTexture.deviceName != MyPrefs.CameraName)
                faceTracking.ChangeWebcam();
        }

        private void ChangeSmoothing(TMP_Dropdown sender)
        {
            MyPrefs.SmoothingType = sender.options[sender.value].text;
            bool averageActive = MyPrefs.SmoothingType == MyPrefs.SmoothingTypeEnum.Average.ToString();
            averageSlider.transform.parent.gameObject.SetActive(averageActive);
            bool kalmanActive = MyPrefs.SmoothingType == MyPrefs.SmoothingTypeEnum.Kalman.ToString();
            qSlider.transform.parent.gameObject.SetActive(kalmanActive);
        }

        private void ChangeAvgFrames(Slider slider)
        {
            MyPrefs.FramesSmoothed = (int) slider.value;
            averageValue.text = slider.value + " frames";
        }
    }
}