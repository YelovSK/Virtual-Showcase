using System.Collections;
using UnityEngine;
using VirtualShowcase.FaceTracking.GlassesCheck;
using VirtualShowcase.Showcase;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking.Transform
{
    public class CameraTransform : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private Projection projection;

        [SerializeField]
        private CalibrationController calibrationController;

        [SerializeField]
        private Detector detector;

        #endregion

        #region Event Functions

        private void Start()
        {
            ColourChecker.GlassesCheckSkipped.AddListener(Transform);
            ColourChecker.GlassesCheckPassed.AddListener(Transform);
            projection.UpdateCameraProjection();
        }

        #endregion

        public void Transform(FaceDetection detection)
        {
            // Map coords based on the calibration.
            // Middle of the screen is 0.0f, left is -0.5f, right is 0.5f.
            float centerX = detection.EyesCenter.x.Map(MyPrefs.LeftCalibration, MyPrefs.RightCalibration, 0.0f, 1.0f);
            float centerY = detection.EyesCenter.y.Map(MyPrefs.BottomCalibration, MyPrefs.TopCalibration, 0.0f, 1.0f);

            // Get local x, y coordinates of the head.
            float x = (centerX - 0.5f) * Projection.ScreenWidth;
            float y = (centerY - 0.5f) * Projection.ScreenHeight;

            // Update head position.
            if (MyPrefs.TrackingInterpolation && !calibrationController.Enabled &&
                WebcamInput.Instance.AverageFramesBetweenUpdates >= 2)
            {
                StartCoroutine(InterpolateCamera(new Vector3(x, y, transform.localPosition.z)));
            }
            else
            {
                transform.localPosition = new Vector3(x, y, transform.localPosition.z);
                projection.UpdateCameraProjection();
            }
        }

        public void Transform()
        {
            FaceDetection detection = detector.LastDetection ?? FaceDetection.GetDefault();
            Transform(detection);
        }

        /// <summary>
        ///     Smooths out the translation of the head over multiple frames, but not longer than the frametime of the webcam.
        /// </summary>
        /// <param name="target">Final position</param>
        /// <returns></returns>
        private IEnumerator InterpolateCamera(Vector3 target)
        {
            int positionCount = WebcamInput.Instance.AverageFramesBetweenUpdates + 1;
            for (var i = 1; i <= positionCount; i++)
            {
                float t = i * (1f / positionCount);
                transform.position = Vector3.Lerp(transform.position, target, t);
                projection.UpdateCameraProjection();
                yield return null;

                // Sometimes new Webcam frame might come earlier than expected.
                if (WebcamInput.Instance.CameraUpdatedThisFrame)
                {
                    yield break;
                }
            }
        }
    }
}