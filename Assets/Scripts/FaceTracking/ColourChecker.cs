using System;
using TMPro;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.SceneManagement;

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
            // If glasses check is off, don't display overlay and set to true to transform anyway.
            if (MyPrefs.GlassesCheck == 0)
            {
                HideUI();
                return true;
            }

            colorBox.gameObject.SetActive(true);
            // Resolution of the 1:1 texture.
            var resolution = Math.Min(tex.width, tex.height);

            // Map coords from 0.0 - 1.0 to width and height of WebcamTexture.
            var leftEye = new Vector2(
                EyeSmoother.LeftEyeSmoothed.x * resolution,
                EyeSmoother.LeftEyeSmoothed.y * resolution);
            var rightEye = new Vector2(
                EyeSmoother.RightEyeSmoothed.x * resolution,
                EyeSmoother.RightEyeSmoothed.y * resolution);
            var center = (leftEye + rightEye) / 2;

            // Look from (startX, startY) to (endX, endY).
            CalculateColourBoxSize(resolution, leftEye, rightEye, out var startX, out var endX, out var startY, out var endY);
            var boxWidth = endX - startX;
            var boxHeight = endY - startY;
            var allPixelsCount = boxWidth * boxHeight;

            // Tex has original aspect ratio, but the texture in UI is square
            // so we have to offset the starting position to get pixels of the inner square
            // e.g. if tex is 1280x720, then the inner square is 720x720 and starting X=(1280-720) / 2.
            var offset = Math.Abs(tex.width - tex.height) / 2;

            // Look at the pixels in the box.
            var (foundPixelsArr, foundPixelsCount) = FindPixels(tex, offset, startX, startY, boxWidth, boxHeight);

            // Check if threshold was passed.
            const float threshold = 0.05f;
            var passed = (float) foundPixelsCount / allPixelsCount > threshold;

            // Don't compute overlay if preview is not showing in main scene.
            if (MyPrefs.PreviewOn == 0 && SceneManager.GetActiveScene().name == "Main")
                return passed;

            // Set label text to show number of found pixels.
            _pixelCountText.color = passed ? Color.green : Color.red;
            _pixelCountText.text = foundPixelsCount + " / " + allPixelsCount;

            #region Set Overlay Box Position and Size
            var cBox = (RectTransform) colorBox.transform;
            var cBoxParent = (RectTransform) cBox.parent;
            var cBoxParentRect = cBoxParent.rect;

            // Center mapped to 0.0-1.0.
            var centerFloat = new Vector2(center.x / resolution, center.y / resolution);
            cBox.anchoredPosition = centerFloat * cBoxParentRect.size;

            // Box size.
            var width = (float) boxWidth / resolution;
            var height = (float) boxHeight / resolution;
            var size = new Vector2(width, height) * cBoxParentRect.size;
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
            #endregion
            
            #region Highlight Pixels
            // Create an empty texture.
            if (_colOverlayTexture != null)
                Destroy(_colOverlayTexture);
            _colOverlayTexture = new Texture2D(boxWidth, boxHeight);
            
            // Set found pixels to white (from FindPixels method).
            _colOverlayTexture.SetPixels32(foundPixelsArr);
            
            // Apply and set the texture.
            _colOverlayTexture.Apply();
            colorBox.texture = _colOverlayTexture;
            #endregion

            return passed;
        }
        
        public void HideUI() => colorBox.gameObject.SetActive(false);
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
            // Size of the box.
            var xRadius = (int) rightEye.x - leftEye.x;
            var yRadius = (int) Math.Abs(leftEye.y - rightEye.y) + xRadius / 3;
            
            // Start and end points of the box.
            startX = (int) (leftEye.x - xRadius);
            endX = (int) (rightEye.x + xRadius);
            startY = (int) (Math.Min(leftEye.y, rightEye.y) - yRadius);
            endY = (int) (Math.Max(leftEye.y, rightEye.y) + yRadius);

            // If out of bounds, set to min/max.
            startX = Math.Max(0, startX);
            endX = Math.Min(resolution, endX);
            startY = Math.Max(0, startY);
            endY = Math.Min(resolution, endY);
        }

        /// <summary>
        /// Goes through pixels in a box and finds those that are in colour threshold
        /// </summary>
        /// <param name="tex">Input texture</param>
        /// <param name="offset">Offset from the original texture (because of different aspect ratio)</param>
        /// <param name="startX">Left X coordinate of box</param>
        /// <param name="startY">Upper Y coordinate of box</param>
        /// <param name="boxWidth">Width of the box</param>
        /// <param name="boxHeight">Height of the box</param>
        /// <returns>Tuple: Color array for filling the texture and the number of pixels found in threshold</returns>
        private Tuple<Color32[], int> FindPixels(WebCamTexture tex, int offset, int startX, int startY, int boxWidth, int boxHeight)
        {
            // Get pixels in range <(startX, startY), (endX, endY)>.
            var textureColours = tex.GetPixels(startX+offset, startY, boxWidth, boxHeight);
            var textureColoursNative = new NativeArray<Color>(textureColours.Length, Allocator.TempJob);
            textureColoursNative.CopyFrom(textureColours);
            
            // There's no built-in native counter, this utility is by Marnielle Lloyd Estrada.
            var foundPixelsCounter = new NativeCounter(Allocator.TempJob);
            
            // Create a job that goes through all pixels in the box.
            var job = new ColourCheckJob
            {
                textureColours = textureColoursNative,
                foundPixelsArr = new NativeArray<Color32>(textureColours.Length, Allocator.TempJob),
                counter = foundPixelsCounter,
                hueThresh = MyPrefs.HueThreshold,
                targetHue = MyPrefs.Hue
            };
            var jobHandle = job.Schedule(textureColours.Length, 250);
            jobHandle.Complete();

            // Get outputs from job.
            var foundPixelsCount = foundPixelsCounter.Count;
            Color32[] foundPixelsArr = job.foundPixelsArr.ToArray();
            
            // Dispose of NativeArrays.
            job.textureColours.Dispose();
            job.foundPixelsArr.Dispose();
            foundPixelsCounter.Dispose();

            return Tuple.Create(foundPixelsArr, foundPixelsCount);
        }
        #endregion
    }

    [BurstCompile]
    public struct ColourCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color> textureColours;
        [WriteOnly] public NativeArray<Color32> foundPixelsArr;
        public NativeCounter.ParallelWriter counter;
        [ReadOnly] public int hueThresh;
        [ReadOnly] public int targetHue;
        [ReadOnly] private static readonly Color32 Colour = Color.white;

        public void Execute(int index)
        {
            var pixel = textureColours[index];
            if (PixelInThreshold(pixel, hueThresh, targetHue))
            {
                foundPixelsArr[index] = Colour;
                counter.Increment();
            }
        }

        private static bool PixelInThreshold(Color pixel, int thresh, int targetHue)
        {
            // Get HSV values.
            Color.RGBToHSV(pixel, out var h, out var s, out var v);
            
            // Brighter than 20% and higher saturation than 30%, otherwise not in threshold.
            if (v < 0.2 || s < 0.3)
                return false;
            
            // Map from 0.0 - 1.0 to 0-360.
            var hue = (int) (360 * h);
            
            // Get difference in 360 degrees.
            var diff = hue > targetHue ? hue - targetHue : targetHue - hue;
            var hueDifference = Math.Min(diff, 360 - diff);
            return hueDifference < thresh;
        }
    }
    
}