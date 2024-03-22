using UnityEngine;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.MainScene
{
    public class ShowcaseInitializer : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Cameras")]
        [SerializeField] private GameObject leftCam;
        [SerializeField] private GameObject rightCam;
        [SerializeField] private GameObject monoCam;

        [Header("Canvas")]
        [SerializeField] private GameObject camPreview;
        [SerializeField] private GameObject darkenImage;

        [Header("Face tracking object")]
        [SerializeField] private GameObject faceTracking;

        #endregion

        #region Event Functions

        private void Awake()
        {
            MyPrefs.CheckPlayerPrefs();
        }

        private async void Start()
        {
            faceTracking.SetActive(true);
            SetCameraPreviewEnabled(MyPrefs.PreviewOn);
            SetStereo(MyPrefs.StereoOn);
            await ModelLoader.Instance.LoadObjects();
        }

        #endregion
        
        /// <summary>
        /// Lower left corner during calibration.
        /// </summary>
        public void ShowSmallCamPreview()
        {
            SetCameraPreviewEnabled(true);

            // Make the webcam preview smaller and put it in the left corner.
            camPreview.transform.localScale = Vector3.one * 0.5f;
            camPreview.transform.localPosition = new Vector3(-710, -310, 0);
        }

        public void SetCameraPreviewEnabled(bool previewEnabled)
        {
            camPreview.gameObject.SetActive(previewEnabled);
            darkenImage.gameObject.SetActive(previewEnabled);
        }
        
        public void SetStereo(bool stereoEnabled)
        {
            monoCam.SetActive(!stereoEnabled);
            leftCam.SetActive(stereoEnabled);
            rightCam.SetActive(stereoEnabled);
        }
    }
}