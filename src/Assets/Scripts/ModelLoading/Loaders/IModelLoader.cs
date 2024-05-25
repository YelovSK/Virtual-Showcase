using System.Threading.Tasks;
using UnityEngine;

namespace VirtualShowcase.ModelLoading
{
    public interface IModelLoader
    {
        /// <summary>
        /// Loads and instantiates a model.
        /// </summary>
        /// <param name="filePath">Full or relative path of the model.</param>
        /// <param name="parent">Parent GameObject of the new instantiated GameObject.</param>
        /// <returns>Instantiated GameObject</returns>
        Task<GameObject> InstantiateModel(string filePath, GameObject parent);
    }
}