// source: https://github.com/Emerix/AsymFrustum
/// <summary>
/// Asym frustum.
/// based on http://paulbourke.net/stereographics/stereorender/
/// and http://answers.unity3d.com/questions/165443/asymmetric-view-frusta-selective-region-rendering.html
/// </summary>

using UnityEngine;

namespace VirtualVitrine.FaceTracking.Transform
{
    [ExecuteInEditMode]
    public class AsymFrustum : MonoBehaviour
    {
        /// <summary>
        ///     Screen/window to virtual world width (in units. I suggest using meters)
        /// </summary>
        public float width;

        /// <summary>
        ///     Screen/window to virtual world height (in units. I suggest using meters)
        /// </summary>
        public float height;

        /// <summary>
        ///     The maximum height the camera can have (up axis in local coordinates from  the virtualWindow) (in units. I suggest
        ///     using meters)
        /// </summary>
        public float maxHeight = 2000.0f;

        public bool verbose;
        private Camera[] _cameras;
        private GameObject _virtualWindow;

        private void Awake()
        {
            _virtualWindow = transform.parent.gameObject;
            _cameras = GetComponentsInChildren<Camera>(true);
        }

        /// <summary>
        ///     Draws gizmos in the Edit window.
        /// </summary>
        public virtual void OnDrawGizmos()
        {

            foreach (var cam in _cameras)
            {
                if (!cam.isActiveAndEnabled)
                    continue;
                DrawScreen(cam);
            }
        }

        private void DrawScreen(Camera cam)
        {
            var virtWindowTransform = _virtualWindow.transform;
            
            Gizmos.DrawLine(cam.transform.position, cam.transform.position + cam.transform.up * 10);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(virtWindowTransform.position,
                virtWindowTransform.position + virtWindowTransform.up);

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(virtWindowTransform.position - _virtualWindow.transform.forward * 0.5f * height,
                virtWindowTransform.position + virtWindowTransform.forward * 0.5f * height);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(virtWindowTransform.position - _virtualWindow.transform.right * 0.5f * width,
                virtWindowTransform.position + virtWindowTransform.right * 0.5f * width);
            Gizmos.color = Color.cyan;
            var leftBottom = virtWindowTransform.position - virtWindowTransform.right * 0.5f * width -
                             virtWindowTransform.forward * 0.5f * height;
            var leftTop = virtWindowTransform.position - virtWindowTransform.right * 0.5f * width +
                          virtWindowTransform.forward * 0.5f * height;
            var rightBottom = virtWindowTransform.position + virtWindowTransform.right * 0.5f * width -
                              virtWindowTransform.forward * 0.5f * height;
            var rightTop = virtWindowTransform.position + virtWindowTransform.right * 0.5f * width +
                           virtWindowTransform.forward * 0.5f * height;

            Gizmos.DrawLine(leftBottom, leftTop);
            Gizmos.DrawLine(leftTop, rightTop);
            Gizmos.DrawLine(rightTop, rightBottom);
            Gizmos.DrawLine(rightBottom, leftBottom);
            Gizmos.color = Color.grey;
            var pos = cam.transform.position;
            Gizmos.DrawLine(pos, leftTop);
            Gizmos.DrawLine(pos, rightTop);
            Gizmos.DrawLine(pos, rightBottom);
            Gizmos.DrawLine(pos, leftBottom);
        }

        public void UpdateProjectionMatrix()
        {
            foreach (var cam in _cameras)
            {
                if (!cam.isActiveAndEnabled)
                    continue;
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
            // Focal length = orthogonal distance to image plane
            var newpos = pos;
            //newpos.Scale (new Vector3 (1, 1, 1));

            newpos = new Vector3(newpos.x, newpos.z, newpos.y);
            if (verbose) Debug.Log(newpos.x + ";" + newpos.y + ";" + newpos.z);

            float focal = Mathf.Clamp(newpos.z, 0.001f, maxHeight);

            // Ratio for intercept theorem
            var ratio = focal / nearDist;

            // Compute size for focal
            var imageLeft = -width / 2.0f - newpos.x;
            var imageRight = width / 2.0f - newpos.x;
            var imageTop = height / 2.0f - newpos.y;
            var imageBottom = -height / 2.0f - newpos.y;

            // Intercept theorem
            var nearLeft = imageLeft / ratio;
            var nearRight = imageRight / ratio;
            var nearTop = imageTop / ratio;
            var nearBottom = imageBottom / ratio;

            var m = PerspectiveOffCenter(nearLeft, nearRight, nearBottom, nearTop, cam.nearClipPlane, cam.farClipPlane);
            cam.projectionMatrix = m;
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
            var x = 2.0f * near / (right - left);
            var y = 2.0f * near / (top - bottom);
            var a = (right + left) / (right - left);
            var b = (top + bottom) / (top - bottom);
            var c = -(far + near) / (far - near);
            var d = -(2.0f * far * near) / (far - near);
            var e = -1.0f;

            var m = new Matrix4x4
            {
                [0, 0] = x,
                [0, 1] = 0,
                [0, 2] = a,
                [0, 3] = 0,
                [1, 0] = 0,
                [1, 1] = y,
                [1, 2] = b,
                [1, 3] = 0,
                [2, 0] = 0,
                [2, 1] = 0,
                [2, 2] = c,
                [2, 3] = d,
                [3, 0] = 0,
                [3, 1] = 0,
                [3, 2] = e,
                [3, 3] = 0
            };
            return m;
        }
    }
}