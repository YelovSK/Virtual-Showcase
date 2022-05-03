using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualVitrine
{
    public class SceneSwitcher : MonoBehaviour
    {
        public static bool InMainScene => SceneManager.GetActiveScene().name != "Menu";

        #region Event Functions

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                SwitchScene();
        }

        #endregion

        public static void Quit()
        {
            Application.Quit();
        }

        public static void SwitchScene()
        {
            SceneManager.LoadScene(InMainScene ? "Menu" : MyPrefs.MainScene);
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