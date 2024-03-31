using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Core;
using VirtualShowcase.Enums;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu.Options
{
    public class GraphicsOptions : MonoBehaviour, IDisabledOnAwake
    {
        #region Serialized Fields

        [SerializeField]
        private TMP_Dropdown resolutionDropdown;

        [SerializeField]
        private TMP_Dropdown qualityDropdown;

        [SerializeField]
        private TMP_Dropdown screenModeDropdown;

        [SerializeField]
        private Slider fpsLimitSlider;

        [SerializeField]
        private Toggle vsyncToggle;

        #endregion

        private List<Resolution> _resolutions;

        #region Event Functions

        private void Awake()
        {
            _resolutions = Screen.resolutions
                .OrderByDescending(r => r.width)
                .ThenByDescending(r => r.refreshRateRatio)
                .ToList();

            AddResolutionOptions();
            AddScreenModeOptions();

            SetDefaults();
        }

        #endregion

        public void Initialize()
        {
            throw new NotImplementedException();
        }

        private void SetDefaults()
        {
            // Resolution
            int ix = resolutionDropdown.options.FindIndex(option => option.text == MyPrefs.Resolution.ToString());
            resolutionDropdown.value = ix == -1 ? 0 : ix;

            // Quality
            qualityDropdown.value = (int)MyPrefs.Quality;

            // FPS Limit
            fpsLimitSlider.value = MyPrefs.FpsLimit;

            // Vsync
            vsyncToggle.isOn = MyPrefs.Vsync;

            // Screen mode
            screenModeDropdown.value = (int)MyPrefs.ScreenMode;
        }

        private void AddScreenModeOptions()
        {
            // Need to be ordered the same as FullScreenMode enum.
            var options = new List<TMP_Dropdown.OptionData>
            {
                new("Exclusive Fullscreen"),
                new("Borderless Window"),
                new("Maximized Window"),
                new("Windowed"),
            };

            screenModeDropdown.AddOptions(options);
        }

        private void AddResolutionOptions()
        {
            List<TMP_Dropdown.OptionData> options = _resolutions
                .Select(res => new TMP_Dropdown.OptionData(res.ToString()))
                .ToList();

            resolutionDropdown.AddOptions(options);
        }

        public void SetResolution(TMP_Dropdown sender)
        {
            Resolution chosenRes = _resolutions[sender.value];
            Screen.SetResolution(chosenRes.width, chosenRes.height, MyPrefs.ScreenMode,
                chosenRes.refreshRateRatio);
            MyPrefs.Resolution = chosenRes;
        }

        public void SetScreenMode(TMP_Dropdown sender)
        {
            var mode = (FullScreenMode)sender.value;
            MyPrefs.ScreenMode = mode;
            Screen.fullScreenMode = mode;
        }

        public void SetQuality(TMP_Dropdown sender)
        {
            QualitySettings.SetQualityLevel(sender.value, true);
            if (MyPrefs.Quality == (GraphicsQuality)sender.value)
            {
                return;
            }

            MyPrefs.Quality = (GraphicsQuality)sender.value;

            // If quality changed, destroy model to load it again with updated max triangle count.
            MyEvents.ModelsRemoveRequest?.Invoke(gameObject);
        }

        public void SetFpsLimit(Slider sender)
        {
            Application.targetFrameRate = (int)sender.value;
            MyPrefs.FpsLimit = Application.targetFrameRate;
        }

        public void SetVsync(Toggle sender)
        {
            QualitySettings.vSyncCount = sender.isOn ? 1 : 0;
            MyPrefs.Vsync = sender.isOn;
        }
    }
}