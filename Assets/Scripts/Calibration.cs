using System;
using System.Collections;
using System.Collections.Generic;
using MediaPipe.BlazeFace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Calibration : MonoBehaviour
{
    // UI elements
    [SerializeField] GameObject CalibrationUI;
    [SerializeField] Image MonitorImage;
    [SerializeField] Sprite[] MonitorSprites;
    [SerializeField] Transform CameraPreviewTransform;
    [SerializeField] TMP_Text GuideText;
    // head for changing the distance from the display
    [SerializeField] GameObject Head;
    // scene to change the size based on the display size
    [SerializeField] GameObject Scene;
    // sliders for settings the distance from the display
    [SerializeField] GameObject Sliders;
    
    [SerializeField] Slider DistanceSlider;
    [SerializeField] TMP_Text DistanceValue;
    // sliders for settings the size of the display
    [SerializeField] Slider SizeSlider;
    [SerializeField] TMP_Text SizeValue;
    
    // camera preview position for setting it to the original transform after calibration
    Vector3 _origCameraPreviewPosition;
    Vector3 _origCameraPreviewScale;
    // 0: off, 1: left edge, 2: right edge, 3: bottom edge, 4: top edge, 5: reset to 0
    int _state = 0;
    
    void Start()
    {
        // make UI invisible/disabled
        CalibrationUI.SetActive(false);
        // save the transform of camera preview, to reset it back after calibration
        _origCameraPreviewPosition = CameraPreviewTransform.position;
        _origCameraPreviewScale = CameraPreviewTransform.localScale;

        // set sliders to the player prefs
        DistanceSlider.value = PlayerPrefs.GetInt("distanceFromScreenCm");
        SizeSlider.value = PlayerPrefs.GetInt("screenDiagonalInches");
        
        // set delegates
        ChangeDistanceValue(DistanceSlider);
        DistanceSlider.onValueChanged.AddListener(delegate
        {
            ChangeDistanceValue(DistanceSlider);
        });
        
        ChangeSizeValue(SizeSlider);
        SizeSlider.onValueChanged.AddListener(delegate
        {
            ChangeSizeValue(SizeSlider);
        });
    }

    private void ChangeDistanceValue(Slider sender)
    {
        DistanceValue.text = sender.value + "cm";
    }


    private void ChangeSizeValue(Slider sender)
    {
        SizeValue.text = sender.value + "''";
        PlayerPrefs.SetInt("screenDiagonalInches", (int) sender.value);
        var (width, height) = DiagonalToWidthAndHeight((int) sender.value);
        // base width of 53cm is for 24'' diagonal, so 24'' is scale 1.0
        int baseWidth = 53;
        float ratio = (float) width / baseWidth;
        Scene.transform.localScale = new Vector3(ratio, ratio, ratio);
        // set the size in asym frustum, which deals with the projection
        Head.GetComponent<AsymFrustum>().width = width;
        Head.GetComponent<AsymFrustum>().height = height;
    }


    void Update()
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

    void UpdateState()
    {
        switch (_state)
        {
            // highlight left edge
            case 1:
                TurnOnPreview();
                SetGuideText(edgeText: "left");
                HighlightEdge();
                break;
            // set left edge, highlight right edge
            case 2:
                PlayerPrefs.SetFloat("LeftCalibration", EyeTracker.EyeCenter.x);
                SetGuideText(edgeText: "right");
                HighlightEdge();
                break;
            // set right edge, highlight bottom edge
            case 3:
                PlayerPrefs.SetFloat("RightCalibration", EyeTracker.EyeCenter.x);
                SetGuideText(edgeText: "bottom");
                HighlightEdge();
                break;
            // set bottom edge, highlight top edge
            case 4:
                PlayerPrefs.SetFloat("BottomCalibration", EyeTracker.EyeCenter.y);
                SetGuideText(edgeText: "top");
                HighlightEdge();
                break;
            // set top edge, show sliders
            case 5:
                PlayerPrefs.SetFloat("TopCalibration", EyeTracker.EyeCenter.y);
                GuideText.text = "Set the sliders and keep your head pointed at the display from the given distance, then press 'Enter'";
                HighlightEdge();
                Sliders.SetActive(true);
                break;
            // set focal length and hide UI
            case 6:
                SetFocalLength();
                TurnOffPreview();
                _state = 0;
                break;
        }
    }

    public static float GetEyesDistance()
    {
        // difference of leftEye and rightEye x,y coords
        var leftEye = EyeTracker.LeftEye;
        var rightEye = EyeTracker.RightEye;
        var eyesDifference = leftEye - rightEye;
        // distance of eyes diagonal (thanks pythagoras)
        var eyesDistance = Math.Sqrt(Math.Pow(eyesDifference.x, 2) + Math.Pow(eyesDifference.y, 2));
        // todo calculate distance properly when head rotated
        // var mouthX = FindObjectOfType<EyeTracker>().Detection.mouth.x;
        // var eyesMiddleX = (leftEye.x + rightEye.x) / 2;
        // eyesDistance *= 1 + Math.Abs(mouthX - eyesMiddleX)*15;
        return (float) eyesDistance;
    }

    public static float GetFocalLength(float distanceFromScreen)
    {
        // eyes distance on camera
        var eyesDistance = GetEyesDistance();
        // real life distance of eyes in cm
        const int realEyesDistance = 6;
        // calculate focal length
        var focal = (eyesDistance * distanceFromScreen) / realEyesDistance;
        return focal;
    }

    private void SetFocalLength()
    {
        PlayerPrefs.SetInt("distanceFromScreenCm", (int) DistanceSlider.value);
        // position of the head is offset by sender.value from the display
        Head.transform.localPosition = new Vector3(0, (int) DistanceSlider.value, 0);
        PlayerPrefs.SetFloat("focalLength", GetFocalLength(DistanceSlider.value));
    }

    public static float GetRealHeadDistance()
    {
        var focal = PlayerPrefs.GetFloat("focalLength");
        const int realEyesDistance = 6;
        var realDistance = (realEyesDistance * focal) / GetEyesDistance();
        return realDistance;
    }

    void SetGuideText(string edgeText)
    {
        var text = "Align your eyes with the " + edgeText + " edge of the display and press 'Enter'";
        GuideText.text = text;
    }

    void TurnOnPreview()
    {
        CalibrationUI.SetActive(true);
        PlayerPrefs.SetInt("previewIx", 0);
        GetComponent<CanvasGroup>().alpha = 1;
        CameraPreviewTransform.gameObject.SetActive(true);
        // make the webcam preview smaller and put it in a corner
        CameraPreviewTransform.position = new Vector3(200, 200, 0);
        CameraPreviewTransform.localScale = new Vector3(0.4f, 0.4f, 1);
    }

    void TurnOffPreview()
    {
        PlayerPrefs.SetInt("previewIx", 1);
        GetComponent<CanvasGroup>().alpha = 0;
        // set back the webcam preview location and size
        CameraPreviewTransform.position = _origCameraPreviewPosition;
        CameraPreviewTransform.localScale = _origCameraPreviewScale;
        CalibrationUI.SetActive(false);
        Sliders.SetActive(false);
    }

    void HighlightEdge() => MonitorImage.sprite = MonitorSprites[_state - 1];

    /// <summary>
    /// Returns width and height of display in centimeters from diagonal inches.
    /// </summary>
    /// <param name="diagonalInches"></param>
    /// <returns></returns>
    public static Tuple<int, int> DiagonalToWidthAndHeight(int diagonalInches)
    {
        double aspectRatio = (double) 16 / 9;
        double height = diagonalInches / Math.Sqrt((aspectRatio * aspectRatio) + 1);
        double width = aspectRatio * height;
        const float cmsInInch = 2.54f;
        return Tuple.Create((int) (width * cmsInInch), (int) (height * cmsInInch));
    }
}
