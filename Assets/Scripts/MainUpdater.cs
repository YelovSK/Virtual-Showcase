using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.FaceTracking;
using VirtualVitrine.FaceTracking.GlassesCheck;
using VirtualVitrine.FaceTracking.Marker;
using VirtualVitrine.FaceTracking.Transform;
using VirtualVitrine.MainScene;

namespace VirtualVitrine
{
    public sealed class MainUpdater : MonoBehaviour
    {
        private static bool initialized;

        #region Serialized Fields

        [SerializeField] private RawImage previewUI;
        [SerializeField] private Texture2D defaultCamTexture;
        [SerializeField] private CameraTransform cameraTransform;
        [SerializeField] private KeyPointsUpdater keyPointsUpdater;

        #endregion

        private ColourChecker colourChecker;
        private Detector detector;
        private TMP_Text distanceText;
        private EyeSmoother eyeSmoother;

        #region Event Functions

        private void Awake()
        {
            colourChecker = GetComponent<ColourChecker>();
            eyeSmoother = GetComponent<EyeSmoother>();
            detector = GetComponent<Detector>();
            distanceText = previewUI.GetComponentInChildren<TMP_Text>();

            // Spawn webcam input object at start.
            if (initialized) return;
            initialized = true;
            var webcamInputObject = new GameObject("WebcamInput");
            webcamInputObject.AddComponent<WebcamInput>();
            DontDestroyOnLoad(webcamInputObject);
        }

        private void Start()
        {
            WebcamInput.WebcamTexture.Play();

            // Broken webcam, image set to "NO WEBCAM SHOWING".
            if (!WebcamInput.IsCameraRunning)
                previewUI.texture = defaultCamTexture;
        }

        /// <summary>
        ///     Main loop of the program, calls other components. Detects face, updates UI and transforms camera.
        /// </summary>
        private void Update()
        {
            if (!WebcamInput.CameraUpdated)
                return;

            WebcamInput.SetAspectRatio();
            previewUI.texture = WebcamInput.FinalTexture;
            bool faceFound = detector.RunDetector(WebcamInput.FinalTexture);

            if (!faceFound)
            {
                distanceText.text = "";
                colourChecker.HideUI();
                return;
            }

            eyeSmoother.SmoothEyes();
            keyPointsUpdater.UpdateKeyPoints();
            bool glassesOn = colourChecker.CheckGlassesOn();
            UpdateHeadDistanceInUI();
            if (glassesOn && SceneSwitcher.InMainScene)
                cameraTransform.Transform();
        }

        #endregion

        private void UpdateHeadDistanceInUI()
        {
            // Threshold in cm for distance to be considered "close" to the calibrated distance.
            const int threshold = 10;
            var currentDistance = (int) CalibrationManager.GetRealHeadDistance();
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
            string difference = currentDistance - calibratedDistance + "cm";
            if (difference[0] != '-' && difference[0] != '0')
                difference = "+" + difference;

            // Update UI. Text in brackets is smaller. Difference and current distance is coloured.
            float size = distanceText.fontSize / 2.2f;
            distanceText.text =
                $"<color={color}>{difference}</color> <size={size}>(<color={color}>{currentDistance}cm</color> vs {calibratedDistance}cm)</size>";
            distanceText.ForceMeshUpdate();
        }
    }
}