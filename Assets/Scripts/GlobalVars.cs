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
}
