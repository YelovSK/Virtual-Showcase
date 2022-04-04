using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualVitrine
{
    public class SceneSwitcher : MonoBehaviour
    {
        #region Public Methods
        public void Quit() => Application.Quit();

        public void SwitchMain() => SceneManager.LoadScene("Main");
        #endregion
        
        #region Unity Methods
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name == "Main" ? "Menu" : "Main");
            }
        }
        #endregion
    }
}