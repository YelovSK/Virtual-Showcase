using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualVitrine
{
    public class SceneSwitcher : MonoBehaviour
    {
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

        public static void SwitchMain()
        {
            SceneManager.LoadScene("Main");
        }

        public static void SwitchScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name == "Main" ? "Menu" : "Main");
        }
    }
}