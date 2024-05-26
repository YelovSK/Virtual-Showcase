using UnityEngine;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Showcase
{
    public class CalibrationCanvas : MonoBehaviour
    {
        #region Event Functions

        private void Awake()
        {
            // Stupid.
            gameObject.SetActive(MyPrefs.PreviewOn);
        }

        #endregion
    }
}