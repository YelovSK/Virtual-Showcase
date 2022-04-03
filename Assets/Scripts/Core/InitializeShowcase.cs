using UnityEngine;

namespace VirtualVitrine.Core
{
    public class InitializeShowcase : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Cameras")]
        [SerializeField] private GameObject leftCam;
        [SerializeField] private GameObject rightCam;
        [SerializeField] private GameObject monoCam;
        
        [Header("Canvas")]
        [SerializeField] private GameObject camPreview;
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Face tracking object")]
        [SerializeField] private GameObject faceTracking;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            GlobalManager.CheckPlayerPrefs();
            SetCamPreview();
            SetStereo();
        }

        private void Start()
        {
            faceTracking.SetActive(true);
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
            switch ((GlobalManager.PreviewType) PlayerPrefs.GetInt("previewIx"))
            {
                case GlobalManager.PreviewType.On:
                    camPreview.SetActive(true);
                    canvasGroup.alpha = 1;
                    break;
                case GlobalManager.PreviewType.Off:
                    camPreview.SetActive(true);
                    canvasGroup.alpha = 0;
                    break;
                case GlobalManager.PreviewType.DisabledTracking:
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
                if (PlayerPrefs.GetInt("stereo") == 0)
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