using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Serialization;

namespace VirtualVitrine.FaceTracking
{
    public sealed class WebcamInput : MonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("_aspectRatio")] [SerializeField] private Vector2Int aspectRatio = new Vector2Int(1, 1);
        #endregion
        
        #region Public Fields
        public RenderTexture Texture { get; private set; }
        public WebCamTexture WebCamTexture { get; private set; }
        public bool IsCameraRunning => Texture != null && WebCamTexture.isPlaying;
        public bool CameraUpdated => Texture != null && WebCamTexture.didUpdateThisFrame;
        #endregion

        #region Unity Methods
        private async void Awake()
        {
            WebCamTexture = new WebCamTexture(PlayerPrefs.GetString("cam"));
            WebCamTexture.Play();
            // takes a bit for the webcam to initialize
            while (WebCamTexture.width == 16 || WebCamTexture.height == 16)
                await Task.Yield();
            Texture = new RenderTexture(WebCamTexture.width, WebCamTexture.height, 0);
        }

        private void Update()
        {
            if (!WebCamTexture.didUpdateThisFrame)
                return;
            var aspect1 = (float) WebCamTexture.width / WebCamTexture.height;
            var aspect2 = (float) aspectRatio.x / aspectRatio.y;
            var gap = aspect2 / aspect1;

            var vflip = WebCamTexture.videoVerticallyMirrored;
            var scale = new Vector2(gap, vflip ? -1 : 1);
            var offset = new Vector2((1 - gap) / 2, vflip ? 1 : 0);

            // put buffer (default 1:1 aspect ratio) into webcam
            Graphics.Blit(WebCamTexture, Texture, scale, offset);
        }

        private void OnDestroy()
        {
            if (WebCamTexture == null)
                return;
            WebCamTexture.Stop();
            Destroy(WebCamTexture);
            Destroy(Texture);
        }
        #endregion
    }
}