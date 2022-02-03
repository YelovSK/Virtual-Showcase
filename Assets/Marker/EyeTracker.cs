using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MediaPipe.BlazeFace
{

    public sealed class EyeTracker : MonoBehaviour
    {
        Marker _marker;
        // Current eye position
        Vector2 _leftEye;
        Vector2 _rightEye;
        // Average
        List<Vector2> _leftEyeHistory;
        List<Vector2> _rightEyeHistory;
        // Kalman
        KalmanFilter<Vector2> _leftMeasurement;
        KalmanFilter<Vector2> _rightMeasurement;


        void Start()
        {
            _marker = GetComponent<Marker>();
            _leftEyeHistory = new List<Vector2>();
            _rightEyeHistory = new List<Vector2>();
            _leftMeasurement = new KalmanFilter<Vector2>(PlayerPrefs.GetFloat("kalmanQ"), PlayerPrefs.GetFloat("kalmanR"));
            _rightMeasurement = new KalmanFilter<Vector2>(PlayerPrefs.GetFloat("kalmanQ"), PlayerPrefs.GetFloat("kalmanR"));
        }

        void LateUpdate()
        {
            var detection = _marker.detection;
            _leftEye = detection.leftEye;
            _rightEye = detection.rightEye;
            switch (PlayerPrefs.GetString("smoothing"))
            {
                case "Kalman":
                    SmoothKalman();
                    break;
                case "Average":
                    SmoothAverage();
                    break;
                case "Off":
                    break;
            }
        }

        void SmoothKalman()
        {
            _leftEye = _leftMeasurement.Update(_leftEye, PlayerPrefs.GetFloat("kalmanQ"), PlayerPrefs.GetFloat("kalmanR"));
            _rightEye = _rightMeasurement.Update(_rightEye, PlayerPrefs.GetFloat("kalmanQ"), PlayerPrefs.GetFloat("kalmanR"));
        }

        void SmoothAverage()
        {
            if (PlayerPrefs.GetFloat("framesSmoothed") < 1)
                return;
            _leftEyeHistory.Add(_leftEye);
            _rightEyeHistory.Add(_rightEye);
            if (_leftEyeHistory.Count > PlayerPrefs.GetInt("framesSmoothed"))   // remove first values
            {
                _leftEyeHistory.RemoveRange(0, _leftEyeHistory.Count - PlayerPrefs.GetInt("framesSmoothed"));
                _rightEyeHistory.RemoveRange(0, _rightEyeHistory.Count - PlayerPrefs.GetInt("framesSmoothed"));
            }
            _leftEye = new Vector2(
                _leftEyeHistory.Average(x => x[0]),
                _leftEyeHistory.Average(x => x[1])
            );
            _rightEye = new Vector2(
                _rightEyeHistory.Average(x => x[0]),
                _rightEyeHistory.Average(x => x[1])
            );
        }

        public Vector2 GetLeftEye()
        {
            return _leftEye;
        }

        public Vector2 GetRightEye()
        {
            return _rightEye;
        }

        public FaceDetector.Detection GetDetection()
        {
            return _marker.detection;
        }

    }

} // namespace MediaPipe.BlazeFace
