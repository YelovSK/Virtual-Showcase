using System.Threading.Tasks;
using GLTFast;
using UnityEngine;

namespace VirtualShowcase.ModelLoading
{
    public class GLBLoader : IModelLoader
    {
        private const int X_ROTATION = -90;
        
        public GLBLoader()
        {
            GltfImportBase.SetDefaultDeferAgent(new UninterruptedDeferAgent());
        }
        
        public async Task<GameObject> InstantiateModel(string path, GameObject parent)
        {
            var gltf = new GltfImport();
            
            await gltf.Load($"file://{path}").ContinueWith(
                async t =>
                {
                    if (t.Result == false)
                    {
                        Debug.LogError($"Failed to load model: {path}");
                        return;
                    }
                    
                    await gltf.InstantiateMainSceneAsync(parent.transform);
                },
                TaskScheduler.FromCurrentSynchronizationContext()
            );
            
            GameObject model = parent.transform.GetChild(0).gameObject;
            model.transform.localRotation = Quaternion.Euler(X_ROTATION, 0, 0);

            return model;
        }
    }
}