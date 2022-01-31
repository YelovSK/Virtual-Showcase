using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        EyeTracker _tracker;
        private Camera camera;
        float camX = 0;
        float camY = 1;
        float camZ = -10;
        float camFOV = 30;

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
        }

        void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name == "Main")
                Transform();
        }

        void Transform()
        {
            Vector2 leftEye = _tracker.getLeftEye();
            Vector2 rightEye = _tracker.getRightEye();
            var diff = rightEye - leftEye;
            var angle = Vector2.Angle(diff, Vector2.right);
            if (leftEye.y > rightEye.y) angle *= -1;
            var x = (leftEye[0] - 0.5f + rightEye[0] - 0.5f) / 2;
            var y = (leftEye[1] - 0.5f + rightEye[1] - 0.5f) / 2;
            var z = Mathf.Abs(leftEye[0] - rightEye[0]);
            var fov = camFOV + Mathf.Abs(z) * 200;
            
            Vector3 pos;
            pos.x = camX - (x * 5);
            pos.y = camY + (y * 5);
            pos.z = camZ + z * 5;
            UpdateCamView(fov, pos, angle);
        }

        void UpdateCamView(float fov, Vector3 pos, float angle)
        {
            var camTransform = camera.transform;
            camera.fieldOfView = fov;
            camTransform.position = pos;
            camTransform.localEulerAngles = new Vector3(0, 0, -angle);
        }
    }

}