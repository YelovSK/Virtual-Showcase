using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualShowcase.Enums;
using VirtualShowcase.FaceTracking.Marker;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking
{
    public class EyeTracker : MonoBehaviour
    {
        // Average
        private readonly List<Vector2> _leftEyeHistory = new();
        private readonly List<Vector2> _rightEyeHistory = new();

        // Kalman
        private KalmanFilter<Vector2> _leftMeasurement;
        private KalmanFilter<Vector2> _rightMeasurement;

        public static Vector2 LeftEyeSmoothed { get; private set; }
        public static Vector2 RightEyeSmoothed { get; private set; }
        public static Vector2 EyeCenter => (LeftEyeSmoothed + RightEyeSmoothed) / 2;
        public static float EyesDistance => (LeftEyeSmoothed - RightEyeSmoothed).magnitude;

        #region Event Functions

        private void Start()
        {
            _leftMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
            _rightMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
        }

        #endregion


        public void SmoothEyes()
        {
            switch (MyPrefs.SmoothingType)
            {
                case SmoothingType.Kalman:
                    SmoothKalman();
                    break;
                case SmoothingType.Average:
                    SmoothAverage();
                    break;
                case SmoothingType.Off:
                    LeftEyeSmoothed = KeyPointsUpdater.Detection.leftEye;
                    RightEyeSmoothed = KeyPointsUpdater.Detection.rightEye;
                    break;
            }
        }

        public static float GetRealHeadDistance()
        {
            // https://www.youtube.com/watch?v=jsoe1M2AjFk
            const int real_eyes_distance = 6;
            float realDistance = real_eyes_distance * MyPrefs.FocalLength / EyesDistance;
            return realDistance;
        }

        public static float GetFocalLength(float distanceFromScreen)
        {
            // Eyes distance on camera.
            float eyesDistance = EyesDistance;

            // Real life distance of eyes in cm.
            const int real_eyes_distance = 6;

            // Calculate focal length.
            return eyesDistance * distanceFromScreen / real_eyes_distance;
        }

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
                _leftEyeHistory.Average(eye => eye.x),
                _leftEyeHistory.Average(eye => eye.y)
            );
            RightEyeSmoothed = new Vector2(
                _rightEyeHistory.Average(eye => eye.x),
                _rightEyeHistory.Average(eye => eye.y)
            );
        }
    }
}