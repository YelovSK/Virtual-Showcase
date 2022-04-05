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
        }
    }
}