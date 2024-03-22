using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualShowcase.Enums;
using VirtualShowcase.MainScene;

namespace VirtualShowcase.Utilities
{
    /// <summary>
    ///     I store all persistent settings with Unity's PlayerPrefs.
    ///     Typing the same string over and over is sketchy and sometimes
    ///     I need to store a more complex object as a string (which is stupid),
    ///     so some properties have custom parsing.
    /// </summary>
    public static class MyPrefs
    {
        #region Events

        public static event EventHandler ScreenSizeChanged;
        public static event EventHandler ScreenDistanceChanged;

        #endregion

        #region Properties

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
                if (paths.Count == 0 || string.IsNullOrEmpty(paths[0]))
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

        public static bool PreviewOn
        {
            get => PlayerPrefs.GetInt("previewOn").ToBool();
            set => PlayerPrefs.SetInt("previewOn", value.ToInt());
        }

        public static bool StereoOn
        {
            get => PlayerPrefs.GetInt("stereoOn").ToBool();
            set => PlayerPrefs.SetInt("stereoOn", value.ToInt());
        }

        public static bool GlassesCheck
        {
            get => PlayerPrefs.GetInt("glassesCheck").ToBool();
            set => PlayerPrefs.SetInt("glassesCheck", value.ToInt());
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
                ScreenSizeChanged?.Invoke(null, EventArgs.Empty);
                if (value > 1)
                    PlayerPrefs.SetInt("screenDiagonalInches", value);
            }
        }

        public static int ScreenDistance
        {
            get => PlayerPrefs.GetInt("distanceFromScreenCm");
            set
            {
                ScreenDistanceChanged?.Invoke(null, EventArgs.Empty);
                if (value > 1)
                    PlayerPrefs.SetInt("distanceFromScreenCm", value);
            }
        }

        public static float FocalLength
        {
            get => PlayerPrefs.GetFloat("focalLength");
            set => PlayerPrefs.SetFloat("focalLength", value);
        }

        public static eGraphicsQuality QualityIndex
        {
            get => (eGraphicsQuality) PlayerPrefs.GetInt("qualityIndex");
            set => PlayerPrefs.SetInt("qualityIndex", (int) value);
        }

        public static bool UpdateHeadDistance
        {
            get => PlayerPrefs.GetInt("updateHeadDistance").ToBool();
            set => PlayerPrefs.SetInt("updateHeadDistance", value.ToInt());
        }

        public static bool InterpolatedPosition
        {
            get => PlayerPrefs.GetInt("interpolatedPosition").ToBool();
            set => PlayerPrefs.SetInt("interpolatedPosition", value.ToInt());
        }

        /// <summary>
        ///     Saves as string: {width},{height},{refreshRate.denominator},{refreshRate.numerator}
        /// </summary>
        public static Resolution? Resolution
        {
            get
            {
                string res = PlayerPrefs.GetString("resolution");
                if (string.IsNullOrEmpty(res))
                    return null;

                List<string> split = res.Split(',').ToList();
                if (split.Count != 4)
                    return null;

                return new Resolution
                {
                    width = int.Parse(split[0]),
                    height = int.Parse(split[1]),
                    refreshRateRatio = new RefreshRate {denominator = uint.Parse(split[2]), numerator = uint.Parse(split[3])},
                };
            }
            set
            {
                if (value is null)
                    PlayerPrefs.SetString("resolution", string.Empty);
                else
                    PlayerPrefs.SetString("resolution",
                        $"{value.Value.width},{value.Value.height},{value.Value.refreshRateRatio.denominator},{value.Value.refreshRateRatio.numerator}");
            }
        }

        private static bool PrefsLoaded => PlayerPrefs.HasKey("SmoothingType");

        #endregion
        
        /// <returns>Whether path was added (no duplicate)</returns>
        public static bool AddModelPath(string path)
        {
            List<string> paths = ModelPaths;
            if (paths.Contains(path)) return false;

            paths.Add(path);
            ModelPaths = paths;
            return true;
        }

        /// <returns>Whether removed (was set)</returns>
        public static bool RemoveModelPath(string path)
        {
            List<string> paths = ModelPaths;
            if (!paths.Remove(path)) return false;

            ModelPaths = paths;
            return true;
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
            MainScene = eMainScenes.MainRoom.ToString();

            // Eyes smoothing types.
            SmoothingType = eSmoothingType.Off.ToString();

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
            PreviewOn = false;
            StereoOn = false;
            UpdateHeadDistance = false;
            GlassesCheck = false;
            InterpolatedPosition = false;

            // Calibration screen edge values.
            BottomCalibration = 0.0f;
            TopCalibration = 1.0f;
            LeftCalibration = 0.0f;
            RightCalibration = 1.0f;

            // Calibration screen parameters.
            ScreenSize = 27;
            ScreenDistance = 80;

            // Focal length for face distance.
            FocalLength = CalibrationManager.GetFocalLength(ScreenDistance);

            // Default quality of 2 is the highest.
            QualityIndex = eGraphicsQuality.High;
        }
    }
}