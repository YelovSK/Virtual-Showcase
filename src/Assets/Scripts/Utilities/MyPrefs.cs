﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualShowcase.Core;
using VirtualShowcase.Enums;

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
        /// <returns>Whether path was added (no duplicate)</returns>
        public static bool AddModelPath(string path)
        {
            List<string> paths = ModelPaths;
            if (paths.Contains(path))
            {
                return false;
            }

            paths.Add(path);
            ModelPaths = paths;
            return true;
        }

        /// <returns>Whether removed (was set)</returns>
        public static bool RemoveModelPath(string path)
        {
            List<string> paths = ModelPaths;
            if (!paths.Remove(path))
            {
                return false;
            }

            ModelPaths = paths;
            return true;
        }

        public static void ResetPlayerPrefsExceptKeyBinds()
        {
            // Save binds temporarily.
            string binds = Rebinds;
            PlayerPrefs.DeleteAll();

            // Set binds back.
            Rebinds = binds;
        }

        #region Properties

        /// <summary>
        ///     Set to null to delete key.
        /// </summary>
        public static string Rebinds
        {
            get => PlayerPrefs.GetString("rebinds");
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    PlayerPrefs.DeleteKey("rebinds");
                }
                else
                {
                    PlayerPrefs.SetString("rebinds", value);
                }
            }
        }

        public static string MainScene
        {
            get => PlayerPrefs.GetString("mainScene", MySceneManager.ROOM_SCENE_NAME);
            set => PlayerPrefs.SetString("mainScene", value);
        }

        public static SmoothingType SmoothingType
        {
            get => (SmoothingType)PlayerPrefs.GetInt("SmoothingType", (int)SmoothingType.Average);
            set => PlayerPrefs.SetInt("SmoothingType", (int)value);
        }

        public static int FramesSmoothed
        {
            get => PlayerPrefs.GetInt("framesSmoothed", 8);
            set
            {
                if (value is < 2 or > 200)
                {
                    return;
                }

                PlayerPrefs.SetInt("framesSmoothed", value);
            }
        }

        public static float KalmanQ
        {
            get => PlayerPrefs.GetFloat("kalmanQ", 0.002f);
            set
            {
                if (value is >= 0.00000001f and <= 0.01f)
                {
                    PlayerPrefs.SetFloat("kalmanQ", value);
                }
            }
        }

        public static float KalmanR
        {
            get => PlayerPrefs.GetFloat("kalmanR", 0.04f);
            set
            {
                if (value is >= 0.0001f and <= 0.5f)
                {
                    PlayerPrefs.SetFloat("kalmanR", value);
                }
            }
        }

        public static string CameraName
        {
            get => PlayerPrefs.GetString("cam", WebCamTexture.devices.First().name);
            set => PlayerPrefs.SetString("cam", value);
        }

        public static int DetectionThreshold
        {
            get => PlayerPrefs.GetInt("threshold", 50);
            set
            {
                if (value is >= 1 and <= 100)
                {
                    PlayerPrefs.SetInt("threshold", value);
                }
            }
        }

        public static int Hue
        {
            get => PlayerPrefs.GetInt("hue", 180);
            set
            {
                if (value is >= 0 and <= 360)
                {
                    PlayerPrefs.SetInt("hue", value);
                }
            }
        }

        public static int HueThreshold
        {
            get => PlayerPrefs.GetInt("hueThresh", 25);
            set
            {
                if (value is >= 1 and <= 100)
                {
                    PlayerPrefs.SetInt("hueThresh", value);
                }
            }
        }

        public static List<string> ModelPaths
        {
            get
            {
                List<string> paths = PlayerPrefs.GetString("modelPath").Split("|").ToList();
                if (paths.Count == 0 || string.IsNullOrEmpty(paths[0]))
                {
                    return new List<string>();
                }

                return paths;
            }
            set
            {
                if (value is null || value.IsEmpty())
                {
                    PlayerPrefs.SetString("modelPath", string.Empty);
                }
                else
                {
                    PlayerPrefs.SetString("modelPath", string.Join("|", value.Distinct()));
                }
            }
        }

        public static bool SimplifyMesh
        {
            get => PlayerPrefs.GetInt("simplifyMesh", 0).ToBool();
            set => PlayerPrefs.SetInt("simplifyMesh", value.ToInt());
        }

        public static bool ShowRealModelSize
        {
            get => PlayerPrefs.GetInt("showRealModelSize", 0).ToBool();
            set => PlayerPrefs.SetInt("showRealModelSize", value.ToInt());
        }

        public static bool LoadModelImmediately
        {
            get => PlayerPrefs.GetInt("loadModelImmediately", 0).ToBool();
            set => PlayerPrefs.SetInt("loadModelImmediately", value.ToInt());
        }

        public static int MaxTriCount
        {
            get => PlayerPrefs.GetInt("maxTriCount", 250_000);
            set => PlayerPrefs.SetInt("maxTriCount", value);
        }

        public static bool PreviewOn
        {
            get => PlayerPrefs.GetInt("previewOn", 0).ToBool();
            set
            {
                PlayerPrefs.SetInt("previewOn", value.ToInt());
                MyEvents.CameraPreviewChanged.Invoke(null, value);
            }
        }

        public static bool StereoOn
        {
            get => PlayerPrefs.GetInt("stereoOn", 0).ToBool();
            set => PlayerPrefs.SetInt("stereoOn", value.ToInt());
        }

        public static bool GlassesCheck
        {
            get => PlayerPrefs.GetInt("glassesCheck", 0).ToBool();
            set => PlayerPrefs.SetInt("glassesCheck", value.ToInt());
        }

        public static float BottomCalibration
        {
            get => PlayerPrefs.GetFloat("BottomCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                {
                    PlayerPrefs.SetFloat("BottomCalibration", value);
                }
            }
        }

        public static float TopCalibration
        {
            get => PlayerPrefs.GetFloat("TopCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                {
                    PlayerPrefs.SetFloat("TopCalibration", value);
                }
            }
        }

        public static float LeftCalibration
        {
            get => PlayerPrefs.GetFloat("LeftCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                {
                    PlayerPrefs.SetFloat("LeftCalibration", value);
                }
            }
        }

        public static float RightCalibration
        {
            get => PlayerPrefs.GetFloat("RightCalibration");
            set
            {
                if (value is >= 0.0f and <= 1.0f)
                {
                    PlayerPrefs.SetFloat("RightCalibration", value);
                }
            }
        }

        public static int ScreenSize
        {
            get => PlayerPrefs.GetInt("screenDiagonalInches", 24);
            set
            {
                if (value > 1)
                {
                    PlayerPrefs.SetInt("screenDiagonalInches", value);
                }
            }
        }

        public static int ScreenDistance
        {
            get => PlayerPrefs.GetInt("distanceFromScreenCm", 100);
            set
            {
                if (value > 1)
                {
                    PlayerPrefs.SetInt("distanceFromScreenCm", value);
                }
            }
        }

        public static float CalibratedFocalLength
        {
            get => PlayerPrefs.GetFloat("focalLength", -1);
            set => PlayerPrefs.SetFloat("focalLength", value);
        }

        public static GraphicsQuality Quality
        {
            get => (GraphicsQuality)PlayerPrefs.GetInt("qualityIndex", (int)GraphicsQuality.High);
            set => PlayerPrefs.SetInt("qualityIndex", (int)value);
        }

        public static int FpsLimit
        {
            get => PlayerPrefs.GetInt("fpsLimit", 240);
            set => PlayerPrefs.SetInt("fpsLimit", value);
        }

        public static bool Vsync
        {
            get => PlayerPrefs.GetInt("vsync", 0).ToBool();
            set => PlayerPrefs.SetInt("vsync", value.ToInt());
        }

        public static FullScreenMode ScreenMode
        {
            get => (FullScreenMode)PlayerPrefs.GetInt("screenMode", (int)FullScreenMode.FullScreenWindow);
            set => PlayerPrefs.SetInt("screenMode", (int)value);
        }

        public static bool TrackingInterpolation
        {
            get => PlayerPrefs.GetInt("trackingInterpolation", 1).ToBool();
            set => PlayerPrefs.SetInt("trackingInterpolation", value.ToInt());
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
                {
                    return null;
                }

                List<string> split = res.Split(',').ToList();
                if (split.Count != 4)
                {
                    return null;
                }

                return new Resolution
                {
                    width = int.Parse(split[0]),
                    height = int.Parse(split[1]),
                    refreshRateRatio = new RefreshRate { denominator = uint.Parse(split[2]), numerator = uint.Parse(split[3]) },
                };
            }
            set
            {
                if (value is null)
                {
                    PlayerPrefs.SetString("resolution", string.Empty);
                }
                else
                {
                    PlayerPrefs.SetString("resolution",
                        $"{value.Value.width},{value.Value.height},{value.Value.refreshRateRatio.denominator},{value.Value.refreshRateRatio.numerator}");
                }
            }
        }

        #endregion
    }
}