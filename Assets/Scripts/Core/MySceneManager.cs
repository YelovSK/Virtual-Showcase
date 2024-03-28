using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualShowcase.Enums;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Core
{
    public class MySceneManager : MonoSingleton<MySceneManager>
    {
        public const string MENU_SCENE_NAME = "Menu";
        public const string ROOM_SCENE_NAME = "MainRoom";
        public const string LINES_SCENE_NAME = "MainLines";
        public const string WHITE_SCENE_NAME = "MainWhite";
        
        public bool IsInMainScene => !IsInMenu;
        public bool IsInMenu => SceneManager.GetActiveScene().name == MENU_SCENE_NAME;

        #region Event Functions

        private void Start()
        {
            Events.MenuSceneOpened?.Invoke(gameObject);
        }

        #endregion

        public void ToggleMenu()
        {
            // Cursor visible in menu, invisible in main scene. Set before switching scene, thus reverse.
            Cursor.visible = IsInMainScene;

            if (IsInMenu)
            {
                Events.MenuSceneOpened?.Invoke(gameObject);
            }
            else
            {
                Events.MenuSceneOpened?.Invoke(gameObject);
            }
            
            SceneManager.LoadScene(IsInMainScene ? MENU_SCENE_NAME : MyPrefs.MainScene);
        }

        public void SwitchToNextMainScene()
        {
            var mainScene = Extensions.ParseEnum<eMainScenes>(MyPrefs.MainScene);
            MyPrefs.MainScene = mainScene.Next().ToString();
            SceneManager.LoadScene(MyPrefs.MainScene);
        }
    }
}