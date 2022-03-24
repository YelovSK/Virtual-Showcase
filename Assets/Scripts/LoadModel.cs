using System.Collections;
using System.Collections.Generic;
using Dummiesman;
using UnityEngine;

public class LoadModel : MonoBehaviour
{
    GameObject _loadedObject;

    void Start()
    {
        LoadObject();
    }

    void Update()
    {
        CheckMouseInput();
        if (Input.GetKeyDown(KeyCode.R))
            ResetTransform();
    }
    
    void LoadObject()
    {
        if (PlayerPrefs.GetString("modelPath") == "")
            return;
        if (StaticVars.loadedObject != null)
        {
            _loadedObject = StaticVars.loadedObject;
            _loadedObject.transform.parent = transform;
            print("Loaded model from static var");
        }
        else
        {
            _loadedObject = new OBJLoader().Load(PlayerPrefs.GetString("modelPath"));
            StaticVars.loadedObject = _loadedObject;
            _loadedObject.transform.parent = transform;
            ResetTransform();
            print("Loaded new model");
        }
        // default is middle of the screen, put half of height below
        var offset = GameObject.Find("HeadNode").GetComponent<AsymFrustum>().height / 2;
        _loadedObject.transform.Translate(new Vector3(0, -offset, 0));
        DontDestroyOnLoad(this);
    }

    void ResetTransform()
    {
        _loadedObject.transform.localPosition = Vector3.zero;
        _loadedObject.transform.localRotation = Quaternion.identity;
        _loadedObject.transform.localScale = Vector3.one;
    }
    
    void CheckMouseInput()
    {
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
