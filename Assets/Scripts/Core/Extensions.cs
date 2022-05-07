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
            return -1 == j ? arr[arr.Length - 1] : arr[j];
        }

        // String to enum.
        public static T ParseEnum<T>(string value)
        {
            return (T) Enum.Parse(typeof(T), value, true);
        }

        // Load saved transform.
        public static void LoadTransform(this Transform original, CopyTransform savedCopy)
        {
            original.position = savedCopy.position;
            original.rotation = savedCopy.rotation;
            original.localScale = savedCopy.localScale;
            var rect = original.GetComponent<RectTransform>();
            if (rect != null)
                rect.sizeDelta = savedCopy.size;
        }
    }

    public class CopyTransform
    {
        public Vector3 localScale;
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 size;

        public CopyTransform(Transform trans)
        {
            position = trans.position;
            rotation = trans.rotation;
            localScale = trans.localScale;
            var rect = trans.GetComponent<RectTransform>();
            if (rect != null)
                size = rect.sizeDelta;
        }
    }
}