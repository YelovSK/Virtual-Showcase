using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GLTFast;
using UnityEngine;
using UnityMeshSimplifier;
using VirtualShowcase.Common;
using VirtualShowcase.Enums;
using VirtualShowcase.FaceTracking.Transform;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.MainScene
{
    public class ModelInfo
    {
        public string Name => Path.GetFileNameWithoutExtension(FullPath);
        public string FullPath { get; set; }
        public GameObject Object { get; set; }
    }

    public class ModelLoader : MonoSingleton<ModelLoader>
    {
        public List<ModelInfo> ModelsInfo { get; } = new();
        public List<GameObject> Models => ModelsInfo.Select(x => x.Object).ToList();

        #region Event Functions

        private void Start()
        {
            GltfImportBase.SetDefaultDeferAgent(new UninterruptedDeferAgent());
        }

        #endregion

        public void ResetTransform()
        {
            if (ModelsInfo.IsEmpty()) return;

            // Reset position.
            foreach (GameObject model in Models)
            {
                model.transform.parent = Instance.transform;
                model.transform.localRotation = Quaternion.identity;
                model.transform.localPosition = Vector3.zero;
            }

            // Scale and center based on the first model.
            // We want to scale all of the models by the same amount,
            // because if we import the same model with small changes, it should be
            // treated equally for the models to overlap properly (if they were exported with the same position and scale).
            GameObject reference = ModelsInfo.First().Object;
            Bounds bounds = GetObjectBounds(reference);

            // Set height to 90% of screen height.
            float currentHeight = bounds.size.y;
            float targetHeight = Projection.ScreenHeight * 0.9f;
            reference.transform.localScale = targetHeight * reference.transform.localScale / currentHeight;

            // Set position to be the middle of the parent (center of the screen).
            reference.transform.position = reference.transform.parent.position;

            // Get new bounds (scale was changed).
            bounds = GetObjectBounds(reference);

            // Lower the object by its center.
            // E.g. if the set center is at the feet of a person, we can lower it by the real center.
            reference.transform.Translate(new Vector3(0, -bounds.center.y, -2));

            // Use the scale and position of the reference object for all other objects.
            foreach (GameObject model in Models.Skip(1))
            {
                model.transform.localScale = reference.transform.localScale;
                model.transform.position = reference.transform.position;
            }
        }

        public async Task LoadObjects()
        {
            var tasks = new List<Task>();

            foreach (string path in MyPrefs.ModelPaths)
            {
                // Check if it's already loaded (in the models list).
                if (ModelsInfo.Any(x => x.FullPath == path) || File.Exists(path) == false) continue;

                var gltf = new GltfImport();

                Task<Task> task = gltf.Load($"file://{path}").ContinueWith(
                    async t =>
                    {
                        if (t.Result == false) return;

                        var obj = new GameObject(path);
                        obj.transform.parent = gameObject.transform;
                        await gltf.InstantiateMainSceneAsync(obj.transform);

                        foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
                        {
                            child.gameObject.layer = Constants.LAYER_MODEL;
                        }

                        SimplifyObject(obj);

                        ModelsInfo.Add(new ModelInfo
                        {
                            FullPath = path,
                            Object = obj,
                        });
                    },
                    TaskScheduler.FromCurrentSynchronizationContext()
                );

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            // Only the first one visible.
            for (var i = 0; i < ModelsInfo.Count; i++)
            {
                ModelsInfo[i].Object.SetActive(i == 0);
            }

            if (tasks.Any()) ResetTransform();
        }

        public void DeleteModel(string path)
        {
            ModelInfo model = ModelsInfo.FirstOrDefault(x => x.FullPath == path);
            if (model is null) return;

            ModelsInfo.Remove(model);
            Destroy(model.Object);
        }

        public void DeleteModels()
        {
            foreach (ModelInfo model in ModelsInfo)
            {
                Destroy(model.Object);
            }

            ModelsInfo.Clear();
        }

        /// <param name="next">true for next, false for previous</param>
        public void SwitchActiveModel(bool next = true)
        {
            int activeIx = ModelsInfo.FindIndex(x => x.Object.activeSelf);
            if (activeIx == -1) return;

            ModelsInfo[activeIx].Object.SetActive(false);

            if (next)
                ModelsInfo[(activeIx + 1) % ModelsInfo.Count].Object.SetActive(true);
            else
                ModelsInfo[(activeIx - 1 + ModelsInfo.Count) % ModelsInfo.Count].Object.SetActive(true);
        }

        private void SimplifyObject(GameObject obj)
        {
            // Set max triangle count depending on the selected quality.
            int maxTriCount = (eGraphicsQuality) QualitySettings.GetQualityLevel() switch
            {
                eGraphicsQuality.Low    => 50_000,
                eGraphicsQuality.Medium => 100_000,
                eGraphicsQuality.High   => int.MaxValue,
                _                       => throw new ArgumentOutOfRangeException(),
            };

            // Sometimes the result is split into multiple meshes, combine them into one.
            // Simplifying multiple meshes can lead to gaps,
            // but simplifying this combined mesh also leads to issues,
            // probably because it uses one material per submesh.
            // Pick your poison I guess.
            MeshFilter mesh = obj.MeshCombine(true);

            // Nothing to simplify if current triangle count is lower than max triangle count.
            int triCount = mesh.sharedMesh.triangles.Length;
            if (triCount < maxTriCount)
                return;

            // Quality is the percentage of triangles to keep. In our case we want maxTriCount vertices.
            float quality = (float) maxTriCount / triCount;

            // Simplify.
            SimplifyMeshFilter(mesh, quality);
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

            // Model was deleted while simplifying.
            if (meshFilter == null) return;

            // Get simplified mesh.
            var finalMesh = meshSimplifier.ToMesh();

            // Optimize mesh.
            finalMesh.Optimize();
            finalMesh.name = "Optimized mesh";

            // Set the simplified mesh to the mesh filter.
            meshFilter.sharedMesh = finalMesh;
            meshFilter.name += "_optimized";
            meshFilter.sharedMesh.name += "_optimized";
        }

        /// <summary>
        ///     Gets the bounds of all the children of the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Encapsulated bounds of children.</returns>
        private Bounds GetObjectBounds(GameObject obj)
        {
            // Get meshes of all children.
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();

            // Encapsulate bounds of all meshes.
            var bounds = new Bounds(Instance.transform.position, Vector3.zero);
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }

            return bounds;
        }
    }
}