﻿using System.Collections;
using UnityEngine;
using VirtualShowcase.Core;
using VirtualShowcase.ModelLoading;

namespace VirtualShowcase.Showcase
{
    public class Canvas : MonoBehaviour
    {
        [SerializeField]
        private GameObject _scenesCanvas;
        
        [SerializeField]
        private GameObject _calibrationCanvas;
        
        [SerializeField]
        private CalibrationController calibrationController;
        
        private Coroutine _hideCursor;

        public void Update()
        {
            // Hide cursor after 3 seconds of inactivity.
            // Idk where to put this, so it's just gonna be here.
            if (Input.GetAxis("Mouse X") == 0 && (Input.GetAxis("Mouse Y") == 0))
            {
                _hideCursor ??= StartCoroutine(HideCursorDelay(3));
            }
            else if (_hideCursor != null)
            {
                StopCoroutine(_hideCursor);
                _hideCursor = null;
                Cursor.visible = true;
            }
        }
        
        public void ToggleScenesCanvas()
        {
            _scenesCanvas.SetActive(!_scenesCanvas.activeSelf);
        }

        public void ToggleCalibrationCanvas()
        {
            _calibrationCanvas.SetActive(!_calibrationCanvas.activeSelf);
            calibrationController.gameObject.SetActive(!calibrationController.gameObject.activeSelf);
            calibrationController.ToggleCalibrationUI();
        }

        public void ShowMenu()
        {
            MySceneManager.Instance.LoadMenuScene();
        }
        
        public void ToggleSideBySideView()
        {
            if (ModelLoaderClient.Instance.IsSideBySideViewEnabled)
            {
                ModelLoaderClient.Instance.DisableSideBySideView();
            }
            else
            {
                ModelLoaderClient.Instance.EnableSideBySideView();
            }
        }
        
        public void ToggleRotation()
        {
            ModelLoaderClient.Instance.IsRotationEnabled = !ModelLoaderClient.Instance.IsRotationEnabled;
        }
        
        public void SwitchNextModel()
        {
            ModelLoaderClient.Instance.CycleActiveModel(next: true);
        }

        public void SwitchPreviousModel()
        {
            ModelLoaderClient.Instance.CycleActiveModel(next: false);
        }

        private IEnumerator HideCursorDelay(int delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            Cursor.visible = false;
        }
    }
}