using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.FaceTracking;
using VirtualShowcase.FaceTracking.Transform;

namespace VirtualShowcase.MainScene
{
    public class CalibrationManager : MonoBehaviour
    {
        // Is set by InputHandler in Awake().
        public static string NextStateKeybind;

        private static eCalibrationState eCalibrationState = eCalibrationState.Off;

        #region Serialized Fields

        [Header("UI Elements")]
        [SerializeField] private GameObject calibrationUI;
        [SerializeField] private Image monitorImage;
        [SerializeField] private Sprite[] monitorSprites;
        [SerializeField] private Transform cameraPreviewTransform;
        [SerializeField] private TMP_Text guideText;

        // Head for changing the distance from the display.
        [Space]
        [SerializeField] private Projection head;

        [Header("Display distance sliders")]
        [SerializeField] private Slider distanceSlider;
        [SerializeField] private TMP_Text distanceValue;

        [Header("Display size sliders")]
        [SerializeField] private Slider sizeSlider;
        [SerializeField] private TMP_Text sizeValue;

        #endregion

        private CopyTransform origCameraPreviewTransform;
        public static bool Enabled => eCalibrationState != eCalibrationState.Off;
        private static float EyesDistance => (EyeSmoother.LeftEyeSmoothed - EyeSmoother.RightEyeSmoothed).magnitude;

        #region Event Functions

        private void Start()
        {
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

        public static float GetRealHeadDistance()
        {
            // https://www.youtube.com/watch?v=jsoe1M2AjFk
            const int real_eyes_distance = 6;
            float realDistance = real_eyes_distance * MyPrefs.FocalLength / EyesDistance;
            return realDistance;
        }

        public static float GetFocalLength(float distanceFromScreen)
        {
            // Eyes distance on camera.
            float eyesDistance = EyesDistance;

            // Real life distance of eyes in cm.
            const int real_eyes_distance = 6;

            // Calculate focal length.
            float focal = eyesDistance * distanceFromScreen / real_eyes_distance;
            return focal;
        }

        public void ToggleCalibrationUI()
        {
            // If UI is disabled, go to the first state.
            if (eCalibrationState == eCalibrationState.Off)
            {
                Cursor.visible = true;
                eCalibrationState = eCalibrationState.Next();
                UpdateCalibrationState();
            }

            // Else disable UI.
            else
            {
                Cursor.visible = false;
                eCalibrationState = eCalibrationState.Off;
                TurnOffPreview();
            }
        }

        public void SetNextState()
        {
            // If UI is disabled, don't continue.
            if (eCalibrationState == eCalibrationState.Off)
                return;

            eCalibrationState = eCalibrationState.Next();
            UpdateCalibrationState();
        }

        public void SetState(eCalibrationState s)
        {
            eCalibrationState = s;
            UpdateCalibrationState(false);
        }

        private void ChangeDistanceValue(Slider sender)
        {
            distanceValue.text = sender.value + "cm";
            head.ScreenDistance = (int) sender.value;
        }

        private void ChangeSizeValue(Slider sender)
        {
            var screenSize = (int) sender.value;

            // Update UI.
            sizeValue.text = screenSize + "''";

            // Update size of screen.
            head.ScreenSize = screenSize;
        }

        private void UpdateCalibrationState(bool updatePrefs = true)
        {
            switch (eCalibrationState)
            {
                // Highlight left edge.
                case eCalibrationState.Left:
                    TurnOnPreview();
                    SetGuideText("left");
                    HighlightEdge();
                    break;

                // Set left edge, highlight right edge.
                case eCalibrationState.Right:
                    if (updatePrefs) MyPrefs.LeftCalibration = EyeSmoother.EyeCenter.x;
                    SetGuideText("right");
                    HighlightEdge();
                    break;

                // Set right edge, highlight bottom edge.
                case eCalibrationState.Bottom:
                    if (updatePrefs) MyPrefs.RightCalibration = EyeSmoother.EyeCenter.x;
                    SetGuideText("bottom");
                    HighlightEdge();
                    break;

                // Set bottom edge, highlight top edge.
                case eCalibrationState.Top:
                    if (updatePrefs) MyPrefs.BottomCalibration = EyeSmoother.EyeCenter.y;
                    SetGuideText("top");
                    HighlightEdge();
                    break;

                // Set top edge, highlight middle.
                case eCalibrationState.Sliders:
                    if (updatePrefs) MyPrefs.TopCalibration = EyeSmoother.EyeCenter.y;
                    guideText.text =
                        $"Set the sliders and keep your head pointed at the display from the given distance, then press '{NextStateKeybind}'";
                    HighlightEdge();
                    break;

                // Set focal length and hide UI.
                case eCalibrationState.Reset:
                    MyPrefs.FocalLength = GetFocalLength(distanceSlider.value);
                    TurnOffPreview();
                    eCalibrationState = 0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetGuideText(string edgeText)
        {
            var text = $"Align your head with the {edgeText} edge of the display and press '{NextStateKeybind}'";
            guideText.text = text;
        }

        private void TurnOnPreview()
        {
            calibrationUI.SetActive(true);

            // Enable camera preview.
            MyPrefs.PreviewOn = 1;
            GetComponent<ShowcaseInitializer>().SetCamPreview();

            // Make the webcam preview smaller and put it in the left corner.
            cameraPreviewTransform.localScale = Vector3.one * 0.5f;
            cameraPreviewTransform.localPosition = new Vector3(-710, -310, 0);
        }

        private void TurnOffPreview()
        {
            calibrationUI.SetActive(false);

            // Disable camera preview.
            MyPrefs.PreviewOn = 0;
            GetComponent<ShowcaseInitializer>().SetCamPreview();

            // Set back the webcam preview location and size.
            cameraPreviewTransform.LoadTransform(origCameraPreviewTransform);
        }

        private void HighlightEdge()
        {
            monitorImage.sprite = monitorSprites[(int) eCalibrationState.Prev()];
        }
    }
}