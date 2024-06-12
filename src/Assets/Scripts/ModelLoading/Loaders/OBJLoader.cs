using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace VirtualShowcase.ModelLoading
{
    public class OBJLoader : MonoBehaviour, IModelLoader
    {
        public Task<GameObject> InstantiateModel(string filePath, GameObject parent)
        {
            GameObject obj = new Dummiesman.OBJLoader().Load(filePath, FindMaterialFile(filePath));
            ConvertMaterialsToUrp(obj);
            obj.transform.parent = parent.transform;
            return Task.FromResult(obj);
        }

        private void ConvertMaterialsToUrp(GameObject model)
        {
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");

            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            IEnumerable<Material> materials = renderers.SelectMany(meshRenderer => meshRenderer.materials);

            foreach (Material material in materials)
            {
                // Cache original values.
                Texture tex = material.mainTexture;
                float smoothness = material.GetFloat("_Glossiness");

                material.shader = urpShader;

                // Set back original values.
                material.mainTexture = tex;
                material.SetFloat("_Smoothness", smoothness);
            }
        }

        /// <summary>
        /// Tries to find a .mtl file in the same directory as the .obj file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns>File path of the .mtl file or null</returns>
        [CanBeNull]
        private string FindMaterialFile(string filePath) =>
            Directory
                .GetFiles(Path.GetDirectoryName(filePath))
                .FirstOrDefault(file => file.EndsWith(".mtl"));
    }
}