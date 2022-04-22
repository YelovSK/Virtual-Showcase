using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualVitrine.FaceTracking.Marker;

namespace VirtualVitrine.FaceTracking
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
                leftEyeHistory.Average(x => x[0]),
                leftEyeHistory.Average(x => x[1])
            );
            RightEyeSmoothed = new Vector2(
                rightEyeHistory.Average(x => x[0]),
                rightEyeHistory.Average(x => x[1])
            );
        }
    }
}