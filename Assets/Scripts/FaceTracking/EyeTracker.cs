using System;
using System.Collections.Generic;
using System.Linq;
using MediaPipe.BlazeFace;
using UnityEngine;
using VirtualVitrine.Core;

namespace VirtualVitrine.FaceTracking
{
    public sealed class EyeTracker : MonoBehaviour
    {
        #region Private Fields
        private static Marker _marker;

        // Current eye position

        private static WebcamInput _webcamInput;

        // Average
        private List<Vector2> _leftEyeHistory;

        // Kalman
        private KalmanFilter<Vector2> _leftMeasurement;
        private List<Vector2> _rightEyeHistory;
        private KalmanFilter<Vector2> _rightMeasurement;
        #endregion

        #region Public Fields
        public static FaceDetector.Detection Detection => _marker.Detection;
        public static Vector2 LeftEye { get; private set; }
        public static Vector2 RightEye { get; private set; }
        public static Vector2 EyeCenter => (LeftEye + RightEye) / 2;
        public static bool CameraUpdated => _webcamInput != null && _webcamInput.CameraUpdated;
        public static bool DetectedThisFrame => CameraUpdated && Visualizer.IsFaceFound;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _webcamInput = GameObject.FindWithTag("Face tracking").GetComponent<WebcamInput>();
            _marker = GetComponent<Marker>();
        }

        private void Start()
        {
            // average
            _leftEyeHistory = new List<Vector2>();
            _rightEyeHistory = new List<Vector2>();

            // kalman
            var kQ = PlayerPrefs.GetFloat("kalmanQ");
            var kR = PlayerPrefs.GetFloat("kalmanR");
            _leftMeasurement = new KalmanFilter<Vector2>(kQ, kR);
            _rightMeasurement = new KalmanFilter<Vector2>(kQ, kR);
        }

        private void LateUpdate()
        {
            if (!DetectedThisFrame)
                return;
            var detection = _marker.Detection;
            LeftEye = detection.leftEye;
            RightEye = detection.rightEye;
            Enum.TryParse(PlayerPrefs.GetString("smoothing"), out GlobalManager.SmoothType smoothString);
            switch (smoothString)
            {
                case GlobalManager.SmoothType.Kalman:
                    SmoothKalman();
                    break;
                case GlobalManager.SmoothType.Average:
                    SmoothAverage();
                    break;
                case GlobalManager.SmoothType.Off:
                    break;
            }
        }
        #endregion

        #region Private Methods
        private void SmoothKalman()
        {
            var kQ = PlayerPrefs.GetFloat("kalmanQ");
            var kR = PlayerPrefs.GetFloat("kalmanR");
            LeftEye = _leftMeasurement.Update(LeftEye, kQ, kR);
            RightEye = _rightMeasurement.Update(RightEye, kQ, kR);
        }

        private void SmoothAverage()
        {
            if (PlayerPrefs.GetInt("framesSmoothed") == 1)
                return;
            
            // add new measurement
            _leftEyeHistory.Add(LeftEye);
            _rightEyeHistory.Add(RightEye);
            
            // remove oldest values
            if (_leftEyeHistory.Count > PlayerPrefs.GetInt("framesSmoothed"))
            {
                _leftEyeHistory.RemoveRange(0, _leftEyeHistory.Count - PlayerPrefs.GetInt("framesSmoothed"));
                _rightEyeHistory.RemoveRange(0, _rightEyeHistory.Count - PlayerPrefs.GetInt("framesSmoothed"));
            }

            // calculate new average
            LeftEye = new Vector2(
                _leftEyeHistory.Average(x => x[0]),
                _leftEyeHistory.Average(x => x[1])
            );
            RightEye = new Vector2(
                _rightEyeHistory.Average(x => x[0]),
                _rightEyeHistory.Average(x => x[1])
            );
        }
        #endregion
    }
}