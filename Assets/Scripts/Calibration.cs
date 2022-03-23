using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MediaPipe.BlazeFace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Calibration : MonoBehaviour
{
    [SerializeField] GameObject CalibrationUI;
    [SerializeField] Image MonitorImage;
    [SerializeField] Sprite[] MonitorSprites;
    [SerializeField] Transform CameraPreviewTransform;
    [SerializeField] TMP_Text GuideText;
    Vector3 _origCameraPreviewPosition;
    Vector3 _origCameraPreviewScale;
    // 0: off, 1: left edge, 2: right edge, 3: bottom edge, 4: top edge, 5: reset to 0
    int _state = 0;

    void Start()
    {
        CalibrationUI.SetActive(false);
        // save the transform of camera preview, to reset it back after calibration
        _origCameraPreviewPosition = CameraPreviewTransform.position;
        _origCameraPreviewScale = CameraPreviewTransform.localScale;
    }

    void Update()
    {
        if (Input.GetKeyDown("c"))
        {
            _state++;
            UpdateState();
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

    Vector2 GetEyeCenter()
    {
        return FindObjectOfType<EyeTracker>().EyeCenter;
    }

    void HighlightEdge()
    {
        MonitorImage.sprite = MonitorSprites[_state - 1];
    }
}
