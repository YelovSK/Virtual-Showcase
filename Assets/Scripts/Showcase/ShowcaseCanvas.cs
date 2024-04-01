using UnityEngine;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Showcase
{
    public class ShowcaseCanvas : MonoBehaviour
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