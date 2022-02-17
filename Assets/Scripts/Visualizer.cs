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

    FaceDetector _detector;
    Marker[] _markers = new Marker[1]; // default 16

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
        _previewUI.texture = input;
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        if (!_webcam.IsCameraRunning())   // broken webcam, image set to "NO WEBCAM SHOWING"
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
        // Webcam test: Run the detector every frame.
        if (_webcam != null && _webcam.IsCameraRunning() && _webcam.CameraUpdated()) RunDetector(_webcam.Texture);
    }

    #endregion
}
