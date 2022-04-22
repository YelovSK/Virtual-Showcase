using System;
using System.Threading.Tasks;
using UnityEngine;

namespace VirtualVitrine.FaceTracking
{
    public sealed class WebcamInput : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private int resolutionWidth;

        #endregion


        public RenderTexture RenderTexture { get; private set; }
        public WebCamTexture WebCamTexture { get; private set; }
        public bool IsCameraRunning => RenderTexture != null && WebCamTexture.isPlaying;
        public bool CameraUpdated => RenderTexture != null && WebCamTexture.didUpdateThisFrame;

        #region Event Functions

        private async void Awake()
        {
            WebCamTexture = new WebCamTexture(MyPrefs.CameraName, resolutionWidth, resolutionWidth);
            WebCamTexture.Play();

            // Takes a bit for the webcam to initialize.
            while (WebCamTexture.width == 16 || WebCamTexture.height == 16)
                await Task.Yield();

            int smallerDimension = Math.Min(WebCamTexture.width, WebCamTexture.height);
            RenderTexture = new RenderTexture(smallerDimension, smallerDimension, 0);
        }

        private void OnDestroy()
        {
            if (WebCamTexture == null)
                return;
            WebCamTexture.Stop();
            Destroy(WebCamTexture);
            Destroy(RenderTexture);
        }

        #endregion


        public void SetAspectRatio()
        {
            if (!WebCamTexture.didUpdateThisFrame)
                return;

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