using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualShowcase.Enums;
using VirtualShowcase.Showcase;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.Core
{
    public class MySceneManager : MonoSingleton<MySceneManager>
    {
        public const string MENU_SCENE_NAME = "Menu";
        public const string ROOM_SCENE_NAME = "MainRoom";
        public const string LINES_SCENE_NAME = "MainLines";
        public const string WHITE_SCENE_NAME = "MainWhite";

        public bool IsInMenu => SceneManager.GetActiveScene().name == MENU_SCENE_NAME;
        public bool IsInMainScene => !IsInMenu;

        #region Event Functions

        private void Start()
        {
            MyEvents.MenuSceneOpened?.Invoke(gameObject);
        }

        #endregion

        public void ToggleMenu()
        {
            // Set before switching scene, thus reverse.
            Cursor.visible = IsInMainScene;

            if (IsInMenu)
            {
                LoadShowcaseScene();
            }
            else
            {
                LoadMenuScene();
            }
        }

        public async void LoadShowcaseScene()
        {
            await ModelLoader.Instance.LoadModels();
            MyEvents.MainSceneOpened?.Invoke(gameObject);
            StartCoroutine(LoadSceneCoroutine(MyPrefs.MainScene));
        }

        public void LoadMenuScene()
        {
            MyEvents.MenuSceneOpened?.Invoke(gameObject);
            SceneManager.LoadScene(MENU_SCENE_NAME);
        }

        public void LoadNextShowcaseScene()
        {
            var mainScene = Extensions.ParseEnum<MainScenes>(MyPrefs.MainScene);
            MyPrefs.MainScene = mainScene.Next().ToString();
            SceneManager.LoadScene(MyPrefs.MainScene);
        }

        public void LoadPreviousShowcaseScene()
        {
            var mainScene = Extensions.ParseEnum<MainScenes>(MyPrefs.MainScene);
            MyPrefs.MainScene = mainScene.Prev().ToString();
            SceneManager.LoadScene(MyPrefs.MainScene);
        }

        public void Quit()
        {
            Application.Quit();
        }

        private IEnumerator LoadSceneCoroutine(string scene)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

            do
            {
                MyEvents.SceneLoadProgress?.Invoke(gameObject, asyncLoad.progress);
                yield return null;
            } while (asyncLoad.progress < 1f);
        }
    }
}