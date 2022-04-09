using UnityEngine;

namespace VirtualVitrine.FaceTracking.Transform
{
    public class CameraTransform : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private AsymFrustum head;
        #endregion
        
        #region Public Methods
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
        #endregion
        
        #region Unity Methods
        private void Start()
        {
            head.UpdateProjectionMatrix();
        }
        #endregion
        
        #region Private Methods
        public void Transform()
        {
            // map coords based on the calibration
            var centerX = Map(EyeSmoother.EyeCenter.x, MyPrefs.LeftCalibration, MyPrefs.RightCalibration, 0.0f, 1.0f);
            var centerY = Map(EyeSmoother.EyeCenter.y, MyPrefs.BottomCalibration, MyPrefs.TopCalibration, 0.0f, 1.0f);
            
            // middle is 0.0f, left is -0.5f, right is 0.5f
            var x = (centerX - 0.5f) * head.ScreenWidth;
            var y = (centerY - 0.5f) * head.ScreenHeight;
            
            // update the position of the head
            head.transform.position = new Vector3(x, y, head.transform.position.z);
            head.UpdateProjectionMatrix();
        }
        #endregion
    }
}