using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualVitrine.Core
{
    public class SceneSwitcher : MonoBehaviour
    {
        #region Public Methods
        public void Quit()
        {
            Application.Quit();
        }
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