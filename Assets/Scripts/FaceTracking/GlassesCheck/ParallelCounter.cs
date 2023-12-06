// source: https://coffeebraingames.wordpress.com/2021/10/24/some-dots-utilities-nativecounter-and-nativesum/

using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace VirtualShowcase.FaceTracking.GlassesCheck
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public unsafe struct NativeCounter
    {
        // The actual pointer to the allocated count needs to have restrictions relaxed so jobs can be scheduled with this container
        [NativeDisableUnsafePtrRestriction]
        private int* countIntegers;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;

        // The dispose sentinel tracks memory leaks. It is a managed type so it is cleared to null when scheduling a job
        // The job cannot dispose the container, and no one else can dispose it until the job has run, so it is ok to not pass it along
        // This attribute is required, without it this NativeContainer cannot be passed to a job; since that would give the job access to a managed object
        [NativeSetClassTypeToNullOnSchedule]
        private DisposeSentinel m_DisposeSentinel;
#endif

        // Keep track of where the memory for this was allocated
        private readonly Allocator m_AllocatorLabel;

        public const int IntsPerCacheLine = JobsUtility.CacheLineSize / sizeof(int);

        public NativeCounter(Allocator label)
        {
            // This check is redundant since we always use an int that is blittable.
            // It is here as an example of how to check for type correctness for generic types.
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!UnsafeUtility.IsBlittable<int>())
                throw new ArgumentException(
                    string.Format("{0} used in NativeQueue<{0}> must be blittable", typeof(int)));
#endif
            m_AllocatorLabel = label;

            // Allocate native memory for a single integer
            countIntegers = (int*) UnsafeUtility.Malloc(
                UnsafeUtility.SizeOf<int>() * IntsPerCacheLine * JobsUtility.MaxJobThreadCount, 4, label);

            // Create a dispose sentinel to track memory leaks. This also creates the AtomicSafetyHandle
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
#endif

            // Initialize the count to 0 to avoid uninitialized data
            Count = 0;
        }

        public void Increment()
        {
            // Verify that the caller has write permission on this data. 
            // This is the race condition protection, without these checks the AtomicSafetyHandle is useless
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            (*countIntegers)++;
        }

        public int Count
        {
            get
            {
                // Verify that the caller has read permission on this data. 
                // This is the race condition protection, without these checks the AtomicSafetyHandle is useless
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                var count = 0;
                for (var i = 0; i < JobsUtility.MaxJobThreadCount; ++i)
                {
                    count += countIntegers[IntsPerCacheLine * i];
                }

                return count;
            }

            set
            {
                // Verify that the caller has write permission on this data. 
                // This is the race condition protection, without these checks the AtomicSafetyHandle is useless
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

                // Clear all locally cached counts, 
                // set the first one to the required value
                for (var i = 1; i < JobsUtility.MaxJobThreadCount; ++i)
                {
                    countIntegers[IntsPerCacheLine * i] = 0;
                }

                *countIntegers = value;
            }
        }

        public bool IsCreated => countIntegers != null;

        public void Dispose()
        {
            // Let the dispose sentinel know that the data has been freed so it does not report any memory leaks
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            UnsafeUtility.Free(countIntegers, m_AllocatorLabel);
            countIntegers = null;
        }

        [NativeContainer]

        // This attribute is what makes it possible to use NativeCounter.Concurrent in a ParallelFor job
        [NativeContainerIsAtomicWriteOnly]
        public struct ParallelWriter
        {
            // Copy of the pointer from the full NativeCounter
            [NativeDisableUnsafePtrRestriction]
            private int* countIntegers;

            // Copy of the AtomicSafetyHandle from the full NativeCounter. The dispose sentinel is not copied since this inner struct does not own the memory and is not responsible for freeing it.
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            private AtomicSafetyHandle m_Safety;
#endif

            // The current worker thread index; it must use this exact name since it is injected
            [NativeSetThreadIndex] private int mThreadIndex;

            // This is what makes it possible to assign to NativeCounter.Concurrent from NativeCounter
            public static implicit operator ParallelWriter(NativeCounter cnt)
            {
                ParallelWriter parallelWriter;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(cnt.m_Safety);
                parallelWriter.m_Safety = cnt.m_Safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref parallelWriter.m_Safety);
#endif

                parallelWriter.countIntegers = cnt.countIntegers;
                parallelWriter.mThreadIndex = 0;

                return parallelWriter;
            }

            public void Increment()
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif

                // No need for atomics any more since we are just incrementing the local count
                ++countIntegers[IntsPerCacheLine * mThreadIndex];
            }
        }
    }
}