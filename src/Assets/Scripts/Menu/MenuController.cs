using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using VirtualShowcase.Core;

namespace VirtualShowcase.Menu
{
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class MenuController : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        private Page initialPage;

        [SerializeField]
        private List<GameObject> initialEnabled;

        [SerializeField]
        private List<GameObject> initialDisabled;

        [SerializeField]
        private LoadingScreen loadingScreen;

        #endregion

        private readonly Stack<Page> _pageStack = new();
        private InputActions _inputActions;
        private Canvas _rootCanvas;

        #region Event Functions

        private void Awake()
        {
            _inputActions = new InputActions();
            _inputActions.Menu.Enable();
            _rootCanvas = GetComponent<Canvas>();

            MyEvents.ModelsLoadingStart.AddListener(_ => { loadingScreen.ShowProgress(LoadingType.Models, 0f); });
            MyEvents.ModelLoaded.AddListener((_, tuple) =>
            {
                (int loadedCount, int totalCount) = tuple;
                loadingScreen.ShowProgress(LoadingType.Models, (float)loadedCount / totalCount);
            });
            MyEvents.ModelsLoadingEnd.AddListener(_ => { loadingScreen.Finish(); });
            MyEvents.SceneLoadProgress.AddListener((_, progress) => { loadingScreen.ShowProgress(LoadingType.Scene, progress); });
            _inputActions.Menu.Back.performed += Back;

            foreach (GameObject go in initialEnabled)
            {
                go.SetActive(true);
            }

            foreach (GameObject go in initialDisabled)
            {
                // I need the Awake methods to run XD
                go.SetActive(true);
                go.SetActive(false);
            }
        }

        private void Start()
        {
            if (initialPage != null)
            {
                PushPage(initialPage);
            }
        }

        private void OnDestroy()
        {
            _inputActions.Menu.Back.performed -= Back;
        }

        #endregion

        private void Back(InputAction.CallbackContext context)
        {
            if (_pageStack.Count > 1)
            {
                PopPage();
            }
            else
            {
                MySceneManager.Instance.LoadShowcaseScene();
            }
        }

        private void OnCancel()
        {
            if (_rootCanvas.enabled && _rootCanvas.gameObject.activeInHierarchy && _pageStack.Count > 0)
            {
                PopPage();
            }
        }

        public void PushPage(Page page)
        {
            page.gameObject.SetActive(true);
            page.Enter();

            if (_pageStack.Count > 0)
            {
                Page currentPage = _pageStack.Peek();

                if (currentPage.exitOnNewPagePush)
                {
                    currentPage.Exit();
                }
            }

            _pageStack.Push(page);
        }

        public void PopPage()
        {
            if (_pageStack.Count <= 1)
            {
                Debug.LogWarning("Trying to pop a page but only 1 page remains in the stack!");
                return;
            }

            Page page = _pageStack.Pop();
            page.Exit();

            Page newCurrentPage = _pageStack.Peek();
            if (!newCurrentPage.gameObject.activeSelf)
            {
                newCurrentPage.gameObject.SetActive(true);
            }

            if (newCurrentPage.exitOnNewPagePush)
            {
                newCurrentPage.Enter();
            }
        }

        // Stupid. Scene manager does not get destroyed on load, so cannot reference it in the buttons.
        public void Quit()
        {
            Application.Quit();
        }

        public void LoadShowcaseScene()
        {
            MySceneManager.Instance.LoadShowcaseScene();
        }
    }
}