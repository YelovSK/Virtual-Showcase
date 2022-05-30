using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualVitrine.FaceTracking
{
    public sealed class WebcamInput : MonoBehaviour
    {
        public static bool Mirrored;

        #region Serialized Fields

        [SerializeField] private int resolutionWidth = 1280;

        #endregion

        public static RenderTexture RenderTexture { get; private set; }
        public static WebCamTexture WebcamTexture { get; private set; }
        public static bool IsCameraRunning => RenderTexture != null && WebcamTexture.isPlaying;
        public static bool CameraUpdated => RenderTexture != null && WebcamTexture.didUpdateThisFrame;

        #region Event Functions

        private async void Awake()
        {
            WebcamTexture = new WebCamTexture(MyPrefs.CameraName, resolutionWidth, resolutionWidth);
            WebcamTexture.Play();

            // Takes a bit for the webcam to initialize.
            // Might not be needed anymore, seems to work without it.
            while (WebcamTexture.width == 16 || WebcamTexture.height == 16)
                await Task.Yield();

            int smallerDimension = Math.Min(WebcamTexture.width, WebcamTexture.height);
            int largerDimension = Math.Max(WebcamTexture.width, WebcamTexture.height);
            RenderTexture = new RenderTexture(largerDimension, largerDimension, 0);
            Mirrored = WebCamTexture.devices.FirstOrDefault(x => x.name == WebcamTexture.deviceName).isFrontFacing;
            print($"Webcam resolution: {WebcamTexture.width}x{WebcamTexture.height}");
            print($"Webcam is mirrored: {Mirrored}");
        }

        private void OnDestroy()
        {
            WebcamTexture.Stop();
            RenderTexture.Release();
            Destroy(WebcamTexture);
            Destroy(RenderTexture);
        }

        #endregion

        public static void ChangeWebcam()
        {
            WebcamTexture.Stop();
            WebcamTexture.deviceName = MyPrefs.CameraName;
            WebcamTexture.Play();
            Mirrored = WebCamTexture.devices.FirstOrDefault(x => x.name == WebcamTexture.deviceName).isFrontFacing;
        }

        public static void SetAspectRatio()
        {
            float aspect = (float) WebcamTexture.width / WebcamTexture.height;
            var scale = new Vector2(Mirrored ? -1 : 1, aspect);
            var offset = new Vector2(Mirrored ? 1 : 0, (1 - aspect) / 2);

            // Put 1:1 WebCamTexture into RenderTexture.
            Graphics.Blit(WebcamTexture, RenderTexture, scale, offset);
        }
    }
}