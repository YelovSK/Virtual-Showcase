using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Object;

namespace VirtualShowcase.Utilities
{
    public static class Extensions
    {
        // Next enum value.
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
            }

            var arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(arr, src) + 1;
            return arr.Length == j ? arr[0] : arr[j];
        }

        // Previous enum value.
        public static T Prev<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");
            }

            var arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf(arr, src) - 1;
            return -1 == j ? arr[^1] : arr[j];
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static void LoadTransform(this Transform original, CopyTransform savedCopy)
        {
            original.position = savedCopy.Position;
            original.rotation = savedCopy.Rotation;
            original.localScale = savedCopy.LocalScale;
            var rect = original.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = savedCopy.Size;
            }
        }

        public static bool IsEmpty<T>(this IEnumerable<T> source)
        {
            return source.Any() == false;
        }

        public static int ToInt(this bool value)
        {
            return value ? 1 : 0;
        }

        public static bool ToBool(this int value)
        {
            return value switch
            {
                0 => false,
                1 => true,
                _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be 0 or 1."),
            };
        }

        /// <summary>
        ///     https://github.com/BlackxSnow/MaterialSafeMeshCombine
        /// </summary>
        public static MeshFilter MeshCombine(this GameObject gameObject, bool destroyObjects = false, params GameObject[] ignore)
        {
            Vector3 originalPosition = gameObject.transform.position;
            Quaternion originalRotation = gameObject.transform.rotation;
            Vector3 originalScale = gameObject.transform.localScale;
            gameObject.transform.position = Vector3.zero;
            gameObject.transform.rotation = Quaternion.identity;
            gameObject.transform.localScale = Vector3.one;

            var materials = new List<Material>();
            var combineInstanceLists = new List<List<CombineInstance>>();
            MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>()
                .Where(m => !ignore.Contains(m.gameObject) && !ignore.Any(i => m.transform.IsChildOf(i.transform))).ToArray();

            foreach (MeshFilter meshFilter in meshFilters)
            {
                var meshRenderer = meshFilter.GetComponent<MeshRenderer>();

                if (!meshRenderer ||
                    !meshFilter.sharedMesh ||
                    meshRenderer.sharedMaterials.Length != meshFilter.sharedMesh.subMeshCount)
                {
                    continue;
                }

                for (var s = 0; s < meshFilter.sharedMesh.subMeshCount; s++)
                {
                    int materialArrayIndex = materials.FindIndex(m => m.name == meshRenderer.sharedMaterials[s].name);
                    if (materialArrayIndex == -1)
                    {
                        materials.Add(meshRenderer.sharedMaterials[s]);
                        materialArrayIndex = materials.Count - 1;
                    }

                    combineInstanceLists.Add(new List<CombineInstance>());

                    var combineInstance = new CombineInstance();
                    combineInstance.transform = meshRenderer.transform.localToWorldMatrix;
                    combineInstance.subMeshIndex = s;
                    combineInstance.mesh = meshFilter.sharedMesh;
                    combineInstanceLists[materialArrayIndex].Add(combineInstance);
                }
            }

            // Get / Create mesh filter & renderer
            var meshFilterCombine = gameObject.GetComponent<MeshFilter>();
            if (meshFilterCombine == null)
            {
                meshFilterCombine = gameObject.AddComponent<MeshFilter>();
            }

            var meshRendererCombine = gameObject.GetComponent<MeshRenderer>();
            if (meshRendererCombine == null)
            {
                meshRendererCombine = gameObject.AddComponent<MeshRenderer>();
            }

            // Combine by material index into per-material meshes
            // also, Create CombineInstance array for next step
            var meshes = new Mesh[materials.Count];
            var combineInstances = new CombineInstance[materials.Count];

            for (var m = 0; m < materials.Count; m++)
            {
                CombineInstance[] combineInstanceArray = combineInstanceLists[m].ToArray();
                meshes[m] = new Mesh();
                meshes[m].CombineMeshes(combineInstanceArray, true, true);

                combineInstances[m] = new CombineInstance();
                combineInstances[m].mesh = meshes[m];
                combineInstances[m].subMeshIndex = 0;
            }

            // Combine into one
            meshFilterCombine.sharedMesh = new Mesh();
            meshFilterCombine.sharedMesh.CombineMeshes(combineInstances, false, false);

            // Destroy other meshes
            foreach (Mesh oldMesh in meshes)
            {
                oldMesh.Clear();
                DestroyImmediate(oldMesh);
            }

            // Assign materials
            Material[] materialsArray = materials.ToArray();
            meshRendererCombine.materials = materialsArray;

            if (destroyObjects)
            {
                IEnumerable<Transform> toDestroy = meshFilters.Select(m => m.transform);
                var toSave = new List<Transform>(8);
                Transform child;
                for (var i = 0; i < meshFilters.Length; i++)
                {
                    if (meshFilters[i].gameObject == gameObject)
                    {
                        continue;
                    }

                    //Check if any children should be saved
                    for (var c = 0; c < meshFilters[i].transform.childCount; c++)
                    {
                        child = meshFilters[i].transform.GetChild(c);
                        if (!toDestroy.Contains(child))
                        {
                            toSave.Add(child);
                        }
                    }

                    //Move toSave children to root object
                    for (var s = 0; s < toSave.Count; s++)
                    {
                        toSave[s].parent = gameObject.transform;
                    }

                    toSave.Clear();

                    Destroy(meshFilters[i].gameObject);
                }
            }
            else
            {
                for (var i = 0; i < meshFilters.Length; i++)
                {
                    if (meshFilters[i].gameObject == gameObject)
                    {
                        continue;
                    }

                    Destroy(meshFilters[i].GetComponent<MeshRenderer>());
                    Destroy(meshFilters[i]);
                }
            }

            gameObject.transform.position = originalPosition;
            gameObject.transform.rotation = originalRotation;
            gameObject.transform.localScale = originalScale;
            return meshFilterCombine;
        }

        /// <summary>
        ///     Gets the bounds of all the children of the given object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Encapsulated bounds of children.</returns>
        public static Bounds GetObjectBounds(this GameObject obj)
        {
            MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();

            var bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                bounds.Encapsulate(meshRenderer.bounds);
            }

            return bounds;
        }

        public static void SetEnabled(this InputActionMap map, bool enabled)
        {
            if (enabled)
            {
                map.Enable();
            }
            else
            {
                map.Disable();
            }
        }

        public static void SetEnabled(this InputAction action, bool enabled)
        {
            if (enabled)
            {
                action.Enable();
            }
            else
            {
                action.Disable();
            }
        }

        /// <summary>
        ///     Maps a value from a range to another range.
        /// </summary>
        /// <param name="n">Value to map</param>
        /// <param name="start1">Start of the original range</param>
        /// <param name="stop1">End of the original range</param>
        /// <param name="start2">Start of the target range</param>
        /// <param name="stop2">End of the target range</param>
        /// <returns>Mapped value</returns>
        public static float Map(this float n, float start1, float stop1, float start2, float stop2)
        {
            return (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
        }

        public static bool EqualsWithDelta(this float value, float other, float delta = 0.0001f)
        {
            return Mathf.Abs(value - other) < delta;
        }
    }

    public class CopyTransform
    {
        public Vector3 LocalScale;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector2 Size;

        public CopyTransform(Transform trans)
        {
            Position = trans.position;
            Rotation = trans.rotation;
            LocalScale = trans.localScale;
            var rect = trans.GetComponent<RectTransform>();
            if (rect != null)
            {
                Size = rect.sizeDelta;
            }
        }
    }
}