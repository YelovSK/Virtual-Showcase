using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualVitrine.FaceTracking;
using VirtualVitrine.FaceTracking.Transform;

namespace VirtualVitrine
{
    public class CalibrationManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI Elements")]
        [SerializeField] private GameObject calibrationUI;
        [SerializeField] private Image monitorImage;
        [SerializeField] private Sprite[] monitorSprites;
        [SerializeField] private Transform cameraPreviewTransform;
        [SerializeField] private TMP_Text guideText;

        // head for changing the distance from the display
        [Space]
        [SerializeField] private AsymFrustum head;

        [Header("Display distance sliders")]
        [SerializeField] private Slider distanceSlider;
        [SerializeField] private TMP_Text distanceValue;

        [Header("Display size sliders")]
        [SerializeField] private Slider sizeSlider;
        [SerializeField] private TMP_Text sizeValue;
        #endregion
        
        #region Public Fields
        public bool Enabled => _state != States.OFF;
        #endregion

        #region Private Fields
        // camera preview position for setting it to the original transform after calibration
        private Vector3 _origCameraPreviewPosition;
        private Vector3 _origCameraPreviewScale;

        private enum States { OFF, LEFT, RIGHT, BOTTOM, TOP, SLIDERS, RESET };
        private States _state = States.OFF;
        #endregion

        #region Public Methods
        public static float GetRealHeadDistance()
        {
            // https://www.youtube.com/watch?v=jsoe1M2AjFk
            const int realEyesDistance = 6;
            var realDistance = realEyesDistance * MyPrefs.FocalLength / GetEyesDistance();
            return realDistance;
        }

        public static float GetFocalLength(float distanceFromScreen)
        {
            // eyes distance on camera
            var eyesDistance = GetEyesDistance();

            // real life distance of eyes in cm
            const int realEyesDistance = 6;
            
            // calculate focal length
            var focal = eyesDistance * distanceFromScreen / realEyesDistance;
            return focal;
        }

        public void ToggleCalibrationUI()
        {
            // if UI is disabled, go to the first state
            if (_state == States.OFF)
            {
                _state = _state.Next();
                UpdateState();
            }
            // else disable UI
            else
            {
                _state = States.OFF;
                TurnOffPreview();
            }
        }

        public void SetNextState()
        {
            // if UI is disabled, don't continue
            if (_state == States.OFF)
                return;

            _state = _state.Next();
            UpdateState();
        }
        #endregion
        
        #region Unity Methods

        private void Start()
        {
            // make UI invisible/disabled
            calibrationUI.SetActive(false);

            // save the transform of camera preview, to reset it back after calibration
            _origCameraPreviewPosition = cameraPreviewTransform.position;
            _origCameraPreviewScale = cameraPreviewTransform.localScale;

            // set sliders to the player prefs
            distanceSlider.value = MyPrefs.ScreenDistance;
            sizeSlider.value = MyPrefs.ScreenSize;

            // set delegates
            ChangeDistanceValue(distanceSlider);
            distanceSlider.onValueChanged.AddListener(delegate { ChangeDistanceValue(distanceSlider); });

            ChangeSizeValue(sizeSlider);
            sizeSlider.onValueChanged.AddListener(delegate { ChangeSizeValue(sizeSlider); });
        }
        #endregion
        
        #region Private Methods
        private static float GetEyesDistance() => (EyeSmoother.LeftEyeSmoothed - EyeSmoother.RightEyeSmoothed).magnitude;

        private void ChangeDistanceValue(Slider sender)
        {
            distanceValue.text = sender.value + "cm";
            head.ScreenDistance = (int) sender.value;
        }

        private void ChangeSizeValue(Slider sender)
        {
            var screenSize = (int) sender.value;
            
            // update UI
            sizeValue.text = screenSize + "''";
            
            // update size of screen
            head.ScreenSize = screenSize;
        }

        private void UpdateState()
        {
            switch (_state)
            {
                // highlight left edge
                case States.LEFT:
                    TurnOnPreview();
                    SetGuideText("left");
                    HighlightEdge();
                    break;
                // set left edge, highlight right edge
                case States.RIGHT:
                    MyPrefs.LeftCalibration = EyeSmoother.EyeCenter.x;
                    SetGuideText("right");
                    HighlightEdge();
                    break;
                // set right edge, highlight bottom edge
                case States.BOTTOM:
                    MyPrefs.RightCalibration = EyeSmoother.EyeCenter.x;
                    SetGuideText("bottom");
                    HighlightEdge();
                    break;
                // set bottom edge, highlight top edge
                case States.TOP:
                    MyPrefs.BottomCalibration = EyeSmoother.EyeCenter.y;
                    SetGuideText("top");
                    HighlightEdge();
                    break;
                // set top edge, highlight middle
                case States.SLIDERS:
                    MyPrefs.TopCalibration = EyeSmoother.EyeCenter.y;
                    guideText.text =
                        "Set the sliders and keep your head pointed at the display from the given distance, then press 'Enter'";
                    HighlightEdge();
                    break;
                // set focal length and hide UI
                case States.RESET:
                    MyPrefs.FocalLength = GetFocalLength(distanceSlider.value);
                    TurnOffPreview();
                    _state = 0;
                    break;
            }
        }

        private void SetGuideText(string edgeText)
        {
            var text = "Align your head with the " + edgeText + " edge of the display and press 'Enter'";
            guideText.text = text;
        }

        private void TurnOnPreview()
        {
            calibrationUI.SetActive(true);
            
            // enable camera preview
            MyPrefs.PreviewOn = 1;
            GetComponent<ShowcaseInitializer>().SetCamPreview();

            // make the webcam preview smaller and put it in a corner
            cameraPreviewTransform.position = new Vector3(200, 200, 0);
            cameraPreviewTransform.localScale = new Vector3(0.4f, 0.4f, 1);
        }

        private void TurnOffPreview()
        {
            calibrationUI.SetActive(false);
            
            // disable camera preview
            MyPrefs.PreviewOn = 0;
            GetComponent<ShowcaseInitializer>().SetCamPreview();
            
            // set back the webcam preview location and size
            cameraPreviewTransform.position = _origCameraPreviewPosition;
            cameraPreviewTransform.localScale = _origCameraPreviewScale;
        }

        private void HighlightEdge() => monitorImage.sprite = monitorSprites[(int) _state.Prev()];
        #endregion
    }
}