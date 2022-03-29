using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VirtualVitrine.FaceTracking.Transform
{
    public class ColourCheck : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject colorBox;
        [SerializeField] private TMP_Text pixelCountText;
        #endregion
        
        #region Private Fields
        private Texture2D _colOverlayTexture;
        private WebcamInput _webcam;
        #endregion
        
        #region Public Fields
        public bool GlassesOn { get; private set; }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            _webcam = GetComponent<WebcamInput>();
        }

        private void LateUpdate()
        {
            if (EyeTracker.DetectedThisFrame)
                GlassesOn = CheckGlassesOn();
            else if (EyeTracker.CameraUpdated)
                HideUI();
        }
        #endregion

        #region Private Methods
        private bool CheckGlassesOn()
        {
            // if glasses check is off, don't display overlay and set to true to transform anyway
            if (PlayerPrefs.GetInt("glassesCheck") == 0)
            {
                HideUI();
                return true;
            }

            // convert RenderTexture from WebcamInput to Texture2D
            // to be able to read pixels
            // not using WebCamTexture because it has different aspect ratio
            var tex = RenderTextureToTexture2D(_webcam.Texture);

            // map coords from 0.0 - 1.0 to width and height of WebcamTexture
            var leftEye = new Vector2(
                EyeTracker.LeftEye.x * tex.width,
                EyeTracker.LeftEye.y * tex.height);
            var rightEye = new Vector2(
                EyeTracker.RightEye.x * tex.width,
                EyeTracker.RightEye.y * tex.height);
            var center = (leftEye + rightEye) / 2;

            // look from (startX, startY) to (endX, endY)
            CalculateColourBoxSize(tex, leftEye, rightEye, out var startX, out var endX, out var startY, out var endY);
            var boxWidth = endX - startX;
            var boxHeight = endY - startY;
            var allPixels = boxWidth * boxHeight;

            // find pixels in threshold [(x, y), (x, y), ...]
            var (foundPixels, foundPixelsCount) = FindPixels(tex, startX, startY, boxWidth, boxHeight);

            // check if threshold was passed
            const float threshold = 0.05f;
            var passed = (float) foundPixelsCount / allPixels > threshold;
            // don't compute overlay if preview is not showing or not in menu
            if (PlayerPrefs.GetInt("previewIx") != 0 && SceneManager.GetActiveScene().name == "Main")
            {
                Destroy(tex);
                return passed;
            }

            // set label text to show number of found pixels
            pixelCountText.color = passed ? Color.green : Color.red;
            pixelCountText.text = foundPixelsCount + " / " + allPixels;

            #region Set Overlay Box Position and Size
            var cBox = (RectTransform) colorBox.transform;
            var cBoxParent = (RectTransform) cBox.parent;
            var cBoxParentRect = cBoxParent.rect;

            // center mapped to 0.0-1.0
            var centerFloat = new Vector2(center.x / tex.width, center.y / tex.height);
            cBox.anchoredPosition = centerFloat * cBoxParentRect.size;

            // box size
            var width = (float) boxWidth / tex.width;
            var height = (float) boxHeight / tex.height;
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
            var fillPixels = new Color[boxWidth * boxHeight];
            var col = Color.cyan;
            for (var y = 0; y < boxHeight; y++)
            for (var x = 0; x < boxWidth; x++)
                if (foundPixels[x, y])
                    fillPixels[x + y * boxWidth] = col;
            _colOverlayTexture.SetPixels(fillPixels);

            // apply and set the texture
            _colOverlayTexture.Apply();
            colorBox.GetComponent<RawImage>().texture = _colOverlayTexture;
            #endregion

            Destroy(tex);
            return passed;
        }

        private static Texture2D RenderTextureToTexture2D(RenderTexture rTex)
        {
            // set active texture
            var activeBefore = RenderTexture.active;
            RenderTexture.active = rTex;
            // create new texture2D
            var dest = new Texture2D(rTex.width, rTex.height, TextureFormat.RGBA32, false);
            // copy texture and read pixels
            Graphics.CopyTexture(rTex, dest);
            dest.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
            // set back the original active texture
            RenderTexture.active = activeBefore;
            return dest;
        }

        private static void CalculateColourBoxSize(Texture tex, Vector2 leftEye, Vector2 rightEye, out int startX,
            out int endX, out int startY, out int endY)
        {
            var xRadius = (int) (rightEye.x - leftEye.x);
            var yRadius = (int) Math.Abs(leftEye.y - rightEye.y) + xRadius / 3;
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

        /// <summary>
        ///     Goes through pixels in a box and finds those that are in colour threshold
        /// </summary>
        /// <param name="tex">Texture to go through</param>
        /// <param name="startX"></param>
        /// <param name="startY"></param>
        /// <param name="boxWidth"></param>
        /// <param name="boxHeight"></param>
        /// <returns>Returns a tuple: 2D array of bools, number of found pixels</returns>
        private Tuple<bool[,], int> FindPixels(Texture2D tex, int startX, int startY, int boxWidth, int boxHeight)
        {
            // get pixels in range <(startX, startY), (endX, endY)>
            var pixels = tex.GetPixels(startX, startY, boxWidth, boxHeight);
            // go through every pixel and check if in colour threshold
            var targetHue = PlayerPrefs.GetInt("hue");
            var hueThresh = PlayerPrefs.GetInt("hueThresh");
            // 2D array of pixels, true if in threshold, false if not
            var foundPixelsArr = new bool[boxWidth, boxHeight];
            var pixelsInThresholdCount = 0;
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
            Color.RGBToHSV(pixel, out var h, out var s, out var v);
            // brighter than 20% and higher saturation than 30%, otherwise not in threshold
            if (v < 0.2 || s < 0.3)
                return false;
            // map from 0.0 - 1.0 to 0-360
            var hue = (int) (360 * h);
            // get difference in 360 degrees
            var diff = Math.Abs(hue - targetHue);
            var hueDifference = Math.Min(diff, 360 - diff);
            return hueDifference < thresh;
        }

        private void HideUI()
        {
            if (_colOverlayTexture != null)
                Destroy(_colOverlayTexture);
            pixelCountText.text = "";
        }
        #endregion
    }
}