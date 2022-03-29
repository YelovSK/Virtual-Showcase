using System;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.UI.Main;

namespace VirtualVitrine.FaceTracking.Transform
{
    public class CameraTransform : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject head;
        [SerializeField] private Text distance;
        #endregion
        
        #region Private Fields
        private ColourCheck _colourCheck;
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
        private void Awake()
        {
            _colourCheck = GetComponent<ColourCheck>();
        }

        private void Start()
        {
            head.GetComponent<AsymFrustum>().UpdateProjectionMatrix();
        }

        private void LateUpdate()
        {
            if (!EyeTracker.DetectedThisFrame)
            {
                if (EyeTracker.CameraUpdated)
                    distance.text = "";
                return;
            }
            if (_colourCheck.GlassesOn)
                Transform();
            head.GetComponent<AsymFrustum>().UpdateProjectionMatrix();
        }
        #endregion
        
        #region Private Methods
        private void Transform()
        {
            // map coords based on the calibration
            var centerX = Map(EyeTracker.EyeCenter.x, PlayerPrefs.GetFloat("LeftCalibration"),
                PlayerPrefs.GetFloat("RightCalibration"), 0.0f, 1.0f);
            var centerY = Map(EyeTracker.EyeCenter.y, PlayerPrefs.GetFloat("BottomCalibration"),
                PlayerPrefs.GetFloat("TopCalibration"), 0.0f, 1.0f);
            // middle is 0.0f, left is -0.5f, right is 0.5f
            var x = (centerX - 0.5f) * head.GetComponent<AsymFrustum>().width;
            var y = (centerY - 0.5f) * head.GetComponent<AsymFrustum>().height;
            // update text in UI
            distance.text = (int) Calibration.GetRealHeadDistance() + "cm";
            // update the position of the head
            head.transform.position = new Vector3(x, y, head.transform.position.z);
        }
        #endregion
    }
}