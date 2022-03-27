using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MediaPipe.BlazeFace
{
    public sealed class EyeTracker : MonoBehaviour
    {
        static Marker _marker;
        // Current eye position
        static Vector2 _leftEye;
        static Vector2 _rightEye;
        // Average
        List<Vector2> _leftEyeHistory;
        List<Vector2> _rightEyeHistory;
        // Kalman
        KalmanFilter<Vector2> _leftMeasurement;
        KalmanFilter<Vector2> _rightMeasurement;

        static WebcamInput _webcamInput;
        public static FaceDetector.Detection Detection => _marker.detection;
        public static Vector2 LeftEye => _leftEye;
        public static Vector2 RightEye => _rightEye;
        public static Vector2 EyeCenter => (_leftEye + _rightEye) / 2;
        public static bool DetectedThisFrame => _webcamInput != null && _webcamInput.CameraUpdated() && Visualizer.DetectedFace;


        void Start()
        {
            _webcamInput = GameObject.FindWithTag("Face tracking").GetComponent<WebcamInput>();
            _marker = GetComponent<Marker>();
            _leftEyeHistory = new List<Vector2>();
            _rightEyeHistory = new List<Vector2>();
            float kQ = PlayerPrefs.GetFloat("kalmanQ");
            float kR = PlayerPrefs.GetFloat("kalmanR");
            _leftMeasurement = new KalmanFilter<Vector2>(kQ, kR);
            _rightMeasurement = new KalmanFilter<Vector2>(kQ, kR);
        }

        void LateUpdate()
        {
            if (!DetectedThisFrame)
                return;
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
            float kQ = PlayerPrefs.GetFloat("kalmanQ");
            float kR = PlayerPrefs.GetFloat("kalmanR");
            _leftEye = _leftMeasurement.Update(_leftEye, kQ, kR);
            _rightEye = _rightMeasurement.Update(_rightEye, kQ, kR);
        }

        void SmoothAverage()
        {
            if (PlayerPrefs.GetInt("framesSmoothed") == 1)
                return;
            _leftEyeHistory.Add(_leftEye);
            _rightEyeHistory.Add(_rightEye);
            if (_leftEyeHistory.Count > PlayerPrefs.GetInt("framesSmoothed"))   // remove oldest values
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
    }
}