using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.FaceTracking;
using VirtualVitrine.MainScene;

namespace VirtualVitrine.Menu
{
    public class MenuManager : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private TMP_Dropdown resolutionDropdown;
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

        #endregion

        private WebcamInput webcamInput;

        public static QualityEnum Quality => (QualityEnum) QualitySettings.GetQualityLevel();

        #region Event Functions

        private void Awake()
        {
            webcamInput = GameObject.Find("WebcamInput").GetComponent<WebcamInput>();
        }

        private void Start()
        {
            // I sometimes disable menu objects in editor because they overlap.
            EnableCanvasObjects();
            MyPrefs.CheckPlayerPrefs();
            SetElementsToPlayerPrefs();
            SetDelegates();
        }

        #endregion

        public enum QualityEnum
        {
            Low,
            Medium,
            High
        }

        private static void EnableCanvasObjects()
        {
            GameObject canvas = GameObject.Find("Canvas");
            canvas.transform.Find("Main menu").gameObject.SetActive(true);
            canvas.transform.Find("Options menu").gameObject.SetActive(true);
            canvas.transform.Find("Rebind menu").gameObject.SetActive(true);
        }


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
            SetResolution(MyPrefs.Resolution);
            qSlider.value = MyPrefs.KalmanQ;
            rSlider.value = MyPrefs.KalmanR;
            thresholdSlider.value = MyPrefs.DetectionThreshold;
            hueSlider.value = MyPrefs.Hue;
            hueThreshSlider.value = MyPrefs.HueThreshold;
            glassesCheck.isOn = MyPrefs.GlassesCheck == 1;
            qualityDropdown.value = MyPrefs.QualityIndex;
        }

        private void SetResolution(string resolution)
        {
            Resolution[] resolutions = Screen.resolutions.Reverse().ToArray();

            // If resolutions weren't added to the dropdown yet, add them.
            if (resolutionDropdown.options.Count == 0)
            {
                List<TMP_Dropdown.OptionData> options = resolutions
                    .Select(res => new TMP_Dropdown.OptionData(res.ToString()))
                    .ToList();
                resolutionDropdown.AddOptions(options);
            }

            // Resolution wasn't set yet, set it to the first one.
            if (string.IsNullOrEmpty(resolution))
            {
                Resolution res = resolutions.First();
                Screen.SetResolution(res.width, res.height, FullScreenMode.ExclusiveFullScreen);
            }

            // Find given resolution in the given dropdown and set it. If not found, sets to the first one.
            else
            {
                Resolution res = MyPrefs.ResolutionParsed;
                Screen.SetResolution(res.width, res.height, FullScreenMode.ExclusiveFullScreen, res.refreshRate);
                int ix = resolutionDropdown.options.FindIndex(x => x.text == $"{res.width} x {res.height} @ {res.refreshRate}Hz");
                resolutionDropdown.value = ix;
            }
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

            ChangeQuality(qualityDropdown);
            qualityDropdown.onValueChanged.AddListener(delegate { ChangeQuality(qualityDropdown); });

            ChangeResolution(resolutionDropdown);
            resolutionDropdown.onValueChanged.AddListener(delegate { ChangeResolution(resolutionDropdown); });
        }

        private static void ChangeResolution(TMP_Dropdown sender)
        {
            Resolution[] resolutions = Screen.resolutions.Reverse().ToArray();
            Resolution chosenRes = resolutions[sender.value];
            Screen.SetResolution(chosenRes.width, chosenRes.height, FullScreenMode.ExclusiveFullScreen, chosenRes.refreshRate);
            MyPrefs.Resolution = $"{chosenRes.width}x{chosenRes.height}x{chosenRes.refreshRate}";
        }

        private static void ChangeQuality(TMP_Dropdown sender)
        {
            QualitySettings.SetQualityLevel(sender.value, true);
            if (MyPrefs.QualityIndex == sender.value) return;
            MyPrefs.QualityIndex = sender.value;

            // If quality changed, destroy model to load it again with updated max triangle count.
            Destroy(ModelLoader.Model);
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

        private static void ChangeGlassesCheck(Toggle sender)
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
            if (WebcamInput.WebCamTexture.deviceName != MyPrefs.CameraName)
                WebcamInput.ChangeWebcam();
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