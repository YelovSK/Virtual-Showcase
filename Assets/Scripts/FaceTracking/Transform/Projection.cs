using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualVitrine.FaceTracking.Transform
{
    [ExecuteInEditMode]
    public sealed class Projection : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private bool drawGizmos;
        [SerializeField] private Camera[] cameras;
        [SerializeField] private GameObject scene;
        #endregion

        #region Public Fields
        public int ScreenDistance
        {
            get => MyPrefs.ScreenDistance;
            set
            {
                MyPrefs.ScreenDistance = value;
                // Set the head distance.
                transform.localPosition = new Vector3(0, value, 0);
                // Set the camera's near according to the distance
                // because Unity's fog is affected by the camera's near.
                foreach (var cam in cameras)
                    cam.nearClipPlane = value - 29f;
            }
        }

        public int ScreenSize
        {
            get => MyPrefs.ScreenSize;
            set
            {
                MyPrefs.ScreenSize = value;
                // Set the new size of scene.
                scene.transform.localScale = Vector3.one * (ScreenWidth / BaseScreenWidth);
            }
        }

        public float ScreenWidth => DiagonalToWidthAndHeight(ScreenSize, AspectRatio).x;
        public float ScreenHeight => DiagonalToWidthAndHeight(ScreenSize, AspectRatio).y;
        #endregion

        #region Private Fields
        private static float BaseScreenWidth => DiagonalToWidthAndHeight(BaseScreenDiagonal, AspectRatio).x;
        private const int BaseScreenDiagonal = 24;
        private const float AspectRatio = 16 / 9f;
        private GameObject _virtualWindow;
        private IEnumerable<Camera> ActiveCameras => cameras.Where(x => x.isActiveAndEnabled);
        #endregion

        private void Awake()
        {
            _virtualWindow = transform.parent.gameObject;
        }

        /// <summary>
        /// This update runs only in the editor so that the frustum can be updated in real time
        /// </summary>
        private void Update()
        {
            if (Application.isPlaying)
                return;

            ScreenSize = BaseScreenDiagonal;
            UpdateCameraProjection();
        }

        /// <summary>
        ///     Returns width and height of display in centimeters from diagonal inches.
        /// </summary>
        /// <param name="diagonalInches"></param>
        /// <param name="aspectRatio"></param>
        /// <returns></returns>
        public static Vector2 DiagonalToWidthAndHeight(int diagonalInches, float aspectRatio)
        {
            const float cmsInInch = 2.54f;
            var height = diagonalInches / Math.Sqrt(aspectRatio * aspectRatio + 1);
            var width = aspectRatio * height;
            return new Vector2((float)(width * cmsInInch), (float)(height * cmsInInch));
        }

        public void UpdateCameraProjection()
        {
            foreach (var cam in ActiveCameras)
                SetCameraFrustum(cam);
        }

        private void SetCameraFrustum(Camera cam)
        {
            // Cache variables.
            var width = ScreenWidth;
            var height = ScreenHeight;
            var screen = _virtualWindow.transform;

            // Head (camera) position relative to the screen.
            var headPos = screen.InverseTransformPoint(cam.transform.position);

            // Swap X, Y coordinates, so that Z describes the depth and Y describes the height.
            (headPos.y, headPos.z) = (headPos.z, headPos.y);

            // Coordinates of the frustum's sides at screen distance.
            var screenLeft = -(width / 2) - headPos.x;
            var screenRight = (width / 2) - headPos.x;
            var screenBottom = -(height / 2) - headPos.y;
            var screenTop = (height / 2) - headPos.y;

            // Ratio for intercept theorem: zNear / screenDistance.
            var ratio = cam.nearClipPlane / headPos.z;

            // Coordinates of the frustum's sides at near clip distance (using intercept theorem).
            var left = screenLeft * ratio;
            var right = screenRight * ratio;
            var bottom = screenBottom * ratio;
            var top = screenTop * ratio;

            // Load the perpendicular projection.
            cam.projectionMatrix = Matrix4x4.Frustum(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
        }

        #region Gizmos drawing
        /// <summary>
        /// Draws gizmos in the Edit window.
        /// </summary>
        public void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            // Draw lines to show the screen.
            DrawScreen();

            // Draw lines from cameras to screen corners.
            foreach (var cam in ActiveCameras)
                DrawCameraGizmos(cam);
        }

        private void DrawScreen()
        {
            var window = _virtualWindow.transform;
            Gizmos.color = Color.magenta;
            Gizmos.DrawIcon(transform.position, "head.png");
            // Gizmos.DrawSphere(transform.position, 5f);

            // Draw a line towards camera.
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                window.position,
                window.position + window.up
            );

            // Draw vertical line.
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                window.position - _virtualWindow.transform.forward * 0.5f * ScreenHeight,
                window.position + window.forward * 0.5f * ScreenHeight
            );

            // Draw horizontal line.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                window.position - _virtualWindow.transform.right * 0.5f * ScreenWidth,
                window.position + window.right * 0.5f * ScreenWidth
            );

            GetScreenCorners(out var leftBottom, out var leftTop, out var rightBottom, out var rightTop);

            // Draw border.
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftBottom, leftTop);
            Gizmos.DrawLine(leftTop, rightTop);
            Gizmos.DrawLine(rightTop, rightBottom);
            Gizmos.DrawLine(rightBottom, leftBottom);
            Gizmos.color = Color.grey;
        }

        private void DrawCameraGizmos(Camera cam)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(cam.transform.position, 1f);

            GetScreenCorners(out var leftBottom, out var leftTop, out var rightBottom, out var rightTop);

            // Draw lines from camera to corners.
            var pos = cam.transform.position;
            Gizmos.DrawLine(pos, leftTop);
            Gizmos.DrawLine(pos, rightTop);
            Gizmos.DrawLine(pos, rightBottom);
            Gizmos.DrawLine(pos, leftBottom);
        }

        private void GetScreenCorners(out Vector3 leftBottom, out Vector3 leftTop, out Vector3 rightBottom, out Vector3 rightTop)
        {
            var screen = _virtualWindow.transform;
            var width = ScreenWidth;
            var height = ScreenHeight;

            leftBottom = screen.position - screen.right * 0.5f * width - screen.forward * 0.5f * height;
            leftTop = screen.position - screen.right * 0.5f * width + screen.forward * 0.5f * height;
            rightBottom = screen.position + screen.right * 0.5f * width - screen.forward * 0.5f * height;
            rightTop = screen.position + screen.right * 0.5f * width + screen.forward * 0.5f * height;
        }
        #endregion
    }
}