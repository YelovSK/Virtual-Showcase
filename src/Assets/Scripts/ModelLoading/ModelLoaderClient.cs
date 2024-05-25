using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityMeshSimplifier;
using VirtualShowcase.Common;
using VirtualShowcase.Core;
using VirtualShowcase.FaceTracking.Transform;
using VirtualShowcase.Utilities;

namespace VirtualShowcase.ModelLoading
{
    public class ModelLoaderClient : MonoSingleton<ModelLoaderClient>
    {
        public static string[] SUPPORTED_EXTENSIONS => LOADERS_DICT.Keys.ToArray();
        private static readonly Dictionary<string, IModelLoader> LOADERS_DICT = new()
        {
            { ".glb", new GLBLoader() },
        };
        public List<ModelInfo> ModelsInfo { get; } = new();
        public List<GameObject> Models => ModelsInfo.Select(x => x.Object).ToList();

        public void Start()
        {
            MyEvents.ScreenSizeChanged.AddListener((sender, size) =>
            {
                if (MyPrefs.ShowRealModelSize)
                {
                    ResetTransform();
                }
            });
            MyEvents.ModelAdded.AddListener(async (sender, path) => await LoadModels(MyPrefs.ModelPaths));
            MyEvents.ModelRemoved.AddListener((sender, path) => DeleteModel(path));
            MyEvents.ModelsRemoveRequest.AddListener(sender => DeleteModels());
        }
        
        public async Task LoadModels(IEnumerable<string> paths)
        {
            MyEvents.ModelsLoadingStart?.Invoke(gameObject);

            var tasks = new List<Task<GameObject>>();
            foreach (string path in paths)
            {
                // Check if it's already loaded (in the models list).
                if (ModelsInfo.Any(x => x.FullPath == path) || File.Exists(path) == false)
                {
                    continue;
                }

                // Hierarchy: {this} -> {obj} -> {model}
                var obj = new GameObject(path)
                {
                    transform = { parent = gameObject.transform },
                };
                
                IModelLoader loader = GetLoader(path);
                Task<GameObject> task = loader.InstantiateModel(path, obj).ContinueWith(t =>
                {
                    ModelsInfo.Add(new ModelInfo
                    {
                        FullPath = path,
                        Object = obj,
                    });
                    return t.Result;
                });
                tasks.Add(task);
            }

            // Load + periodically report progress
            Task<GameObject[]> entireTask = Task.WhenAll(tasks);
            while (await Task.WhenAny(entireTask, Task.Delay(50)) != entireTask)
            {
                MyEvents.ModelLoaded.Invoke(gameObject, (ModelsInfo.Count, MyPrefs.ModelPaths.Count));
            }
            GameObject[] models = entireTask.Result;

            // Post-load
            await Task.WhenAll(models.Select(ApplyPostLoadingBehavior));
 
            // Only the first one visible.
            for (var i = 0; i < ModelsInfo.Count; i++)
            {
                ModelsInfo[i].Object.SetActive(i == 0);
            }

            if (tasks.Any())
            {
                ResetTransform();
            }
            
            MyEvents.ModelsLoadingEnd?.Invoke(gameObject);
        }
        
        private void DeleteModel(string path)
        {
            ModelInfo model = ModelsInfo.FirstOrDefault(x => x.FullPath == path);
            if (model is null)
            {
                return;
            }
            
            ModelsInfo.Remove(model);
            Destroy(model.Object);
        }
        
        private void DeleteModels()
        {
            foreach (ModelInfo model in ModelsInfo)
            {
                Destroy(model.Object);
            }
            
            ModelsInfo.Clear();
        }
        
        /// <param name="next">true for next, false for previous</param>
        public void CycleActiveModel(bool next = true)
        {
            int activeIx = ModelsInfo.FindIndex(x => x.Object.activeSelf);
            if (activeIx == -1)
            {
                return;
            }
            
            ModelsInfo[activeIx].Object.SetActive(false);
            
            if (next)
            {
                ModelsInfo[(activeIx + 1) % ModelsInfo.Count].Object.SetActive(true);
            }
            else
            {
                ModelsInfo[(activeIx - 1 + ModelsInfo.Count) % ModelsInfo.Count].Object.SetActive(true);
            }
        }

