using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using VirtualShowcase.Enums;
using VirtualShowcase.MainScene;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Core
{
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Scene objects")]
        [SerializeField] private ShowcaseInitializer showcaseInitializer;
        [SerializeField] private CalibrationManager calibrationManager;

        [Header("Rotation images")]
        [SerializeField] private Image rotationImage;
        [SerializeField] private Sprite[] rotationImages;

        #endregion

        private InputActions _inputActions;
        private eRotationImage _currentRotationImage;
        private KeyControl[] _rotationKeys;
        
        #region Event Functions

        private void Awake()
        {
            _inputActions = new InputActions();
            _rotationKeys = new[] {Keyboard.current.xKey, Keyboard.current.yKey, Keyboard.current.zKey};
        }

        private void OnEnable()
        {
            _inputActions.Enable();

            _inputActions.Model.Get().SetEnabled(!SceneSwitcher.Instance.InMenu);
            _inputActions.Calibration.Get().SetEnabled(calibrationManager.Enabled);
            _inputActions.MainGeneral.Get().SetEnabled(true);

            SubscribeToInputActions();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
            _inputActions.Dispose();
        }

        private void SubscribeToInputActions()
        {
            // Enable/disable action maps depending on the state.
            SceneSwitcher.Instance.OnMenuOpened.AddListener(() => _inputActions.Model.Disable());
            SceneSwitcher.Instance.OnMainOpened.AddListener(() => _inputActions.Model.Enable());
            MyPrefs.PreviewEnabled.AddListener(() =>
            {
                _inputActions.Calibration.Disable();
                _inputActions.Model.Disable();
                _inputActions.MainGeneral.Calibrationtoggle.SetEnabled(false);
            });
            MyPrefs.PreviewDisabled.AddListener(() =>
            {
                _inputActions.Calibration.Enable();
                _inputActions.Model.Enable();
                _inputActions.MainGeneral.Calibrationtoggle.SetEnabled(true);
            });
            calibrationManager.StateChanged += (sender, state) =>
            {
                _inputActions.Calibration.Get().SetEnabled(state != eCalibrationState.Off);
                _inputActions.Model.Get().SetEnabled(state == eCalibrationState.Off);
                _inputActions.MainGeneral.Previewtoggle.SetEnabled(state == eCalibrationState.Off);
            };

            // Subscribe to input actions.
            _inputActions.MainGeneral.Mainsceneswitch.performed += _ => SceneSwitcher.Instance.SwitchDifferentMain();
            _inputActions.MainGeneral.Previewtoggle.performed += _ =>
            {
                MyPrefs.PreviewOn = !MyPrefs.PreviewOn;
                showcaseInitializer.SetCameraPreviewEnabled(MyPrefs.PreviewOn);
            };
            _inputActions.MainGeneral.Stereotoggle.performed += _ =>
            {
                MyPrefs.StereoOn = !MyPrefs.StereoOn;
                showcaseInitializer.SetStereo(MyPrefs.StereoOn);
            };
            _inputActions.MainGeneral.Calibrationtoggle.performed += _ => calibrationManager.ToggleCalibrationUI();
            _inputActions.MainGeneral.Menutoggle.performed += _ => SceneSwitcher.Instance.ToggleMenu();

            _inputActions.Calibration.Nextcalibration.performed += _ => calibrationManager.SetNextState();
            _inputActions.Calibration.Topedge.performed += _ => calibrationManager.SetState(eCalibrationState.Top);
            _inputActions.Calibration.Bottomedge.performed += _ => calibrationManager.SetState(eCalibrationState.Bottom);
            _inputActions.Calibration.Leftedge.performed += _ => calibrationManager.SetState(eCalibrationState.Left);
            _inputActions.Calibration.Rightedge.performed += _ => calibrationManager.SetState(eCalibrationState.Right);

            _inputActions.Model.Resettransformreal.performed += _ => ModelLoader.Instance.ResetTransform(showRealSize: true);
            _inputActions.Model.Resettransform.performed += _ => ModelLoader.Instance.ResetTransform(showRealSize: false);
            _inputActions.Model.Nextmodel.performed += _ => ModelLoader.Instance.SwitchActiveModel();
            _inputActions.Model.Previousmodel.performed += _ => ModelLoader.Instance.SwitchActiveModel(false);

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
            _inputActions.Model.RotateY.performed += ctx =>
            {
                var delta = ctx.ReadValue<float>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(0, -delta, 0, Space.World));
            };
            _inputActions.Model.RotateZ.performed += ctx =>
            {
                var delta = ctx.ReadValue<float>();
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(0, 0, -delta, Space.World));
            };
        }
        
        private void Update()
        {
            ShowRotationImage();
        }

        private void ShowRotationImage()
        {
            // I want to show the image even if only the modifier is pressed,
            // and unfortunately the input system only sends an event when the mouse is moving.
            KeyControl pressedKey = _rotationKeys.FirstOrDefault(x => x.IsPressed());
            rotationImage.gameObject.SetActive(pressedKey != null);

            if (pressedKey == null)
                return;

            var rotation = (eRotationImage)Enum.Parse(typeof(eRotationImage), pressedKey.name.ToUpper());
            if (rotation != _currentRotationImage)
            {
                _currentRotationImage = rotation;
                rotationImage.sprite = rotationImages[(int) rotation];
            }
        }

        #endregion

        public void ResetAllBindings()
        {
            _inputActions.Model.Get().RemoveAllBindingOverrides();
            MyPrefs.Rebinds = null;
        }

        private enum eRotationImage
        {
            X = 0,
            Y = 1,
            Z = 2,
        }
    }
}