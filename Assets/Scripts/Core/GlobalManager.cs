using System;
using System.Linq;
using UnityEngine;
using VirtualVitrine.UI.Main;

namespace VirtualVitrine.Core
{
    public class GlobalManager : MonoBehaviour
    {
        #region Public Fields
        public static GameObject loadedObject = null;
        public enum PreviewType
        {
            On = 0,
            Off = 1,
            DisabledTracking = 2
        }
        
        public enum SmoothType
        {
            Kalman,
            Average,
            Off
        }
        #endregion
        
        #region Private Fields
        private static bool PrefsLoaded => PlayerPrefs.HasKey("smoothing");
        #endregion

        #region Public Methods
        public static void CheckPlayerPrefs()
        {
            if (!PrefsLoaded)
                SetDefaultPlayerPrefs();
        }

        public static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
            SetDefaultPlayerPrefs();
        }
        #endregion

        #region Private Methods
        private static void SetDefaultPlayerPrefs()
        {
            PlayerPrefs.SetString("smoothing", SmoothType.Average.ToString());
            PlayerPrefs.SetString("cam", WebCamTexture.devices.First().name);
            PlayerPrefs.SetFloat("threshold", 0.5f); // 0.0 - 1.0
            PlayerPrefs.SetInt("hue", 240); // 0 - 360
            PlayerPrefs.SetInt("hueThresh", 20); // 1 - 100
            PlayerPrefs.SetString("modelPath", "");
            PlayerPrefs.SetInt("previewIx", (int) PreviewType.Off);
            PlayerPrefs.SetInt("framesSmoothed", 8); // 1-200
            PlayerPrefs.SetFloat("kalmanQ", 0.002f); // 1e-08 - 1e-02
            PlayerPrefs.SetFloat("kalmanR", 0.04f); // 0.0001 - 0.5
            PlayerPrefs.SetInt("stereo",0); // 0: off, 1: on
            PlayerPrefs.SetFloat("BottomCalibration", 0.0f);
            PlayerPrefs.SetFloat("TopCalibration", 1.0f);
            PlayerPrefs.SetFloat("LeftCalibration", 0.0f);
            PlayerPrefs.SetFloat("RightCalibration", 1.0f);
            PlayerPrefs.SetInt("glassesCheck", 1); // 0: off, 1: on
            PlayerPrefs.SetInt("screenDiagonalInches", 24);
            PlayerPrefs.SetInt("distanceFromScreenCm", 50);
            PlayerPrefs.SetFloat("focalLength", Calibration.GetFocalLength(PlayerPrefs.GetInt("distanceFromScreenCm")));
        }
        #endregion
    }
}