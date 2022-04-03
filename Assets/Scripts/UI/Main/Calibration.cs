using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using VirtualVitrine.FaceTracking;
using VirtualVitrine.FaceTracking.Transform;

namespace VirtualVitrine.UI.Main
{
    public class Calibration : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI Elements")]
        [FormerlySerializedAs("CalibrationUI")] [SerializeField] private GameObject calibrationUI;
        [FormerlySerializedAs("MonitorImage")] [SerializeField] private Image monitorImage;
        [FormerlySerializedAs("MonitorSprites")] [SerializeField] private Sprite[] monitorSprites;
        [FormerlySerializedAs("CameraPreviewTransform")] [SerializeField] private Transform cameraPreviewTransform;
        [FormerlySerializedAs("GuideText")] [SerializeField] private TMP_Text guideText;

        // head for changing the distance from the display
        [Space]
        [FormerlySerializedAs("Head")] [SerializeField] private GameObject head;

        // scene for changing the size based on the display size
        [Space]
        [FormerlySerializedAs("Scene")] [SerializeField] private GameObject scene;

        [Header("Display distance sliders")]
        [FormerlySerializedAs("Sliders")] [SerializeField] private GameObject sliders;
        [FormerlySerializedAs("DistanceSlider")] [SerializeField] private Slider distanceSlider;
        [FormerlySerializedAs("DistanceValue")] [SerializeField] private TMP_Text distanceValue;

        [Header("Display size sliders")]
        [FormerlySerializedAs("SizeSlider")] [SerializeField] private Slider sizeSlider;
        [FormerlySerializedAs("SizeValue")] [SerializeField] private TMP_Text sizeValue;
        #endregion

        #region Private Fields
        // camera preview position for setting it to the original transform after calibration
        private Vector3 _origCameraPreviewPosition;

        private Vector3 _origCameraPreviewScale;

        // 0: off, 1: left edge, 2: right edge, 3: bottom edge, 4: top edge, 5: reset to 0
        private int _state;
        #endregion

        #region Public Methods
        public static float GetRealHeadDistance()
        {
            // https://www.youtube.com/watch?v=jsoe1M2AjFk
            var focal = PlayerPrefs.GetFloat("focalLength");
            const int realEyesDistance = 6;
            var realDistance = realEyesDistance * focal / GetEyesDistance();
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
            distanceSlider.value = PlayerPrefs.GetInt("distanceFromScreenCm");
            sizeSlider.value = PlayerPrefs.GetInt("screenDiagonalInches");

            // set delegates
            ChangeDistanceValue(distanceSlider);
            distanceSlider.onValueChanged.AddListener(delegate { ChangeDistanceValue(distanceSlider); });

            ChangeSizeValue(sizeSlider);
            sizeSlider.onValueChanged.AddListener(delegate { ChangeSizeValue(sizeSlider); });
        }


        private void Update()
        {
            // Enter continues to the next state (doesn't initialize)
            if (Input.GetKeyDown(KeyCode.Return) && _state != 0)
            {
                _state++;
                UpdateState();
            }

            // C initializes or disables calibration
            if (Input.GetKeyDown("c"))
            {
                if (_state == 0)
                {
                    _state++;
                    UpdateState();
                }
                else
                {
                    _state = 0;
                    TurnOffPreview();
                }
            }
        }
        #endregion
        
        #region Private Methods
        /// <summary>
        ///     Returns width and height of display in centimeters from diagonal inches.
        /// </summary>
        /// <param name="diagonalInches"></param>
        /// <returns></returns>
        private static Tuple<int, int> DiagonalToWidthAndHeight(int diagonalInches)
        {
            const float cmsInInch = 2.54f;
            const double aspectRatio = (double) 16 / 9;
            var height = diagonalInches / Math.Sqrt(aspectRatio * aspectRatio + 1);
            var width = aspectRatio * height;
            return Tuple.Create((int) (width * cmsInInch), (int) (height * cmsInInch));
        }
        
