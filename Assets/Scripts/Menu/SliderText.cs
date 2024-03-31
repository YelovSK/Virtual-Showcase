using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualShowcase.Menu
{
    public class SliderText : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private TMP_Text valueText;

        [SerializeField]
        private string prefix;

        [SerializeField]
        private string suffix;

        #endregion

        #region Event Functions

        private void Start()
        {
            prefix ??= string.Empty;
            suffix ??= string.Empty;
        }

        #endregion

        public void UpdateValue(Slider sender)
        {
            valueText.text = $"{prefix}{sender.value:0.###}{suffix}";
        }
    }
}