using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using VirtualShowcase.Enums;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Core
{
    public class SceneSwitcher : MonoSingleton<SceneSwitcher>
    {
        public bool InMainScene => !InMenu;
        public bool InMenu => SceneManager.GetActiveScene().name == "Menu";

        public UnityEvent OnMenuOpened = new();
        public UnityEvent OnMainOpened = new();

        #region Event Functions

        private void Start()
        {
            OnMenuOpened.Invoke();
        }

        #endregion

        public void ToggleMenu()
        {
            // Cursor visible in menu, invisible in main scene. Set before switching scene, thus reverse.
            Cursor.visible = InMainScene;

            if (InMenu)
            {
                OnMainOpened.Invoke();
            }
            else
            {
                OnMenuOpened.Invoke();
            }
            
            SceneManager.LoadScene(InMainScene ? "Menu" : MyPrefs.MainScene);
        }

        public void SwitchDifferentMain()
        {
            var mainScene = Extensions.ParseEnum<eMainScenes>(MyPrefs.MainScene);
            MyPrefs.MainScene = mainScene.Next().ToString();
            SceneManager.LoadScene(MyPrefs.MainScene);
        }
    }
}