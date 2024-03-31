using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualShowcase.Core;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking
{
    public class WebcamInput : MonoSingleton<WebcamInput>
    {
        private readonly List<int> _framesBetweenUpdatesHistory = new();
        private Color32[] _colorBuffer;
        private bool _isMirrored;
        private WebCamTexture _webcamTexture;

        public int FramesBetweenUpdates { get; private set; }
        public int AverageFramesBetweenUpdates { get; private set; }

        public Texture2D Texture { get; private set; }

        public bool IsCameraRunning => Texture != null && _webcamTexture != null && _webcamTexture.isPlaying;
        public bool CameraUpdatedThisFrame => Texture != null && _webcamTexture != null && _webcamTexture.didUpdateThisFrame;
        public string DeviceName => _webcamTexture?.deviceName;

        #region Event Functions

        private void Start()
        {
            MyEvents.CameraChanged.AddListener((sender, cameraName) => ChangeWebcam(cameraName));
            // Webcam gets paused when switching scenes.
            SceneManager.sceneLoaded += (scene, mode) => _webcamTexture?.Play();
        }

        private void Update()
        {
            if (_webcamTexture is null || !_webcamTexture.isPlaying)
            {
                return;
            }

            if (_webcamTexture.didUpdateThisFrame)
            {
                CalculateAverageFramesBetweenUpdates();
                FramesBetweenUpdates = 0;
                UpdateTexture2D();
                MyEvents.CameraUpdated.Invoke(gameObject);
            }
            else
            {
                FramesBetweenUpdates++;
            }
        }

        private void OnDestroy()
        {
            if (_webcamTexture != null)
            {
                _webcamTexture.Stop();
                Destroy(_webcamTexture);
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

            _isMirrored = device.isFrontFacing;

            if (_webcamTexture == null)
            {
                _webcamTexture = new WebCamTexture(device.name);
                _webcamTexture.requestedFPS = int.MaxValue;
                _webcamTexture.Play();
            }
            else if (_webcamTexture.deviceName != device.name)
            {
                _webcamTexture.Stop();
                _webcamTexture.deviceName = device.name;
                _webcamTexture.requestedFPS = int.MaxValue;
                _webcamTexture.Play();
            }
            else if (!_webcamTexture.isPlaying)
            {
                _webcamTexture.Play();
            }

            // Might take a bit for the webcam to initialize (thanks Unity).
            while (_webcamTexture.width == 16 || _webcamTexture.height == 16)
            {
                await Task.Yield();
            }

            Initialize();

            print($"Webcam resolution: {_webcamTexture.width}x{_webcamTexture.height}");
            print($"Webcam is mirrored: {_isMirrored}");
        }

        private void Initialize()
        {
            int largerDimension = Math.Max(_webcamTexture.width, _webcamTexture.height);

            // Initialize target texture.
            Texture = new Texture2D(largerDimension, largerDimension, TextureFormat.RGBA32, false);
            Color32[] pixels = Enumerable.Repeat(new Color32(0, 0, 0, 150), Texture.width * Texture.height).ToArray();
            Texture.SetPixels32(pixels);

            // Initialize color buffer.
            _colorBuffer = new Color32[_webcamTexture.width * _webcamTexture.height];
        }

        private void CalculateAverageFramesBetweenUpdates()
        {
            const int history_count = 10;
            if (_framesBetweenUpdatesHistory.Count == history_count)
            {
                _framesBetweenUpdatesHistory.RemoveAt(0);
            }

            _framesBetweenUpdatesHistory.Add(FramesBetweenUpdates);

            // Take the highest from the lowest 80% to avoid outliers.
            // Technically not average, but naming is difficult.
            AverageFramesBetweenUpdates = _framesBetweenUpdatesHistory
                .OrderByDescending(framesCount => framesCount)
                .TakeLast(history_count - 2)
                .Max();
        }

        private void UpdateTexture2D()
        {
            int startY = (Texture.height - _webcamTexture.height) / 2;
            _webcamTexture.GetPixels32(_colorBuffer);

            if (_isMirrored)
            {
                var job = new MirrorJob
                {
                    Width = _webcamTexture.width,
                };
                JobHandle jobHandle = job.Schedule(_webcamTexture.height, 10);
                jobHandle.Complete();
            }

            Texture.SetPixels32(0, startY, _webcamTexture.width, _webcamTexture.height, _colorBuffer);
            Texture.Apply();
        }

        private struct MirrorJob : IJobParallelFor
        {
            #region Serialized Fields

            public int Width;

            #endregion

            public void Execute(int index)
            {
                Array.Reverse(Instance._colorBuffer, index * Width, Width);
            }
        }
    }
}