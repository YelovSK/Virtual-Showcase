using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.Enums;
using VirtualShowcase.FaceTracking;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Showcase
{
    public class CalibrationController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("UI Elements")]
        [SerializeField]
        private GameObject calibrationUI;

        [SerializeField]
        private Image monitorImage;

        [SerializeField]
        private Sprite[] monitorSprites;

        [SerializeField]
        private Transform cameraPreviewTransform;

        [SerializeField]
        private TMP_Text guideText;

        [SerializeField]
        private CameraPreview cameraPreview;

        [Header("Display distance sliders")]
        [SerializeField]
        private Slider distanceSlider;

        [Header("Display size sliders")]
        [SerializeField]
        private Slider sizeSlider;

        #endregion

        private CalibrationState _calibrationState = CalibrationState.Off;
        private string _nextStateKeybind;

        public bool Enabled => _calibrationState != CalibrationState.Off;

        #region Event Functions

        private void Start()
        {
            _nextStateKeybind = new InputActions().Calibration.Nextcalibration.GetBindingDisplayString();

            // Make UI invisible/disabled.
            calibrationUI.SetActive(false);

            // Set sliders to the player prefs.
            distanceSlider.value = MyPrefs.ScreenDistance;
            sizeSlider.value = MyPrefs.ScreenSize;

            // Callbacks.
            distanceSlider.onValueChanged.AddListener(_ => ChangeDistanceValue(distanceSlider));
            sizeSlider.onValueChanged.AddListener(_ => ChangeSizeValue(sizeSlider));
        }

        #endregion

        public void ToggleCalibrationUI()
        {
            // If UI is disabled, go to the first state.
            if (_calibrationState == CalibrationState.Off)
            {
                Cursor.visible = true;
                _calibrationState = _calibrationState.Next();
                UpdateCalibrationState();
            }
            // Else disable UI.
            else
            {
                Cursor.visible = false;
                _calibrationState = CalibrationState.Off;
                UpdateCalibrationState();
            }
        }

        public void SetNextState()
        {
            // If UI is disabled, don't continue.
            if (_calibrationState == CalibrationState.Off)
            {
                return;
            }

            _calibrationState = _calibrationState.Next();
            UpdateCalibrationState();
        }

        public void SetState(CalibrationState state)
        {
            _calibrationState = state;
            UpdateCalibrationState(false);
        }

        private void ChangeDistanceValue(Slider sender)
        {
            MyPrefs.ScreenDistance = (int)sender.value;
            MyEvents.ScreenDistanceChanged?.Invoke(gameObject, (int)sender.value);
        }

        private void ChangeSizeValue(Slider sender)
        {
            MyPrefs.ScreenSize = (int)sender.value;
            MyEvents.ScreenSizeChanged?.Invoke(gameObject, (int)sender.value);
        }

        private void UpdateCalibrationState(bool updatePrefs = true)
        {
            switch (_calibrationState)
            {
                // Highlight left edge.
                case CalibrationState.Left:
                    TurnOnPreview();
                    SetGuideText("left");
                    HighlightEdge();
                    break;

                // Set left edge, highlight right edge.
                case CalibrationState.Right:
                    if (updatePrefs)
                    {
                        MyPrefs.LeftCalibration = EyeTracker.EyeCenter.x;
                    }

                    SetGuideText("right");
                    HighlightEdge();
                    break;

                // Set right edge, highlight bottom edge.
                case CalibrationState.Bottom:
                    if (updatePrefs)
                    {
                        MyPrefs.RightCalibration = EyeTracker.EyeCenter.x;
                    }

                    SetGuideText("bottom");
                    HighlightEdge();
                    break;

                // Set bottom edge, highlight top edge.
                case CalibrationState.Top:
                    if (updatePrefs)
                    {
                        MyPrefs.BottomCalibration = EyeTracker.EyeCenter.y;
                    }

                    SetGuideText("top");
                    HighlightEdge();
                    break;

                // Set top edge, highlight middle.
                case CalibrationState.Sliders:
                    if (updatePrefs)
                    {
                        MyPrefs.TopCalibration = EyeTracker.EyeCenter.y;
                    }

                    guideText.text =
                        $"Set the sliders and keep your head pointed at the display from the given distance, then press '{_nextStateKeybind}'";
                    HighlightEdge();
                    break;

                // Set focal length and hide UI.
                case CalibrationState.Reset:
                    MyPrefs.FocalLength = EyeTracker.GetFocalLength(distanceSlider.value);
                    TurnOffPreview();
                    _calibrationState = CalibrationState.Off;
                    break;

                case CalibrationState.Off:
                    TurnOffPreview();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            MyEvents.CalibrationChanged?.Invoke(gameObject, _calibrationState != CalibrationState.Off);
        }

        private void SetGuideText(string edgeText)
        {
            var text = $"Align your head with the {edgeText} edge of the display and press '{_nextStateKeybind}'";
            guideText.text = text;
        }

        private void TurnOnPreview()
        {
            calibrationUI.SetActive(true);

            // Enable camera preview.
            cameraPreview.ShowSmallPreview();
        }

        private void TurnOffPreview()
        {
            calibrationUI.SetActive(false);

            // Disable camera preview.
            cameraPreview.Disable();
        }

        private void HighlightEdge()
        {
            monitorImage.sprite = monitorSprites[(int)_calibrationState.Prev()];
        }
    }
}