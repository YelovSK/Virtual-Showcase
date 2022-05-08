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
            // When scene changes in the editor, enable/disable depending on the scene.
            if (EditorSceneManager.GetActiveScene().name == "Menu")
                Enable();
            EditorSceneManager.sceneOpened += CheckScene;
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

        private static void OnSceneGUI(SceneView sceneview)
        {
            Handles.BeginGUI();
            GUILayout.BeginHorizontal("box");

            if (GUILayout.Button("All"))
            {
                main.SetActive(true);
                options.SetActive(true);
                rebind.SetActive(true);
            }

            if (GUILayout.Button("Main"))
            {
                main.SetActive(true);
                options.SetActive(false);
                rebind.SetActive(false);
            }

            if (GUILayout.Button("Options"))
            {
                main.SetActive(false);
                options.SetActive(true);
                rebind.SetActive(false);
            }

            if (GUILayout.Button("Rebind"))
            {
                main.SetActive(false);
                options.SetActive(false);
                rebind.SetActive(true);
            }

            GUILayout.EndHorizontal();
            Handles.EndGUI();
        }
    }
}