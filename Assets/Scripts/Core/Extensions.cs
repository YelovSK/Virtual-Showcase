using System;
using UnityEngine;

namespace VirtualVitrine
{
    public static class Extensions
    {
        // Next enum value.
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            var arr = (T[]) Enum.GetValues(src.GetType());
            int j = Array.IndexOf(arr, src) + 1;
            return arr.Length == j ? arr[0] : arr[j];
        }

        // Previous enum value.
        public static T Prev<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException($"Argument {typeof(T).FullName} is not an Enum");

            var arr = (T[]) Enum.GetValues(src.GetType());
            int j = Array.IndexOf(arr, src) - 1;
            return -1 == j ? arr[^1] : arr[j];
        }

        // String to enum.
        public static T ParseEnum<T>(string value) => (T) Enum.Parse(typeof(T), value, true);

        // Load saved transform.
        public static void LoadTransform(this Transform original, CopyTransform savedCopy)
        {
            original.position = savedCopy.Position;
            original.rotation = savedCopy.Rotation;
            original.localScale = savedCopy.LocalScale;
            var rect = original.GetComponent<RectTransform>();
            if (rect != null)
                rect.sizeDelta = savedCopy.Size;
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
                Size = rect.sizeDelta;
        }
    }
}