using System;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        // stuff
        [SerializeField] GameObject head;

        EyeTracker _tracker;
        WebcamInput _webcam;
        ColourCheck _colourCheck;

        // waiting for coloured glasses around eyes

        void Start()
        {
            _tracker = GetComponent<Visualizer>().EyeTracker;
            _webcam = GetComponent<WebcamInput>();
            _colourCheck = GetComponent<ColourCheck>();
        }

        void LateUpdate()
        {
            if (!_webcam.CameraUpdated())
                return;
            if (_colourCheck.GlassesOn)
                Transform();
            head.GetComponent<AsymFrustum>().UpdateProjectionMatrix();
        }

        void Transform()
        {
            // map coords based on the calibration
            float centerX = Map(_tracker.EyeCenter.x, PlayerPrefs.GetFloat("LeftCalibration"), PlayerPrefs.GetFloat("RightCalibration"), 1.0f, 0.0f);
            float centerY = Map(_tracker.EyeCenter.y, PlayerPrefs.GetFloat("BottomCalibration"), PlayerPrefs.GetFloat("TopCalibration"), 0.0f, 1.0f);
            // middle is 0.0f, left is -0.5f, right is 0.5f
            float x = -(centerX - 0.5f) * head.GetComponent<AsymFrustum>().width;
            float y = (centerY - 0.5f) * head.GetComponent<AsymFrustum>().height;
            // update the position of the head
            head.transform.position = new Vector3(x, y, head.transform.position.z);
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