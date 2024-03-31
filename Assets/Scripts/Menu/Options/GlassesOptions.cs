using UnityEngine;
using UnityEngine.UI;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu.Options
{
    public class GlassesOptions : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private Slider hueSlider;

        [SerializeField]
        private Image hueSliderImage;

        [SerializeField]
        private Slider hueThresholdSlider;

        [SerializeField]
        private Toggle checkGlassesToggle;

        #endregion

        #region Event Functions

        private void Awake()
        {
            SetDefaults();
        }

        #endregion

        private void SetDefaults()
        {
            // Hue
            hueSlider.value = MyPrefs.Hue;

            // Hue Threshold
            hueThresholdSlider.value = MyPrefs.HueThreshold;

            // Check Glasses
            checkGlassesToggle.isOn = MyPrefs.GlassesCheck;
        }

        public void SetHue(Slider sender)
        {
            MyPrefs.Hue = (int)sender.value;

            Color rgb = Color.HSVToRGB(sender.value / 360, 1, 1);
            hueSliderImage.color = rgb;
        }

        public void SetHueThreshold(Slider sender)
        {
            MyPrefs.HueThreshold = (int)sender.value;
        }

        public void SetCheckGlasses(Toggle sender)
        {
            MyPrefs.GlassesCheck = sender.isOn;
        }
    }
}