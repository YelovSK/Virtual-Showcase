using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.Enums;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu.Options
{
    public class WebcamOptions : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private TMP_Dropdown webcamDropdown;

        [SerializeField]
        private Slider detectionThresholdSlider;

        [SerializeField]
        private TMP_Dropdown smoothingDropdown;

        [SerializeField]
        private Toggle interpolationToggle;

        [SerializeField]
        private GameObject kalmanOptions;

        [SerializeField]
        private Slider kalmanQSlider;

        [SerializeField]
        private Slider kalmanRSlider;

        [SerializeField]
        private GameObject averageOptions;

        [SerializeField]
        private Slider framesSmoothedSlider;

        #endregion

        #region Event Functions

        private void Awake()
        {
            AddWebcamOptions();
            AddSmoothingOptions();

            SetDefaults();
        }

        #endregion

        private void SetDefaults()
        {
            // Webcam
            int ix = webcamDropdown.options.FindIndex(option => option.text == MyPrefs.CameraName);
            webcamDropdown.value = ix == -1 ? 0 : ix;
            SetWebcam(webcamDropdown);

            // Threshold
            detectionThresholdSlider.value = MyPrefs.DetectionThreshold;
            SetThreshold(detectionThresholdSlider);

            // Smoothing
            ix = smoothingDropdown.options.FindIndex(option => option.text == MyPrefs.SmoothingType.ToString());
            smoothingDropdown.value = ix == -1 ? 0 : ix;
            SetSmoothing(smoothingDropdown);

            // Kalman Q
            kalmanQSlider.value = MyPrefs.KalmanQ;
            SetKalmanQ(kalmanQSlider);

            // Kalman R
            kalmanRSlider.value = MyPrefs.KalmanR;
            SetKalmanR(kalmanRSlider);

            // Smoothed Frames
            framesSmoothedSlider.value = MyPrefs.FramesSmoothed;
            SetSmoothedFrames(framesSmoothedSlider);

            // Interpolation
            interpolationToggle.isOn = MyPrefs.TrackingInterpolation;
            SetInterpolation(interpolationToggle);
        }

        public void SetWebcam(TMP_Dropdown sender)
        {
            string cameraName = sender.options[sender.value].text;
            MyPrefs.CameraName = cameraName;
            MyEvents.CameraChanged?.Invoke(gameObject, cameraName);
        }

        public void SetSmoothing(TMP_Dropdown sender)
        {
            var smoothingType = (SmoothingType)Enum.Parse(typeof(SmoothingType), sender.options[sender.value].text);
            MyPrefs.SmoothingType = smoothingType;

            switch (smoothingType)
            {
                case SmoothingType.Off:
                    kalmanOptions.SetActive(false);
                    averageOptions.SetActive(false);
                    break;
                case SmoothingType.Average:
                    kalmanOptions.SetActive(false);
                    averageOptions.SetActive(true);
                    break;
                case SmoothingType.Kalman:
                    kalmanOptions.SetActive(true);
                    averageOptions.SetActive(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SetInterpolation(Toggle sender)
        {
            MyPrefs.TrackingInterpolation = sender.isOn;
        }

        public void SetThreshold(Slider sender)
        {
            MyPrefs.DetectionThreshold = (int)sender.value;
        }

        public void SetKalmanQ(Slider sender)
        {
            MyPrefs.KalmanQ = sender.value;
        }

        public void SetKalmanR(Slider sender)
        {
            MyPrefs.KalmanR = sender.value;
        }

        public void SetSmoothedFrames(Slider sender)
        {
            MyPrefs.FramesSmoothed = (int)sender.value;
        }

        private void AddSmoothingOptions()
        {
            smoothingDropdown.AddOptions(Enum.GetNames(typeof(SmoothingType)).ToList());
        }

        private void AddWebcamOptions()
        {
            List<TMP_Dropdown.OptionData> options =
                WebCamTexture.devices.Select(device => new TMP_Dropdown.OptionData(device.name)).ToList();
            webcamDropdown.AddOptions(options);
        }
    }
}