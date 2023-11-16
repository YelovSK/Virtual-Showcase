using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;

namespace VirtualVitrine.FaceTracking
{
    public sealed class WebcamInput : MonoBehaviour
    {
        private static bool _mirrored;
        private static Color32[] colorBuffer;
        public static int FramesBetweenUpdates;
        public static int AverageFramesBetweenUpdates;

        private readonly List<int> _framesBetweenUpdatesHistory = new();

        public static Texture2D FinalTexture { get; private set; }
        public static WebCamTexture WebcamTexture { get; private set; }
        public static bool IsCameraRunning => FinalTexture != null && WebcamTexture.isPlaying;
        public static bool CameraUpdated => FinalTexture != null && WebcamTexture.didUpdateThisFrame;

        #region Event Functions

        private void Awake()
        {
            ChangeWebcam();

            print($"Webcam resolution: {WebcamTexture.width}x{WebcamTexture.height}");
            print($"Webcam is mirrored: {_mirrored}");
        }

        private void LateUpdate()
        {
            if (WebcamTexture.didUpdateThisFrame)
            {
                CalculateAverageFramesBetweenUpdates();
                FramesBetweenUpdates = 0;
            }
            else FramesBetweenUpdates++;
        }

        private void OnDestroy()
        {
            WebcamTexture.Stop();
            Destroy(WebcamTexture);
            Destroy(FinalTexture);
        }

        #endregion

        private static void Initialize()
        {
            int largerDimension = Math.Max(WebcamTexture.width, WebcamTexture.height);

            // Initialize target texture.
            FinalTexture = new Texture2D(largerDimension, largerDimension, TextureFormat.RGBA32, false);
            var borderCol = new Color32(0, 0, 0, 150);
            Color32[] pixels = Enumerable.Repeat(borderCol, FinalTexture.width * FinalTexture.height).ToArray();
            FinalTexture.SetPixels32(pixels);

            // Initialize color buffer.
            colorBuffer = new Color32[WebcamTexture.width * WebcamTexture.height];

            // Check if device is mirrored.
            WebCamDevice device = WebCamTexture.devices.FirstOrDefault(x => x.name == WebcamTexture.deviceName);
            _mirrored = device.isFrontFacing;
        }

        private void CalculateAverageFramesBetweenUpdates()
        {
            const int history_count = 10;
            if (_framesBetweenUpdatesHistory.Count == history_count)
                _framesBetweenUpdatesHistory.RemoveAt(0);

            _framesBetweenUpdatesHistory.Add(FramesBetweenUpdates);

            // Take the highest from the lowest 80% to avoid outliers.
            // Technically not average, but naming is difficult.
            AverageFramesBetweenUpdates = _framesBetweenUpdatesHistory
                .OrderByDescending(x => x)
                .TakeLast(history_count - 2)
                .Max();
        }

        public static async void ChangeWebcam()
        {
            if (WebcamTexture == null) WebcamTexture = new WebCamTexture(MyPrefs.CameraName);
            WebcamTexture.Stop();
            WebcamTexture.deviceName = MyPrefs.CameraName;
            WebcamTexture.requestedFPS = 500; // This should hopefully get the highest FPS of the webcam.
            WebcamTexture.Play();

            // Might take a bit for the webcam to initialize (thanks Unity).
            while (WebcamTexture.width == 16 || WebcamTexture.height == 16)
            {
                await Task.Yield();
            }

            Initialize();
        }

        public static void SetAspectRatio()
        {
            int startY = (FinalTexture.height - WebcamTexture.height) / 2;
            WebcamTexture.GetPixels32(colorBuffer);

            if (_mirrored)
            {
                var job = new MirrorJob
                {
                    Width = WebcamTexture.width,
                };
                JobHandle jobHandle = job.Schedule(WebcamTexture.height, 10);
                jobHandle.Complete();
            }

            FinalTexture.SetPixels32(0, startY, WebcamTexture.width, WebcamTexture.height, colorBuffer);
            FinalTexture.Apply();
        }

        private struct MirrorJob : IJobParallelFor
        {
            public int Width;

            public void Execute(int index)
            {
                Array.Reverse(colorBuffer, index * Width, Width);
            }
        }
    }
}