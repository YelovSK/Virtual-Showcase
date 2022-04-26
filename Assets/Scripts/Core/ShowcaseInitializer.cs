using UnityEngine;

namespace VirtualVitrine
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

        private void Start()
        {
            faceTracking.SetActive(true);
            SetCamPreview();
            SetStereo();
        }

        #endregion


        public void SetCamPreview(bool toggle = false)
        {
            if (toggle)
                MyPrefs.PreviewOn = MyPrefs.PreviewOn == 0 ? 1 : 0;
            camPreview.gameObject.SetActive(MyPrefs.PreviewOn == 1);
            darkenImage.gameObject.SetActive(MyPrefs.PreviewOn == 1);
        }

        public void SetStereo(bool toggle = false)
        {
            if (toggle)
                MyPrefs.StereoOn = MyPrefs.StereoOn == 0 ? 1 : 0;
            bool stereoOn = MyPrefs.StereoOn == 1;

            monoCam.SetActive(!stereoOn);
            leftCam.SetActive(stereoOn);
            rightCam.SetActive(stereoOn);
        }
    }
}