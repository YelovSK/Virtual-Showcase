using UnityEngine;
using UnityEngine.InputSystem;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Menu.Options
{
    public class BindsOptions : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private InputActionAsset inputActions;

        #endregion

        public void ResetBinds()
        {
            foreach (InputActionMap map in inputActions.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }

            MyPrefs.Rebinds = null;
        }
    }
}