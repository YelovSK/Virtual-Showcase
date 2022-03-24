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
    [SerializeField] TMP_Text DistanceText;
    [SerializeField] Slider DistanceSlider;
    [SerializeField] TMP_Text DistanceValue;
    // sliders for settings the size of the display
    [SerializeField] TMP_Text SizeText;
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
        PlayerPrefs.SetInt("distanceFromScreenCm", (int) sender.value);
        // position of the head is offset by sender.value from the display
        Head.transform.localPosition = new Vector3(0, (int) sender.value, 0);
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
                PlayerPrefs.SetFloat("LeftCalibration", GetEyeCenter().x);
                SetGuideText(edgeText: "right");
                HighlightEdge();
                break;
            // set right edge, highlight bottom edge
            case 3:
                PlayerPrefs.SetFloat("RightCalibration", GetEyeCenter().x);
                SetGuideText(edgeText: "bottom");
                HighlightEdge();
                break;
            // set bottom edge, highlight top edge
            case 4:
                PlayerPrefs.SetFloat("BottomCalibration", GetEyeCenter().y);
                SetGuideText(edgeText: "top");
                HighlightEdge();
                break;
            // set top edge, hide UI
            case 5:
                PlayerPrefs.SetFloat("TopCalibration", GetEyeCenter().y);
                TurnOffPreview();
                _state = 0;
                break;
        }
    }

    /// <summary>
    /// Change the 5th word to edgeText.
    /// </summary>
    /// <param name="edgeText"></param>
    void SetGuideText(string edgeText)
    {
        int wordIndex = 5;
        var words = GuideText.text.Split();
        words[wordIndex] = edgeText;
        GuideText.text = string.Join(" ", words);
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
    }

    Vector2 GetEyeCenter() => FindObjectOfType<EyeTracker>().EyeCenter;

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
        float cmsInInch = 2.54f;
        return Tuple.Create((int) (width * cmsInInch), (int) (height * cmsInInch));
    }
}
