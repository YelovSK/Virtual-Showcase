using System.Linq;
using System.Threading;
using MediaPipe.BlazeFace;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam;
    [Space]
    [SerializeField] RawImage _previewUI;
    [Space]
    [SerializeField] ResourceSet _resources;
    [SerializeField] Marker _markerPrefab;
    [SerializeField] Texture2D _defaultCamTexture;

    #endregion

    #region Private members

    const int FACES_COUNT = 1;  // number of faces to detect
    FaceDetector _detector;
    Marker[] _markers = new Marker[FACES_COUNT];

    void RunDetector(Texture input)
    {
        // Face detection
        _detector.ProcessImage(input, PlayerPrefs.GetFloat("threshold"));

        // Marker update
        var i = 0;

        foreach (var detection in _detector.Detections)
        {
            if (i == _markers.Length) break;
            var marker = _markers[i++];
            marker.detection = detection;
            marker.gameObject.SetActive(true);
        }

        for (; i < _markers.Length; i++)
            _markers[i].gameObject.SetActive(false);

        // UI update
        _previewUI.texture = _webcam.Texture;
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        // broken webcam, image set to "NO WEBCAM SHOWING"
        if (!_webcam.IsCameraRunning())
        {
            _previewUI.texture = _defaultCamTexture;
            return;
        }
        // Face detector initialization
        _detector = new FaceDetector(_resources);

        // Marker population
        for (var i = 0; i < _markers.Length; i++)
            _markers[i] = Instantiate(_markerPrefab, _previewUI.transform);
    }

    void OnDestroy()
    {
        _detector?.Dispose();
        foreach (var marker in _markers)
            if (marker != null)
                Destroy(marker.gameObject);
    }

    void LateUpdate()
    {
        if (_webcam != null && _webcam.IsCameraRunning() && _webcam.CameraUpdated())
            RunDetector(_webcam.Texture);
    }

    #endregion
}
