using TMPro;
using UnityEngine;

namespace VirtualShowcase.FaceTracking.Marker
{
    public sealed class KeyPointsUpdater : MonoBehaviour
    {
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

        public void UpdateKeyPoints(FaceDetection detection)
        {
            // Bounding box center.
            Rect rect = parent.rect;
            xForm.anchoredPosition = detection.Center * rect.size;

            // Bounding box size.
            Vector2 size = detection.Extent * rect.size;
            xForm.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            xForm.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // print(detection.center + " | " + detection.extent);

            // Key points
            SetKeyPoint(keyPoints[0], detection.LeftEye);
            SetKeyPoint(keyPoints[1], detection.RightEye);
            // SetKeyPoint(keyPoints[2], Detection.mouth);
            // SetKeyPoint(keyPoints[3], Detection.nose);
            // SetKeyPoint(keyPoints[4], Detection.leftEar);
            // SetKeyPoint(keyPoints[5], Detection.rightEar);

            // Label.
            confidenceText.text = $"{(int)(detection.Score * 100)}%";
        }


        private void SetKeyPoint(RectTransform form, Vector2 point)
        {
            form.anchoredPosition = point * parent.rect.size - xForm.anchoredPosition;
        }
    }
}