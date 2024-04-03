using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VirtualShowcase.Core;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking
{
    public class WebcamInput : MonoSingleton<WebcamInput>
    {
        [NonSerialized]
        public static UnityEvent CameraChanged = new();

        [NonSerialized]
        public static UnityEvent CameraUpdated = new();

        #region Serialized Fields

        [SerializeField]
        private ComputeShader webcamToRenderTextureShader;

        #endregion

        private readonly List<int> _framesBetweenUpdatesHistory = new();
        private bool _isFrontFacing;
        private WebCamTexture _webcamTexture;

        public int FramesBetweenUpdates { get; private set; }
        public int AverageFramesBetweenUpdates { get; private set; }
        public RenderTexture Texture { get; private set; }

        public bool IsCameraRunning => Texture != null && _webcamTexture != null && _webcamTexture.isPlaying;
        public bool CameraUpdatedThisFrame => Texture != null && _webcamTexture != null && _webcamTexture.didUpdateThisFrame;
        public string DeviceName => _webcamTexture?.deviceName;

        #region Event Functions

        private void Start()
        {
            ChangeWebcam(MyPrefs.CameraName);
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

            if (!_webcamTexture.didUpdateThisFrame)
            {
                FramesBetweenUpdates++;
                return;
            }

            CalculateAverageFramesBetweenUpdates();
            FramesBetweenUpdates = 0;
            CopyToRenderTexture();
            CameraUpdated.Invoke();
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
                Debug.LogWarning("No webcam devices were found");
                return;
            }

            WebCamDevice device = devices.First(x => x.name == deviceName);

            _isFrontFacing = device.isFrontFacing;

            if (_webcamTexture == null)
            {
                _webcamTexture = new WebCamTexture(device.name);
                _webcamTexture.requestedFPS = int.MaxValue;
                _webcamTexture.Play();
            }
            else if (_webcamTexture.deviceName != device.name)
            {
                _webcamTexture.Stop();
                _webcamTexture = new WebCamTexture(device.name);
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

            // Initialize target texture.
            int largerDimension = Math.Max(_webcamTexture.width, _webcamTexture.height);
            Texture = new RenderTexture(largerDimension, largerDimension, 24);
            Texture.enableRandomWrite = true;
            Texture.Create();

            print($"Webcam resolution: {_webcamTexture.width}x{_webcamTexture.height}");
            print($"Webcam is mirrored: {_isFrontFacing}");

            CameraChanged.Invoke();
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

        private void CopyToRenderTexture()
        {
            int startY = (Texture.height - _webcamTexture.height) / 2;

            int kernel = _isFrontFacing
                ? webcamToRenderTextureShader.FindKernel("CopyMirrored")
                : webcamToRenderTextureShader.FindKernel("Copy");

            webcamToRenderTextureShader.SetTexture(kernel, "InputImage", _webcamTexture);
            webcamToRenderTextureShader.SetTexture(kernel, "OutputImage", Texture);

            webcamToRenderTextureShader.SetInt("SourceWidth", _webcamTexture.width);
            webcamToRenderTextureShader.SetInt("SourceHeight", _webcamTexture.height);
            webcamToRenderTextureShader.SetInt("TargetWidth", Texture.width);
            webcamToRenderTextureShader.SetInt("TargetHeight", Texture.height);
            webcamToRenderTextureShader.SetInt("StartY", startY);

            const int shader_threads = 16;
            webcamToRenderTextureShader.Dispatch(kernel, Texture.width / shader_threads, Texture.height / shader_threads, 1);
        }
    }
}