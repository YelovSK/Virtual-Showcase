using System;
using System.Collections.Generic;
using System.Linq;
using MediaPipe.BlazeFace;
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
        [SerializeField] private ResourceSet resources;
        [SerializeField] private Texture2D defaultCamTexture;
        [SerializeField] private KeyPointsUpdater keyPointsUpdater;
        [SerializeField] private CameraTransform cameraTransform;

        #endregion

        private ColourChecker colourChecker;

        // Barracuda face detector
        private FaceDetector detector;

        // text showing the face distance from the camera
        private TMP_Text distanceText;
        private EyeSmoother eyeSmoother;

        #region Event Functions

        private void Awake()
        {
            colourChecker = GetComponent<ColourChecker>();
            eyeSmoother = GetComponent<EyeSmoother>();
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
            detector = new FaceDetector(resources);
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
            // Check if camera got new frame.
            if (!WebcamInput.CameraUpdated)
                return;

            // Set aspect ratio of camera to 1:1.
            WebcamInput.SetAspectRatio();

            // Update camera preview.
            previewUI.texture = WebcamInput.RenderTexture;

            // Run detection and update marker in UI.
            bool faceFound = RunDetector(WebcamInput.RenderTexture);

            // If face not found, hide UI and return.
            if (!faceFound)
            {
                distanceText.text = "";
                colourChecker.HideUI();
                return;
            }

            // Smooth left and right eye key points.
            eyeSmoother.SmoothEyes();

            // Update key points in UI.
            keyPointsUpdater.UpdateKeyPoints();

            // Check colour around eyes.
            bool glassesOn = colourChecker.CheckGlassesOn();

            UpdateHeadDistanceInUI();

            // If glasses found and in main scene, transform camera.
            if (glassesOn && SceneSwitcher.InMainScene)
                cameraTransform.Transform();
        }

        private void OnDestroy()
        {
            detector?.Dispose();
            distanceText.text = "";
        }

        #endregion


        /// <summary>
        ///     Runs detection and returns true if face was found
        /// </summary>
        /// <param name="input">Texture to detect from</param>
        /// <returns>True if face was found, False if wasn't found</returns>
        private bool RunDetector(Texture input)
        {
            // Face detection.
            detector.ProcessImage(input, MyPrefs.DetectionThreshold);

            // Check if any detections were found.
            List<FaceDetector.Detection> detections = detector.Detections.ToList();
            bool faceFound = detections.Any();

            // Activate/Deactivate marker if face was/wasn't found.
            keyPointsUpdater.gameObject.SetActive(faceFound);

            if (faceFound)
            {
                // Get detection with largest bounding box.
                FaceDetector.Detection largestFace = detections
                    .OrderByDescending(x => x.extent.magnitude)
                    .First();
                KeyPointsUpdater.Detection = largestFace;
            }

            return faceFound;
        }

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