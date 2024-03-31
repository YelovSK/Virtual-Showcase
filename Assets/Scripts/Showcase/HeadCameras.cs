using UnityEngine;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Showcase
{
    public class HeadCameras : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Cameras")]
        [SerializeField]
        private GameObject leftCam;

        [SerializeField]
        private GameObject rightCam;

        [SerializeField]
        private GameObject monoCam;

        #endregion

        #region Event Functions

        private void Start()
        {
            if (MyPrefs.StereoOn)
            {
                EnableStereo();
            }
            else
            {
                DisableStereo();
            }
        }

        #endregion

        public void EnableStereo()
        {
            leftCam.SetActive(true);
            rightCam.SetActive(true);
            monoCam.SetActive(false);
        }

        public void DisableStereo()
        {
            leftCam.SetActive(false);
            rightCam.SetActive(false);
            monoCam.SetActive(true);
        }
    }
}