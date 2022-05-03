using UnityEngine;
using UnityEngine.UI;

namespace VirtualVitrine
{
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private ShowcaseInitializer showcaseInitializer;
        [SerializeField] private CalibrationManager calibrationManager;
        [SerializeField] private Image rotationImage;
        [SerializeField] private Sprite[] rotationImages;

        #endregion

        #region Event Functions

        private void Update()
        {
            // Toggle menu/main scene.
            if (Input.GetKeyDown(KeyCode.Escape))
                SceneSwitcher.SwitchScene();

            if (Input.GetKeyDown("s"))
                SceneSwitcher.SwitchDifferentMain();

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

            // Right mouse button pressed => lower/raise object.
            if (Input.GetMouseButton(1))
                ModelLoader.Model.transform.Translate(0, mouseY * mouseFactor, 0);
            // Left mouse button pressed => move object.
            else if (Input.GetMouseButton(0))
                ModelLoader.Model.transform.Translate(mouseX * mouseFactor, 0, mouseY * mouseFactor, Space.World);
            // X pressed => rotate object around X-axis.
            else if (Input.GetKey("x"))
            {
                ModelLoader.Model.transform.Rotate(-mouseY, 0, 0);
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[0];
            }
            // Y pressed => rotate object around Y-axis.
            else if (Input.GetKey("y"))
            {
                ModelLoader.Model.transform.Rotate(0, -mouseX, 0);
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[1];
            }
            // Z pressed => rotate object around Z-axis.
            else if (Input.GetKey("z"))
            {
                ModelLoader.Model.transform.Rotate(0, 0, mouseX);
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[2];
            }
            else
                rotationImage.gameObject.SetActive(false);

            // Mouse wheel => scale object.
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) ModelLoader.Model.transform.localScale *= 1.1f;
            if (Input.GetAxis("Mouse ScrollWheel") < 0f) ModelLoader.Model.transform.localScale *= 0.9f;
        }
    }
}