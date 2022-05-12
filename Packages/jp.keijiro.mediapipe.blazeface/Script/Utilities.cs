using Unity.Barracuda;
using UnityEngine;

namespace MediaPipe.BlazeFace
{
    internal static class IWorkerExtensions
    {
        //
        // Retrieves an output tensor from a NN worker and returns it as a
        // temporary render texture. The caller must release it using
        // RenderTexture.ReleaseTemporary.
        //
        public static RenderTexture
            CopyOutputToTempRT(this IWorker worker, string name, int w, int h)
        {
            const RenderTextureFormat fmt = RenderTextureFormat.RFloat;
            var shape = new TensorShape(1, h, w, 1);
            RenderTexture rt = RenderTexture.GetTemporary(w, h, 0, fmt);
            using Tensor tensor = worker.PeekOutput(name).Reshape(shape);
            tensor.ToRenderTexture(rt);
            tensor.Dispose();
            return rt;
        }
    }
} // namespace MediaPipe.BlazeFace