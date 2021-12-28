using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        Marker _marker;
        private new Camera camera;
        float camX = 0;
        float camY = 1;
        float camZ = -10;
        float camFOV = 30;
        float prevAngle = 0;
        List<Vector2> leftEyeHistory;
        List<Vector2> rightEyeHistory;
        int framesSmoothed = 30;
        KalmanFilter _kalmanFilter;
        KalmanFilter.Measurement<Vector2> leftMeasurement;
        KalmanFilter.Measurement<Vector2> rightMeasurement;
        Vector2 leftEye;
        Vector2 rightEye;

        void Start()
        {
            leftEyeHistory = new List<Vector2>();
            rightEyeHistory = new List<Vector2>();
            _kalmanFilter = new KalmanFilter(0.0005f, 0.2f);
            leftMeasurement = new KalmanFilter.Measurement<Vector2>();
            rightMeasurement = new KalmanFilter.Measurement<Vector2>();
            _marker = GetComponent<Marker>();
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        }

        void LateUpdate()
        {
            var detection = _marker.detection;
            leftEye = detection.leftEye;
            rightEye = detection.rightEye;
            switch (GlobalVars.smoothing)
            {
                case "Kalman":
                    smoothKalman();
                    break;
                case "Average":
                    smoothAverage();
                    break;
                case "Off":
                    break;
            }
            Transform(leftEye, rightEye);
        }

        void smoothKalman()
        {
            leftMeasurement.SetObservedValue(leftEye);
            rightMeasurement.SetObservedValue(rightEye);
            _kalmanFilter.KalmanUpdate(leftMeasurement);
            _kalmanFilter.KalmanUpdate(rightMeasurement);
            leftEye = leftMeasurement.x;
            rightEye = rightMeasurement.x;
        }

        void smoothAverage()
        {
            if (framesSmoothed > 1)
            {
                leftEyeHistory.Add(leftEye);
                rightEyeHistory.Add(rightEye);
                if (leftEyeHistory.Count > framesSmoothed)
                {
                    leftEyeHistory.RemoveAt(0);
                    rightEyeHistory.RemoveAt(0);
                }
                var leftSum = new Vector2(0, 0);
                var rightSum = new Vector2(0, 0);
                foreach (var l in leftEyeHistory)
                {
                    leftSum += l;
                }
                foreach (var l in rightEyeHistory)
                {
                    rightSum += l;
                }
                leftEye = leftSum / leftEyeHistory.Count;
                rightEye = rightSum / rightEyeHistory.Count;
            }
        }

        void Transform(Vector2 leftEye, Vector2 rightEye)
        {
            var diff = rightEye - leftEye;
            var angle = Vector2.Angle(diff, Vector2.right);
            if (leftEye.y > rightEye.y) angle *= -1;
            Vector3 pos = camera.transform.position;
            var x = (leftEye[0] - 0.5f + rightEye[0] - 0.5f) / 2;
            var y = (leftEye[1] - 0.5f + rightEye[1] - 0.5f) / 2;
            var z = Mathf.Abs(leftEye[0] - rightEye[0]);
            var fov = camFOV + Mathf.Abs(z) * 200;
            pos.x = camX - (x * 5);
            pos.y = camY + (y * 5);
            pos.z = camZ + z * 5;
            camera.transform.position = pos;
            camera.fieldOfView = fov;
            camera.transform.Rotate(0, 0, prevAngle);
            camera.transform.Rotate(0, 0, -angle);
            prevAngle = angle;
        }
}

}