        /// <summary>
        ///     Adjusts the position and size of the model.
        /// </summary>
        public void ResetTransform()
        {
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
            Bounds bounds = reference.GetObjectBounds();

            if (MyPrefs.ShowRealModelSize)
            {
                // Stupid temp hack to set the real life scale of model.
                // Model is exported with 1 unit = 1 meter, however we use 1 unit = 1 cm.
                // Plus it's all scaled according to the base screen size. Dumb, I know.
                float sizeRatio = (float)Constants.SCREEN_BASE_DIAGONAL_INCHES / MyPrefs.ScreenSize;
                const int cm_in_meter = 100;
                float scale = sizeRatio * cm_in_meter;
                reference.transform.localScale = new Vector3(scale, scale, scale);
            }
            else
            {
                // Set height to 90% of screen height.
                float currentHeight = bounds.size.y;
                float targetHeight = Projection.ScreenHeight * 0.9f;
                reference.transform.localScale = targetHeight * reference.transform.localScale / currentHeight;
            }

            // Set position to be the middle of the parent (center of the screen).
            reference.transform.localPosition = reference.transform.parent.position;

            // Get new bounds (scale was changed).
            bounds = reference.GetObjectBounds();

            // Lower the object by its center.
            // E.g. if the set center is at the feet of a person, we can lower it by the real center.
            // Translating in the Z direction probably because of the 
            reference.transform.Translate(new Vector3(0, -bounds.center.y, 0), Instance.transform);

            // Use the scale and position of the reference object for all other objects.
            foreach (GameObject model in Models.Skip(1))
            {
                model.transform.localScale = reference.transform.localScale;
                model.transform.localPosition = reference.transform.localPosition;
            }
        }

        private IModelLoader GetLoader(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            IModelLoader loader = LOADERS_DICT.GetValueOrDefault(extension);

            // Unknown extension should NEVER be selected,
            // because the file browser should only allow selecting files with the supported extensions.
            if (loader is null)
            {
                throw new System.NotImplementedException($"Model type not supported: {extension}");
            }
            
            return loader;
        }

        private async Task ApplyPostLoadingBehavior(GameObject obj)
        {
            foreach (Transform child in obj.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.layer = Constants.LAYER_MODEL;
            }
            
            if (MyPrefs.SimplifyMesh)
            {
                await SimplifyObject(obj, MyPrefs.MaxTriCount);
            }
        }
        
        private async Task SimplifyObject(GameObject obj, int maxTriCount)
        {
            // Sometimes the result is split into multiple meshes, combine them into one.
            // Simplifying multiple meshes can lead to gaps,
            // but simplifying this combined mesh also leads to issues,
            // probably because it uses one material per submesh.
            // Pick your poison I guess.
            MeshFilter mesh = obj.MeshCombine(true);

            // Nothing to simplify if current triangle count is lower than max triangle count.
            int triCount = mesh.sharedMesh.triangles.Length;
            if (triCount < maxTriCount)
            {
                return;
            }

            // Quality is the percentage of triangles to keep. In our case we want maxTriCount vertices.
            float quality = (float)maxTriCount / triCount;

            // Simplify.
            await SimplifyMeshFilter(mesh, quality);
        }

        private async Task SimplifyMeshFilter(MeshFilter meshFilter, float quality)
        {
            Mesh sourceMesh = meshFilter.sharedMesh;
            if (sourceMesh == null)
            {
                return;
            }

            // Create mesh simplifier with the given mesh.
            var meshSimplifier = new MeshSimplifier(sourceMesh);

            // Simplify mesh with the given quality.
            // Runs asynchronously.
            // await Task.Run(() => meshSimplifier.SimplifyMesh(quality));
            meshSimplifier.SimplifyMesh(quality);

            // Model was deleted while simplifying.
            if (meshFilter == null)
            {
                return;
            }

            // Get simplified mesh.
            var finalMesh = meshSimplifier.ToMesh();

            // Optimize mesh.
            finalMesh.Optimize();
            finalMesh.name += "Optimized mesh";

            // Set the simplified mesh to the mesh filter.
            meshFilter.sharedMesh = finalMesh;
            meshFilter.name += "_optimized";
            meshFilter.sharedMesh.name += "_optimized";
        }

    }
}