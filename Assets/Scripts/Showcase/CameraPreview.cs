using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.FaceTracking;

namespace VirtualShowcase.Showcase
{
    public class CameraPreview : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private Texture2D defaultCamTexture;

        #endregion

        private RawImage _image;

        private Vector3 _originalPosition;
        private Vector3 _originalScale;

        #region Event Functions

        private void Awake()
        {
            _originalScale = transform.localScale;
            _originalPosition = transform.localPosition;
            _image = GetComponent<RawImage>();

            WebcamInput.Instance.CameraChanged.AddListener(SetTexture);
        }

        #endregion

        /// <summary>
        ///     This is relevant only in the showcase scene. Stupid.
        /// </summary>
        public void ShowSmallPreview()
        {
            Enable();

            // Stupid.
            transform.localScale = Vector3.one * 0.5f;
            transform.localPosition = new Vector3(-710, -310, 0);
        }

        public void ShowLargePreview()
        {
            Enable();

            transform.localScale = _originalScale;
            transform.localPosition = _originalPosition;
        }

        public void Enable()
        {
            gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
        }

        private void SetTexture()
        {
            if (WebcamInput.Instance.IsCameraRunning)
            {
                _image.texture = WebcamInput.Instance.Texture;
            }
            else
            {
                _image.texture = defaultCamTexture;
            }
        }
    }
}