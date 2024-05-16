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

        private Vector2 _originalPivot;
        private Vector3 _originalPosition;
        private RectTransform _rectTransform;

        #region Event Functions

        private void Awake()
        {
            _image = GetComponent<RawImage>();
            _rectTransform = GetComponent<RectTransform>();

            _originalPivot = _rectTransform.pivot;
            _originalPosition = _rectTransform.position;

            WebcamInput.CameraChanged.AddListener(OnCameraChanged);
            Detector.FaceDetected.AddListener(OnFaceDetected);
            Detector.FaceNotDetected.AddListener(OnFaceNotDetected);

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

        private void OnFaceNotDetected()
        {
            distanceText.gameObject.SetActive(false);
            keyPointsUpdater.gameObject.SetActive(false);
        }

        private void OnFaceDetected(FaceDetection detection)
        {
            keyPointsUpdater.UpdateKeyPoints(detection);
            UpdateHeadDistanceUI(detection);

            distanceText.gameObject.SetActive(true);
            keyPointsUpdater.gameObject.SetActive(true);
        }

        /// <summary>
        ///     This is relevant only in the showcase scene. Stupid.
        /// </summary>
        public void ShowSmallPreview()
        {
            Enable();

            // Stupid.
            _rectTransform.pivot = new Vector2(0, 0);
            _rectTransform.position = new Vector3(0, 0, 0);
            transform.localScale = Vector3.one * 0.5f;
        }

        public void ShowLargePreview()
        {
            Enable();

            _rectTransform.pivot = _originalPivot;
            _rectTransform.position = _originalPosition;
            transform.localScale = Vector3.one;
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

        private void UpdateHeadDistanceUI(FaceDetection detection)
        {
            // Uncalibrated.
            if (MyPrefs.CalibratedFocalLength.EqualsWithDelta(-1))
            {
                distanceText.text = "<size=50><color=red>Uncalibrated</color></size>";
                return;
            }

            // Threshold in cm for distance to be considered "close" to the calibrated distance.
            const int threshold = 10;
            var currentDistance = (int)detection.GetRealHeadDistance(MyPrefs.CalibratedFocalLength);

            // Green text if within threshold, else red.
            string color = Math.Abs(currentDistance - MyPrefs.ScreenDistance) <= threshold ? "green" : "red";

            // Difference in cm, show "+" if too far, "-" if too close.
            int difference = currentDistance - MyPrefs.ScreenDistance;
            string differenceText = difference + "cm";
            if (difference > 0)
            {
                differenceText = "+" + differenceText;
            }

            // Update UI. Text in brackets is smaller. Difference and current distance is coloured.
            float size = distanceText.fontSize / 2.2f;
            distanceText.text =
                $"<color={color}>{differenceText}</color> <size={size}>(<color={color}>{currentDistance}cm</color> vs {MyPrefs.ScreenDistance}cm)</size>";
            distanceText.ForceMeshUpdate();
        }
    }
}