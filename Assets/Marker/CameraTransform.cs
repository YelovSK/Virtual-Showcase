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
        // stuff
        EyeTracker _tracker;
        Camera _cam;
        Camera _lCam;
        Camera _rCam;
        WebcamInput _webcam;

        // waiting for coloured glasses around eyes
        GameObject _colorBox;
        TMP_Text _pixelCountText;

        void Start()
        {
            _cam = GameObject.Find("Main Camera").GetComponent<Camera>();
            // _lCam = GameObject.FindWithTag("leftCam").GetComponent<Camera>();
            // _rCam = GameObject.FindWithTag("rightCam").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
            _webcam = GameObject.FindWithTag("Face tracking").GetComponent<WebcamInput>();
            _colorBox = GameObject.FindWithTag("ColorBox");
            _pixelCountText = _colorBox.GetComponentInChildren<TMP_Text>();
        }

        void LateUpdate()
        {
            if (!_webcam.WebCamTexture.didUpdateThisFrame)
                return;
            // if (CheckGlassesOn())
            //     Transform();
            Transform();
            _cam.GetComponent<AsymFrustum>().UpdateProjectionMatrix();
        }
        
        void Transform()
        {
            // don't transform in menu
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            Vector2 eyeCenter = (_tracker.LeftEye + _tracker.RightEye) / 2;
            // X middle is 0.0f, left is -0.5f, right is 0.5f
            float x = -(eyeCenter.x - 0.5f) * _cam.GetComponent<AsymFrustum>().width;
            // Y middle is 0.5f, bottom is 0.0f, top is 1.0f
            float y = eyeCenter.y * _cam.GetComponent<AsymFrustum>().height;
            _cam.transform.position = new Vector3(x, y, _cam.transform.position.z);
        }

        // todo: hide box when eye not detected
        private bool CheckGlassesOn()
        {
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
            CalculateColourBoxSize(tex, leftEye, rightEye, out var startX, out var endX, out var startY, out var endY);
            int boxWidth = endX - startX;
            int boxHeight = endY - startY;
            int allPixels = boxWidth * boxHeight;

            // find pixels in threshold [(x, y), (x, y), ...]
            var foundPixels = FindPixels(tex, startX, startY, boxWidth, boxHeight);

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
            // </SET OVERLAY BOX POSITION AND SIZE>

            // <HIGHLIGHT PIXELS> START
            // create an empty texture
            var tex2D = new Texture2D(boxWidth, boxHeight);

            // set all texture pixels to transparent
            Color[] fillPixels = new Color[boxWidth * boxHeight];
            tex2D.SetPixels(fillPixels);
            
            // set found pixels to a given colour
            foreach (var pixel in foundPixels)
            {
                int x = pixel[0];
                int y = pixel[1];
                tex2D.SetPixel(x, y, Color.cyan);
            }

            // apply and set the texture
            tex2D.Apply();
            _colorBox.GetComponent<RawImage>().texture = tex2D;
            // </HIGHLIGHT PIXELS> END
            
            return passed;
        }

        private void CalculateColourBoxSize(WebCamTexture tex, Vector2 leftEye, Vector2 rightEye, out int startX, out int endX, out int startY, out int endY)
        {
            int xRadius = (int) (rightEye.x - leftEye.x);
            int yRadius = (int) Math.Abs(leftEye.y - rightEye.y) + xRadius / 3;
            startX = (int) (leftEye.x - xRadius);
            startX = Math.Max(0, (int) (leftEye.x - xRadius));
            endX = (int) (rightEye.x + xRadius);
            endX = Math.Min(tex.width, endX);
            if (leftEye.y < rightEye.y)
            {
                startY = (int) (leftEye.y - yRadius);
                endY = (int) (rightEye.y + yRadius);
            }
            else
            {
                startY = (int) (rightEye.y - yRadius);
                endY = (int) (leftEye.y + yRadius);
            }
            startY = Math.Max(0, startY);
            endY = Math.Min(tex.height, endY);
        }

        private List<List<int>> FindPixels(WebCamTexture tex, int startX, int startY, int boxWidth, int boxHeight)
        {
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
            return foundPixels;
        }
        
        private bool PixelInTreshold(Color pixel, int thresh, int targetHue)
        {
            Color.RGBToHSV(pixel, out var h, out var s, out var v);
            // brighter than 20% and higher saturation than 40%, otherwise not in threshold
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