using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using VirtualShowcase.MainScene;

namespace VirtualShowcase
{
    public class InputHandler : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Input")]
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Scene objects")]
        [SerializeField] private ShowcaseInitializer showcaseInitializer;
        [SerializeField] private CalibrationManager calibrationManager;

        [Header("Rotation images")]
        [SerializeField] private Image rotationImage;
        [SerializeField] private Sprite[] rotationImages;

        #endregion

        #region Event Functions

        private void Awake()
        {
            CalibrationManager.NextStateKeybind = playerInput.actions["Next calibration"].GetBindingDisplayString();
        }

        private void Update()
        {
            // Toggle menu/main scene.
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                SceneSwitcher.ToggleMenu();

            if (SceneSwitcher.InMenu)
                return;

            if (playerInput.actions["Main scene switch"].WasPressedThisFrame())
                SceneSwitcher.SwitchDifferentMain();

            // Toggle webcam preview (ignore if calibration is active).
            if (playerInput.actions["Preview toggle"].WasPressedThisFrame() && !CalibrationManager.Enabled)
                showcaseInitializer.SetCamPreview(true);

            // Toggle stereo/mono.
            if (playerInput.actions["Stereo toggle"].WasPressedThisFrame())
                showcaseInitializer.SetStereo(true);

            // Toggle calibration UI.
            if (playerInput.actions["Calibration toggle"].WasPressedThisFrame())
                calibrationManager.ToggleCalibrationUI();

            // Go to next calibration state.
            if (playerInput.actions["Next calibration"].WasPressedThisFrame())
                calibrationManager.SetNextState();

            // Reset loaded object position.
            if (playerInput.actions["Reset transform"].WasPressedThisFrame())
                ModelLoader.Instance.ResetTransform();

            if (playerInput.actions["Next model"].WasPressedThisFrame())
                ModelLoader.Instance.SwitchActiveModel();
            else if (playerInput.actions["Previous model"].WasPressedThisFrame()) ModelLoader.Instance.SwitchActiveModel(false);

            // Loaded object gets controlled with mouse input.
            if (!CalibrationManager.Enabled)
                HandleMouseInput();
        }

        #endregion

        public void ResetAllBindings()
        {
            foreach (InputActionMap map in inputActions.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }

            PlayerPrefs.DeleteKey("rebinds");
        }

        private void HandleMouseInput()
        {
            if (ModelLoader.Instance.ModelsInfo.Any() == false)
                return;

            const float move_sens = 0.015f;
            const float rotation_sens = 0.075f;
            float mouseX = Mouse.current.delta.ReadValue().x;
            float mouseY = Mouse.current.delta.ReadValue().y;

            // Left mouse button pressed => move object.
            if (playerInput.actions["Move on ground"].IsPressed())
                ModelLoader.Instance.Models.ForEach(model => model.transform.Translate(mouseX * move_sens, 0, mouseY * move_sens, Space.World));

            // Right mouse button pressed => lower/raise object.
            else if (playerInput.actions["Move vertically"].IsPressed())
                ModelLoader.Instance.Models.ForEach(model => model.transform.Translate(0, mouseY * move_sens, 0, Space.World));

            // X pressed => rotate object around X-axis.
            else if (playerInput.actions["Rotate X"].IsPressed())
            {
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(mouseY * rotation_sens, 0, 0, Space.World));
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[0];
            }

            // Y pressed => rotate object around Y-axis.
            else if (playerInput.actions["Rotate Y"].IsPressed())
            {
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(0, -mouseX * rotation_sens, 0, Space.World));
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[1];
            }

            // Z pressed => rotate object around Z-axis.
            else if (playerInput.actions["Rotate Z"].IsPressed())
            {
                ModelLoader.Instance.Models.ForEach(model => model.transform.Rotate(0, 0, -mouseX * rotation_sens, Space.World));
                rotationImage.gameObject.SetActive(true);
                rotationImage.sprite = rotationImages[2];
            }
            else
                rotationImage.gameObject.SetActive(false);

            // Mouse wheel => scale object.
            float scroll = Mouse.current.scroll.ReadValue().y;
            if (scroll > 0f) ModelLoader.Instance.Models.ForEach(model => model.transform.localScale *= 1.1f);

            if (scroll < 0f) ModelLoader.Instance.Models.ForEach(model => model.transform.localScale *= 0.9f);
        }
    }
}