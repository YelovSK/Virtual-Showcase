using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using VirtualShowcase.Core;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking
{
    public class WebcamInput : MonoSingleton<WebcamInput>
    {
        private readonly List<int> framesBetweenUpdatesHistory = new();
        private Color32[] colorBuffer;
        private bool isMirrored;
        private WebCamTexture webcamTexture;

        public int FramesBetweenUpdates { get; private set; }
        public int AverageFramesBetweenUpdates { get; private set; }

        public Texture2D Texture { get; private set; }

        public bool IsCameraRunning => Texture != null && webcamTexture != null && webcamTexture.isPlaying;
        public bool CameraUpdatedThisFrame => Texture != null && webcamTexture != null && webcamTexture.didUpdateThisFrame;
        public string DeviceName => webcamTexture?.deviceName;

        #region Event Functions

        private void Start()
        {
            Events.CameraChanged.AddListener((sender, cameraName) => ChangeWebcam(cameraName));
            Events.CameraUpdated.AddListener(_ => UpdateTexture2D());
            // Webcam gets paused when switching scenes.
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += (scene, mode) => webcamTexture?.Play();
        }

        private void Update()
        {
            if (webcamTexture.didUpdateThisFrame)
            {
                CalculateAverageFramesBetweenUpdates();
                FramesBetweenUpdates = 0;
                Events.CameraUpdated.Invoke(gameObject);
            }
            else FramesBetweenUpdates++;
        }

        private void OnDestroy()
        {
            if (webcamTexture != null)
            {
                webcamTexture.Stop();
                Destroy(webcamTexture);
            }

            if (Texture != null)
            {
                Destroy(Texture);
            }
        }

        #endregion

        public async void ChangeWebcam(string deviceName)
        {
            if (DeviceName == deviceName)
            {
                return;
            }
            
            WebCamDevice[] devices = WebCamTexture.devices;
            if (devices.Length == 0)
            {
                print("No webcam devices were found");
                return;
            }

            WebCamDevice device = devices.First(x => x.name == deviceName);
            
            isMirrored = device.isFrontFacing;

            if (webcamTexture == null)
            {
                webcamTexture = new WebCamTexture(device.name);
                webcamTexture.requestedFPS = int.MaxValue;
                webcamTexture.Play();
            }
            else if (webcamTexture.deviceName != device.name)
            {
                webcamTexture.Stop();
                webcamTexture.deviceName = device.name;
                webcamTexture.requestedFPS = int.MaxValue;
                webcamTexture.Play();
            }
            else if (!webcamTexture.isPlaying) webcamTexture.Play();

            // Might take a bit for the webcam to initialize (thanks Unity).
            while (webcamTexture.width == 16 || webcamTexture.height == 16)
            {
                await Task.Yield();
            }

            Initialize();

            print($"Webcam resolution: {webcamTexture.width}x{webcamTexture.height}");
            print($"Webcam is mirrored: {isMirrored}");
        }

        private void Initialize()
        {
            int largerDimension = Math.Max(webcamTexture.width, webcamTexture.height);

            // Initialize target texture.
            Texture = new Texture2D(largerDimension, largerDimension, TextureFormat.RGBA32, false);
            Color32[] pixels = Enumerable.Repeat(new Color32(0, 0, 0, 150), Texture.width * Texture.height).ToArray();
            Texture.SetPixels32(pixels);

            // Initialize color buffer.
            colorBuffer = new Color32[webcamTexture.width * webcamTexture.height];
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
                .OrderByDescending(framesCount => framesCount)
                .TakeLast(history_count - 2)
                .Max();
        }

        private void UpdateTexture2D()
        {
            int startY = (Texture.height - webcamTexture.height) / 2;
            webcamTexture.GetPixels32(colorBuffer);

            if (isMirrored)
            {
                var job = new MirrorJob
                {
                    Width = webcamTexture.width,
                };
                JobHandle jobHandle = job.Schedule(webcamTexture.height, 10);
                jobHandle.Complete();
            }

            Texture.SetPixels32(0, startY, webcamTexture.width, webcamTexture.height, colorBuffer);
            Texture.Apply();
        }

        private struct MirrorJob : IJobParallelFor
        {
            public int Width;

            public void Execute(int index)
            {
                Array.Reverse(Instance.colorBuffer, index * Width, Width);
            }
        }
    }
}