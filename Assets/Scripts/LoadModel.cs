using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;

public class LoadModel : MonoBehaviour
{
    public GameObject camPreview;
    public CanvasGroup canvasGroup;
    GameObject loadedObject;
    void Start()
    {
        if (loadedObject != null)
            Destroy(loadedObject);
        Debug.Log(GlobalVars.modelPath);
        loadedObject = new OBJLoader().Load(GlobalVars.modelPath);
        loadedObject.transform.Translate(0, 0, -7);
        loadedObject.transform.Rotate(0, 180, 0);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            loadedObject.transform.localScale = new Vector3(1f, 1f, 1f);
            loadedObject.transform.localPosition = new Vector3(0f, 0f, -7f);
            loadedObject.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
        }
        if (Input.GetKeyDown("f12"))
        {
            GlobalVars.previewIx++;
            if (GlobalVars.previewIx == 3)
                GlobalVars.previewIx = 0;
            switch (GlobalVars.previewIx)
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
        if (Cursor.visible)    // means 3D settings are showing in UI
            return;
        var mouseX = Input.GetAxis("Mouse X");
        var mouseY = Input.GetAxis("Mouse Y");
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            loadedObject.transform.Translate(0, mouseY/20, 0);
            return;
        }
        if (Input.GetMouseButton(0))
        {
            loadedObject.transform.Rotate(0, -mouseX, 0);
        }
        if (Input.GetMouseButton(1))
        {
            loadedObject.transform.Translate(mouseX/20, 0, mouseY/20, Space.World);
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            loadedObject.transform.localScale *= 1.1f;
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            loadedObject.transform.localScale *= 0.9f;
        }
    }

}
