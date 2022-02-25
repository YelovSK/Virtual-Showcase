using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MediaPipe.BlazeFace {

    public class CameraTransform : MonoBehaviour
    {
        EyeTracker _tracker;
        Camera _camera;
        Vector3 _camPos = new Vector3(0, 1, -10);
        WebcamInput _webcam;
        float _camFOV = 30;

        // waiting for coloured glasses around eyes
        bool _transformEnabled = false;
        int _currFrame = 0;
        Dictionary<string, int> colours = new Dictionary<string, int>()
        {
            {"red", 0}, {"yellow", 60}, {"green", 120}, {"cyan", 180}, {"blue", 240}, {"magenta", 300}
        };

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
            _webcam = GameObject.FindWithTag("Face tracking").GetComponent<WebcamInput>();
        }

        void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name == "Main" && _webcam.WebCamTexture.didUpdateThisFrame)
                Transform();
        }

        void Transform()
        {
            if (!_transformEnabled)
            {
                CheckGlassesPosition();
                return;
            }
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

        private void CheckGlassesPosition()
        {
            // run every 10th frame
            _currFrame++;
            if (_currFrame % 10 != 0)
                return;
            
            WebCamTexture tex = _webcam.WebCamTexture;
            
            // map coords from 0.0 - 1.0 to width and height of WebcamTexture
            var leftEye = new Vector2(
                _tracker.LeftEye.x * tex.width,
                _tracker.LeftEye.y * tex.height);
            var rightEye = new Vector2(
                _tracker.RightEye.x * tex.width,
                _tracker.RightEye.y * tex.height);
            Vector2 center = leftEye + (rightEye - leftEye);
            
            // look from (startX, startY) to (endX, endY)
            int radius = (int) (rightEye.x - leftEye.x);
            int startX = (int) (leftEye.x - radius);
            if (startX < 0)
                startX = 0;
            int endX = (int) (rightEye.x + radius);
            if (endX > tex.width)
                endX = tex.width;
            int startY = (int) (center.y - radius/2);
            if (startY < 0)
                startY = 0;
            int endY = (int) (center.y + radius/2);
            if (endY > tex.height)
                endY = tex.height;
            
            // percentage of found pixels in the area
            float threshold = 0.3f;
            int allPixels = (endX - startX) * (endY - startY);
            int foundPixels = 0;
            
            // get pixels in range <(startX, startY), (endX, endY)>
            var pixels = tex.GetPixels(startX, startY, endX - startX, endY - startY);
            
            // go through every pixel
            int hueThreshold = 30;
            foreach (var col in pixels)
            {
                // hue component of HSL
                int hue = GetHueFromPixel(col);
                if (Math.Abs(hue - colours["blue"]) < hueThreshold)
                    foundPixels++;
            }

            // check if threshold was passed
            if ((float) foundPixels / allPixels > threshold)
            {
                print("Threshold passed");
                // _transformEnabled = true;    // todo: test more
            }
            else
                print(foundPixels + " / " + allPixels);
        }
        
        private int GetHueFromPixel(Color pixel)
        {
            float h, s, v;
            Color.RGBToHSV(pixel, out h, out s, out v);
            // map from 0.0 - 1.0 to 0-360
            int hue = (int) (360 * h);
            return hue;
        }
    }
    

}