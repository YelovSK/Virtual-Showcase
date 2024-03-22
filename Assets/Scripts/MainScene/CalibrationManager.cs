using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VirtualShowcase.Enums;
using VirtualShowcase.FaceTracking;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.MainScene
{
    public class CalibrationManager : MonoBehaviour
    {
        private eCalibrationState _calibrationState = eCalibrationState.Off;

        #region Serialized Fields

        [Header("UI Elements")]
        [SerializeField] private GameObject calibrationUI;
        [SerializeField] private Image monitorImage;
        [SerializeField] private Sprite[] monitorSprites;
        [SerializeField] private Transform cameraPreviewTransform;
        [SerializeField] private TMP_Text guideText;

        [Header("Display distance sliders")]
        [SerializeField] private Slider distanceSlider;
        [SerializeField] private TMP_Text distanceValue;

        [Header("Display size sliders")]
        [SerializeField] private Slider sizeSlider;
        [SerializeField] private TMP_Text sizeValue;

        #endregion

        private CopyTransform origCameraPreviewTransform;
        public bool Enabled => _calibrationState != eCalibrationState.Off;
        public event EventHandler<eCalibrationState> StateChanged;
        private string _nextStateKeybind;

        #region Event Functions

        private void Start()
        {
            _nextStateKeybind = new InputActions().Calibration.Nextcalibration.GetBindingDisplayString();

            // Make UI invisible/disabled.
            calibrationUI.SetActive(false);

            // Save the transform of camera preview, to reset it back after calibration.
            origCameraPreviewTransform = new CopyTransform(cameraPreviewTransform);

            // Set sliders to the player prefs.
            distanceSlider.value = MyPrefs.ScreenDistance;
            sizeSlider.value = MyPrefs.ScreenSize;

            // Set delegates.
            ChangeDistanceValue(distanceSlider);
            distanceSlider.onValueChanged.AddListener(delegate
            {
                ChangeDistanceValue(distanceSlider);
            });

            ChangeSizeValue(sizeSlider);
            sizeSlider.onValueChanged.AddListener(delegate
            {
                ChangeSizeValue(sizeSlider);
            });
        }

        #endregion
        
        public void ToggleCalibrationUI()
        {
            // If UI is disabled, go to the first state.
            if (_calibrationState == eCalibrationState.Off)
            {
                Cursor.visible = true;
                _calibrationState = _calibrationState.Next();
                UpdateCalibrationState();
            }

            // Else disable UI.
            else
            {
                Cursor.visible = false;
                _calibrationState = eCalibrationState.Off;
                UpdateCalibrationState();
            }
        }

        public void SetNextState()
        {
            // If UI is disabled, don't continue.
            if (_calibrationState == eCalibrationState.Off)
                return;

            _calibrationState = _calibrationState.Next();
            UpdateCalibrationState();
        }

        public void SetState(eCalibrationState s)
        {
            _calibrationState = s;
            UpdateCalibrationState(false);
        }

        private void ChangeDistanceValue(Slider sender)
        {
            distanceValue.text = sender.value + "cm";
            MyPrefs.ScreenDistance = (int) sender.value;
        }

        private void ChangeSizeValue(Slider sender)
        {
            var screenSize = (int) sender.value;

            // Update UI.
            sizeValue.text = screenSize + "''";

            // Update size of screen.
            MyPrefs.ScreenSize = screenSize;
        }

        private void UpdateCalibrationState(bool updatePrefs = true)
        {
            switch (_calibrationState)
            {
                // Highlight left edge.
                case eCalibrationState.Left:
                    TurnOnPreview();
                    SetGuideText("left");
                    HighlightEdge();
                    break;

                // Set left edge, highlight right edge.
                case eCalibrationState.Right:
                    if (updatePrefs) MyPrefs.LeftCalibration = EyeTracker.EyeCenter.x;
                    SetGuideText("right");
                    HighlightEdge();
                    break;

                // Set right edge, highlight bottom edge.
                case eCalibrationState.Bottom:
                    if (updatePrefs) MyPrefs.RightCalibration = EyeTracker.EyeCenter.x;
                    SetGuideText("bottom");
                    HighlightEdge();
                    break;

                // Set bottom edge, highlight top edge.
                case eCalibrationState.Top:
                    if (updatePrefs) MyPrefs.BottomCalibration = EyeTracker.EyeCenter.y;
                    SetGuideText("top");
                    HighlightEdge();
                    break;

                // Set top edge, highlight middle.
                case eCalibrationState.Sliders:
                    if (updatePrefs) MyPrefs.TopCalibration = EyeTracker.EyeCenter.y;
                    guideText.text =
                        $"Set the sliders and keep your head pointed at the display from the given distance, then press '{_nextStateKeybind}'";
                    HighlightEdge();
                    break;

                // Set focal length and hide UI.
                case eCalibrationState.Reset:
                    MyPrefs.FocalLength = EyeTracker.GetFocalLength(distanceSlider.value);
                    TurnOffPreview();
                    _calibrationState = eCalibrationState.Off;
                    break;

                case eCalibrationState.Off:
                    TurnOffPreview();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            StateChanged?.Invoke(this, _calibrationState);
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
            GetComponent<ShowcaseInitializer>().ShowSmallCamPreview();
        }

        private void TurnOffPreview()
        {
            calibrationUI.SetActive(false);

            // Disable camera preview.
            GetComponent<ShowcaseInitializer>().SetCameraPreviewEnabled(false);

            // Set back the webcam preview location and size.
            cameraPreviewTransform.LoadTransform(origCameraPreviewTransform);
        }

        private void HighlightEdge()
        {
            monitorImage.sprite = monitorSprites[(int) _calibrationState.Prev()];
        }
    }
}