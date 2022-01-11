using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MediaPipe.BlazeFace
{

    public sealed class EyeTracker : MonoBehaviour
    {
        Marker _marker;

        // Smoothing
        Vector2 leftEye;
        Vector2 rightEye;
        // Average
        List<Vector2> leftEyeHistory;
        List<Vector2> rightEyeHistory;
        int framesSmoothed = 30;
        // Kalman
        KalmanFilter<Vector2> leftMeasurement;
        KalmanFilter<Vector2> rightMeasurement;


        void Start()
        {
            _marker = GetComponent<Marker>();
            leftEyeHistory = new List<Vector2>();
            rightEyeHistory = new List<Vector2>();
            float _aQ = 0.0001f;
            float _aR = 0.1f;
            leftMeasurement = new KalmanFilter<Vector2>(aQ: _aQ, aR: _aR);
            rightMeasurement = new KalmanFilter<Vector2>(aQ: _aQ, aR: _aR);
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
        }

        void smoothKalman()
        {
            leftEye = leftMeasurement.Update(leftEye);
            rightEye = rightMeasurement.Update(rightEye);
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
                leftEye = new Vector2(
                    leftEyeHistory.Average(x => x[0]),
                    leftEyeHistory.Average(x => x[1])
                );
                rightEye = new Vector2(
                    rightEyeHistory.Average(x => x[0]),
                    rightEyeHistory.Average(x => x[1])
                );
            }
        }

        public Vector2 getLeftEye()
        {
            return leftEye;
        }

        public Vector2 getRightEye()
        {
            return rightEye;
        }

        public FaceDetector.Detection getDetection()
        {
            return _marker.detection;
        }

    }

} // namespace MediaPipe.BlazeFace
