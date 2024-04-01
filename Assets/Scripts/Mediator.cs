using UnityEngine;
using VirtualShowcase.Core;
using VirtualShowcase.FaceTracking;
using VirtualShowcase.FaceTracking.GlassesCheck;
using VirtualShowcase.FaceTracking.Transform;
using VirtualShowcase.Utilities;

namespace VirtualShowcase
{
    public sealed class Mediator : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private CameraTransform cameraTransform;

        #endregion

        private ColourChecker _colorChecker;
        private Detector _detector;
        private EyeTracker _eyeTracker;

        #region Event Functions

        private void Awake()
        {
            MyEvents.CameraUpdated.AddListener(_ => HandleCameraUpdate());

            _colorChecker = GetComponent<ColourChecker>();
            _eyeTracker = GetComponent<EyeTracker>();
            _detector = GetComponent<Detector>();
        }

        private void Start()
        {
            WebcamInput.Instance.ChangeWebcam(MyPrefs.CameraName);
        }

        #endregion

        private void HandleCameraUpdate()
        {
            bool faceFound = _detector.RunDetector(WebcamInput.Instance.Texture);

            _eyeTracker.SmoothEyes();

            MyEvents.FaceDetectionDone.Invoke(gameObject, faceFound);

            if (!faceFound)
            {
                return;
            }

            bool glassesOn = !MyPrefs.GlassesCheck || _colorChecker.CheckGlassesOn(WebcamInput.Instance.Texture);

            if (glassesOn && MySceneManager.Instance.IsInMainScene)
            {
                cameraTransform.Transform();
            }
        }
    }
}