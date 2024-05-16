using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    #region Serialized Fields

    public InputActionAsset actions;

    #endregion

    #region Event Functions

    // TODO: this mf is not persistent cuz i'm using the generated c# InputActions.cs class
    public void OnEnable()
    {
        string rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
        {
            actions.LoadBindingOverridesFromJson(rebinds);
        }
    }

    public void OnDisable()
    {
        string rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }

    #endregion
}