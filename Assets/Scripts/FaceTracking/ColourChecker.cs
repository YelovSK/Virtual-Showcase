using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.Core;

namespace VirtualVitrine.FaceTracking
{
    public class ColourChecker : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private RawImage colorBox;
        #endregion
        
        #region Private Fields
        private Texture2D _colOverlayTexture;
        private TMP_Text _pixelCountText;
        #endregion

        #region Public Methods
        /// <summary>
        /// Checks pixels in the color box and updates the text to show the number of pixels
        /// </summary>
        /// <param name="tex">Source texture</param>
        /// <returns>True if threshold passed</returns>
        public bool CheckGlassesOn(WebCamTexture tex)
        {
            // if glasses check is off, don't display overlay and set to true to transform anyway
            if (PlayerPrefs.GetInt("glassesCheck") == 0)
            {
                HideUI();
                return true;
            }
            colorBox.gameObject.SetActive(true);
            // resolution of the 1:1 texture
            var resolution = Math.Min(tex.width, tex.height);

            // map coords from 0.0 - 1.0 to width and height of WebcamTexture
            var leftEye = new Vector2(
                EyeSmoother.LeftEyeSmoothed.x * resolution,
                EyeSmoother.LeftEyeSmoothed.y * resolution);
            var rightEye = new Vector2(
                EyeSmoother.RightEyeSmoothed.x * resolution,
                EyeSmoother.RightEyeSmoothed.y * resolution);
            var center = (leftEye + rightEye) / 2;

            // look from (startX, startY) to (endX, endY)
            CalculateColourBoxSize(resolution, leftEye, rightEye, out var startX, out var endX, out var startY, out var endY);
            var boxWidth = endX - startX;
            var boxHeight = endY - startY;
            var allPixels = boxWidth * boxHeight;

            // tex has original aspect ratio, but the texture in UI is square
            // so we have to offset the starting position to get pixels of the inner square
            // e.g. if tex is 1280x720, then the inner square is 720x720 and starting X=(1280-720) / 2
            int offset = Math.Abs(tex.width - tex.height) / 2;
            // find pixels in threshold [(x, y), (x, y), ...]
            var (foundPixels, foundPixelsCount) = FindPixels(tex, offset, startX, startY, boxWidth, boxHeight);

            // check if threshold was passed
            const float threshold = 0.05f;
            var passed = (float) foundPixelsCount / allPixels > threshold;
            // don't compute overlay if preview is not showing or not in menu
            if (PlayerPrefs.GetInt("previewIx") != 0 && GlobalManager.InMainScene)
            {
                // Destroy(tex);
                return passed;
            }

            // set label text to show number of found pixels
            _pixelCountText.color = passed ? Color.green : Color.red;
            _pixelCountText.text = foundPixelsCount + " / " + allPixels;

            #region Set Overlay Box Position and Size
            var cBox = (RectTransform) colorBox.transform;
            var cBoxParent = (RectTransform) cBox.parent;
            var cBoxParentRect = cBoxParent.rect;

            // center mapped to 0.0-1.0
            var centerFloat = new Vector2(center.x / resolution, center.y / resolution);
            cBox.anchoredPosition = centerFloat * cBoxParentRect.size;

            // box size
            var width = (float) boxWidth / resolution;
            var height = (float) boxHeight / resolution;
            var size = new Vector2(width, height) * cBoxParentRect.size;
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            #endregion
            
            #region Highlight Pixels
            // create an empty texture
            if (_colOverlayTexture != null)
                Destroy(_colOverlayTexture);
            _colOverlayTexture = new Texture2D(boxWidth, boxHeight);

            // set found pixels to a given colour
            var fillPixels = new Color32[boxWidth * boxHeight];
            // var col = new Color32(255, 255, 255, 255);
            var col = Color.white;
            for (var y = 0; y < boxHeight; y++)
            for (var x = 0; x < boxWidth; x++)
                if (foundPixels[x, y])
                    fillPixels[x + y * boxWidth] = col;
            _colOverlayTexture.SetPixels32(fillPixels);

            // apply and set the texture
            _colOverlayTexture.Apply();
            colorBox.texture = _colOverlayTexture;
            #endregion

            // Destroy(tex);
            return passed;
        }
        
        public void HideUI()
        {
            colorBox.gameObject.SetActive(false);
            if (_colOverlayTexture != null)
                Destroy(_colOverlayTexture);
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _pixelCountText = colorBox.GetComponentInChildren<TMP_Text>();
        }
        #endregion
        
        #region Private Methods
        private static void CalculateColourBoxSize(int resolution, Vector2 leftEye, Vector2 rightEye, out int startX,
            out int endX, out int startY, out int endY)
        {
            // size of the box
            var xRadius = (int) rightEye.x - leftEye.x;
            var yRadius = (int) Math.Abs(leftEye.y - rightEye.y) + xRadius / 3;
            
            // start and end points of the box
            startX = (int) (leftEye.x - xRadius);
            endX = (int) (rightEye.x + xRadius);
            startY = (int) (Math.Min(leftEye.y, rightEye.y) - yRadius);
            endY = (int) (Math.Max(leftEye.y, rightEye.y) + yRadius);

            // if out of bounds, set to min/max
            startX = Math.Max(0, startX);
            endX = Math.Min(resolution, endX);
            startY = Math.Max(0, startY);
            endY = Math.Min(resolution, endY);
        }

        /// <summary>
        ///     Goes through pixels in a box and finds those that are in colour threshold
        /// </summary>
        /// <param name="tex">Texture to go through</param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <returns>Returns a tuple: 2D array of bools, number of found pixels</returns>
        private static Tuple<bool[,], int> FindPixels(WebCamTexture tex, int offset, int startX, int startY, int boxWidth, int boxHeight)
        {
            // get pixels in range <(startX, startY), (endX, endY)>
            var pixels = tex.GetPixels(startX+offset, startY, boxWidth, boxHeight);
            
            // 2D array of pixels, true if in threshold, false if not
            var foundPixelsArr = new bool[boxWidth, boxHeight];
            var pixelsInThresholdCount = 0;
            
            var targetHue = PlayerPrefs.GetInt("hue");
            var hueThresh = PlayerPrefs.GetInt("hueThresh");
            
            // go through every pixel
            for (var y = 0; y < boxHeight; y++)
            for (var x = 0; x < boxWidth; x++)
            {
                var pixel = pixels[x + y * boxWidth];
                var inThreshold = PixelInThreshold(pixel, hueThresh, targetHue);
                foundPixelsArr[x, y] = inThreshold;
                if (inThreshold)
                    pixelsInThresholdCount++;
            }

            return Tuple.Create(foundPixelsArr, pixelsInThresholdCount);
        }

        private static bool PixelInThreshold(Color pixel, int thresh, int targetHue)
        {
            // get hsv values
            Color.RGBToHSV(pixel, out var h, out var s, out var v);
            
            // brighter than 20% and higher saturation than 30%, otherwise not in threshold
            if (v < 0.2 || s < 0.3)
                return false;
            
            // map from 0.0 - 1.0 to 0-360
            var hue = (int) (360 * h);
            
            // get difference in 360 degrees
            var diff = hue > targetHue ? hue - targetHue : targetHue - hue;
            var hueDifference = Math.Min(diff, 360 - diff);
            return hueDifference < thresh;
        }
        #endregion
    }
    
}