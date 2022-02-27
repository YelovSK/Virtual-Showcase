using System;
using System.Collections;
using System.Collections.Generic;
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
        int _currFrame = 0;
        Dictionary<string, int> colours = new Dictionary<string, int>()
        {
            {"red", 0}, {"yellow", 60}, {"green", 120}, {"cyan", 180}, {"blue", 240}, {"magenta", 300}
        };
        GameObject _colorBox;
        TMP_Text _pixelCountText;

        void Start()
        {
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
            _tracker = GetComponent<EyeTracker>();
            _webcam = GameObject.FindWithTag("Face tracking").GetComponent<WebcamInput>();
            _colorBox = GameObject.FindWithTag("ColorBox");
            _pixelCountText = _colorBox.GetComponentInChildren<TMP_Text>();
        }

        void LateUpdate()
        {
            if (SceneManager.GetActiveScene().name != "Main")
                return;
            print("UPDATED");
            if (_webcam.WebCamTexture.didUpdateThisFrame)
                Transform();
        }
        
        void Transform()
        {
            if (!CheckGlassesOn())
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
            // todo: show overlay in menu
            // run every 10th frame todo: performance seems to be fine running every frame
            // _currFrame++;
            // if (_currFrame % 10 != 0)
            //     return;
            
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
            int radius = (int) (rightEye.x - leftEye.x);
            int startX = (int) (leftEye.x - radius);
            if (startX < 0)
                startX = 0;
            int endX = (int) (rightEye.x + radius);
            if (endX > tex.width)
                endX = tex.width;
            int startY = (int) (center.y - (float)radius/2);
            if (startY < 0)
                startY = 0;
            int endY = (int) (center.y + (float)radius/2);
            if (endY > tex.height)
                endY = tex.height;
            
            // percentage of found pixels in the area
            float threshold = 0.3f;
            int allPixels = (endX - startX) * (endY - startY);
            
            // get pixels in range <(startX, startY), (endX, endY)>
            // go through every pixel and check if in colour threshold
            var foundPixels = new List<List<int>>();    // coords of found pixels
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    var pixel = tex.GetPixel(x, y);
                    if (PixelInTreshold(pixel))
                        foundPixels.Add(new List<int>(){x, y});
                }
            }
            
            // check if threshold was passed
            bool passed = (float) foundPixels.Capacity / allPixels > threshold;

            // don't compute overlay if preview is not showing
            if (PlayerPrefs.GetInt("previewIx") != 0)
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
            var width = (float) (endX - startX) / tex.width;
            var height = (float) (endY - startY) / tex.height;
            var size = new Vector2(width, height) * cBoxParentRect.size;
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            // <SET OVERLAY BOX POSITION AND SIZE>

            // <HIGHLIGHT PIXELS>
            // create an empty texture
            var tex2D = new Texture2D((int)endX - startX, (int)endY - startY);
            
            // set all texture pixels to transparent except the border
            int borderSize = 2;
            for (int x = borderSize; x < (int)(endX - startX) - borderSize; x++)
            {
                for (int y = borderSize; y < (int)endY - startY - borderSize; y++)
                {
                    tex2D.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
            
            // set found pixels to a given colour
            Color col;
            foreach (var pixel in foundPixels)
            {
                int x = pixel[0] - startX;
                int y = pixel[1] - startY;
                tex2D.SetPixel(x, y, Color.cyan);
            }
            
            // apply and set the texture
            tex2D.Apply();
            _colorBox.GetComponent<RawImage>().texture = tex2D;
            // <HIGHLIGHT PIXELS>
            
            return passed;
        }
        
        private bool PixelInTreshold(Color pixel)
        {
            int threshold = 30;
            float h, s, v;
            Color.RGBToHSV(pixel, out h, out s, out v);
            if (v < 0.2)
                return false;
            // map from 0.0 - 1.0 to 0-360
            int hue = (int) (360 * h);
            if (Math.Abs(hue - colours["blue"]) < threshold)
                return true;
            return false;
        }
    }
    

}