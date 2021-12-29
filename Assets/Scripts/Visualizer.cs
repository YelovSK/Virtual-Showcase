using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe.BlazeFace {

public sealed class Visualizer : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] WebcamInput _webcam = null;
    [Space]
    [SerializeField] UI.RawImage _previewUI = null;
    [Space]
    [SerializeField] ResourceSet _resources = null;
    [SerializeField] Marker _markerPrefab = null;

    #endregion

    #region Private members

    FaceDetector _detector;
    Marker[] _markers = new Marker[16];

    void RunDetector(Texture input)
    {
        // Face detection
        _detector.ProcessImage(input, GlobalVars.threshold);

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
        if (_webcam != null) RunDetector(_webcam.Texture);
    }

    #endregion
}

} // namespace MediaPipe.BlazeFace