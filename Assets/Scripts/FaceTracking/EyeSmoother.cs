using System;
using System.Collections.Generic;
using System.Linq;
using MediaPipe.BlazeFace;
using UnityEngine;
using VirtualVitrine.Core;

namespace VirtualVitrine.FaceTracking
{
    /// <summary>
    /// I'm using this class for exposing the most important face tracking stuff to other classes.
    /// Also smooths the face tracking data.
    /// </summary>
    public class EyeSmoother : MonoBehaviour
    {
        #region Private Fields
        // Average
        private readonly List<Vector2> _leftEyeHistory = new List<Vector2>();
        private readonly List<Vector2> _rightEyeHistory = new List<Vector2>();

        // Kalman
        private KalmanFilter<Vector2> _leftMeasurement;
        private KalmanFilter<Vector2> _rightMeasurement;

        // Player prefs
        private static float KalmanQ => PlayerPrefs.GetFloat("kalmanQ");
        private static float KalmanR => PlayerPrefs.GetFloat("kalmanR");
        private static int FramesSmoothed => PlayerPrefs.GetInt("framesSmoothed");
        #endregion

        #region Public Fields
        public static Vector2 LeftEyeSmoothed { get; private set; }
        public static Vector2 RightEyeSmoothed { get; private set; }
        public static Vector2 EyeCenter => (LeftEyeSmoothed + RightEyeSmoothed) / 2;
        #endregion
        
        #region Public Methods
        public void SmoothEyes(FaceDetector.Detection detection)
        {
            Enum.TryParse(PlayerPrefs.GetString("smoothing"), out GlobalManager.SmoothType smoothString);
            switch (smoothString)
            {
                case GlobalManager.SmoothType.Kalman:
                    SmoothKalman(detection);
                    break;
                case GlobalManager.SmoothType.Average:
                    SmoothAverage(detection);
                    break;
                case GlobalManager.SmoothType.Off:
                    break;
            }
        }
        #endregion

        #region Unity Methods
        private void Start()
        {
            _leftMeasurement = new KalmanFilter<Vector2>(KalmanQ, KalmanR);
            _rightMeasurement = new KalmanFilter<Vector2>(KalmanQ, KalmanR);
        }
        #endregion

        #region Private Methods
        private void SmoothKalman(FaceDetector.Detection detection)
        {
            var kQ = KalmanQ;
            var kR = KalmanR;
            LeftEyeSmoothed = _leftMeasurement.Update(detection.leftEye, kQ, kR);
            RightEyeSmoothed = _rightMeasurement.Update(detection.rightEye, kQ, kR);
        }

        private void SmoothAverage(FaceDetector.Detection detection)
        {
            if (FramesSmoothed == 1)
            {
                LeftEyeSmoothed = detection.leftEye;
                RightEyeSmoothed = detection.rightEye;
                return;
            }

            // add new measurement
            _leftEyeHistory.Add(detection.leftEye);
            _rightEyeHistory.Add(detection.rightEye);
            
            // remove oldest values
            if (_leftEyeHistory.Count > FramesSmoothed)
            {
                _leftEyeHistory.RemoveRange(0, _leftEyeHistory.Count - FramesSmoothed);
                _rightEyeHistory.RemoveRange(0, _rightEyeHistory.Count - FramesSmoothed);
            }

            // calculate new average
            LeftEyeSmoothed = new Vector2(
                _leftEyeHistory.Average(x => x[0]),
                _leftEyeHistory.Average(x => x[1])
            );
            RightEyeSmoothed = new Vector2(
                _rightEyeHistory.Average(x => x[0]),
                _rightEyeHistory.Average(x => x[1])
            );
        }
        #endregion
    }
}