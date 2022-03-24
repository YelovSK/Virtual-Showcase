using System.Linq;
using UnityEngine;

public class StaticVars : MonoBehaviour
{
    public static GameObject loadedObject = null;

    public static void SetDefaultPlayerPrefs()
    {
        if (!PlayerPrefs.HasKey("smoothing"))
            PlayerPrefs.SetString("smoothing", "Kalman");   // Kalman, Average, Off
        if (!PlayerPrefs.HasKey("cam"))
            PlayerPrefs.SetString("cam", WebCamTexture.devices.First().name);
        if (!PlayerPrefs.HasKey("threshold"))
            PlayerPrefs.SetFloat("threshold", 0.5f);    // 0.0 - 1.0
        if (!PlayerPrefs.HasKey("hue"))
            PlayerPrefs.SetInt("hue", 240);    // 0 - 360
        if (!PlayerPrefs.HasKey("hueThresh"))
            PlayerPrefs.SetInt("hueThresh", 20);    // 1 - 100
        if (!PlayerPrefs.HasKey("modelPath"))
            PlayerPrefs.SetString("modelPath", "");
        if (!PlayerPrefs.HasKey("previewIx"))
            PlayerPrefs.SetInt("previewIx", 0); // 0, 1, 2
        if (!PlayerPrefs.HasKey("framesSmoothed"))
            PlayerPrefs.SetInt("framesSmoothed", 8);   // 1-200
        if (!PlayerPrefs.HasKey("kalmanQ"))
            PlayerPrefs.SetFloat("kalmanQ", 0.002f);   // 1e-08 - 1e-02
        if (!PlayerPrefs.HasKey("kalmanR"))
            PlayerPrefs.SetFloat("kalmanR", 0.04f);  // 0.0001 - 0.5
        if (!PlayerPrefs.HasKey("stereo"))  // 0 / 1
            PlayerPrefs.SetInt("stereo", 0);
        if (!PlayerPrefs.HasKey("BottomCalibration"))    // 0.0 - 1.0
            PlayerPrefs.SetFloat("BottomCalibration", 0.0f);
        if (!PlayerPrefs.HasKey("TopCalibration"))    // 0.0 - 1.0
            PlayerPrefs.SetFloat("TopCalibration", 1.0f);
        if (!PlayerPrefs.HasKey("LeftCalibration"))    // 0.0 - 1.0
            PlayerPrefs.SetFloat("LeftCalibration", 0.0f);
        if (!PlayerPrefs.HasKey("RightCalibration"))    // 0.0 - 1.0
            PlayerPrefs.SetFloat("RightCalibration", 1.0f);
        if (!PlayerPrefs.HasKey("glassesCheck"))    // 0 / 1
            PlayerPrefs.SetInt("glassesCheck", 1);
        if (!PlayerPrefs.HasKey("screenDiagonalInches"))
            PlayerPrefs.SetInt("screenDiagonalInches", 24);
        if (!PlayerPrefs.HasKey("distanceFromScreenCm"))
            PlayerPrefs.SetInt("distanceFromScreenCm", 50);

    }
}