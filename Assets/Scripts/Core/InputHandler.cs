using UnityEngine;

namespace VirtualVitrine
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private ShowcaseInitializer showcaseInitializer;
        [SerializeField] private CalibrationManager calibrationManager;
        
        private void Update()
        {
            // toggle menu/main scene
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneSwitcher.SwitchScene();
            
            // toggle webcam preview (ignore if calibration is active)
            if (Input.GetKeyDown("p") && !calibrationManager.Enabled)
                showcaseInitializer.SetCamPreview(toggle: true);

            // toggle stereo/mono
            if (Input.GetKeyDown(KeyCode.Tab))
                showcaseInitializer.SetStereo(toggle: true);
            
            // toggle calibration UI
            if (Input.GetKeyDown("c"))
                calibrationManager.ToggleCalibrationUI();

            // go to next calibration state
            if (Input.GetKeyDown(KeyCode.Return))
                calibrationManager.SetNextState();
            
            // reset loaded object position
            if (Input.GetKeyDown(KeyCode.R))
                ModelLoader.ResetTransform();
            
            // loaded object gets controlled with mouse input
            HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            if (ModelLoader.Model == null)
                return;
            
            var mouseX = Input.GetAxis("Mouse X");
            var mouseY = Input.GetAxis("Mouse Y");
            const float mouseFactor = 0.25f;
            
            // both mouse buttons pressed => lower/raise object
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
                ModelLoader.Model.transform.Translate(0, mouseY * mouseFactor, 0);
            // right mouse button pressed => move object
            else if (Input.GetMouseButton(1))
                ModelLoader.Model.transform.Translate(mouseX * mouseFactor, 0, mouseY * mouseFactor, Space.World);
            // left mouse button pressed => rotate object
            else if (Input.GetMouseButton(0))
                ModelLoader.Model.transform.Rotate(0, -mouseX, 0);
            
            // mouse wheel => scale object
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) ModelLoader.Model.transform.localScale *= 1.1f;
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) ModelLoader.Model.transform.localScale *= 0.9f;
        }
    }
}