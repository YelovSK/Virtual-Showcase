using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Dummiesman;

public class InitializeShowcase : MonoBehaviour
{
    [SerializeField] private GameObject leftCam;
    [SerializeField] private GameObject rightCam;
    [SerializeField] private GameObject monoCam;
    [SerializeField] GameObject camPreview;
    [SerializeField] CanvasGroup canvasGroup;
    GameObject _loadedObject;
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        SetCamPreview();
        if (PlayerPrefs.GetString("modelPath") == "")
            return;
        if (StaticVars.loadedObject != null)
        {
            _loadedObject = StaticVars.loadedObject;
            print("Loaded model from static var");
        }
        else
        {
            _loadedObject = new OBJLoader().Load(PlayerPrefs.GetString("modelPath"));
            _loadedObject.transform.Translate(0, 0, -7);
            _loadedObject.transform.Rotate(0, 180, 0);
            StaticVars.loadedObject = _loadedObject;
            print("Loaded new model");
        }
        DontDestroyOnLoad(_loadedObject);
        if (PlayerPrefs.GetInt("stereo") == 0)
            ActivateMono();
        else
            ActivateStereo();
    }

    void SetCamPreview()
    {
        if (PlayerPrefs.GetInt("previewIx") == 3)
            PlayerPrefs.SetInt("previewIx", 0);
        switch (PlayerPrefs.GetInt("previewIx"))
        {
            case 0: // Preview on
                camPreview.SetActive(true);
                canvasGroup.alpha = 1;
                break;
            case 1: // Preview off
                camPreview.SetActive(true);
                canvasGroup.alpha = 0;
                break;
            case 2: // Preview off and disabled tracking
                camPreview.SetActive(false);
                break;
        }
    }

    private void ActivateMono()
    {
        monoCam.SetActive(true);
        leftCam.SetActive(false);
        rightCam.SetActive(false);
    }

    private void ActivateStereo()
    {
        monoCam.SetActive(false);
        leftCam.SetActive(true);
        rightCam.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown("f12"))
        {
            PlayerPrefs.SetInt("previewIx", PlayerPrefs.GetInt("previewIx")+1);
            SetCamPreview();
        }
        if (Cursor.visible || _loadedObject == null)    // means 3D settings are showing in UI
            return;
        if (Input.GetKeyDown(KeyCode.R))
        {
            _loadedObject.transform.localScale = new Vector3(1f, 1f, 1f);
            _loadedObject.transform.localPosition = new Vector3(0f, 0f, -7f);
            _loadedObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var current = PlayerPrefs.GetInt("stereo");
            if (current == 0)
            {
                PlayerPrefs.SetInt("stereo", 1);
                ActivateStereo();
            }
            else
            {
                PlayerPrefs.SetInt("stereo", 0);
                ActivateMono();
            }
        }
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            _loadedObject.transform.Translate(0, mouseY/20, 0);
        }
        else if (Input.GetMouseButton(0))
        {
            _loadedObject.transform.Rotate(0, -mouseX, 0);
        }
        else if (Input.GetMouseButton(1))
        {
            _loadedObject.transform.Translate(mouseX/20, 0, mouseY/20, Space.World);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            _loadedObject.transform.localScale *= 1.1f;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            _loadedObject.transform.localScale *= 0.9f;
        }
    }
}
