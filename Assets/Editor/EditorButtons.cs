using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VirtualVitrine.Menu
{
    [InitializeOnLoad]
    [ExecuteInEditMode]
    // Class is for selecting which menus to show in the editor because they overlap due to animations.
    public class EditorButtons : MonoBehaviour
    {
        private static GameObject main;
        private static GameObject options;
        private static GameObject rebind;

        static EditorButtons()
        {
            SetPlayModeStartScene();

            // When scene changes in the editor, enable/disable depending on the scene.
            // delayCall because sceneOpened doesn't get called on startup.
            EditorApplication.delayCall += () =>
            {
                if (SceneManager.GetActiveScene().name == "Menu")
                    Enable();
            };

            EditorSceneManager.sceneOpened += CheckScene;
        }

        private static void SetPlayModeStartScene()
        {
            const string scenePath = "Assets/Scenes/Menu.unity";
            var myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (myWantedStartScene != null)
                EditorSceneManager.playModeStartScene = myWantedStartScene;
            else
                Debug.Log("Could not find Scene " + scenePath);
        }

        private static void CheckScene(Scene scene, OpenSceneMode mode)
        {
            bool inMenu = scene.name == "Menu";
            if (inMenu)
                Enable();
            else
                Disable();
        }

        private static void Enable()
        {
            GameObject canvas = GameObject.Find("Canvas");
            main = canvas.transform.Find("Main menu").gameObject;
            options = canvas.transform.Find("Options menu").gameObject;
            rebind = canvas.transform.Find("Rebind menu").gameObject;

            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void Disable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private static void SetActive(bool mainActive, bool optionsActive, bool rebindActive)
        {
            main.SetActive(mainActive);
            options.SetActive(optionsActive);
            rebind.SetActive(rebindActive);
            // If all selected, don't expand.
            if (mainActive && optionsActive && rebindActive)
            {
                mainActive = false;
                optionsActive = false;
                rebindActive = false;
            }

            SceneHierarchyUtility.SetExpanded(main, mainActive);
            SceneHierarchyUtility.SetExpanded(options, optionsActive);
            SceneHierarchyUtility.SetExpanded(rebind, rebindActive);
        }

        private static void OnSceneGUI(SceneView sceneview)
        {
            if (SceneManager.GetActiveScene().name != "Menu")
                Disable();

            Handles.BeginGUI();
            GUILayout.BeginHorizontal("box");

            if (GUILayout.Button("All")) SetActive(true, true, true);

            if (GUILayout.Button("Main")) SetActive(true, false, false);

            if (GUILayout.Button("Options")) SetActive(false, true, false);

            if (GUILayout.Button("Rebind")) SetActive(false, false, true);

            GUILayout.EndHorizontal();
            Handles.EndGUI();
        }
    }

    /// <summary>
    ///     https://github.com/sandolkakos/unity-utilities/blob/main/Scripts/Editor/SceneHierarchyUtility.cs
    ///     Editor functionalities from internal SceneHierarchyWindow and SceneHierarchy classes.
    ///     For that we are using reflection.
    /// </summary>
    public static class SceneHierarchyUtility
    {
        /// <summary>
        ///     Check if the target GameObject is expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static bool IsExpanded(GameObject go)
        {
            return GetExpandedGameObjects().Contains(go);
        }

        /// <summary>
        ///     Get a list of all GameObjects which are expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static List<GameObject> GetExpandedGameObjects()
        {
            object sceneHierarchy = GetSceneHierarchy();

            MethodInfo methodInfo = sceneHierarchy
                .GetType()
                .GetMethod("GetExpandedGameObjects");

            object result = methodInfo.Invoke(sceneHierarchy, new object[0]);

            return (List<GameObject>) result;
        }

        /// <summary>
        ///     Set the target GameObject as expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static void SetExpanded(GameObject go, bool expand)
        {
            object sceneHierarchy = GetSceneHierarchy();

            MethodInfo methodInfo = sceneHierarchy
                .GetType()
                .GetMethod("ExpandTreeViewItem", BindingFlags.NonPublic | BindingFlags.Instance);

            methodInfo.Invoke(sceneHierarchy, new object[] {go.GetInstanceID(), expand});
        }

        /// <summary>
        ///     Set the target GameObject and all children as expanded (aka unfolded) in the Hierarchy view.
        /// </summary>
        public static void SetExpandedRecursive(GameObject go, bool expand)
        {
            object sceneHierarchy = GetSceneHierarchy();

            MethodInfo methodInfo = sceneHierarchy
                .GetType()
                .GetMethod("SetExpandedRecursive", BindingFlags.Public | BindingFlags.Instance);

            methodInfo.Invoke(sceneHierarchy, new object[] {go.GetInstanceID(), expand});
        }

        private static object GetSceneHierarchy()
        {
            EditorWindow window = GetHierarchyWindow();

            object sceneHierarchy = typeof(EditorWindow).Assembly
                .GetType("UnityEditor.SceneHierarchyWindow")
                .GetProperty("sceneHierarchy")
                ?.GetValue(window);

            return sceneHierarchy;
        }

        private static EditorWindow GetHierarchyWindow()
        {
            // For it to open, so that it the current focused window.
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            return EditorWindow.focusedWindow;
        }
    }
}