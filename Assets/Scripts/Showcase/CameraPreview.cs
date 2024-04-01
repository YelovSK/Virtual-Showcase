using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.FaceTracking;
using VirtualShowcase.FaceTracking.Marker;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Showcase
{
    public class CameraPreview : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private Texture2D defaultCamTexture;

        [SerializeField]
        private KeyPointsUpdater keyPointsUpdater;

        [SerializeField]
        private TMP_Text distanceText;

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

            WebcamInput.Instance.CameraChanged.AddListener(OnCameraChanged);
            MyEvents.FaceDetectionDone.AddListener(UpdateUI);

            if (MyPrefs.PreviewOn)
            {
                Enable();
            }
            // Stupid.
            else if (SceneManager.GetActiveScene().name != MySceneManager.MENU_SCENE_NAME)
            {
                Disable();
            }
        }

        private void Start()
        {
            // Camera is not yet running in Awake. Stupid. Event doesn't get called on scene load.
            OnCameraChanged();
        }

        #endregion

        private void UpdateUI(GameObject sender, bool faceFound)
        {
            if (faceFound)
            {
                keyPointsUpdater.UpdateKeyPoints();
                UpdateHeadDistanceUI();
            }

            distanceText.gameObject.SetActive(faceFound);
            keyPointsUpdater.gameObject.SetActive(faceFound);
        }

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

        private void OnCameraChanged()
        {
            if (WebcamInput.Instance.IsCameraRunning)
            {
                _image.texture = WebcamInput.Instance.Texture;
                distanceText.gameObject.SetActive(true);
                keyPointsUpdater.gameObject.SetActive(true);
            }
            else
            {
                _image.texture = defaultCamTexture;
                distanceText.gameObject.SetActive(false);
                keyPointsUpdater.gameObject.SetActive(false);
            }
        }

        private void UpdateHeadDistanceUI()
        {
            // Threshold in cm for distance to be considered "close" to the calibrated distance.
            const int threshold = 10;
            var currentDistance = (int)EyeTracker.GetRealHeadDistance();
            int calibratedDistance = MyPrefs.ScreenDistance;

            // Uncalibrated.
            if (currentDistance == 0)
            {
                distanceText.text = "<size=50><color=red>Uncalibrated</color></size>";
                return;
            }

            // Green text if within threshold, else red.
            string color = Math.Abs(currentDistance - calibratedDistance) <= threshold ? "green" : "red";

            // Difference in cm, show "+" if too far, "-" if too close.
            int difference = currentDistance - calibratedDistance;
            string differenceText = difference + "cm";
            if (difference > 0)
            {
                differenceText = "+" + differenceText;
            }

            // Update UI. Text in brackets is smaller. Difference and current distance is coloured.
            float size = distanceText.fontSize / 2.2f;
            distanceText.text =
                $"<color={color}>{differenceText}</color> <size={size}>(<color={color}>{currentDistance}cm</color> vs {calibratedDistance}cm)</size>";
            distanceText.ForceMeshUpdate();
        }
    }
}