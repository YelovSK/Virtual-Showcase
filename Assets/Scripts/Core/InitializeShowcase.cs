using UnityEngine;

namespace VirtualVitrine.Core
{
    public class InitializeShowcase : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject leftCam;
        [SerializeField] private GameObject rightCam;
        [SerializeField] private GameObject monoCam;
        [SerializeField] private GameObject camPreview;
        [SerializeField] private CanvasGroup canvasGroup;
        #endregion

        #region Unity Methods
        private void Start()
        {
            StaticVars.CheckPlayerPrefs();
            SetCamPreview();
            SetStereo();
        }

        private void Update()
        {
            CheckKeyInput();
        }
        #endregion

        #region Private Methods
        private void SetStereo()
        {
            if (PlayerPrefs.GetInt("stereo") == 0)
                ActivateMono();
            else
                ActivateStereo();
        }

        private void ActivateMono()
        {
            monoCam.SetActive(true);
            leftCam.SetActive(false);
            rightCam.SetActive(false);
        }

        private void ActivateStereo()
        {
            monoCam.SetActive(false);
            leftCam.SetActive(true);
            rightCam.SetActive(true);
        }

        private void SetCamPreview()
        {
            if (PlayerPrefs.GetInt("previewIx") == 3)
                PlayerPrefs.SetInt("previewIx", 0);
            switch (PlayerPrefs.GetInt("previewIx"))
            {
                case 0: // Preview on
                    camPreview.SetActive(true);
                    canvasGroup.alpha = 1;
                    break;
                case 1: // Preview off
                    camPreview.SetActive(true);
                    canvasGroup.alpha = 0;
                    break;
                case 2: // Preview off and disabled tracking
                    camPreview.SetActive(false);
                    break;
            }
        }

        private void CheckKeyInput()
        {
            if (Input.GetKeyDown("f12"))
            {
                PlayerPrefs.SetInt("previewIx", PlayerPrefs.GetInt("previewIx") + 1);
                SetCamPreview();
            }

            if (Input.GetKeyDown(KeyCode.Tab))
            {
                var current = PlayerPrefs.GetInt("stereo");
                if (current == 0)
                {
                    PlayerPrefs.SetInt("stereo", 1);
                    ActivateStereo();
                }
                else
                {
                    PlayerPrefs.SetInt("stereo", 0);
                    ActivateMono();
                }
            }
        }
        #endregion
    }
}