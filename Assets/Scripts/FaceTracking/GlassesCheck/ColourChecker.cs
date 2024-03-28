using System;
using TMPro;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking.GlassesCheck
{
    public class ColourChecker : MonoBehaviour
    {
        private Texture2D colorOverlayTexture;

        #region Event Functions

        private void Start()
        {
            colorOverlayTexture = new Texture2D(1, 1);
        }

        #endregion

        /// <summary>
        ///     Checks pixels in the color box and updates the text to show the number of pixels
        /// </summary>
        /// <returns>True if threshold passed</returns>
        public bool CheckGlassesOn(Texture2D texture, RawImage colorOverlay, TMP_Text pixelCountText)
        {
            colorOverlay.gameObject.SetActive(true);
            var resolution = new Vector2(texture.width, texture.height);

            // Map coords from 0.0 - 1.0 to width and height of WebcamTexture.
            var leftEye = new Vector2(
                EyeTracker.LeftEyeSmoothed.x * resolution.x,
                EyeTracker.LeftEyeSmoothed.y * resolution.y);
            var rightEye = new Vector2(
                EyeTracker.RightEyeSmoothed.x * resolution.x,
                EyeTracker.RightEyeSmoothed.y * resolution.y);
            Vector2 center = (leftEye + rightEye) / 2;

            // Look from (startX, startY) to (endX, endY).
            CalculateColourBoxSize(resolution, leftEye, rightEye, out int startX, out int endX, out int startY, out int endY);
            int boxWidth = endX - startX;
            int boxHeight = endY - startY;
            int allPixelsCount = boxWidth * boxHeight;

            // Look at the pixels in the box.
            (Color32[] foundPixelsArr, int foundPixelsCount) = FindPixels(texture, startX, startY, boxWidth, boxHeight);

            // Check if threshold was passed.
            const float threshold = 0.05f;
            bool passed = (float) foundPixelsCount / allPixelsCount > threshold;

            // Don't compute overlay if preview is not showing in main scene.
            if (MyPrefs.PreviewOn == false && MySceneManager.Instance.IsInMainScene)
                return passed;

            // Set label text to show number of found pixels.
            pixelCountText.color = passed ? Color.green : Color.red;
            pixelCountText.text = foundPixelsCount + " / " + allPixelsCount;

            var cBox = (RectTransform) colorOverlay.transform;
            var cBoxParent = (RectTransform) cBox.parent;
            Rect cBoxParentRect = cBoxParent.rect;

            // Center mapped to 0.0-1.0.
            var centerFloat = new Vector2(center.x / resolution.x, center.y / resolution.y);
            cBox.anchoredPosition = centerFloat * cBoxParentRect.size;

            // Box size.
            float width = boxWidth / resolution.x;
            float height = boxHeight / resolution.y;
            Vector2 size = new Vector2(width, height) * cBoxParentRect.size;
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
            cBox.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

            // Resize texture.
            colorOverlayTexture.Reinitialize(boxWidth, boxHeight);

            // Set found pixels to white (from FindPixels method).
            colorOverlayTexture.SetPixels32(foundPixelsArr);

            // Apply and set the texture.
            colorOverlayTexture.Apply();
            colorOverlay.texture = colorOverlayTexture;

            return passed;
        }

        private void CalculateColourBoxSize(
            Vector2 resolution, Vector2 leftEye, Vector2 rightEye,
            out int startX, out int endX, out int startY, out int endY)
        {
            // Size of the box.
            float xRadius = (int) (rightEye.x - leftEye.x) * 0.9f;
            float yRadius = (int) Math.Abs(leftEye.y - rightEye.y) + xRadius / 2.5f;

            // Start and end points of the box.
            startX = (int) (leftEye.x - xRadius);
            endX = (int) (rightEye.x + xRadius);
            startY = (int) (Math.Min(leftEye.y, rightEye.y) - yRadius);
            endY = (int) (Math.Max(leftEye.y, rightEye.y) + yRadius);

            // If out of bounds, set to min/max.
            startX = Math.Max(0, startX);
            endX = Math.Min((int) resolution.x, endX);
            startY = Math.Max(0, startY);
            endY = Math.Min((int) resolution.y, endY);
        }

        /// <summary>
        ///     Goes through pixels in a box and finds those that are in colour threshold
        /// </summary>
        /// <param name="tex">Input texture</param>
        /// <param name="startX">Left X coordinate of box</param>
        /// <param name="startY">Upper Y coordinate of box</param>
        /// <param name="boxWidth">Width of the box</param>
        /// <param name="boxHeight">Height of the box</param>
        /// <returns>Tuple: Color array for filling the texture and the number of pixels found in threshold</returns>
        private Tuple<Color32[], int> FindPixels(Texture2D tex, int startX, int startY, int boxWidth, int boxHeight)
        {
            // Get pixels in range <(startX, startY), (endX, endY)>.
            Color[] textureColours = tex.GetPixels(startX, startY, boxWidth, boxHeight);
            var textureColoursNative = new NativeArray<Color>(textureColours.Length, Allocator.TempJob);
            textureColoursNative.CopyFrom(textureColours);

            // There's no built-in native counter, this utility is by Marnielle Lloyd Estrada.
            var foundPixelsCounter = new NativeCounter(Allocator.TempJob);

            // Create a job that goes through all pixels in the box.
            var job = new ColourCheckJob
            {
                TextureColours = textureColoursNative,
                FoundPixelsArr = new NativeArray<Color32>(textureColours.Length, Allocator.TempJob),
                Counter = foundPixelsCounter,
                HueThresh = MyPrefs.HueThreshold,
                TargetHue = MyPrefs.Hue,
            };
            JobHandle jobHandle = job.Schedule(textureColours.Length, 250);
            jobHandle.Complete();

            // Get outputs from job.
            int foundPixelsCount = foundPixelsCounter.Count;
            Color32[] foundPixelsArr = job.FoundPixelsArr.ToArray();

            // Dispose of NativeArrays.
            job.TextureColours.Dispose();
            job.FoundPixelsArr.Dispose();
            foundPixelsCounter.Dispose();

            return Tuple.Create(foundPixelsArr, foundPixelsCount);
        }
    }

    [BurstCompile]
    public struct ColourCheckJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Color> TextureColours;
        [WriteOnly] public NativeArray<Color32> FoundPixelsArr;
        public NativeCounter.ParallelWriter Counter;
        [ReadOnly] public int HueThresh;
        [ReadOnly] public int TargetHue;
        [ReadOnly] private static readonly Color32 Colour = Color.white;

        public void Execute(int index)
        {
            Color pixel = TextureColours[index];
            if (PixelInThreshold(pixel, HueThresh, TargetHue))
            {
                FoundPixelsArr[index] = Colour;
                Counter.Increment();
            }
        }

        private bool PixelInThreshold(Color pixel, int thresh, int targetHue)
        {
            // Get HSV values.
            Color.RGBToHSV(pixel, out float h, out float s, out float v);

            // Brighter than 20% and higher saturation than 30%, otherwise not in threshold.
            if (v < 0.2 || s < 0.3)
                return false;

            // Map from 0.0 - 1.0 to 0-360.
            var hue = (int) (360 * h);

            // Get difference in 360 degrees.
            int diff = hue > targetHue ? hue - targetHue : targetHue - hue;
            int hueDifference = Math.Min(diff, 360 - diff);
            return hueDifference < thresh;
        }
    }
}