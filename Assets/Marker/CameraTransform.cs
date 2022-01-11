using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        EyeTracker _tracker;
        new Camera camera;
        float camX = 0;
        float camY = 1;
        float camZ = -10;
        float camFOV = 30;
        float prevAngle = 0;

        void Start()
        {
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
        }

        void LateUpdate()
        {
            Transform();
        }

        void Transform()
        {
            Vector2 leftEye = _tracker.getLeftEye();
            Vector2 rightEye = _tracker.getRightEye();
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