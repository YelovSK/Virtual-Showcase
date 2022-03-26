using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        [SerializeField] GameObject head;
        [SerializeField] Text distance;

        WebcamInput _webcam;
        ColourCheck _colourCheck;

        void Start()
        {
            _webcam = GetComponent<WebcamInput>();
            _colourCheck = GetComponent<ColourCheck>();
        }

        void LateUpdate()
        {
            if (!EyeTracker.DetectedThisFrame)
                return;
            if (_colourCheck.GlassesOn)
                Transform();
            head.GetComponent<AsymFrustum>().UpdateProjectionMatrix();
        }

        void Transform()
        {
            // head distance ratio of CurrentDistance / CalibratedDistance
            var offset = Calibration.GetRealHeadDistance() / PlayerPrefs.GetInt("distanceFromScreenCm");
            // at calibrated distance, mapping to low: 0.0 - high: 1.0
            // if distance changes, mapping changes due to the camera seeing the face at different coordinates
            var diff = offset - 1.0f;
            var low = 0.0f - (diff / 2);
            var high = 1.0f + (diff / 2);
            // map coords based on the calibration
            float centerX = Map(EyeTracker.EyeCenter.x, PlayerPrefs.GetFloat("LeftCalibration"), PlayerPrefs.GetFloat("RightCalibration"), low, high);
            float centerY = Map(EyeTracker.EyeCenter.y, PlayerPrefs.GetFloat("BottomCalibration"), PlayerPrefs.GetFloat("TopCalibration"), low, high);
            // middle is 0.0f, left is -0.5f, right is 0.5f
            float x = (centerX - 0.5f) * head.GetComponent<AsymFrustum>().width;
            float y = (centerY - 0.5f) * head.GetComponent<AsymFrustum>().height;
            // update text in UI
            distance.text = (int) Calibration.GetRealHeadDistance() + "cm";
            // update the position of the head
            head.transform.position = new Vector3(x, y, -Calibration.GetRealHeadDistance());
        }

        /// <summary>
        /// Maps a value from a range to another range.
        /// </summary>
        /// <param name="n">Value to map</param>
        /// <param name="start1">Start of the original range</param>
        /// <param name="stop1">End of the original range</param>
        /// <param name="start2">Start of the target range</param>
        /// <param name="stop2">End of the target range</param>
        /// <returns>Mapped value</returns>
        public static float Map(float n, float start1, float stop1, float start2, float stop2)
        {
            return ((n-start1) / (stop1-start1)) * (stop2-start2) + start2;
        }


    }
}