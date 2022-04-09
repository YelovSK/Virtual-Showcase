using System.IO;
using System.Linq;
using UnityEngine;
using Dummiesman;
using JetBrains.Annotations;
using VirtualVitrine.FaceTracking.Transform;

namespace VirtualVitrine
{
    public class ModelLoader : MonoBehaviour
    {
        #region Public Fields
        public static GameObject Model { get; private set; }
        #endregion
        
        #region Private Fields
        private static ModelLoader _instance;
        #endregion
        
        #region Public Methods
        public static void ResetTransform()
        {
            if (Model == null)
                return;

            var objTransform = Model.transform;
            objTransform.localRotation = Quaternion.identity;
            objTransform.localPosition = Vector3.zero;
            
            // set height to 90% of screen height
            var currentHeight = GetObjectBounds(Model).size.y;
            var targetHeight = FindObjectOfType<AsymFrustum>().ScreenHeight * 0.9f;
            objTransform.localScale = targetHeight * objTransform.localScale / currentHeight;

            // 0 is the middle of the screen, move the object half the screen lower
            objTransform.Translate(new Vector3(0, -(targetHeight / 2), 0));
        }
        #endregion
        
        #region Unity Methods

        private void Awake()
        {
            if (_instance == null)
            {    
                _instance = this; // in first scene, make us the singleton
                DontDestroyOnLoad(gameObject);  // keep model loaded between scene switches
            }
            else if (_instance != this)
                Destroy(gameObject); // on reload, singleton already set, so destroy duplicate

            LoadObject();
        }
        #endregion

        #region Private Methods
        private void LoadObject()
        {
            // no model was chosen
            if (MyPrefs.ModelPath == "")
                return;

            // model already loaded
            if (Model != null) return;
            
            // model loading for the first time
            var mtlFilePath = CheckMtlFile();
            Model = new OBJLoader().Load(MyPrefs.ModelPath, mtlFilePath);
            Model.transform.parent = _instance.transform;
            ResetTransform();
            print("Loaded new model");
        }

        /// <summary>
        /// Checks if there's a .mtl file alongside the .obj file.
        /// </summary>
        /// <returns>Path of the .mtl file or null if file not found</returns>
        [CanBeNull]
        private static string CheckMtlFile()
        {
            var objPath = MyPrefs.ModelPath;
            var objDir = Path.GetDirectoryName(objPath);
            if (objDir == null)
                return null;
            
            // get files ending with .mtl in the same directory as the .obj file
            var mtlFiles = Directory
                .GetFiles(objDir)
                .Where(file => file.EndsWith(".mtl"))
                .ToList();
            
            // return the first .mtl file found or null if no file found
            return mtlFiles.Count == 0 ? null : mtlFiles.First();
        }

        /// <summary>
        /// Gets the bounds of all the children of the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Encapsulated bounds of children.</returns>
        private static Bounds GetObjectBounds(GameObject obj)
        {
            // get meshes of all children
            var meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
            
            // encapsulate bounds of all meshes
            var bounds = new Bounds(_instance.transform.position, Vector3.zero);
            foreach (var meshRenderer in meshRenderers)
                bounds.Encapsulate(meshRenderer.bounds);

            return bounds;
        }
        #endregion
    }
}