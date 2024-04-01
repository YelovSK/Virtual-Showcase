using MediaPipe.BlazeFace;
using TMPro;
using UnityEngine;

namespace VirtualShowcase.FaceTracking.Marker
{
    public sealed class KeyPointsUpdater : MonoBehaviour
    {
        public static Detection Detection;

        #region Serialized Fields

        [SerializeField]
        private RectTransform[] keyPoints;

        [SerializeField]
        private TMP_Text confidenceText;

        [SerializeField]
        private RectTransform parent;

        [SerializeField]
        private RectTransform xForm;

        #endregion

        #region Event Functions

        private void Awake()
        {
            xForm = GetComponent<RectTransform>();
            parent = (RectTransform)xForm.parent;
        }

        #endregion

        public void UpdateKeyPoints()
        {
            // Bounding box center.
            Rect rect = parent.rect;
            xForm.anchoredPosition = Detection.center * rect.size;

            // Bounding box size.
            Vector2 size = Detection.extent * rect.size;
            xForm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            xForm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // print(detection.center + " | " + detection.extent);

            // Key points
            SetKeyPoint(keyPoints[0], EyeTracker.LeftEyeSmoothed);
            SetKeyPoint(keyPoints[1], EyeTracker.RightEyeSmoothed);
            // SetKeyPoint(keyPoints[2], Detection.mouth);
            // SetKeyPoint(keyPoints[3], Detection.nose);
            // SetKeyPoint(keyPoints[4], Detection.leftEar);
            // SetKeyPoint(keyPoints[5], Detection.rightEar);

            // Label.
            confidenceText.text = $"{(int)(Detection.score * 100)}%";
        }


        private void SetKeyPoint(RectTransform form, Vector2 point)
        {
            form.anchoredPosition = point * parent.rect.size - xForm.anchoredPosition;
        }

        // private RectTransform parent;
        // private RectTransform xForm;
    }
}