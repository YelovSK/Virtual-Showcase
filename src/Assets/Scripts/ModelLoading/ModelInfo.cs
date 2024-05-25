using System.IO;
using UnityEngine;

namespace VirtualShowcase.ModelLoading
{
    public class ModelInfo
    {
        public string Name => Path.GetFileNameWithoutExtension(FullPath);
        public string FullPath { get; set; }
        public GameObject Object { get; set; }
    }

}