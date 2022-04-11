using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazeFace;
using UnityEngine.SceneManagement;
using VirtualVitrine.FaceTracking.Marker;

namespace VirtualVitrine
{
    public sealed class MainUpdater : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private RawImage previewUI;
        [SerializeField] private ResourceSet resources;
        [SerializeField] private Texture2D defaultCamTexture;
        [SerializeField] private KeyPointsUpdater keyPointsUpdater;
        #endregion

        #region Private Fields
        // Barracuda face detector
        private FaceDetector _detector;
        
        // face tracking scripts
        private FaceTracking.WebcamInput _webcamInput;
        private FaceTracking.ColourChecker _colourChecker;
        private FaceTracking.EyeSmoother _eyeSmoother;
        private FaceTracking.Transform.CameraTransform _cameraTransform;
        
        // text showing the face distance from the camera
        private TMP_Text _distanceText;
        #endregion
        
        #region Unity Methods
        private void Awake()
        {
            _webcamInput = GetComponent<FaceTracking.WebcamInput>();
            _colourChecker = GetComponent<FaceTracking.ColourChecker>();
            _eyeSmoother = GetComponent<FaceTracking.EyeSmoother>();
            _cameraTransform = GetComponent<FaceTracking.Transform.CameraTransform>();
            _distanceText = previewUI.GetComponentInChildren<TMP_Text>();
        }

        private void Start()
        {
            // Broken webcam, image set to "NO WEBCAM SHOWING".
            if (!_webcamInput.IsCameraRunning)
            {
                previewUI.texture = defaultCamTexture;
                return;
            }
            
            _detector = new FaceDetector(resources);
        }

        /// <summary>
        /// Main loop of the program, calls other components. Detects face, updates UI and transforms camera.
        /// </summary>
        private void Update()
        {
            // Check if camera got new frame.
            if (!_webcamInput.CameraUpdated)
                return;

            // Set aspect ratio of camera to 1:1.
            _webcamInput.SetAspectRatio();
            
            // Update camera preview.
            previewUI.texture = _webcamInput.RenderTexture;

            // Run detection and update marker in UI.
            bool faceFound = RunDetector(_webcamInput.RenderTexture);

            // If face not found, hide UI and return.
            if (!faceFound)
            {
                _distanceText.text = "";
                _colourChecker.HideUI();
                return;
            }
            
            // Smooth left and right eye key points.
            _eyeSmoother.SmoothEyes();
            
            // Update key points in UI.
            keyPointsUpdater.UpdateKeyPoints();
            
            // Check colour around eyes.
            bool glassesOn = _colourChecker.CheckGlassesOn(_webcamInput.WebCamTexture);

            UpdateHeadDistanceInUI();
            
            // If glasses found and in main scene, transform camera.
            if (glassesOn && SceneManager.GetActiveScene().name == "Main")
                _cameraTransform.Transform();
        }
        
        private void OnDestroy()
        {
            _detector?.Dispose();
            _distanceText.text = "";
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        /// Runs detection and returns true if face was found
        /// </summary>
        /// <param name="input">Texture to detect from</param>
        /// <returns>True if face was found, False if wasn't found</returns>
        private bool RunDetector(Texture input)
        {
            // Face detection.
            _detector.ProcessImage(input, MyPrefs.DetectionThreshold);

            // Check if any detections were found.
            bool faceFound = _detector.Detections.Any();

            // Activate/Deactivate marker if face was/wasn't found.
            keyPointsUpdater.gameObject.SetActive(faceFound);

            if (faceFound)
            {
                // Get detection with largest bounding box.
                var largestFace = _detector.Detections
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
            var calibratedDistance = MyPrefs.ScreenDistance;
            
            // Green text if within threshold, else red.
            var color = Math.Abs(currentDistance - calibratedDistance) <= threshold ? "green" : "red";

            // Difference in cm, show "+" if too far, "-" if too close.
            var difference = (currentDistance - calibratedDistance) + "cm";
            if (difference[0] != '-' && difference[0] != '0')
                difference = "+" + difference;
            
            // Update UI. Text in brackets is smaller. Difference and current distance is coloured.
            var size = _distanceText.fontSize / 2.2f;
            _distanceText.text = $"<color={color}>{difference}</color> <size={size}>(<color={color}>{currentDistance}cm</color> vs {calibratedDistance}cm)</size>";
        }
        #endregion
    }
}