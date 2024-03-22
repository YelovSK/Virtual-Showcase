using System.Collections;
using UnityEngine;
using VirtualShowcase.MainScene;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking.Transform
{
    public class CameraTransform : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Projection projection;
        [SerializeField] private CalibrationManager calibrationManager;

        #endregion

        #region Event Functions

        private void Start()
        {
            projection.UpdateCameraProjection();
        }

        #endregion


        /// <summary>
        ///     Maps a value from a range to another range.
        /// </summary>
        /// <param name="n">Value to map</param>
        /// <param name="start1">Start of the original range</param>
        /// <param name="stop1">End of the original range</param>
        /// <param name="start2">Start of the target range</param>
        /// <param name="stop2">End of the target range</param>
        /// <returns>Mapped value</returns>
        public static float Map(float n, float start1, float stop1, float start2, float stop2) => (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;


        public void Transform()
        {
            // Map coords based on the calibration.
            // Middle of the screen is 0.0f, left is -0.5f, right is 0.5f.
            float centerX = Map(EyeTracker.EyeCenter.x, MyPrefs.LeftCalibration, MyPrefs.RightCalibration, 0.0f, 1.0f);
            float centerY = Map(EyeTracker.EyeCenter.y, MyPrefs.BottomCalibration, MyPrefs.TopCalibration, 0.0f, 1.0f);

            // Get local x, y coordinates of the head.
            float x = (centerX - 0.5f) * Projection.ScreenWidth;
            float y = (centerY - 0.5f) * Projection.ScreenHeight;

            // Update head position.
            if (MyPrefs.InterpolatedPosition && !calibrationManager.Enabled && WebcamInput.Instance.AverageFramesBetweenUpdates >= 2)
                StartCoroutine(SmoothTranslation(new Vector3(x, y, transform.localPosition.z)));
            else
            {
                transform.localPosition = new Vector3(x, y, transform.localPosition.z);
                projection.UpdateCameraProjection();
            }
        }

        /// <summary>
        ///     Smooths out the translation of the head over multiple frames, but not longer than the frametime of the webcam.
        /// </summary>
        /// <param name="target">Final position</param>
        /// <returns></returns>
        private IEnumerator SmoothTranslation(Vector3 target)
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
                    yield break;
            }
        }
    }
}