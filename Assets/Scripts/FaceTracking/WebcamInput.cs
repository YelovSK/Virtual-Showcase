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
        private static bool mirrored;
        private static Color32[] colorBuffer;
        public static int FramesBetweenUpdates;
        public static int AverageFramesBetweenUpdates;

        #region Serialized Fields

        [SerializeField] private int resolutionWidth = 1280;

        #endregion

        private readonly List<int> framesBetweenUpdatesHistory = new();

        public static Texture2D FinalTexture { get; private set; }
        public static WebCamTexture WebcamTexture { get; private set; }
        public static bool IsCameraRunning => FinalTexture != null && WebcamTexture.isPlaying;
        public static bool CameraUpdated => FinalTexture != null && WebcamTexture.didUpdateThisFrame;

        #region Event Functions

        private async void Awake()
        {
            WebcamTexture = new WebCamTexture(MyPrefs.CameraName, resolutionWidth, resolutionWidth);
            WebcamTexture.Play();

            // Takes a bit for the webcam to initialize.
            // Might not be needed anymore, seems to work without it.
            while (WebcamTexture.width == 16 || WebcamTexture.height == 16)
                await Task.Yield();

            Initialize();

            print($"Webcam resolution: {WebcamTexture.width}x{WebcamTexture.height}");
            print($"Webcam is mirrored: {mirrored}");
        }

        private void Update()
        {
            if (!WebcamTexture.didUpdateThisFrame)
                FramesBetweenUpdates++;
        }

        private void LateUpdate()
        {
            if (WebcamTexture.didUpdateThisFrame)
            {
                CalculateAverageFramesBetweenUpdates();
                FramesBetweenUpdates = 0;
            }
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
            Color[] pixels = Enumerable.Repeat(Color.black, FinalTexture.width * FinalTexture.height).ToArray();
            FinalTexture.SetPixels(pixels);

            // Initialize color buffer.
            colorBuffer = new Color32[WebcamTexture.width * WebcamTexture.height];

            // Check if device is mirrored.
            mirrored = WebCamTexture.devices.FirstOrDefault(x => x.name == WebcamTexture.deviceName).isFrontFacing;
        }

        private void CalculateAverageFramesBetweenUpdates()
        {
            const int history_count = 10;
            if (framesBetweenUpdatesHistory.Count == history_count)
                framesBetweenUpdatesHistory.RemoveAt(0);

            framesBetweenUpdatesHistory.Add(FramesBetweenUpdates);

            // Take the highest from the lowest 80% to avoid outliers.
            // Technically not average, but naming is difficult.
            AverageFramesBetweenUpdates = framesBetweenUpdatesHistory
                .OrderByDescending(x => x)
                .TakeLast(history_count - 2)
                .Max();
        }

        public static void ChangeWebcam()
        {
            WebcamTexture.Stop();
            WebcamTexture.deviceName = MyPrefs.CameraName;
            WebcamTexture.Play();

            Initialize();
        }

        public static void SetAspectRatio()
        {
            int startY = (FinalTexture.height - WebcamTexture.height) / 2;
            WebcamTexture.GetPixels32(colorBuffer);

            if (mirrored)
            {
                var job = new FlipJob
                {
                    Width = WebcamTexture.width
                };
                JobHandle jobHandle = job.Schedule(WebcamTexture.height, 10);
                jobHandle.Complete();
            }

            FinalTexture.SetPixels32(0, startY, WebcamTexture.width, WebcamTexture.height, colorBuffer);
            FinalTexture.Apply();
        }

        private struct FlipJob : IJobParallelFor
        {
            public int Width;

            public void Execute(int index)
            {
                Array.Reverse(colorBuffer, index * Width, Width);
            }
        }
    }
}