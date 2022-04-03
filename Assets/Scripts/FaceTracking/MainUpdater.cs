using System.Linq;
using MediaPipe.BlazeFace;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VirtualVitrine.Core;
using VirtualVitrine.FaceTracking.Marker;
using VirtualVitrine.FaceTracking.Transform;
using VirtualVitrine.UI.Main;

namespace VirtualVitrine.FaceTracking
{
    public sealed class MainUpdater : MonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("_previewUI")] [SerializeField] private RawImage previewUI;
        [FormerlySerializedAs("_resources")] [SerializeField] private ResourceSet resources;
        [FormerlySerializedAs("_markerPrefab")] [SerializeField] private Marker.Marker markerPrefab;
        [FormerlySerializedAs("_defaultCamTexture")] [SerializeField] private Texture2D defaultCamTexture;
        #endregion

        #region Private Fields
        private WebcamInput _webcamInput;
        private FaceDetector _detector;
        private Marker.Marker _marker;
        private ColourChecker _colourChecker;
        private CameraTransform _cameraTransform;
        private EyeSmoother _eyeSmoother;
        private KeyPointsUpdater _keyPointsUpdater;
        private TMP_Text _distanceText;
        private FaceDetector.Detection Detection => _marker.Detection;
        #endregion
        
        #region Unity Methods
        private void Awake()
        {
            _colourChecker = GetComponent<ColourChecker>();
            _cameraTransform = GetComponent<CameraTransform>();
            _webcamInput = GetComponent<WebcamInput>();
            _distanceText = previewUI.GetComponentInChildren<TMP_Text>();
            _eyeSmoother = GetComponent<EyeSmoother>();
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
            _marker = Instantiate(markerPrefab, previewUI.transform);
            _keyPointsUpdater = _marker.GetComponent<KeyPointsUpdater>();
        }

        /// <summary>
        /// Main loop of the program, calls other components. Detects face, updates UI and transforms camera.
        /// </summary>
        private void LateUpdate()
        {
            // check if camera got new frame
            if (!_webcamInput.CameraUpdated)
                return;
            
            // update camera preview
            previewUI.texture = _webcamInput.RenderTexture;

            // run detection
            bool faceFound = RunDetector(_webcamInput.RenderTexture);
            
            // if face not found, hide UI and return
            if (!faceFound)
            {
                _distanceText.text = "";
                _colourChecker.HideUI();
                return;
            }
            
            // smooth left and right eye key points
            _eyeSmoother.SmoothEyes(Detection);
            
            // update key points in UI
            _keyPointsUpdater.UpdateKeyPoints(Detection);
            
            // check colour around eyes
            bool glassesOn = _colourChecker.CheckGlassesOn(_webcamInput.WebCamTexture);

            // update head distance in UI
            _distanceText.text = (int) Calibration.GetRealHeadDistance() + "cm";
            
            // if not in menu and glasses are on, transform camera
            if (GlobalManager.InMainScene && glassesOn)
                _cameraTransform.Transform();
        }
        
        private void OnDestroy()
        {
            _detector?.Dispose();
            if (_marker != null)
                Destroy(_marker.gameObject);
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
            _marker.gameObject.SetActive(faceFound);
            if (!faceFound)
                return false;
            
            // Get detection with largest bounding box
            var largestFace = _detector.Detections
                .OrderByDescending(x => x.extent.magnitude)
                .First();
            _marker.Detection = largestFace;

            return true;
        }
        
        #endregion
    }
}