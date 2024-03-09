using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.FaceTracking.Transform
{
    [ExecuteInEditMode]
    public sealed class Projection : MonoBehaviour
    {
        private const int base_screen_diagonal = 24;
        private const float aspect_ratio = 16 / 9f;

        #region Serialized Fields

        [SerializeField] private bool drawGizmos;
        [SerializeField] private Camera[] cameras;
        [SerializeField] private GameObject virtualWindow;

        #endregion

        public int ScreenDistance
        {
            get => MyPrefs.ScreenDistance;
            set
            {
                MyPrefs.ScreenDistance = value;
                SetCameraDistance();
            }
        }

        public int ScreenSize
        {
            get => MyPrefs.ScreenSize;
            set
            {
                MyPrefs.ScreenSize = value;
                SetCameraDistance();
            }
        }

        public static float ScreenWidth => DiagonalToWidthAndHeight(base_screen_diagonal, aspect_ratio).x;
        public static float ScreenHeight => DiagonalToWidthAndHeight(base_screen_diagonal, aspect_ratio).y;
        private IEnumerable<Camera> ActiveCameras => cameras.Where(x => x.isActiveAndEnabled);

        #region Event Functions

        private void Start()
        {
            SetCameraDistance();
        }

        /// <summary>
        ///     This update runs only in the editor so that the frustum can be updated in real time
        /// </summary>
        private void Update()
        {
            if (!Application.isPlaying)
                UpdateCameraProjection();
        }


        /// <summary>
        ///     Draws gizmos in the Edit window.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;

            // Draw lines to show the screen.
            DrawScreen();

            // Draw lines from cameras to screen corners.
            foreach (Camera cam in ActiveCameras)
            {
                DrawCameraGizmos(cam);
            }
        }

        #endregion

        /// <summary>
        ///     Returns width and height of display in centimeters from diagonal inches.
        /// </summary>
        /// <param name="diagonalInches"></param>
        /// <param name="aspectRatio"></param>
        /// <returns></returns>
        public static Vector2 DiagonalToWidthAndHeight(int diagonalInches, float aspectRatio)
        {
            const float cms_in_inch = 2.54f;
            double height = diagonalInches / Math.Sqrt(aspectRatio * aspectRatio + 1);
            double width = aspectRatio * height;
            return new Vector2((float) (width * cms_in_inch), (float) (height * cms_in_inch));
        }

        public void SetCameraDistance()
        {
            // Instead of setting the camera distance to the exact value set in the prefs
            // and scaling the scene according to the screen size, the screen size stays
            // the same and only the head distance changes. The field of view is the same
            // as if the scene/screen got scaled to the new size.
            float sizeRatio = (float) base_screen_diagonal / ScreenSize;
            float headDistance = ScreenDistance * sizeRatio;
            transform.localPosition = new Vector3(0, 0, -headDistance);

            // Likewise, eye separation needs to be adjusted with the same ratio.
            cameras[1].transform.localPosition = new Vector3(3 * sizeRatio, 0, 0); // right eye
            cameras[2].transform.localPosition = new Vector3(-3 * sizeRatio, 0, 0); // left eye

            // Set the camera's near according to the distance
            // because Unity's fog is affected by the camera's near.
            foreach (Camera cam in cameras)
            {
                cam.nearClipPlane = Math.Max(headDistance - 30f, 0.1f);
                cam.farClipPlane = headDistance + 200;
            }

            // Update camera view to avoid flicker while using the sliders.
            GetComponent<CameraTransform>().Transform();
        }

        public void UpdateCameraProjection()
        {
            foreach (Camera cam in cameras)
            {
                SetCameraFrustum(cam);
            }
        }

        private void SetCameraFrustum(Camera cam)
        {
            // Cache variables.
            float width = ScreenWidth;
            float height = ScreenHeight;

            // Screen position relative to the head (camera).
            Vector3 screenPos = cam.transform.InverseTransformPoint(virtualWindow.transform.position);

            // Coordinates of the frustum's sides at screen distance.
            float screenLeftX = screenPos.x - width / 2;
            float screenRightX = screenPos.x + width / 2;
            float screenBottomY = screenPos.y - height / 2;
            float screenTopY = screenPos.y + height / 2;

            // Ratio for intercept theorem: zNear / screenDistance.
            float ratio = cam.nearClipPlane / screenPos.z;

            // Coordinates of the frustum's sides at near clip distance (using intercept theorem).
            float leftX = screenLeftX * ratio;
            float rightX = screenRightX * ratio;
            float bottomY = screenBottomY * ratio;
            float topY = screenTopY * ratio;

            // Load the perpendicular projection.
            cam.projectionMatrix = Matrix4x4.Frustum(leftX, rightX, bottomY, topY, cam.nearClipPlane, cam.farClipPlane);
        }

        private void DrawScreen()
        {
            UnityEngine.Transform window = virtualWindow.transform;
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
                window.position - virtualWindow.transform.up * 0.5f * ScreenHeight,
                window.position + window.up * 0.5f * ScreenHeight
            );

            // Draw horizontal line.
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                window.position - virtualWindow.transform.right * 0.5f * ScreenWidth,
                window.position + window.right * 0.5f * ScreenWidth
            );

            GetScreenCorners(out Vector3 leftBottom, out Vector3 leftTop, out Vector3 rightBottom, out Vector3 rightTop);

            // Draw border.
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftBottom, leftTop);
            Gizmos.DrawLine(leftTop, rightTop);
            Gizmos.DrawLine(rightTop, rightBottom);
            Gizmos.DrawLine(rightBottom, leftBottom);
            Gizmos.color = Color.grey;
        }

        private void DrawCameraGizmos(Component cam)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(cam.transform.position, 1f);

            GetScreenCorners(out Vector3 leftBottom, out Vector3 leftTop, out Vector3 rightBottom, out Vector3 rightTop);

            // Draw lines from camera to corners.
            Vector3 pos = cam.transform.position;
            Gizmos.DrawLine(pos, leftTop);
            Gizmos.DrawLine(pos, rightTop);
            Gizmos.DrawLine(pos, rightBottom);
            Gizmos.DrawLine(pos, leftBottom);
        }

        private void GetScreenCorners(out Vector3 leftBottom, out Vector3 leftTop, out Vector3 rightBottom, out Vector3 rightTop)
        {
            UnityEngine.Transform screen = virtualWindow.transform;
            float width = ScreenWidth;
            float height = ScreenHeight;

            leftBottom = screen.position - screen.right * 0.5f * width - screen.up * 0.5f * height;
            leftTop = screen.position - screen.right * 0.5f * width + screen.up * 0.5f * height;
            rightBottom = screen.position + screen.right * 0.5f * width - screen.up * 0.5f * height;
            rightTop = screen.position + screen.right * 0.5f * width + screen.up * 0.5f * height;
        }
    }
}