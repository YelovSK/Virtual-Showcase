using System;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;

namespace MediaPipe.BlazeFace
{
//
// Main face detector class
//
    public sealed partial class FaceDetector : IDisposable
    {
        #region Compile-time constants

        // Maximum number of detections. This value must be matched with
        // MAX_DETECTION in Common.hlsl.
        private const int MaxDetection = 10;

        #endregion

        #region Neural network inference function

        private void RunModel(Texture source, float threshold)
        {
            // Reset the compute buffer counters.
            post1Buffer.SetCounterValue(0);
            DetectionBuffer.SetCounterValue(0);

            // Preprocessing
            ComputeShader pre = resources.preprocess;
            pre.SetInt("_ImageSize", size);
            pre.SetTexture(0, "_Texture", source);
            pre.SetBuffer(0, "_Tensor", preBuffer);
            pre.Dispatch(0, size / 8, size / 8, 1);

            // Run the BlazeFace model.
            using (var tensor = new Tensor(1, size, size, 3, preBuffer))
            {
                worker.Execute(tensor);
            }

            // Output tensors -> Temporary render textures
            RenderTexture scores1RT = worker.CopyOutputToTempRT("Identity", 1, 512);
            RenderTexture scores2RT = worker.CopyOutputToTempRT("Identity_1", 1, 384);
            RenderTexture boxes1RT = worker.CopyOutputToTempRT("Identity_2", 16, 512);
            RenderTexture boxes2RT = worker.CopyOutputToTempRT("Identity_3", 16, 384);

            // 1st postprocess (bounding box aggregation)
            ComputeShader post1 = resources.postprocess1;
            post1.SetFloat("_ImageSize", size);
            post1.SetFloat("_Threshold", threshold);

            post1.SetTexture(0, "_Scores", scores1RT);
            post1.SetTexture(0, "_Boxes", boxes1RT);
            post1.SetBuffer(0, "_Output", post1Buffer);
            post1.Dispatch(0, 1, 1, 1);

            post1.SetTexture(1, "_Scores", scores2RT);
            post1.SetTexture(1, "_Boxes", boxes2RT);
            post1.SetBuffer(1, "_Output", post1Buffer);
            post1.Dispatch(1, 1, 1, 1);

            // Release the temporary render textures.
            RenderTexture.ReleaseTemporary(scores1RT);
            RenderTexture.ReleaseTemporary(scores2RT);
            RenderTexture.ReleaseTemporary(boxes1RT);
            RenderTexture.ReleaseTemporary(boxes2RT);

            // Retrieve the bounding box count.
            ComputeBuffer.CopyCount(post1Buffer, countBuffer, 0);

            // 2nd postprocess (overlap removal)
            ComputeShader post2 = resources.postprocess2;
            post2.SetBuffer(0, "_Input", post1Buffer);
            post2.SetBuffer(0, "_Count", countBuffer);
            post2.SetBuffer(0, "_Output", DetectionBuffer);
            post2.Dispatch(0, 1, 1, 1);

            // Retrieve the bounding box count after removal.
            ComputeBuffer.CopyCount(DetectionBuffer, countBuffer, 0);

            // Read cache invalidation
            post2ReadCache = null;
        }

        #endregion

        #region Public accessors

        public ComputeBuffer DetectionBuffer { get; private set; }

        public void SetIndirectDrawCount(ComputeBuffer drawArgs)
        {
            ComputeBuffer.CopyCount(DetectionBuffer, drawArgs, sizeof(uint));
        }

        public IEnumerable<Detection> Detections
            => post2ReadCache ?? UpdatePost2ReadCache();

        #endregion

        #region Public methods

        public FaceDetector(ResourceSet resources)
        {
            this.resources = resources;
            AllocateObjects();
        }

        public void Dispose()
        {
            DeallocateObjects();
        }

        public void ProcessImage(Texture image, float threshold = 0.75f)
        {
            RunModel(image, threshold);
        }

        #endregion

        #region Private objects

        private readonly ResourceSet resources;
        private ComputeBuffer preBuffer;
        private ComputeBuffer post1Buffer;
        private ComputeBuffer countBuffer;
        private IWorker worker;
        private int size;

        private void AllocateObjects()
        {
            Model model = ModelLoader.Load(resources.model);
            size = model.inputs[0].shape[6]; // Input tensor width

            preBuffer = new ComputeBuffer(size * size * 3, sizeof(float));

            post1Buffer = new ComputeBuffer
                (MaxDetection, Detection.Size, ComputeBufferType.Append);

            DetectionBuffer = new ComputeBuffer
                (MaxDetection, Detection.Size, ComputeBufferType.Append);

            countBuffer = new ComputeBuffer
                (1, sizeof(uint), ComputeBufferType.Raw);

            worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, model);
        }

        private void DeallocateObjects()
        {
            preBuffer?.Dispose();
            preBuffer = null;

            post1Buffer?.Dispose();
            post1Buffer = null;

            DetectionBuffer?.Dispose();
            DetectionBuffer = null;

            countBuffer?.Dispose();
            countBuffer = null;

            worker?.Dispose();
            worker = null;
        }

        #endregion

        #region GPU to CPU readback

        private Detection[] post2ReadCache;
        private readonly int[] countReadCache = new int[1];

        private Detection[] UpdatePost2ReadCache()
        {
            countBuffer.GetData(countReadCache, 0, 0, 1);
            int count = countReadCache[0];

            post2ReadCache = new Detection[count];
            DetectionBuffer.GetData(post2ReadCache, 0, 0, count);

            return post2ReadCache;
        }

        #endregion
    }
} // namespace MediaPipe.BlazeFace