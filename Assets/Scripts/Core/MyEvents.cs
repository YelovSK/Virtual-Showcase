using UnityEngine;
using UnityEngine.Events;

namespace VirtualShowcase.Core
{
    public class CustomGameEvent<T> : UnityEvent<GameObject, T>
    {
    }

    public class CustomGameEvent : UnityEvent<GameObject>
    {
    }

    /// <summary>
    ///     Static events class cuz I'm lazy.
    ///     Didn't wanna have a spiderweb of references, so I communicate between components through these central events.
    ///     It's stupid, but idc.
    /// </summary>
    public static class MyEvents
    {
        public static CustomGameEvent MenuSceneOpened = new();
        public static CustomGameEvent MainSceneOpened = new();

        public static CustomGameEvent<bool> CameraPreviewChanged = new();
        public static CustomGameEvent<bool> CalibrationChanged = new();
        public static CustomGameEvent<int> ScreenSizeChanged = new();
        public static CustomGameEvent<int> ScreenDistanceChanged = new();
        public static CustomGameEvent<string> CameraChanged = new();

        public static CustomGameEvent<string> ModelAdded = new();
        public static CustomGameEvent<string> ModelRemoved = new();
        public static CustomGameEvent ModelsLoadingStart = new();

        /// <summary>
        ///     (loadedCount, totalCount)
        /// </summary>
        public static CustomGameEvent<(int, int)> ModelLoaded = new();

        public static CustomGameEvent ModelsLoadingEnd = new();
        public static CustomGameEvent<float> SceneLoadProgress = new();

        /// <summary>
        ///     True if found any faces, false otherwise.
        /// </summary>
        public static CustomGameEvent<bool> FaceDetectionDone = new();

        // wtf
        public static CustomGameEvent ModelsRemoveRequest = new();

        public static CustomGameEvent CameraUpdated = new();
    }
}