        private static float GetEyesDistance() => (EyeSmoother.LeftEyeSmoothed - EyeSmoother.RightEyeSmoothed).magnitude;

        private void ChangeDistanceValue(Slider sender) => distanceValue.text = sender.value + "cm";

        private void ChangeSizeValue(Slider sender)
        {
            sizeValue.text = sender.value + "''";
            PlayerPrefs.SetInt("screenDiagonalInches", (int) sender.value);
            var (width, height) = DiagonalToWidthAndHeight((int) sender.value);
            // base width of 53cm is for 24'' diagonal, so 24'' is scale 1.0
            const int baseWidth = 53;
            var ratio = (float) width / baseWidth;
            scene.transform.localScale = new Vector3(ratio, ratio, ratio);
            // set the size in asym frustum, which deals with the projection
            head.GetComponent<AsymFrustum>().width = width;
            head.GetComponent<AsymFrustum>().height = height;
        }

        private void UpdateState()
        {
            switch (_state)
            {
                // highlight left edge
                case 1:
                    TurnOnPreview();
                    SetGuideText("left");
                    HighlightEdge();
                    break;
                // set left edge, highlight right edge
                case 2:
                    PlayerPrefs.SetFloat("LeftCalibration", EyeSmoother.EyeCenter.x);
                    SetGuideText("right");
                    HighlightEdge();
                    break;
                // set right edge, highlight bottom edge
                case 3:
                    PlayerPrefs.SetFloat("RightCalibration", EyeSmoother.EyeCenter.x);
                    SetGuideText("bottom");
                    HighlightEdge();
                    break;
                // set bottom edge, highlight top edge
                case 4:
                    PlayerPrefs.SetFloat("BottomCalibration", EyeSmoother.EyeCenter.y);
                    SetGuideText("top");
                    HighlightEdge();
                    break;
                // set top edge, show sliders
                case 5:
                    PlayerPrefs.SetFloat("TopCalibration", EyeSmoother.EyeCenter.y);
                    guideText.text =
                        "Set the sliders and keep your head pointed at the display from the given distance, then press 'Enter'";
                    HighlightEdge();
                    sliders.SetActive(true);
                    break;
                // set focal length and hide UI
                case 6:
                    SetFocalLength();
                    TurnOffPreview();
                    _state = 0;
                    break;
            }
        }

        private void SetFocalLength()
        {
            PlayerPrefs.SetInt("distanceFromScreenCm", (int) distanceSlider.value);
            // position of the head is offset by sender.value from the display
            head.transform.localPosition = new Vector3(0, (int) distanceSlider.value, 0);
            PlayerPrefs.SetFloat("focalLength", GetFocalLength(distanceSlider.value));
        }

        private void SetGuideText(string edgeText)
        {
            var text = "Align your eyes with the " + edgeText + " edge of the display and press 'Enter'";
            guideText.text = text;
        }

        private void TurnOnPreview()
        {
            calibrationUI.SetActive(true);
            PlayerPrefs.SetInt("previewIx", 0);
            GetComponent<CanvasGroup>().alpha = 1;
            cameraPreviewTransform.gameObject.SetActive(true);
            // make the webcam preview smaller and put it in a corner
            cameraPreviewTransform.position = new Vector3(200, 200, 0);
            cameraPreviewTransform.localScale = new Vector3(0.4f, 0.4f, 1);
        }

        private void TurnOffPreview()
        {
            PlayerPrefs.SetInt("previewIx", 1);
            GetComponent<CanvasGroup>().alpha = 0;
            // set back the webcam preview location and size
            cameraPreviewTransform.position = _origCameraPreviewPosition;
            cameraPreviewTransform.localScale = _origCameraPreviewScale;
            calibrationUI.SetActive(false);
            sliders.SetActive(false);
        }

        private void HighlightEdge() => monitorImage.sprite = monitorSprites[_state - 1];
        #endregion
    }
}