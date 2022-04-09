// source: https://github.com/Emerix/AsymFrustum
/// <summary>
/// Asym frustum.
/// based on http://paulbourke.net/stereographics/stereorender/
/// and http://answers.unity3d.com/questions/165443/asymmetric-view-frusta-selective-region-rendering.html
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace VirtualVitrine.FaceTracking.Transform
{
    [ExecuteInEditMode]
    public sealed class AsymFrustum : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private bool drawGizmos;
        [SerializeField] private Camera[] cameras;
        [SerializeField] private GameObject scene;
        #endregion
        
        #region Public Fields
        public int ScreenDistance
        {
            set
            {
                MyPrefs.ScreenDistance = value;
                // Set the head distance.
                transform.localPosition = new Vector3(0, value, 0);
                // Set the camera's near according to the distance
                // because Unity's fog is affected by the camera's near.
                foreach (var cam in cameras)
                    cam.nearClipPlane = value - 15f;
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
        private const float AspectRatio = 16/9f;
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
            UpdateProjectionMatrix();
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
            return new Vector2((float) (width * cmsInInch), (float) (height * cmsInInch));
        }

        public void UpdateProjectionMatrix()
        {
            foreach (var cam in ActiveCameras)
            {
                var localPos = _virtualWindow.transform.InverseTransformPoint(cam.transform.position);
                SetAsymmetricFrustum(cam, localPos, cam.nearClipPlane);
            }
        }

        /// <summary>
        ///     Sets the asymmetric Frustum for the given virtual Window (at pos 0,0,0 )
        ///     and the camera passed
        /// </summary>
        /// <param name="cam">Camera to get the asymmetric frustum for</param>
        /// <param name="pos">Position of the camera. Usually cam.transform.position</param>
        /// <param name="nearDist">Near clipping plane, usually cam.nearClipPlane</param>
        private void SetAsymmetricFrustum(Camera cam, Vector3 pos, float nearDist)
        {
            // swap y and z, since z makes more sense for describing depth/distance
            pos = new Vector3(pos.x, pos.z, pos.y);

            // Focal length = orthogonal distance to image plane
            var focal = pos.z;

            // Ratio for intercept theorem
            var ratio = focal / nearDist;

            // Compute size for focal
            var imageLeft = -ScreenWidth / 2.0f - pos.x;
            var imageRight = ScreenWidth / 2.0f - pos.x;
            var imageTop = ScreenHeight / 2.0f - pos.y;
            var imageBottom = -ScreenHeight / 2.0f - pos.y;

            // Intercept theorem for getting x, y coordinates of near plane corners
            var nearLeft = imageLeft / ratio;
            var nearRight = imageRight / ratio;
            var nearTop = imageTop / ratio;
            var nearBottom = imageBottom / ratio;

            // update camera's projection matrix
            cam.projectionMatrix = PerspectiveOffCenter(nearLeft, nearRight, nearBottom, nearTop, cam.nearClipPlane, cam.farClipPlane);
        }


        /// <summary>
        ///     Set an off-center projection, where perspective's vanishing
        ///     point is not necessarily in the center of the screen.
        ///     left/right/top/bottom define near plane size, i.e.
        ///     how offset are corners of camera's near plane.
        ///     Tweak the values and you can see camera's frustum change.
        /// </summary>
        /// <returns>The off center.</returns>
        /// <param name="left">Left.</param>
        /// <param name="right">Right.</param>
        /// <param name="bottom">Bottom.</param>
        /// <param name="top">Top.</param>
        /// <param name="near">Near.</param>
        /// <param name="far">Far.</param>
        private static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
        {
            var nearWidth = right - left;
            var nearHeight = top - bottom;
            var frustumDepth = far - near;
            
            var x = (2 * near) / nearWidth;
            var y = (2 * near) / nearHeight;
            var a = (left + right) / nearWidth;
            var b = (bottom + top) / nearHeight;
            var c = -(far + near) / frustumDepth;
            var d = -(2 * far * near) / frustumDepth;

            var m = new Matrix4x4
            {
                [0, 0] = x, [0, 1] = 0, [0, 2] = a, [0, 3] = 0,
                [1, 0] = 0, [1, 1] = y, [1, 2] = b, [1, 3] = 0,
                [2, 0] = 0, [2, 1] = 0, [2, 2] = c, [2, 3] = d,
                [3, 0] = 0, [3, 1] = 0, [3, 2] = -1, [3, 3] = 0
            };
            return m;
        }
        
        /// <summary>
        /// Draws gizmos in the Edit window.
        /// </summary>
        public void OnDrawGizmos()
        {
            if (!drawGizmos)
                return;
            
            // draw lines to show the screen
            DrawScreen();
            
            // draw lines from cameras to screen corners
            foreach (var cam in ActiveCameras)
                DrawCameraGizmos(cam);
        }

        private void DrawScreen()
        {
            var window = _virtualWindow.transform;
            Gizmos.color = Color.magenta;
            Gizmos.DrawIcon(transform.position, "head.png");
            // Gizmos.DrawSphere(transform.position, 5f);
            
            // draw line towards camera
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                window.position,
                window.position + window.up
            );

            // draw vertical line
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(
                window.position - _virtualWindow.transform.forward * 0.5f * ScreenHeight,
                window.position + window.forward * 0.5f * ScreenHeight
            );

            // draw horizontal line
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                window.position - _virtualWindow.transform.right * 0.5f * ScreenWidth,
                window.position + window.right * 0.5f * ScreenWidth
            );
            
            GetScreenCorners(out var leftBottom, out var leftTop, out var rightBottom, out var rightTop);

            // draw border
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

            // draw lines from camera to corners
            var pos = cam.transform.position;
            Gizmos.DrawLine(pos, leftTop);
            Gizmos.DrawLine(pos, rightTop);
            Gizmos.DrawLine(pos, rightBottom);
            Gizmos.DrawLine(pos, leftBottom);
        }
        
        private void GetScreenCorners(out Vector3 leftBottom, out Vector3 leftTop, out Vector3 rightBottom, out Vector3 rightTop)
        {
            var window = _virtualWindow.transform;

            leftBottom = window.position - window.right * 0.5f * ScreenWidth - window.forward * 0.5f * ScreenHeight;
            leftTop = window.position - window.right * 0.5f * ScreenWidth + window.forward * 0.5f * ScreenHeight;
            rightBottom = window.position + window.right * 0.5f * ScreenWidth - window.forward * 0.5f * ScreenHeight;
            rightTop = window.position + window.right * 0.5f * ScreenWidth + window.forward * 0.5f * ScreenHeight;
        }
    }
}