using System.Collections.Generic;
using UnityEngine;
using UI = UnityEngine.UI;

namespace MediaPipe.BlazeFace {

public sealed class KeyPointsUpdater : MonoBehaviour
{
        #region Editable attributes

        [SerializeField] RectTransform[] _keyPoints;
        #endregion

        #region Private members

        Marker _marker;
        RectTransform _xform;
        RectTransform _parent;
        UI.Text _label;

        // Smoothing
        Vector2 leftEye;
        Vector2 rightEye;
        // Average
        List<Vector2> leftEyeHistory;
        List<Vector2> rightEyeHistory;
        int framesSmoothed = 30;
        // Kalman
        KalmanFilter _kalmanFilter;
        KalmanFilter.Measurement<Vector2> leftMeasurement;
        KalmanFilter.Measurement<Vector2> rightMeasurement;

        void SetKeyPoint(RectTransform xform, Vector2 point)
          => xform.anchoredPosition =
               point * _parent.rect.size - _xform.anchoredPosition;

        #endregion

        #region MonoBehaviour implementation

        void Start()
        {
            _marker = GetComponent<Marker>();
            _xform = GetComponent<RectTransform>();
            _parent = (RectTransform)_xform.parent;
            _label = GetComponentInChildren<UI.Text>();
            leftEyeHistory = new List<Vector2>();
            rightEyeHistory = new List<Vector2>();
            _kalmanFilter = new KalmanFilter(0.0005f, 0.2f);
            leftMeasurement = new KalmanFilter.Measurement<Vector2>();
            rightMeasurement = new KalmanFilter.Measurement<Vector2>();
        }

        void LateUpdate()
        {
            var detection = _marker.detection;

            leftEye = detection.leftEye;
            rightEye = detection.rightEye;
            switch (GlobalVars.smoothing)
            {
                case "Kalman":
                    smoothKalman();
                    break;
                case "Average":
                    smoothAverage();
                    break;
                case "Off":
                    break;
            }

            // Bounding box center
            _xform.anchoredPosition = detection.center * _parent.rect.size;

            // Bounding box size
            var size = detection.extent * _parent.rect.size;
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

        void smoothKalman()
        {
            leftMeasurement.SetObservedValue(leftEye);
            rightMeasurement.SetObservedValue(rightEye);
            _kalmanFilter.KalmanUpdate(leftMeasurement);
            _kalmanFilter.KalmanUpdate(rightMeasurement);
            leftEye = leftMeasurement.x;
            rightEye = rightMeasurement.x;
        }

        void smoothAverage()
        {
            if (framesSmoothed > 1)
            {
                leftEyeHistory.Add(leftEye);
                rightEyeHistory.Add(rightEye);
                if (leftEyeHistory.Count > framesSmoothed)
                {
                    leftEyeHistory.RemoveAt(0);
                    rightEyeHistory.RemoveAt(0);
                }
                var leftSum = new Vector2(0, 0);
                var rightSum = new Vector2(0, 0);
                foreach (var l in leftEyeHistory)
                {
                    leftSum += l;
                }
                foreach (var l in rightEyeHistory)
                {
                    rightSum += l;
                }
                leftEye = leftSum / leftEyeHistory.Count;
                rightEye = rightSum / rightEyeHistory.Count;
            }
        }

        #endregion
    }

} // namespace MediaPipe.BlazeFace
