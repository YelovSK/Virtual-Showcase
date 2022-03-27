using System.Linq;
using UnityEngine;

public class StaticVars : MonoBehaviour
{
    public static GameObject loadedObject = null;
    public static bool PrefsLoaded => PlayerPrefs.HasKey("smoothing");
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

    public static void SetDefaultPlayerPrefs()
    {
        PlayerPrefs.SetString("smoothing", "Kalman");   // Kalman, Average, Off
        PlayerPrefs.SetString("cam", WebCamTexture.devices.First().name);
        PlayerPrefs.SetFloat("threshold", 0.5f);    // 0.0 - 1.0
        PlayerPrefs.SetInt("hue", 240);    // 0 - 360
        PlayerPrefs.SetInt("hueThresh", 20);    // 1 - 100
        PlayerPrefs.SetString("modelPath", "");
        PlayerPrefs.SetInt("previewIx", 0); // 0, 1, 2
        PlayerPrefs.SetInt("framesSmoothed", 8);   // 1-200
        PlayerPrefs.SetFloat("kalmanQ", 0.002f);   // 1e-08 - 1e-02
        PlayerPrefs.SetFloat("kalmanR", 0.04f);  // 0.0001 - 0.5
        PlayerPrefs.SetInt("stereo", 0);
        PlayerPrefs.SetFloat("BottomCalibration", 0.0f);
        PlayerPrefs.SetFloat("TopCalibration", 1.0f);
        PlayerPrefs.SetFloat("LeftCalibration", 0.0f);
        PlayerPrefs.SetFloat("RightCalibration", 1.0f);
        PlayerPrefs.SetInt("glassesCheck", 1);
        PlayerPrefs.SetInt("screenDiagonalInches", 24);
        PlayerPrefs.SetInt("distanceFromScreenCm", 50);
        PlayerPrefs.SetFloat("focalLength", Calibration.GetFocalLength(PlayerPrefs.GetInt("distanceFromScreenCm")));
    }

}