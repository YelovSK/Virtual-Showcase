using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class GlobalVars : MonoBehaviour
{
    public static string[] SmoothingOptionsArr = {"Kalman", "Average", "Off"};
    public static string smoothing = SmoothingOptionsArr[0];
    public static List<WebCamDevice> webcams = WebCamTexture.devices.ToList();
    public static WebCamDevice cam = webcams[0];
    public static float threshold = 0.5f;   // 0.0-1.0
    public static string modelPath = "Assets\\Objects\\IronMan.obj";
    public static int previewIx = 0;
    public static int framesSmoothed = 30;
    public static bool stereoChecked = false;
    public static float kalmanQ = 0.0001f;
    public static float kalmanR = 0.1f;
}
