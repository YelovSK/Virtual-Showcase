using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIKeyHandler : MonoBehaviour
{
    public GameObject camPreview;
    public TMP_Dropdown previewDropdown;

    private void Start()
    {
        previewDropdown.value = GlobalVars.previewIx;
        ChangePreview(previewDropdown);
        previewDropdown.onValueChanged.AddListener(delegate
        {
            ChangePreview(previewDropdown);
        });
    }

    private void ChangePreview(TMP_Dropdown sender)
    {
        var camTransform = camPreview.GetComponent<RectTransform>();
        GlobalVars.previewIx = sender.value;
        switch (sender.value)
        {
            case 0: // On
                camPreview.SetActive(true);
                camTransform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                break;
            case 1: // Off
                camPreview.SetActive(true);
                camTransform.localScale = new Vector3(0f, 0f, 0f);
                break;
            case 2: // Disabled tracking
                camPreview.SetActive(false);
                break;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown("f12"))    // hide/show canvas when F12 is pressed
            GetComponent<CanvasGroup>().alpha = GetComponent<CanvasGroup>().alpha == 0 ? 1 : 0;
    }
}
