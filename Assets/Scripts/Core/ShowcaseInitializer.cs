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
        [SerializeField] private CanvasGroup canvasGroup;
        
        [Header("Face tracking object")]
        [SerializeField] private GameObject faceTracking;
        #endregion
        
        #region Public Methods
        public void SetCamPreview(bool toggle = false)
        {
            if (toggle)
                PlayerPrefs.SetInt("previewOn", PlayerPrefs.GetInt("previewOn") == 0 ? 1 : 0);
            canvasGroup.alpha = PlayerPrefs.GetInt("previewOn");
        }

        public void SetStereo(bool toggle = false)
        {
            if (toggle)
                PlayerPrefs.SetInt("stereoOn", PlayerPrefs.GetInt("stereoOn") == 0 ? 1 : 0);
            var stereoOn = PlayerPrefs.GetInt("stereoOn") == 1;
            
            monoCam.SetActive(!stereoOn);
            leftCam.SetActive(stereoOn);
            rightCam.SetActive(stereoOn);
        }
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
        #endregion
    }
}