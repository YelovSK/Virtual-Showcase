using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe.BlazeFace {

public sealed class KeyPointsUpdater : MonoBehaviour
{
        [SerializeField] RectTransform[] _keyPoints;
        EyeTracker _tracker;
        RectTransform _xform;
        RectTransform _parent;
        UI.Text _label;

        void SetKeyPoint(RectTransform xform, Vector2 point)
          => xform.anchoredPosition =
               point * _parent.rect.size - _xform.anchoredPosition;

        void Start()
        {
            _xform = GetComponent<RectTransform>();
            _parent = (RectTransform)_xform.parent;
            _label = GetComponentInChildren<UI.Text>();
            _tracker = GetComponent<EyeTracker>();
        }

        void LateUpdate()
        {
            FaceDetector.Detection detection = _tracker.Detection;
            Vector2 leftEye = _tracker.LeftEye;
            Vector2 rightEye = _tracker.RightEye;

            // Bounding box center
            var rect = _parent.rect;
            _xform.anchoredPosition = detection.center * rect.size;

            // Bounding box size
            var size = detection.extent * rect.size;
            _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            _xform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // Key points
            SetKeyPoint(_keyPoints[0], leftEye);
            SetKeyPoint(_keyPoints[1], rightEye);
            //SetKeyPoint(_keyPoints[2], detection.nose);
            //SetKeyPoint(_keyPoints[3], detection.mouth);
            //SetKeyPoint(_keyPoints[4], detection.leftEar);
            //SetKeyPoint(_keyPoints[5], detection.rightEar);

            // Label
            _label.text = $"{(int)(detection.score * 100)}%";
        }
    }

} // namespace MediaPipe.BlazeFace
