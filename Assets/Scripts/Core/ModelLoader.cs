using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dummiesman;
using JetBrains.Annotations;
using UnityEngine;
using VirtualVitrine.FaceTracking.Transform;

namespace VirtualVitrine
{
    public class ModelLoader : MonoBehaviour
    {
        private static ModelLoader instance;

        public static GameObject Model { get; private set; }

        #region Event Functions

        private void Awake()
        {
            // Singleton stuff so that model stays loaded between scenes.
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            LoadObject();
        }

        #endregion


        public static void ResetTransform()
        {
            if (Model == null)
                return;

            Transform objTransform = Model.transform;
            objTransform.localRotation = Quaternion.identity;
            objTransform.localPosition = Vector3.zero;

            // Set height to 90% of screen height.
            float currentHeight = GetObjectBounds(Model).size.y;
            float targetHeight = Projection.ScreenHeight * 0.9f;
            objTransform.localScale = targetHeight * objTransform.localScale / currentHeight;

            // 0 is the middle of the screen, move the object half the screen lower.
            objTransform.Translate(new Vector3(0, -(Projection.ScreenHeight / 2), -2));
        }


        private void LoadObject()
        {
            // No model was chosen or model is already loaded.
            if (MyPrefs.ModelPath == "" || Model != null)
                return;

            // Model loading for the first time.
            string mtlFilePath = CheckMtlFile();
            Model = new OBJLoader().Load(MyPrefs.ModelPath, mtlFilePath);
            foreach (Transform child in Model.transform)
                child.gameObject.layer = 3;
            Model.transform.parent = instance.transform;
            ResetTransform();
            print("Loaded new model");
        }

        /// <summary>
        ///     Checks if there's a .mtl file alongside the .obj file.
        /// </summary>
        /// <returns>Path of the .mtl file or null if file not found</returns>
        [CanBeNull]
        private static string CheckMtlFile()
        {
            string objPath = MyPrefs.ModelPath;
            string objDir = Path.GetDirectoryName(objPath);
            if (objDir == null)
                return null;

            // Get files ending with .mtl in the same directory as the .obj file.
            List<string> mtlFiles = Directory
                .GetFiles(objDir)
                .Where(file => file.EndsWith(".mtl"))
                .ToList();

            // Return the first .mtl file found or null if no file found.
            return mtlFiles.Count == 0 ? null : mtlFiles.First();
        }

        /// <summary>
        ///     Gets the bounds of all the children of the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Encapsulated bounds of children.</returns>
        private static Bounds GetObjectBounds(GameObject obj)
        {
            // Get meshes of all children.
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();

            // Encapsulate bounds of all meshes.
            var bounds = new Bounds(instance.transform.position, Vector3.zero);
            foreach (MeshRenderer meshRenderer in meshRenderers)
                bounds.Encapsulate(meshRenderer.bounds);

            return bounds;
        }
    }
}