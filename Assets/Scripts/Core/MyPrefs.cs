using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualVitrine.MainScene;

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
                if (value is < 2 or > 200) return;
                PlayerPrefs.SetInt("framesSmoothed", value);
            }
        }

        public static float KalmanQ
        {
            get => PlayerPrefs.GetFloat("kalmanQ");
            set
            {
                if (value is >= 0.00000001f and <= 0.01f)
                    PlayerPrefs.SetFloat("kalmanQ", value);
            }
        }

        public static float KalmanR
        {
            get => PlayerPrefs.GetFloat("kalmanR");
            set
            {
                if (value is >= 0.0001f and <= 0.5f)
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
                if (value is >= 0.0f and <= 1.0f)
                    PlayerPrefs.SetFloat("threshold", value);
            }
        }

        public static int Hue
        {
            get => PlayerPrefs.GetInt("hue");
            set
            {
                if (value is >= 0 and <= 360)
                    PlayerPrefs.SetInt("hue", value);
            }
        }

        public static int HueThreshold
        {
            get => PlayerPrefs.GetInt("hueThresh");
            set
            {
                if (value is >= 1 and <= 100)
                    PlayerPrefs.SetInt("hueThresh", value);
            }
        }

        public static List<string> ModelPaths
        {
            get
            {
                List<string> paths = PlayerPrefs.GetString("modelPath").Split(",").ToList();
                if (paths.Count == 1 && paths[0] == string.Empty)
                    return new List<string>();

                return paths;
            }
            set
            {
                if (value is null || value.Any() == false)
                    PlayerPrefs.SetString("modelPath", string.Empty);
                else
                    PlayerPrefs.SetString("modelPath", string.Join(",", value));
            }
        }

        public static int PreviewOn
        {
            get => PlayerPrefs.GetInt("previewOn");
            set
            {
                if (value is 0 or 1)
                    PlayerPrefs.SetInt("previewOn", value);
            }
        }

        public static int StereoOn
        {
            get => PlayerPrefs.GetInt("stereoOn");
            set
            {
                if (value is 0 or 1)
                    PlayerPrefs.SetInt("stereoOn", value);
            }
        }

        public static int GlassesCheck
        {
            get => PlayerPrefs.GetInt("glassesCheck");
            set
            {
                if (value is 0 or 1)
                    PlayerPrefs.SetInt("glassesCheck", value);
            }
        }

        public static float BottomCalibration
        {
            get => PlayerPrefs.GetFloat("BottomCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                    PlayerPrefs.SetFloat("BottomCalibration", value);
            }
        }

        public static float TopCalibration
        {
            get => PlayerPrefs.GetFloat("TopCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                    PlayerPrefs.SetFloat("TopCalibration", value);
            }
        }

        public static float LeftCalibration
        {
            get => PlayerPrefs.GetFloat("LeftCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                    PlayerPrefs.SetFloat("LeftCalibration", value);
            }
        }

        public static float RightCalibration
        {
            get => PlayerPrefs.GetFloat("RightCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
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

        public static int QualityIndex
        {
            get => PlayerPrefs.GetInt("qualityIndex");
            set
            {
                if (value >= 0 && value <= 2)
                    PlayerPrefs.SetInt("qualityIndex", value);
            }
        }

        public static int UpdateHeadDistance
        {
            get => PlayerPrefs.GetInt("updateHeadDistance");
            set
            {
                if (value is 0 or 1)
                    PlayerPrefs.SetInt("updateHeadDistance", value);
            }
        }

        public static int InterpolatedPosition
        {
            get => PlayerPrefs.GetInt("interpolatedPosition");
            set
            {
                if (value is 0 or 1)
                    PlayerPrefs.SetInt("interpolatedPosition", value);
            }
        }

        public static string Resolution
        {
            get => PlayerPrefs.GetString("resolution");
            set
            {
                // Expects (width)x(height)x(refresh)
                if (value.Split('x').ToList().Count == 3)
                    PlayerPrefs.SetString("resolution", value);
            }
        }

        public static Resolution ResolutionParsed
        {
            get
            {
                if (string.IsNullOrEmpty(Resolution))
                    return new Resolution();
                List<string> split = Resolution.Split('x').ToList();
                var output = new Resolution
                {
                    width = Convert.ToInt16(split[0]),
                    height = Convert.ToInt16(split[1]),
                    refreshRateRatio = new RefreshRate {denominator = 1, numerator = (uint) Convert.ToInt16(split[2])},
                };
                return output;
            }
        }

        private static bool PrefsLoaded => PlayerPrefs.HasKey("SmoothingType");

        /// <returns>Whether path was added (no duplicate)</returns>
        public static bool AddModelPath(string path)
        {
            List<string> paths = ModelPaths;
            if (paths.Contains(path) == false)
            {
                paths.Add(path);
                ModelPaths = paths;
                return true;
            }

            return false;
        }

        /// <returns>Whether removed (was set)</returns>
        public static bool RemoveModelPath(string path)
        {
            List<string> paths = ModelPaths;
            if (paths.Remove(path))
            {
                ModelPaths = paths;
                return true;
            }

            return false;
        }

        public static void CheckPlayerPrefs()
        {
            if (!PrefsLoaded)
                SetDefaultPlayerPrefs();
        }

        public static void ResetPlayerPrefsExceptKeyBinds()
        {
            // Save binds temporarily.
            string binds = PlayerPrefs.GetString("rebinds");
            PlayerPrefs.DeleteAll();

            // Set binds back.
            PlayerPrefs.SetString("rebinds", binds);
            SetDefaultPlayerPrefs();
        }


        private static void SetDefaultPlayerPrefs()
        {
            // Default main scene.
            MainScene = SceneSwitcher.MainScenes.MainRoom.ToString();

            // Eyes smoothing types.
            SmoothingType = SmoothingTypeEnum.Off.ToString();

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
            ModelPaths = null;

            // Checks.
            PreviewOn = 0;
            StereoOn = 0;
            UpdateHeadDistance = 0;
            GlassesCheck = 0;
            InterpolatedPosition = 0;

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

            // Default quality of 2 is the highest.
            QualityIndex = 2;
        }

        #region Nested type: SmoothingTypeEnum

        public enum SmoothingTypeEnum
        {
            Kalman,
            Average,
            Off,
        }

        #endregion
    }
}