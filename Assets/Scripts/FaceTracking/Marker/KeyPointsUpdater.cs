using MediaPipe.BlazeFace;
using TMPro;
using UnityEngine;

namespace VirtualVitrine.FaceTracking.Marker
{
    public sealed class KeyPointsUpdater : MonoBehaviour
    {
        public static Detection Detection;

        #region Serialized Fields

        [SerializeField] private RectTransform[] keyPoints;
        [SerializeField] private TMP_Text confidenceText;

        #endregion

        private RectTransform parent;
        private RectTransform xform;

        #region Event Functions

        private void Awake()
        {
            xform = GetComponent<RectTransform>();
            parent = (RectTransform) xform.parent;
        }

        #endregion


        public void UpdateKeyPoints()
        {
            // Bounding box center.
            Rect rect = parent.rect;
            xform.anchoredPosition = Detection.center * rect.size;

            // Bounding box size.
            Vector2 size = Detection.extent * rect.size;
            xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // print(detection.center + " | " + detection.extent);

            // Key points
            SetKeyPoint(keyPoints[0], EyeSmoother.LeftEyeSmoothed);
            SetKeyPoint(keyPoints[1], EyeSmoother.RightEyeSmoothed);

            // SetKeyPoint(keyPoints[2], Detection.mouth);
            // SetKeyPoint(keyPoints[3], Detection.nose);
            // SetKeyPoint(keyPoints[4], Detection.leftEar);
            // SetKeyPoint(keyPoints[5], Detection.rightEar);

            // Label.
            confidenceText.text = $"{(int) (Detection.score * 100)}%";
        }


        private void SetKeyPoint(RectTransform form, Vector2 point)
        {
            form.anchoredPosition = point * parent.rect.size - xform.anchoredPosition;
        }
    }
}