using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dummiesman;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityMeshSimplifier;
using VirtualVitrine.FaceTracking.Transform;

namespace VirtualVitrine.MainScene
{
    public class ModelLoader : MonoBehaviour
    {
        private static ModelLoader instance;

        #region Serialized Fields

        // UI text for displaying mesh simplification status.
        [SerializeField] private TMP_Text statusText;

        #endregion

        private Dictionary<MeshFilter, bool> runningTasks; // [MeshFilter, isRunning]

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
            else if (instance != this) Destroy(gameObject);

            LoadObject();
        }

        #endregion


        public static void ResetTransform()
        {
            if (Model == null)
                return;

            Model.transform.parent = instance.transform;
            Transform objTransform = Model.transform;
            objTransform.localRotation = Quaternion.identity;
            objTransform.localPosition = Vector3.zero;

            // Set height to 90% of screen height.
            Bounds bounds = GetObjectBounds(Model);
            float currentHeight = bounds.size.y;
            float targetHeight = Projection.ScreenHeight * 0.9f;
            objTransform.localScale = targetHeight * objTransform.localScale / currentHeight;

            // Get new bounds (scale was changed).
            bounds = GetObjectBounds(Model);

            // Set position to be the middle of the parent (center of the screen).
            objTransform.position = objTransform.parent.position;

            // Lower the object by its center.
            // Eg if the set center is at the feet of a person, we can lower it by the real center.
            objTransform.Translate(new Vector3(0, -bounds.center.y, -2));
        }


        private void LoadObject()
        {
            // No model was chosen or model is already loaded.
            if (MyPrefs.ModelPath == "" || Model != null)
                return;

            // Model loading for the first time.
            string mtlFilePath = CheckMtlFile();
            Model = new OBJLoader().Load(MyPrefs.ModelPath, mtlFilePath);

            // Simplify mesh.
            SimplifyObject(Model);

            // Change shader of material for URP compatibility.
            MaterialsToURP(Model);

            // Set layers.
            foreach (Transform child in Model.transform)
                child.gameObject.layer = 3;

            ResetTransform();
        }

        private static void MaterialsToURP(GameObject model)
        {
            Material[] materials = model.GetComponentInChildren<Renderer>().sharedMaterials;
            foreach (Material mat in materials)
            {
                Texture tex = mat.mainTexture;
                mat.shader = Shader.Find("Universal Render Pipeline/Lit");
                mat.mainTexture = tex;
                // Metallic to specular.
                mat.EnableKeyword("_SPECULAR_SETUP");
            }
        }

        private void SimplifyObject(GameObject obj)
        {
            const int maxVertexCount = 200_000;
            MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();

            // Vertex count of all meshes.
            int vertexCount = meshFilters.Sum(x => x.mesh.vertexCount);

            // Quality is the percentage of vertices to keep. In our case we want maxVertexCount vertices.
            float quality = (float) maxVertexCount / vertexCount;
            int percentReduction = Mathf.RoundToInt((1.0f - quality) * 100);
            statusText.text = $"Simplifying mesh ({percentReduction}% reduction)...";

            // Simplify every child mesh of the object.
            runningTasks = meshFilters.ToDictionary(x => x, _ => true);
            foreach (MeshFilter meshFilter in meshFilters)
                SimplifyMeshFilter(meshFilter, quality);
        }

        private async void SimplifyMeshFilter(MeshFilter meshFilter, float quality)
        {
            Mesh sourceMesh = meshFilter.sharedMesh;
            if (sourceMesh == null)
                return;

            // Create mesh simplifier with the given mesh.
            var meshSimplifier = new MeshSimplifier(sourceMesh);

            // Simplify mesh with the given quality.
            // Runs asynchronously.
            await Task.Run(() => meshSimplifier.SimplifyMesh(quality));

            // Get simplified mesh.
            var finalMesh = meshSimplifier.ToMesh();

            // Optimize mesh.
            finalMesh.Optimize();
            finalMesh.name = "Optimized mesh";

            // Set the simplified mesh to the mesh filter.
            meshFilter.sharedMesh = finalMesh;

            // If all tasks are done, update status text.
            runningTasks[meshFilter] = false;
            if (runningTasks.Values.All(x => x == false))
                statusText.text = "";
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