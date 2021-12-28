using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Dummiesman;

public class LoadModel : MonoBehaviour
{
    GameObject loadedObject;
    float angle = 0f;

    void Start()
    {
        if (loadedObject != null)
        {
            Destroy(loadedObject);
        }
        Debug.Log(GlobalVars.modelPath);
        loadedObject = new OBJLoader().Load(GlobalVars.modelPath);
        loadedObject.transform.localPosition = new Vector3(0, 0, 0);
    }

    void Update()
    {
        float moveBy = 10f*Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            loadedObject.transform.localPosition += new Vector3(0, moveBy, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            loadedObject.transform.localPosition += new Vector3(0, -moveBy, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            angle += 1f;
            loadedObject.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            angle -= 1f;
            loadedObject.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
        }
        if (Input.GetKey(KeyCode.A))
        {
            loadedObject.transform.localPosition += new Vector3(-moveBy, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            loadedObject.transform.localPosition += new Vector3(+moveBy, 0, 0);
        }
        if (Input.GetKey(KeyCode.S))
        {
            loadedObject.transform.localPosition += new Vector3(0, 0, -moveBy);
        }
        if (Input.GetKey(KeyCode.W))
        {
            loadedObject.transform.localPosition += new Vector3(0, 0, moveBy);
        }
        if (Input.GetKey(KeyCode.E))
        {
            loadedObject.transform.localScale *= 1.01f;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            loadedObject.transform.localScale *= 0.99f;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            loadedObject.transform.localScale = new Vector3(1f, 1f, 1f);
            loadedObject.transform.localPosition = new Vector3(0f, 0f, 0f);
            loadedObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            angle = 0f;
        }
    }

}
