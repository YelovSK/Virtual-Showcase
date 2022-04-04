using UnityEngine;

namespace VirtualVitrine
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
        private void SetCamPreview(bool toggle = false)
        {
            if (toggle)
                PlayerPrefs.SetInt("previewOn", PlayerPrefs.GetInt("previewOn") == 0 ? 1 : 0);
            canvasGroup.alpha = PlayerPrefs.GetInt("previewOn");
        }

        private void SetStereo(bool toggle = false)
        {
            if (toggle)
                PlayerPrefs.SetInt("stereoOn", PlayerPrefs.GetInt("stereoOn") == 0 ? 1 : 0);
            var stereoOn = PlayerPrefs.GetInt("stereoOn") == 1;
            
            monoCam.SetActive(!stereoOn);
            leftCam.SetActive(stereoOn);
            rightCam.SetActive(stereoOn);
        }

        private void CheckKeyInput()
        {
            if (Input.GetKeyDown("p"))
                SetCamPreview(toggle: true);

            if (Input.GetKeyDown(KeyCode.Tab))
                SetStereo(toggle: true);
        }

        #endregion
    }
}