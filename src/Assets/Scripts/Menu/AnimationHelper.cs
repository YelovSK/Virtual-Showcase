using System;
using System.Collections;
using UnityEngine;
using VirtualShowcase.Enums;

namespace VirtualShowcase.Menu
{
    public static class AnimationHelper
    {
        public static IEnumerator ZoomIn(RectTransform transform, float durationSec, Action onEnd)
        {
            float time = 0;
            while (time < 1)
            {
                transform.localScale = Vector3.Slerp(Vector3.zero, Vector3.one, time);
                yield return null;
                time += Time.deltaTime / durationSec;
            }

            transform.localScale = Vector3.one;

            onEnd?.Invoke();
        }

        public static IEnumerator ZoomOut(RectTransform transform, float durationSec, Action onEnd)
        {
            float time = 0;
            while (time < 1)
            {
                transform.localScale = Vector3.Slerp(Vector3.one, Vector3.zero, time);
                yield return null;
                time += Time.deltaTime / durationSec;
            }

            transform.localScale = Vector3.zero;
            onEnd?.Invoke();
        }

        public static IEnumerator FadeIn(CanvasGroup canvasGroup, float durationSec, Action onEnd)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            float time = 0;
            while (time < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, time);
                yield return null;
                time += Time.deltaTime / durationSec;
            }

            canvasGroup.alpha = 1;
            onEnd?.Invoke();
        }

        public static IEnumerator FadeOut(CanvasGroup canvasGroup, float durationSec, Action onEnd)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            float time = 0;
            while (time < 1)
            {
                canvasGroup.alpha = Mathf.Lerp(1, 0, time);
                yield return null;
                time += Time.deltaTime / durationSec;
            }

            canvasGroup.alpha = 0;
            onEnd?.Invoke();
        }

        public static IEnumerator SlideIn(RectTransform transform, AnimationDirection direction, float durationSec,
            Action onEnd)
        {
            Vector2 startPosition;
            switch (direction)
            {
                case AnimationDirection.Up:
                    startPosition = new Vector2(0, -Screen.height);
                    break;
                case AnimationDirection.Right:
                    startPosition = new Vector2(-Screen.width, 0);
                    break;
                case AnimationDirection.Down:
                    startPosition = new Vector2(0, Screen.height);
                    break;
                case AnimationDirection.Left:
                    startPosition = new Vector2(Screen.width, 0);
                    break;
                default:
                    startPosition = new Vector2(0, -Screen.height);
                    break;
            }

            float time = 0;
            while (time < 1)
            {
                transform.anchoredPosition = Vector3.Slerp(startPosition, Vector2.zero, time);
                yield return null;
                time += Time.deltaTime / durationSec;
            }

            transform.anchoredPosition = Vector2.zero;
            onEnd?.Invoke();
        }

        public static IEnumerator SlideOut(RectTransform transform, AnimationDirection direction, float durationSec,
            Action onEnd)
        {
            Vector2 endPosition;
            switch (direction)
            {
                case AnimationDirection.Up:
                    endPosition = new Vector2(0, Screen.height);
                    break;
                case AnimationDirection.Right:
                    endPosition = new Vector2(Screen.width, 0);
                    break;
                case AnimationDirection.Down:
                    endPosition = new Vector2(0, -Screen.height);
                    break;
                case AnimationDirection.Left:
                    endPosition = new Vector2(-Screen.width, 0);
                    break;
                default:
                    endPosition = new Vector2(0, Screen.height);
                    break;
            }

            float time = 0;
            while (time < 1)
            {
                transform.anchoredPosition = Vector3.Slerp(Vector2.zero, endPosition, time);
                yield return null;
                time += Time.deltaTime / durationSec;
            }

            transform.anchoredPosition = endPosition;
            onEnd?.Invoke();
        }
    }
}