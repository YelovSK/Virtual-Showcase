using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MediaPipe.BlazeFace;
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
            // broken webcam, image set to "NO WEBCAM SHOWING"
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
            // check if camera got new frame
            if (!_webcamInput.CameraUpdated)
                return;

            // set aspect ratio of camera to 1:1
            _webcamInput.SetAspectRatio();
            
            // update camera preview
            previewUI.texture = _webcamInput.RenderTexture;

            // run detection and update marker in UI
            bool faceFound = RunDetector(_webcamInput.RenderTexture);

            // if face not found, hide UI and return
            if (!faceFound)
            {
                _distanceText.text = "";
                _colourChecker.HideUI();
                return;
            }
            
            // smooth left and right eye key points
            _eyeSmoother.SmoothEyes();
            
            // update key points in UI
            keyPointsUpdater.UpdateKeyPoints();
            
            // check colour around eyes
            bool glassesOn = _colourChecker.CheckGlassesOn(_webcamInput.WebCamTexture);

            // update head distance in UI
            _distanceText.text = (int) CalibrationManager.GetRealHeadDistance() + "cm";
            
            // if not in menu and glasses are on, transform camera
            if (glassesOn && GlobalManager.InMainScene)
                _cameraTransform.Transform();
        }
        
        private void OnDestroy()
        {
            _detector?.Dispose();
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
            // Face detection
            _detector.ProcessImage(input, PlayerPrefs.GetFloat("threshold"));

            // Check if any detections were found
            bool faceFound = _detector.Detections.Any();

            // Activate/Deactivate marker if face was/wasn't found
            keyPointsUpdater.gameObject.SetActive(faceFound);

            if (faceFound)
            {
                // Get detection with largest bounding box
                var largestFace = _detector.Detections
                    .OrderByDescending(x => x.extent.magnitude)
                    .First();
                KeyPointsUpdater.Detection = largestFace;
            }

            return faceFound;
        }
        
        #endregion
    }
}