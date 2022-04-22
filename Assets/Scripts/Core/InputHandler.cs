using UnityEngine;

namespace VirtualVitrine
{
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private ShowcaseInitializer showcaseInitializer;
        [SerializeField] private CalibrationManager calibrationManager;

        #endregion

        #region Event Functions

        private void Update()
        {
            // Toggle menu/main scene.
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneSwitcher.SwitchScene();

            // Toggle webcam preview (ignore if calibration is active).
            if (Input.GetKeyDown("p") && !calibrationManager.Enabled)
                showcaseInitializer.SetCamPreview(true);

            // Toggle stereo/mono.
            if (Input.GetKeyDown(KeyCode.Tab))
                showcaseInitializer.SetStereo(true);

            // Toggle calibration UI.
            if (Input.GetKeyDown("c"))
                calibrationManager.ToggleCalibrationUI();

            // Go to next calibration state.
            if (Input.GetKeyDown(KeyCode.Return))
                calibrationManager.SetNextState();

            // Reset loaded object position.
            if (Input.GetKeyDown(KeyCode.R))
                ModelLoader.ResetTransform();

            // Loaded object gets controlled with mouse input.
            if (!calibrationManager.Enabled)
                HandleMouseInput();
        }

        #endregion

        private void HandleMouseInput()
        {
            if (ModelLoader.Model == null)
                return;

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            const float mouseFactor = 0.25f;

            // Both mouse buttons pressed => lower/raise object.
            if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
                ModelLoader.Model.transform.Translate(0, mouseY * mouseFactor, 0);
            // Right mouse button pressed => move object.
            else if (Input.GetMouseButton(1))
                ModelLoader.Model.transform.Translate(mouseX * mouseFactor, 0, mouseY * mouseFactor, Space.World);
            // Left mouse button pressed => rotate object.
            else if (Input.GetMouseButton(0))
                ModelLoader.Model.transform.Rotate(0, -mouseX, 0);

            // Mouse wheel => scale object.
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) ModelLoader.Model.transform.localScale *= 1.1f;
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) ModelLoader.Model.transform.localScale *= 0.9f;
        }
    }
}