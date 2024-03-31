using System;
using UnityEngine;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking.GlassesCheck
{
    public class ColourChecker : MonoBehaviour
    {
        private const float MIN_COLORS_IN_RANGE_PERCENT = 0.05f;

        #region Serialized Fields

        [SerializeField]
        private ComputeShader colorCounterShader;

        #endregion

        /// <summary>
        ///     Checks pixels in the color box and updates the text to show the number of pixels
        /// </summary>
        /// <returns>True if threshold passed</returns>
        public bool CheckGlassesOn(RenderTexture texture)
        {
            var resolution = new Vector2(texture.width, texture.height);

            // Map coords from 0.0 - 1.0 to width and height of WebcamTexture.
            var leftEye = new Vector2(
                EyeTracker.LeftEyeSmoothed.x * resolution.x,
                EyeTracker.LeftEyeSmoothed.y * resolution.y);
            var rightEye = new Vector2(
                EyeTracker.RightEyeSmoothed.x * resolution.x,
                EyeTracker.RightEyeSmoothed.y * resolution.y);

            // Check only in area around detected eyes.
            Box box = GetSearchAreaBox(resolution, leftEye, rightEye);

            // Shader
            int count = CountPixelsInRange(texture, MyPrefs.Hue, MyPrefs.HueThreshold, box, Color.green);

            // Check if threshold was passed.
            bool passed = (float)count / box.Count > MIN_COLORS_IN_RANGE_PERCENT;

            return passed;
        }

        private Box GetSearchAreaBox(Vector2 resolution, Vector2 leftEye, Vector2 rightEye)
        {
            // Size of the box.
            float xRadius = (int)(rightEye.x - leftEye.x) * 0.9f;
            float yRadius = (int)Math.Abs(leftEye.y - rightEye.y) + xRadius / 2.5f;

            // Start and end points of the box.
            var startX = (int)(leftEye.x - xRadius);
            var endX = (int)(rightEye.x + xRadius);
            var startY = (int)(Math.Min(leftEye.y, rightEye.y) - yRadius);
            var endY = (int)(Math.Max(leftEye.y, rightEye.y) + yRadius);

            // If out of bounds, set to min/max.
            startX = Math.Max(0, startX);
            endX = Math.Min((int)resolution.x, endX);
            startY = Math.Max(0, startY);
            endY = Math.Min((int)resolution.y, endY);

            return new Box
            {
                StartX = startX,
                EndX = endX,
                StartY = startY,
                EndY = endY,
            };
        }

        private int CountPixelsInRange(Texture texture, int hue, int hueThreshold, Box box, Color inRangeColor)
        {
            // Shader
            var count = new int[1];
            var cBuffer = new ComputeBuffer(1, sizeof(int));

            int kernelMain = colorCounterShader.FindKernel("CSMain");
            int kernelInit = colorCounterShader.FindKernel("CSInit");

            colorCounterShader.SetBuffer(kernelInit, "ColorCounter", cBuffer);
            colorCounterShader.SetBuffer(kernelMain, "ColorCounter", cBuffer);
            colorCounterShader.SetTexture(kernelMain, "InputImage", texture);
            colorCounterShader.SetInt("Hue", hue);
            colorCounterShader.SetInt("HueThreshold", hueThreshold);
            colorCounterShader.SetInt("StartX", box.StartX);
            colorCounterShader.SetInt("EndX", box.EndX);
            colorCounterShader.SetInt("StartY", box.StartY);
            colorCounterShader.SetInt("EndY", box.EndY);
            colorCounterShader.SetVector("InRangeColor", inRangeColor);

            colorCounterShader.Dispatch(kernelInit, 1, 1, 1);
            colorCounterShader.Dispatch(kernelMain, texture.width / 8, texture.height / 8, 1);

            cBuffer.GetData(count);
            cBuffer.Release();

            return count[0];
        }

        private class Box
        {
            public int EndX;
            public int EndY;
            public int StartX;
            public int StartY;

            public int Count => (EndX - StartX) * (EndY - StartY);
        }
    }
}