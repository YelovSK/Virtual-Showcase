using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        EyeTracker _tracker;
        Camera _camera;
        Vector3 _camPos = new Vector3(0, 1, -10);
        float _camFOV = 30;

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
        }

        void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name == "Main")
                Transform();
        }

        void Transform()
        {
            // get new tracking position
            Vector2 leftEye = _tracker.LeftEye;
            Vector2 rightEye = _tracker.RightEye;
            
            // literally random offset values
            var angle = Vector2.Angle(rightEye - leftEye, Vector2.right);
            if (leftEye.y > rightEye.y) angle *= -1;
            Vector3 offset;
            offset.x = (leftEye[0] - 0.5f + rightEye[0] - 0.5f) * -2.5f;
            offset.y = (leftEye[1] - 0.5f + rightEye[1] - 0.5f) * 2.5f;
            offset.z = Mathf.Abs(leftEye[0] - rightEye[0]) * 5;
            var fov = _camFOV + Mathf.Abs(offset.z / 5) * 200;

            // update camera position
            _camera.fieldOfView = fov;
            var camTransform = _camera.transform;
            camTransform.position = offset + _camPos;
            camTransform.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }

}