using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualVitrine.MainScene;

namespace VirtualVitrine
{
    public class SceneSwitcher : MonoBehaviour
    {
        private static SceneSwitcher instance;

        #region Serialized Fields

        [SerializeField] private GameObject loadingScreen;

        #endregion

        public static bool InMainScene => !InMenu;
        public static bool InMenu => SceneManager.GetActiveScene().name == "Menu";

        #region Event Functions

        private void Awake()
        {
            instance = this;
        }

        #endregion

        public static void Quit()
        {
            Application.Quit();
        }

        public static void ToggleMenu()
        {
            // Show loading screen if model is going to load.
            if (InMenu && ModelLoader.Model == null)
                instance.loadingScreen.SetActive(true);
            SceneManager.LoadSceneAsync(InMainScene ? "Menu" : MyPrefs.MainScene);
        }

        public static void SwitchDifferentMain()
        {
            var mainScene = Extensions.ParseEnum<MainScenes>(MyPrefs.MainScene);
            MyPrefs.MainScene = mainScene.Next().ToString();
            SceneManager.LoadScene(MyPrefs.MainScene);
        }

        public enum MainScenes
        {
            MainRoom,
            MainLines
        }
    }
}