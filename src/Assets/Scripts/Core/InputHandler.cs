using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Enums;
using VirtualShowcase.Showcase;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Core
{
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Scene objects")]
        [SerializeField]
        private CameraPreview cameraPreview;

        [SerializeField]
        private HeadCameras cameras;

        [SerializeField]
        private CalibrationController calibrationController;

        [SerializeField]
        private Canvas canvas;

        [Header("Rotation images")]
        [SerializeField]
        private Image rotationImage;

        [SerializeField]
        private Sprite[] rotationImages;

        #endregion

        private RotationImage _currentRotationImage;

        private InputActions _inputActions;

        #region Event Functions

        private void Awake()
        {
            _inputActions = new InputActions();
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _inputActions.Model.Get().SetEnabled(!MySceneManager.Instance.IsInMenu);
            _inputActions.Calibration.Get().SetEnabled(calibrationController.Enabled);
            _inputActions.MainGeneral.Get().SetEnabled(true);

            SubscribeToInputActions();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
            _inputActions.Dispose();
        }

        #endregion

        private void SubscribeToInputActions()
        {
            // Enable/disable action maps depending on the state.
            MyEvents.MenuSceneOpened.AddListener(_ => _inputActions.Model.Disable());
            MyEvents.MainSceneOpened.AddListener(_ => _inputActions.Model.Enable());
            MyEvents.CameraPreviewChanged.AddListener((sender, isEnabled) =>
            {
                _inputActions.Calibration.Get().SetEnabled(!isEnabled);
                _inputActions.Model.Get().SetEnabled(!isEnabled);
                _inputActions.MainGeneral.Calibrationtoggle.SetEnabled(!isEnabled);
                _inputActions.MainGeneral.Nextscene.SetEnabled(!isEnabled);
                _inputActions.MainGeneral.Previousscene.SetEnabled(!isEnabled);
            });
            MyEvents.CalibrationChanged.AddListener((sender, isEnabled) =>
            {
                _inputActions.Calibration.Get().SetEnabled(isEnabled);
                _inputActions.Model.Get().SetEnabled(!isEnabled);
                _inputActions.MainGeneral.Previewtoggle.SetEnabled(!isEnabled);
                _inputActions.MainGeneral.Nextscene.SetEnabled(!isEnabled);
                _inputActions.MainGeneral.Previousscene.SetEnabled(!isEnabled);
            });

            // Subscribe to input actions.
            _inputActions.MainGeneral.Nextscene.performed += _ => MySceneManager.Instance.LoadNextShowcaseScene();
            _inputActions.MainGeneral.Previousscene.performed += _ => MySceneManager.Instance.LoadPreviousShowcaseScene();
            _inputActions.MainGeneral.Previewtoggle.performed += _ =>
            {
                MyPrefs.PreviewOn = !MyPrefs.PreviewOn;
                if (MyPrefs.PreviewOn)
                {
                    canvas.gameObject.SetActive(true);
                    cameraPreview.ShowLargePreview();
                }
                else
                {
                    canvas.gameObject.SetActive(false);
                    cameraPreview.Disable();
                }
            };
            _inputActions.MainGeneral.Stereotoggle.performed += _ =>
            {
                MyPrefs.StereoOn = !MyPrefs.StereoOn;
                if (MyPrefs.StereoOn)
                {
                    cameras.EnableStereo();
                }
                else
                {
                    cameras.DisableStereo();
                }
            };
            _inputActions.MainGeneral.Calibrationtoggle.performed += _ =>
            {
                canvas.gameObject.SetActive(!canvas.gameObject.activeSelf);
                calibrationController.gameObject.SetActive(!calibrationController.gameObject.activeSelf);
                calibrationController.ToggleCalibrationUI();
            };
            _inputActions.MainGeneral.Menutoggle.performed += _ => MySceneManager.Instance.ToggleMenu();

            _inputActions.Calibration.Nextcalibration.performed += _ => calibrationController.SetNextState();
            _inputActions.Calibration.Topedge.performed += _ => calibrationController.SetState(CalibrationState.Top);
            _inputActions.Calibration.Bottomedge.performed += _ => calibrationController.SetState(CalibrationState.Bottom);
            _inputActions.Calibration.Leftedge.performed += _ => calibrationController.SetState(CalibrationState.Left);
            _inputActions.Calibration.Rightedge.performed += _ => calibrationController.SetState(CalibrationState.Right);

            _inputActions.Model.Resettransform.performed += _ => ModelLoader.Instance.ResetTransform();
            _inputActions.Model.Nextmodel.performed += _ => ModelLoader.Instance.CycleActiveModel();
            _inputActions.Model.Previousmodel.performed += _ => ModelLoader.Instance.CycleActiveModel(false);

            _inputActions.Model.MoveXY.performed += ctx =>
            {
                var delta = ctx.ReadValue<Vector2>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Translate(delta.x, delta.y, 0, Space.World));
            };
            _inputActions.Model.MoveYZ.performed += ctx =>
            {
                var delta = ctx.ReadValue<Vector2>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Translate(delta.x, 0, delta.y, Space.World));
            };
            _inputActions.Model.Scale.performed += ctx =>
            {
                // Wouldn't be "real" size if scaled (:
                if (MyPrefs.ShowRealModelSize)
                {
                    return;
                }

                var delta = ctx.ReadValue<float>();
                float scale = delta > 0
                    ? 1.1f
                    : 0.9f;
                ModelLoader.Instance.Models.ForEach(model => model.transform.localScale *= scale);
            };
            _inputActions.Model.RotateX.performed += ctx =>
            {
                var delta = ctx.ReadValue<float>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(delta, 0, 0, Space.World));
            };
            _inputActions.Model.RotateXmodifier.performed += ctx =>
            {
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[(int)RotationImage.X];
            };
            _inputActions.Model.RotateXmodifier.canceled += ctx => { rotationImage.gameObject.SetActive(false); };
            _inputActions.Model.RotateY.performed += ctx =>
            {
                var delta = ctx.ReadValue<float>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(0, -delta, 0, Space.World));
            };
            _inputActions.Model.RotateYmodifier.performed += ctx =>
            {
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[(int)RotationImage.Y];
            };
            _inputActions.Model.RotateYmodifier.canceled += ctx => { rotationImage.gameObject.SetActive(false); };
            _inputActions.Model.RotateZ.performed += ctx =>
            {
                var delta = ctx.ReadValue<float>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(0, 0, -delta, Space.World));
            };
            _inputActions.Model.RotateZmodifier.performed += ctx =>
            {
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[(int)RotationImage.Z];
            };
            _inputActions.Model.RotateZmodifier.canceled += ctx => { rotationImage.gameObject.SetActive(false); };
        }

        private enum RotationImage
        {
            X = 0,
            Y = 1,
            Z = 2,
        }
    }
}