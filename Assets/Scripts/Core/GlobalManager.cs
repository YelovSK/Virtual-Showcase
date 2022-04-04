using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualVitrine
{
    public static class GlobalManager
    {
        #region Public Fields
        public static GameObject loadedObject = null;
        public static bool InMainScene => SceneManager.GetActiveScene().name == "Main";
        public static bool InMenuScene => SceneManager.GetActiveScene().name == "Menu";

        public enum SmoothType { Kalman, Average, Off }
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
            // eyes smoothing types
            PlayerPrefs.SetString("smoothing", SmoothType.Average.ToString());
            
            // smoothing values
            PlayerPrefs.SetInt("framesSmoothed", 8); // 1-200
            PlayerPrefs.SetFloat("kalmanQ", 0.002f); // 1e-08 - 1e-02
            PlayerPrefs.SetFloat("kalmanR", 0.04f); // 0.0001 - 0.5
            
            // webcam names
            PlayerPrefs.SetString("cam", WebCamTexture.devices.First().name);
            
            // threshold for face detection confidence
            PlayerPrefs.SetFloat("threshold", 0.5f); // 0.0 - 1.0
            
            // hue range for glasses detection
            PlayerPrefs.SetInt("hue", 240); // 0 - 360
            PlayerPrefs.SetInt("hueThresh", 20); // 1 - 100
            
            // path to .obj file to get loaded
            PlayerPrefs.SetString("modelPath", "");
            
            // checks
            PlayerPrefs.SetInt("previewOn", 0); // 0: off, 1: on
            PlayerPrefs.SetInt("stereoOn", 0); // 0: off, 1: on
            PlayerPrefs.SetInt("glassesCheck", 1); // 0: off, 1: on
            
            // calibration screen edge values
            PlayerPrefs.SetFloat("BottomCalibration", 0.0f);    // 0.0 - 1.0
            PlayerPrefs.SetFloat("TopCalibration", 1.0f);    // 0.0 - 1.0
            PlayerPrefs.SetFloat("LeftCalibration", 0.0f);    // 0.0 - 1.0
            PlayerPrefs.SetFloat("RightCalibration", 1.0f);    // 0.0 - 1.0
            
            // calibration screen parameters
            PlayerPrefs.SetInt("screenDiagonalInches", 24);
            PlayerPrefs.SetInt("distanceFromScreenCm", 50);
            
            // focal length for face distance
            PlayerPrefs.SetFloat("focalLength", UI.Main.Calibration.GetFocalLength(PlayerPrefs.GetInt("distanceFromScreenCm")));
        }
        #endregion
    }
}