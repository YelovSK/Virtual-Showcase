using System.IO;
using System.Linq;
using UnityEngine;

namespace VirtualVitrine
{
    public static class MyPrefs
    {
        public static string MainScene
        {
            get => PlayerPrefs.GetString("mainScene");
            set => PlayerPrefs.SetString("mainScene", value);
        }

        public static string SmoothingType
        {
            get => PlayerPrefs.GetString("SmoothingType");
            set => PlayerPrefs.SetString("SmoothingType", value);
        }

        public static int FramesSmoothed
        {
            get => PlayerPrefs.GetInt("framesSmoothed");
            set
            {
                if (value < 2 || value > 200) return;
                PlayerPrefs.SetInt("framesSmoothed", value);
            }
        }

        public static float KalmanQ
        {
            get => PlayerPrefs.GetFloat("kalmanQ");
            set
            {
                if (value < 0.00000001f || value > 0.01f) return;
                PlayerPrefs.SetFloat("kalmanQ", value);
            }
        }

        public static float KalmanR
        {
            get => PlayerPrefs.GetFloat("kalmanR");
            set
            {
                if (value < 0.0001f || value > 0.5f) return;
                PlayerPrefs.SetFloat("kalmanR", value);
            }
        }

        public static string CameraName
        {
            get => PlayerPrefs.GetString("cam");
            set => PlayerPrefs.SetString("cam", value);
        }

        public static float DetectionThreshold
        {
            get => PlayerPrefs.GetFloat("threshold");
            set
            {
                if (value < 0.0f || value > 1.0f) return;
                PlayerPrefs.SetFloat("threshold", value);
            }
        }

        public static int Hue
        {
            get => PlayerPrefs.GetInt("hue");
            set
            {
                if (value < 0 || value > 360) return;
                PlayerPrefs.SetInt("hue", value);
            }
        }

        public static int HueThreshold
        {
            get => PlayerPrefs.GetInt("hueThresh");
            set
            {
                if (value < 1 || value > 100) return;
                PlayerPrefs.SetInt("hueThresh", value);
            }
        }

        public static string ModelPath
        {
            get => PlayerPrefs.GetString("modelPath");
            set
            {
                if (File.Exists(value))
                    PlayerPrefs.SetString("modelPath", value);
            }
        }

        public static int PreviewOn
        {
            get => PlayerPrefs.GetInt("previewOn");
            set
            {
                if (value == 0 || value == 1)
                    PlayerPrefs.SetInt("previewOn", value);
            }
        }

        public static int StereoOn
        {
            get => PlayerPrefs.GetInt("stereoOn");
            set
            {
                if (value == 0 || value == 1)
                    PlayerPrefs.SetInt("stereoOn", value);
            }
        }

        public static int GlassesCheck
        {
            get => PlayerPrefs.GetInt("glassesCheck");
            set
            {
                if (value == 0 || value == 1)
                    PlayerPrefs.SetInt("glassesCheck", value);
            }
        }

        public static float BottomCalibration
        {
            get => PlayerPrefs.GetFloat("BottomCalibration");
            set
            {
                if (value < 0.0f || value > 1.0f) return;
                PlayerPrefs.SetFloat("BottomCalibration", value);
            }
        }

        public static float TopCalibration
        {
            get => PlayerPrefs.GetFloat("TopCalibration");
            set
            {
                if (value < 0.0f || value > 1.0f) return;
                PlayerPrefs.SetFloat("TopCalibration", value);
            }
        }

        public static float LeftCalibration
        {
            get => PlayerPrefs.GetFloat("LeftCalibration");
            set
            {
                if (value < 0.0f || value > 1.0f) return;
                PlayerPrefs.SetFloat("LeftCalibration", value);
            }
        }

        public static float RightCalibration
        {
            get => PlayerPrefs.GetFloat("RightCalibration");
            set
            {
                if (value < 0.0f || value > 1.0f) return;
                PlayerPrefs.SetFloat("RightCalibration", value);
            }
        }

        public static int ScreenSize
        {
            get => PlayerPrefs.GetInt("screenDiagonalInches");
            set
            {
                if (value > 1)
                    PlayerPrefs.SetInt("screenDiagonalInches", value);
            }
        }

        public static int ScreenDistance
        {
            get => PlayerPrefs.GetInt("distanceFromScreenCm");
            set
            {
                if (value > 1)
                    PlayerPrefs.SetInt("distanceFromScreenCm", value);
            }
        }

        public static float FocalLength
        {
            get => PlayerPrefs.GetFloat("focalLength");
            set => PlayerPrefs.SetFloat("focalLength", value);
        }

        private static bool PrefsLoaded => PlayerPrefs.HasKey("SmoothingType");


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


        private static void SetDefaultPlayerPrefs()
        {
            // Default main scene.
            MainScene = SceneSwitcher.MainScenes.MainRoom.ToString();

            // Eyes smoothing types.
            SmoothingType = SmoothingTypeEnum.Average.ToString();

            // Smoothing values.
            FramesSmoothed = 8;
            KalmanQ = 0.002f;
            KalmanR = 0.04f;

            // Webcam names.
            CameraName = WebCamTexture.devices.First().name;

            // Threshold for face detection confidence.
            DetectionThreshold = 0.5f;

            // Hue range for glasses detection.
            Hue = 240;
            HueThreshold = 20;

            // Path to .obj file to get loaded.
            ModelPath = "";

            // Checks.
            PreviewOn = 0;
            StereoOn = 0;
            GlassesCheck = 1;

            // Calibration screen edge values.
            BottomCalibration = 0.0f;
            TopCalibration = 1.0f;
            LeftCalibration = 0.0f;
            RightCalibration = 1.0f;

            // Calibration screen parameters.
            ScreenSize = 24;
            ScreenDistance = 50;

            // Focal length for face distance.
            FocalLength = CalibrationManager.GetFocalLength(ScreenDistance);
        }

        #region Nested type: SmoothingTypeEnum

        public enum SmoothingTypeEnum
        {
            Kalman,
            Average,
            Off
        }

        #endregion
    }
}