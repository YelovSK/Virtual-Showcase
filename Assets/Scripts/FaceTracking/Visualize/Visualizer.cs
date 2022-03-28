using MediaPipe.BlazeFace;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VirtualVitrine.FaceTracking.Visualize
{
    public sealed class Visualizer : MonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("_previewUI")] [SerializeField] private RawImage previewUI;
        [FormerlySerializedAs("_resources")] [SerializeField] private ResourceSet resources;
        [FormerlySerializedAs("_markerPrefab")] [SerializeField] private Marker markerPrefab;
        [FormerlySerializedAs("_defaultCamTexture")] [SerializeField] private Texture2D defaultCamTexture;
        #endregion
        
        #region Public Fields
        public static bool DetectedFace;
        public EyeTracker EyeTracker { get; private set; }
        public WebcamInput WebcamInput { get; private set; }
        public bool Initialized { get; private set; }
        #endregion
        
        #region Private Fields
        private const int FacesCount = 1; // number of faces to detect
        private FaceDetector _detector;
        private readonly Marker[] _markers = new Marker[FacesCount];
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            WebcamInput = GetComponent<WebcamInput>();
            // broken webcam, image set to "NO WEBCAM SHOWING"
            if (!WebcamInput.IsCameraRunning())
            {
                previewUI.texture = defaultCamTexture;
                return;
            }

            // Face detector initialization
            _detector = new FaceDetector(resources);

            // Marker population
            for (var i = 0; i < _markers.Length; i++)
                _markers[i] = Instantiate(markerPrefab, previewUI.transform);
            EyeTracker = _markers[0].EyeTracker;
            Initialized = true;
        }

        private void OnDestroy()
        {
            _detector?.Dispose();
            foreach (var marker in _markers)
                if (marker != null)
                    Destroy(marker.gameObject);
        }

        private void LateUpdate()
        {
            if (WebcamInput.CameraUpdated())
                RunDetector(WebcamInput.Texture);
        }
        #endregion
        
        #region Private Methods
        private void RunDetector(Texture input)
        {
            // Face detection
            _detector.ProcessImage(input, PlayerPrefs.GetFloat("threshold"));

            // Marker update
            var i = 0;

            Marker marker = null;
            foreach (var detection in _detector.Detections)
            {
                if (i == _markers.Length) break;
                marker = _markers[i++];
                marker.Detection = detection;
                marker.gameObject.SetActive(true);
            }

            DetectedFace = marker != null;

            for (; i < _markers.Length; i++)
                _markers[i].gameObject.SetActive(false);

            // UI update
            previewUI.texture = WebcamInput.Texture;
        }
        
        #endregion
    }
}