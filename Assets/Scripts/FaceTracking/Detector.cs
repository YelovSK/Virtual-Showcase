using System;
using System.Collections.Generic;
using System.Linq;
using MediaPipe.BlazeFace;
using UnityEngine;
using UnityEngine.Events;
using VirtualShowcase.Enums;
using VirtualShowcase.Utilities;
using Constants = VirtualShowcase.Common.Constants;

namespace VirtualShowcase.FaceTracking
{
    public class Detector : MonoBehaviour
    {
        [NonSerialized]
        public static UnityEvent<FaceDetection> FaceDetected = new();

        [NonSerialized]
        public static UnityEvent FaceNotDetected = new();

        #region Serialized Fields

        [SerializeField]
        private ResourceSet resources;

        #endregion

        // Average
        private readonly List<Vector2> _leftEyeHistory = new();
        private readonly List<Vector2> _rightEyeHistory = new();

        private FaceDetector _detector;

        // Kalman
        private KalmanFilter<Vector2> _leftMeasurement;
        private KalmanFilter<Vector2> _rightMeasurement;

        [NonSerialized]
        public FaceDetection LastDetection;

        #region Event Functions

        private void Start()
        {
            _detector = new FaceDetector(resources);
            _leftMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);
            _rightMeasurement = new KalmanFilter<Vector2>(MyPrefs.KalmanQ, MyPrefs.KalmanR);

            WebcamInput.CameraUpdated.AddListener(RunDetector);
        }

        private void OnDestroy()
        {
            _detector?.Dispose();
        }

        #endregion

        private void RunDetector()
        {
            // Face detection.
            _detector.ProcessImage(WebcamInput.Instance.Texture, (float)MyPrefs.DetectionThreshold / 100);

            // Check if any detections were found.
            Detection[] detections = _detector.Detections.ToArray();

            if (detections.IsEmpty())
            {
                ResetSmoothing();
                FaceNotDetected.Invoke();
                return;
            }

            // Get detection with largest bounding box.
            Detection detection = detections
                .OrderByDescending(x => x.extent.magnitude)
                .First();
            Eyes eyes = SmoothEyes(detection);

            LastDetection = new FaceDetection
            {
                Center = detection.center,
                Extent = detection.extent,
                LeftEye = eyes.Left,
                RightEye = eyes.Right,
                Mouth = detection.mouth,
                Nose = detection.nose,
                LeftEar = detection.leftEar,
                RightEar = detection.rightEar,
                Score = detection.score,
            };

            FaceDetected.Invoke(LastDetection);
        }

        private Eyes SmoothEyes(Detection detection)
        {
            return MyPrefs.SmoothingType switch
            {
                SmoothingType.Kalman => SmoothKalman(detection),
                SmoothingType.Average => SmoothAverage(detection),
                SmoothingType.Off => new Eyes { Left = detection.leftEye, Right = detection.rightEye },
                _ => throw new ArgumentOutOfRangeException(),
            };
        }

        private Eyes SmoothKalman(Detection detection)
        {
            return new Eyes
            {
                Left = _leftMeasurement.Update(detection.leftEye, MyPrefs.KalmanQ, MyPrefs.KalmanR),
                Right = _rightMeasurement.Update(detection.rightEye, MyPrefs.KalmanQ, MyPrefs.KalmanR),
            };
        }

        private Eyes SmoothAverage(Detection detection)
        {
            if (MyPrefs.FramesSmoothed == 1)
            {
                return new Eyes
                {
                    Left = detection.leftEye,
                    Right = detection.rightEye,
                };
            }

            // Add new measurement.
            _leftEyeHistory.Add(detection.leftEye);
            _rightEyeHistory.Add(detection.rightEye);

            // Remove oldest values.
            if (_leftEyeHistory.Count > MyPrefs.FramesSmoothed)
            {
                _leftEyeHistory.RemoveRange(0, _leftEyeHistory.Count - MyPrefs.FramesSmoothed);
                _rightEyeHistory.RemoveRange(0, _rightEyeHistory.Count - MyPrefs.FramesSmoothed);
            }

            // Calculate new average.
            return new Eyes
            {
                Left = new Vector2(
                    _leftEyeHistory.Average(eye => eye.x),
                    _leftEyeHistory.Average(eye => eye.y)
                ),
                Right = new Vector2(
                    _rightEyeHistory.Average(eye => eye.x),
                    _rightEyeHistory.Average(eye => eye.y)
                ),
            };
        }

        private void ResetSmoothing()
        {
            _leftEyeHistory.Clear();
            _rightEyeHistory.Clear();
            _leftMeasurement.Reset();
            _rightMeasurement.Reset();
        }
    }

    public class FaceDetection
    {
        // Bounding box
        public Vector2 Center;
        public Vector2 Extent;
        public Vector2 LeftEar;

        // Keypoints
        public Vector2 LeftEye;
        public Vector2 Mouth;
        public Vector2 Nose;
        public Vector2 RightEar;
        public Vector2 RightEye;

        public float Score;

        public Vector2 EyesCenter => (LeftEye + RightEye) / 2;
        public float EyesDistance => (LeftEye - RightEye).magnitude;

        // Idfk why this is even here. Fuck it.
        public float GetRealHeadDistance(float referenceFocalLength)
        {
            // https://www.youtube.com/watch?v=jsoe1M2AjFk
            return Constants.EYE_SEPARATION_CM * referenceFocalLength / EyesDistance;
        }

        public float GetFocalLength(float distanceFromScreen)
        {
            return EyesDistance * distanceFromScreen / Constants.EYE_SEPARATION_CM;
        }

        /// <returns>Detection with some random values.</returns>
        public static FaceDetection GetDefault()
        {
            return new FaceDetection
            {
                LeftEye = new Vector2(1.0f, 0.5f),
                RightEye = new Vector2(0.0f, 0.5f),
                Score = 0.0f,
            };
        }
    }

    public class Eyes
    {
        [NonSerialized]
        public Vector2 Left;

        [NonSerialized]
        public Vector2 Right;
    }
}