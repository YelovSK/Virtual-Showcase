using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        Dictionary<string, int> colours = new Dictionary<string, int>()
        {
            {"red", 0}, {"yellow", 60}, {"green", 120},
            {"cyan", 180}, {"blue", 240}, {"magenta", 300}
        };
        GameObject _colorBox;
        TMP_Text _pixelCountText;

        void Start()
        {
            _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
            _webcam = GameObject.FindWithTag("Face tracking").GetComponent<WebcamInput>();
            _colorBox = GameObject.FindWithTag("ColorBox");
            _pixelCountText = _colorBox.GetComponentInChildren<TMP_Text>();
        }

        void LateUpdate()
        {
            if (_webcam.WebCamTexture.didUpdateThisFrame)
                return;
            if (CheckGlassesOn())
                Transform();
        }
        
        void Transform()
        {
            // don't transform in menu
            if (SceneManager.GetActiveScene().name != "Main")
                return;

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

        private bool CheckGlassesOn()
        {
            // todo: hide box when eye not detected
            WebCamTexture tex = _webcam.WebCamTexture;

            // map coords from 0.0 - 1.0 to width and height of WebcamTexture
            var leftEye = new Vector2(
                _tracker.LeftEye.x * tex.width,
                _tracker.LeftEye.y * tex.height);
            var rightEye = new Vector2(
                _tracker.RightEye.x * tex.width,
                _tracker.RightEye.y * tex.height);
            Vector2 center = (leftEye + rightEye) / 2;
            
            // look from (startX, startY) to (endX, endY)
            int xRadius = (int) (rightEye.x - leftEye.x);
            int yRadius = (int) Math.Abs(leftEye.y - rightEye.y) + xRadius / 3;
            int startX = (int) (leftEye.x - xRadius);
            if (startX < 0)
                startX = 0;
            int endX = (int) (rightEye.x + xRadius);
            if (endX > tex.width)
                endX = tex.width;
            int startY = -1;
            int endY = -1;
            if (leftEye.y < rightEye.y)
            {
                startY = (int) (leftEye.y - yRadius);
                endY = (int) (rightEye.y + yRadius);
            }
            else
            {
                startY = (int) (rightEye.y - yRadius);
                endY = (int) (rightEye.y + yRadius);
            }
            if (startY < 0)
                startY = 0;
            if (endY > tex.height)
                endY = tex.height;

            int boxWidth = endX - startX;
            int boxHeight = endY - startY;
            int allPixels = boxWidth * boxHeight;
            
            // get pixels in range <(startX, startY), (endX, endY)>
            var pixels = tex.GetPixels(startX, startY, boxWidth, boxHeight);
            // go through every pixel and check if in colour threshold
            var foundPixels = new List<List<int>>();    // coords of found pixels
            int targetHue = PlayerPrefs.GetInt("hue");
            int hueThresh = PlayerPrefs.GetInt("hueThresh");
            for (int y = 0; y < boxHeight; y++)
            {
                for (int x = 0; x < boxWidth; x++)
                {
                    var pixel = pixels[x + y * boxWidth];
                    if (PixelInTreshold(pixel, hueThresh, targetHue))
                        foundPixels.Add(new List<int>(){x, y});
                }
            }

            // check if threshold was passed
            float threshold = 0.075f;
            bool passed = (float) foundPixels.Capacity / allPixels > threshold;

            // don't compute overlay if preview is not showing or not in menu
            if (PlayerPrefs.GetInt("previewIx") != 0 && SceneManager.GetActiveScene().name == "Main")
                return passed;

            // set label text to show number of found pixels
            string pass = "";
            if (passed)
                pass = " -> PASSED";
            _pixelCountText.text = foundPixels.Count + " / " + allPixels + pass;
            
            // <SET OVERLAY BOX POSITION AND SIZE>
            var cBox = (RectTransform) _colorBox.transform;
            var cBoxParent = (RectTransform) cBox.parent;
            var cBoxParentRect = cBoxParent.rect;

            // center mapped to 0.0-1.0
            var centerFloat = new Vector2((center.x) / tex.width, center.y / tex.height);
            cBox.anchoredPosition = centerFloat * cBoxParentRect.size;

            // box size
            var width = (float) boxWidth / tex.width;
            var height = (float) boxHeight / tex.height;
            var size = new Vector2(width, height) * cBoxParentRect.size;
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            // <SET OVERLAY BOX POSITION AND SIZE>

            // <HIGHLIGHT PIXELS> START
            // create an empty texture
            var tex2D = new Texture2D(boxWidth, boxHeight);

            // set all texture pixels to transparent
            Color[] fillPixels = new Color[boxWidth * boxHeight];
            tex2D.SetPixels(fillPixels);
            
            // set found pixels to a given colour
            Color col;
            foreach (var pixel in foundPixels)
            {
                int x = pixel[0];
                int y = pixel[1];
                tex2D.SetPixel(x, y, Color.cyan);
            }

            // apply and set the texture
            tex2D.Apply();
            _colorBox.GetComponent<RawImage>().texture = tex2D;
            // <HIGHLIGHT PIXELS> END
            
            return passed;
        }
        
        private bool PixelInTreshold(Color pixel, int thresh, int targetHue)
        {
            float h, s, v;
            Color.RGBToHSV(pixel, out h, out s, out v);
            // brighter than 20% and higher saturation than 40%
            if (v < 0.2 || s < 0.4)
                return false;
            // map from 0.0 - 1.0 to 0-360
            int hue = (int) (360 * h);
            int hueDifference = Math.Min(Math.Abs(hue - targetHue), 360 - Math.Abs(hue - targetHue));
            if (hueDifference < thresh)
                return true;
            return false;
        }
    }
    

}