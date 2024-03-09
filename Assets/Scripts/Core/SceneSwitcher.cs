using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualShowcase.Enums;
using VirtualShowcase.MainScene;

namespace VirtualShowcase.Utilities
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
            if (InMenu && ModelLoader.Instance.ModelsInfo.IsEmpty() && MyPrefs.ModelPaths.Any()) instance.loadingScreen.SetActive(true);

            // Cursor visible in menu, invisible in main scene. Set before switching scene, thus reverse.
            Cursor.visible = InMainScene;

            SceneManager.LoadScene(InMainScene ? "Menu" : MyPrefs.MainScene);
        }

        public static void SwitchDifferentMain()
        {
            var mainScene = Extensions.ParseEnum<eMainScenes>(MyPrefs.MainScene);
            MyPrefs.MainScene = mainScene.Next().ToString();
            SceneManager.LoadScene(MyPrefs.MainScene);
        }
    }
}