using MediaPipe.BlazeFace;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace VirtualVitrine.FaceTracking
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
        public static bool IsFaceFound;
        #endregion
        
        #region Private Fields
        private WebcamInput _webcamInput;
        private FaceDetector _detector;
        private const int FacesCount = 1;
        private readonly Marker[] _markers = new Marker[FacesCount];
        #endregion
        
        #region Unity Methods
        private void Awake()
        {
            _webcamInput = GetComponent<WebcamInput>();
        }

        private void Start()
        {
            // broken webcam, image set to "NO WEBCAM SHOWING"
            if (!_webcamInput.IsCameraRunning)
            {
                previewUI.texture = defaultCamTexture;
                return;
            }

            // Face detector initialization
            _detector = new FaceDetector(resources);

            // Marker population
            for (var i = 0; i < _markers.Length; i++)
                _markers[i] = Instantiate(markerPrefab, previewUI.transform);
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
            if (_webcamInput.CameraUpdated)
                RunDetector(_webcamInput.Texture);
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

            IsFaceFound = marker != null;

            for (; i < _markers.Length; i++)
                _markers[i].gameObject.SetActive(false);

            // UI update
            previewUI.texture = _webcamInput.Texture;
        }
        
        #endregion
    }
}