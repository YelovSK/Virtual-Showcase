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
            SetCamPreview();
            SetStereo();
            await ModelLoader.Instance.LoadObjects();
        }

        #endregion


        public void SetCamPreview(bool toggle = false)
        {
            if (toggle)
                MyPrefs.PreviewOn = !MyPrefs.PreviewOn;
            camPreview.gameObject.SetActive(MyPrefs.PreviewOn);
            darkenImage.gameObject.SetActive(MyPrefs.PreviewOn);
        }

        public void SetStereo(bool toggle = false)
        {
            if (toggle)
                MyPrefs.StereoOn = !MyPrefs.StereoOn;
            bool stereoOn = MyPrefs.StereoOn;

            monoCam.SetActive(!stereoOn);
            leftCam.SetActive(stereoOn);
            rightCam.SetActive(stereoOn);
        }
    }
}