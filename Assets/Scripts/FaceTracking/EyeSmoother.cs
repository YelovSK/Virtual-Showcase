using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualShowcase.Enums;
using VirtualShowcase.FaceTracking.Marker;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking
{
    public class EyeSmoother : MonoBehaviour
    {
        // Average
        private readonly List<Vector2> leftEyeHistory = new();
        private readonly List<Vector2> rightEyeHistory = new();

        // Kalman
        private KalmanFilter<Vector2> leftMeasurement;
        private KalmanFilter<Vector2> rightMeasurement;

        public static Vector2 LeftEyeSmoothed { get; private set; }
        public static Vector2 RightEyeSmoothed { get; private set; }
        public static Vector2 EyeCenter => (LeftEyeSmoothed + RightEyeSmoothed) / 2;

        #region Event Functions

        private void Start()
        {
            leftMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
            rightMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
        }

        #endregion


        public void SmoothEyes()
        {
            Enum.TryParse(MyPrefs.SmoothingType, out eSmoothingType smoothType);
            switch (smoothType)
            {
                case eSmoothingType.Kalman:
                    SmoothKalman();
                    break;
                case eSmoothingType.Average:
                    SmoothAverage();
                    break;
                case eSmoothingType.Off:
                    LeftEyeSmoothed = KeyPointsUpdater.Detection.leftEye;
                    RightEyeSmoothed = KeyPointsUpdater.Detection.rightEye;
                    break;
            }
        }


        private void SmoothKalman()
        {
            LeftEyeSmoothed = leftMeasurement.Update(KeyPointsUpdater.Detection.leftEye, MyPrefs.KalmanQ, MyPrefs.KalmanR);
            RightEyeSmoothed = rightMeasurement.Update(KeyPointsUpdater.Detection.rightEye, MyPrefs.KalmanQ, MyPrefs.KalmanR);
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
            leftEyeHistory.Add(KeyPointsUpdater.Detection.leftEye);
            rightEyeHistory.Add(KeyPointsUpdater.Detection.rightEye);

            // Remove oldest values.
            if (leftEyeHistory.Count > MyPrefs.FramesSmoothed)
            {
                leftEyeHistory.RemoveRange(0, leftEyeHistory.Count - MyPrefs.FramesSmoothed);
                rightEyeHistory.RemoveRange(0, rightEyeHistory.Count - MyPrefs.FramesSmoothed);
            }

            // Calculate new average.
            LeftEyeSmoothed = new Vector2(
                leftEyeHistory.Average(eye => eye.x),
                leftEyeHistory.Average(eye => eye.y)
            );
            RightEyeSmoothed = new Vector2(
                rightEyeHistory.Average(eye => eye.x),
                rightEyeHistory.Average(eye => eye.y)
            );
        }
    }
}