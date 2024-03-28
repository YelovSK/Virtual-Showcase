using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.FaceTracking;
using VirtualShowcase.FaceTracking.GlassesCheck;
using VirtualShowcase.FaceTracking.Marker;
using VirtualShowcase.FaceTracking.Transform;
using VirtualShowcase.Utilities;

namespace VirtualShowcase
{
    public sealed class Mediator : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private RawImage previewUI;
        [SerializeField] private Texture2D defaultCamTexture;
        [SerializeField] private CameraTransform cameraTransform;
        [SerializeField] private KeyPointsUpdater keyPointsUpdater;

        #endregion

        private ColourChecker colorChecker;
        private RawImage colorOverlay;
        private Detector detector;
        private TMP_Text distanceText;
        private EyeTracker eyeTracker;

        #region Event Functions

        private void Awake()
        {
            Events.CameraUpdated.AddListener(_ => HandleCameraUpdate());

            colorChecker = GetComponent<ColourChecker>();
            eyeTracker = GetComponent<EyeTracker>();
            detector = GetComponent<Detector>();

            distanceText = previewUI.GetComponentInChildren<TMP_Text>();
            colorOverlay = previewUI.GetComponentsInChildren<RawImage>().First(x => x.gameObject != previewUI.gameObject);
        }

        private void Start()
        {
            WebcamInput.Instance.ChangeWebcam(MyPrefs.CameraName);

            // Broken webcam, image set to "NO WEBCAM SHOWING".
            if (!WebcamInput.Instance.IsCameraRunning)
                previewUI.texture = defaultCamTexture;
        }

        private void HandleCameraUpdate()
        {
            previewUI.texture = WebcamInput.Instance.Texture;

            bool faceFound = detector.RunDetector(WebcamInput.Instance.Texture);

            if (!faceFound)
            {
                HideColorOverlay();
                return;
            }

            eyeTracker.SmoothEyes();

            keyPointsUpdater.UpdateKeyPoints();

            if (MyPrefs.GlassesCheck == false) HideColorOverlay();

            bool glassesOn = MyPrefs.GlassesCheck == false || colorChecker.CheckGlassesOn(WebcamInput.Instance.Texture, colorOverlay, distanceText);

            UpdateHeadDistanceUI();

            if (glassesOn && MySceneManager.Instance.IsInMainScene)
                cameraTransform.Transform();
        }

        #endregion

        private void HideColorOverlay()
        {
            distanceText.text = string.Empty;
            colorOverlay.gameObject.SetActive(false);
        }

        private void UpdateHeadDistanceUI()
        {
            // Threshold in cm for distance to be considered "close" to the calibrated distance.
            const int threshold = 10;
            var currentDistance = (int) EyeTracker.GetRealHeadDistance();
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
                differenceText = "+" + differenceText;

            // Update UI. Text in brackets is smaller. Difference and current distance is coloured.
            float size = distanceText.fontSize / 2.2f;
            distanceText.text =
                $"<color={color}>{differenceText}</color> <size={size}>(<color={color}>{currentDistance}cm</color> vs {calibratedDistance}cm)</size>";
            distanceText.ForceMeshUpdate();
        }
    }
}