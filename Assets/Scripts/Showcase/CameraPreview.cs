using UnityEngine;

namespace VirtualShowcase.Showcase
{
    public class CameraPreview : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private GameObject camPreview;

        [SerializeField]
        private GameObject darkenImage;

        #endregion

        private Vector3 _originalPosition;

        private Vector3 _originalScale;

        #region Event Functions

        private void Awake()
        {
            _originalScale = camPreview.transform.localScale;
            _originalPosition = camPreview.transform.localPosition;
            Disable();
        }

        #endregion

        public void ShowSmallPreview()
        {
            Enable();

            // Stupid.
            camPreview.transform.localScale = Vector3.one * 0.5f;
            camPreview.transform.localPosition = new Vector3(-710, -310, 0);
        }

        public void ShowLargePreview()
        {
            Enable();

            camPreview.transform.localScale = _originalScale;
            camPreview.transform.localPosition = _originalPosition;
        }

        public void Enable()
        {
            camPreview.gameObject.SetActive(true);
            darkenImage.gameObject.SetActive(true);
        }

        public void Disable()
        {
            camPreview.gameObject.SetActive(false);
            darkenImage.gameObject.SetActive(false);
        }
    }
}