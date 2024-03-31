using MediaPipe.BlazeFace;
using TMPro;
using UnityEngine;

namespace VirtualShowcase.FaceTracking.Marker
{
    public sealed class KeyPointsUpdater : MonoBehaviour
    {
        public static Detection Detection;

        #region Serialized Fields

        [SerializeField] private RectTransform[] keyPoints;
        [SerializeField] private TMP_Text confidenceText;

        #endregion

        private RectTransform _parent;
        private RectTransform _xform;

        #region Event Functions

        private void Awake()
        {
            _xform = GetComponent<RectTransform>();
            _parent = (RectTransform) _xform.parent;
        }

        #endregion


        public void UpdateKeyPoints()
        {
            // Bounding box center.
            Rect rect = _parent.rect;
            _xform.anchoredPosition = Detection.center * rect.size;

            // Bounding box size.
            Vector2 size = Detection.extent * rect.size;
            _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // print(detection.center + " | " + detection.extent);

            // Key points
            SetKeyPoint(keyPoints[0], EyeTracker.LeftEyeSmoothed);
            SetKeyPoint(keyPoints[1], EyeTracker.RightEyeSmoothed);

            // SetKeyPoint(keyPoints[2], Detection.mouth);
            // SetKeyPoint(keyPoints[3], Detection.nose);
            // SetKeyPoint(keyPoints[4], Detection.leftEar);
            // SetKeyPoint(keyPoints[5], Detection.rightEar);

            // Label.
            confidenceText.text = $"{(int) (Detection.score * 100)}%";
        }


        private void SetKeyPoint(RectTransform form, Vector2 point)
        {
            form.anchoredPosition = point * _parent.rect.size - _xform.anchoredPosition;
        }
    }
}