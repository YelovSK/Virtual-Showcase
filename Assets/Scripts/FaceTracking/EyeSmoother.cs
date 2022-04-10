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
        #endregion

        #region Public Fields
        public static Vector2 LeftEyeSmoothed { get; private set; }
        public static Vector2 RightEyeSmoothed { get; private set; }
        public static Vector2 EyeCenter => (LeftEyeSmoothed + RightEyeSmoothed) / 2;
        #endregion

        #region Public Methods
        public void SmoothEyes()
        {
            Enum.TryParse(MyPrefs.SmoothingType, out MyPrefs.SmoothingTypeEnum smoothType);
            switch (smoothType)
            {
                case MyPrefs.SmoothingTypeEnum.Kalman:
                    SmoothKalman();
                    break;
                case MyPrefs.SmoothingTypeEnum.Average:
                    SmoothAverage();
                    break;
                case MyPrefs.SmoothingTypeEnum.Off:
                    LeftEyeSmoothed = KeyPointsUpdater.Detection.leftEye;
                    RightEyeSmoothed = KeyPointsUpdater.Detection.rightEye;
                    break;
            }
        }
        #endregion

        #region Unity Methods
        private void Start()
        {
            _leftMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
            _rightMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
        }
        #endregion

        #region Private Methods
        private void SmoothKalman()
        {
            LeftEyeSmoothed = _leftMeasurement.Update(KeyPointsUpdater.Detection.leftEye, MyPrefs.KalmanQ, MyPrefs.KalmanR);
            RightEyeSmoothed = _rightMeasurement.Update(KeyPointsUpdater.Detection.rightEye, MyPrefs.KalmanQ, MyPrefs.KalmanR);
        }

        private void SmoothAverage()
        {
            if (MyPrefs.FramesSmoothed == 1)
            {
                LeftEyeSmoothed = KeyPointsUpdater.Detection.leftEye;
                RightEyeSmoothed = KeyPointsUpdater.Detection.rightEye;
                return;
            }

            // Add new measurement.
            _leftEyeHistory.Add(KeyPointsUpdater.Detection.leftEye);
            _rightEyeHistory.Add(KeyPointsUpdater.Detection.rightEye);

            // Remove oldest values.
            if (_leftEyeHistory.Count > MyPrefs.FramesSmoothed)
            {
                _leftEyeHistory.RemoveRange(0, _leftEyeHistory.Count - MyPrefs.FramesSmoothed);
                _rightEyeHistory.RemoveRange(0, _rightEyeHistory.Count - MyPrefs.FramesSmoothed);
            }

            // Calculate new average.
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