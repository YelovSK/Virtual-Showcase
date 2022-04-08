using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualVitrine.FaceTracking.Marker;

namespace VirtualVitrine.FaceTracking
{
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
        public void SmoothEyes()
        {
            Enum.TryParse(PlayerPrefs.GetString("smoothing"), out GlobalManager.SmoothType smoothType);
            switch (smoothType)
            {
                case GlobalManager.SmoothType.Kalman:
                    SmoothKalman();
                    break;
                case GlobalManager.SmoothType.Average:
                    SmoothAverage();
                    break;
                case GlobalManager.SmoothType.Off:
                    LeftEyeSmoothed = KeyPointsUpdater.Detection.leftEye;
                    RightEyeSmoothed = KeyPointsUpdater.Detection.rightEye;
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
        private void SmoothKalman()
        {
            LeftEyeSmoothed = _leftMeasurement.Update(KeyPointsUpdater.Detection.leftEye, KalmanQ, KalmanR);
            RightEyeSmoothed = _rightMeasurement.Update(KeyPointsUpdater.Detection.rightEye, KalmanQ, KalmanR);
        }

        private void SmoothAverage()
        {
            if (FramesSmoothed == 1)
            {
                LeftEyeSmoothed = KeyPointsUpdater.Detection.leftEye;
                RightEyeSmoothed = KeyPointsUpdater.Detection.rightEye;
                return;
            }

            // add new measurement
            _leftEyeHistory.Add(KeyPointsUpdater.Detection.leftEye);
            _rightEyeHistory.Add(KeyPointsUpdater.Detection.rightEye);

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