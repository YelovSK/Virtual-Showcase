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
        Texture2D _colOverlayTexture;

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
            bool glassesOn = CheckGlassesOn();
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            if (glassesOn)
                Transform();
            _cam.GetComponent<AsymFrustum>().UpdateProjectionMatrix();
        }
        
        void Transform()
        {
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
            // convert RenderTexture from WebcamInput to Texture2D
            // to be able to read pixels
            // not using WebCamTexture because it has different aspect ratio
            var tex = RenderTextureToTexture2D(_webcam.Texture);

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
            float threshold = 0.05f;
            bool passed = (float) foundPixels.Capacity / allPixels > threshold;

            // don't compute overlay if preview is not showing or not in menu
            if (PlayerPrefs.GetInt("previewIx") != 0 && SceneManager.GetActiveScene().name == "Main")
            {
                Destroy(tex);
                return passed;
            }

            // set label text to show number of found pixels
            if (passed)
                _pixelCountText.color = Color.green;
            else
                _pixelCountText.color = Color.red;
            _pixelCountText.text = foundPixels.Count + " / " + allPixels;
            
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
            if (_colOverlayTexture != null)
                Destroy(_colOverlayTexture);
            _colOverlayTexture = new Texture2D(boxWidth, boxHeight);

            // set all texture pixels to transparent
            Color[] fillPixels = new Color[boxWidth * boxHeight];
            _colOverlayTexture.SetPixels(fillPixels);
            
            // set found pixels to a given colour
            foreach (var pixel in foundPixels)
            {
                int x = pixel[0];
                int y = pixel[1];
                _colOverlayTexture.SetPixel(x, y, Color.cyan);
            }

            // apply and set the texture
            _colOverlayTexture.Apply();
            _colorBox.GetComponent<RawImage>().texture = _colOverlayTexture;
            // </HIGHLIGHT PIXELS> END

            Destroy(tex);
            return passed;
        }
        
        private Texture2D RenderTextureToTexture2D(RenderTexture rTex)
        {
            Texture2D dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            Graphics.CopyTexture(rTex, dest);
            dest.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            return dest;
        }

        private void CalculateColourBoxSize(Texture2D tex, Vector2 leftEye, Vector2 rightEye, out int startX, out int endX, out int startY, out int endY)
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

        private List<List<int>> FindPixels(Texture2D tex, int startX, int startY, int boxWidth, int boxHeight)
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
            // brighter than 20% and higher saturation than 30%, otherwise not in threshold
            if (v < 0.2 || s < 0.3)
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