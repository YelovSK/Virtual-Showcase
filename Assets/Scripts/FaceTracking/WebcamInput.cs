using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualVitrine.FaceTracking
{
    public sealed class WebcamInput : MonoBehaviour
    {
        private static WebcamInput instance;

        #region Serialized Fields

        [SerializeField] private int resolutionWidth;

        #endregion

        public static RenderTexture RenderTexture { get; private set; }
        public static WebCamTexture WebCamTexture { get; private set; }
        public static bool IsCameraRunning => RenderTexture != null && WebCamTexture.isPlaying;
        public static bool CameraUpdated => RenderTexture != null && WebCamTexture.didUpdateThisFrame;

        #region Event Functions

        private async void Awake()
        {
            // Singleton stuff.
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                WebCamTexture = new WebCamTexture(MyPrefs.CameraName, resolutionWidth, resolutionWidth);
                WebCamTexture.Play();

                // Takes a bit for the webcam to initialize.
                // Might not be needed anymore, seems to work without it.
                while (WebCamTexture.width == 16 || WebCamTexture.height == 16)
                    await Task.Yield();

                int smallerDimension = Math.Min(WebCamTexture.width, WebCamTexture.height);
                RenderTexture = new RenderTexture(smallerDimension, smallerDimension, 0);
            }
            else if (instance != this) Destroy(gameObject);
        }

        #endregion

        public static void ChangeWebcam()
        {
            WebCamTexture.Stop();
            WebCamTexture.deviceName = MyPrefs.CameraName;
            WebCamTexture.Play();
        }

        public static void SetAspectRatio()
        {
            float aspect = (float) WebCamTexture.width / WebCamTexture.height;
            float gap = 1 / aspect;
            bool vflip = WebCamTexture.videoVerticallyMirrored;
            var scale = new Vector2(gap, vflip ? -1 : 1);
            var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);

            // Put 1:1 WebCamTexture into RenderTexture.
            Graphics.Blit(WebCamTexture, RenderTexture, scale, offset);
        }
    }
}