using UnityEngine;

namespace VirtualVitrine.FaceTracking.Transform
{
    public class CameraTransform : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Projection projection;

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
        public static float Map(float n, float start1, float stop1, float start2, float stop2)
        {
            return (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
        }


        public void Transform()
        {
            // Map coords based on the calibration.
            // Middle of the screen is 0.0f, left is -0.5f, right is 0.5f.
            float centerX = Map(EyeSmoother.EyeCenter.x, MyPrefs.LeftCalibration, MyPrefs.RightCalibration, 0.0f, 1.0f);
            float centerY = Map(EyeSmoother.EyeCenter.y, MyPrefs.BottomCalibration, MyPrefs.TopCalibration, 0.0f, 1.0f);

            // Get local x, y coordinates of the head.
            float x = (centerX - 0.5f) * Projection.ScreenWidth;
            float y = (centerY - 0.5f) * Projection.ScreenHeight;

            // Update the position of the head.
            transform.localPosition = new Vector3(x, y, transform.localPosition.z);
            projection.UpdateCameraProjection();
        }
    }
}