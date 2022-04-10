using TMPro;
using UnityEngine;
using MediaPipe.BlazeFace;

namespace VirtualVitrine.FaceTracking.Marker
{
    public sealed class KeyPointsUpdater : MonoBehaviour
    {
        #region Public Fields
        public static FaceDetector.Detection Detection;
        #endregion
        
        #region Serialized Fields
        [SerializeField] private RectTransform[] keyPoints;
        #endregion

        #region Private Fields
        private TMP_Text _label;
        private RectTransform _parent;
        private RectTransform _xform;
        #endregion

        #region Public Methods
        public void UpdateKeyPoints()
        {
            // Bounding box center.
            var rect = _parent.rect;
            _xform.anchoredPosition = Detection.center * rect.size;

            // Bounding box size.
            var size = Detection.extent * rect.size;
            _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            // print(detection.center + " | " + detection.extent);

            // Key points
            SetKeyPoint(keyPoints[0], EyeSmoother.LeftEyeSmoothed);
            SetKeyPoint(keyPoints[1], EyeSmoother.RightEyeSmoothed);
            // SetKeyPoint(keyPoints[2], detection.mouth);
            // SetKeyPoint(_keyPoints[3], detection.nose);
            // SetKeyPoint(_keyPoints[4], detection.leftEar);
            // SetKeyPoint(_keyPoints[5], detection.rightEar);

            // Label.
            _label.text = $"{(int) (Detection.score * 100)}%";
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _xform = GetComponent<RectTransform>();
            _parent = (RectTransform) _xform.parent;
            _label = GetComponentInChildren<TMP_Text>();
        }
        #endregion
        
        #region Private Methods
        private void SetKeyPoint(RectTransform xform, Vector2 point)
        {
            xform.anchoredPosition =
                point * _parent.rect.size - _xform.anchoredPosition;
        }
        #endregion
    }
}