using System;
using UnityEngine;
using VirtualShowcase.Enums;

namespace VirtualShowcase.Menu
{
    [RequireComponent(typeof(CanvasGroup))]
    [DisallowMultipleComponent]
    public class Page : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField]
        public bool exitOnNewPagePush = true;

        [SerializeField]
        private float animationTimeSeconds = 0.5f;

        [SerializeField]
        private AnimationDirection entryDirection = AnimationDirection.Left;

        [SerializeField]
        private AnimationDirection exitDirection = AnimationDirection.Left;

        [SerializeField]
        private AnimationEntryMode entryMode = AnimationEntryMode.Slide;

        [SerializeField]
        private AnimationEntryMode exitMode = AnimationEntryMode.Slide;

        #endregion

        private Coroutine _animationCoroutine;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;

        #region Event Functions

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
        }

        #endregion

        public void Enter()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            switch (entryMode)
            {
                case AnimationEntryMode.Slide:
                    SlideIn();
                    break;
                case AnimationEntryMode.Zoom:
                    ZoomIn();
                    break;
                case AnimationEntryMode.Fade:
                    FadeIn();
                    break;
                case AnimationEntryMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Exit()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }

            switch (exitMode)
            {
                case AnimationEntryMode.Slide:
                    SlideOut();
                    break;
                case AnimationEntryMode.Zoom:
                    ZoomOut();
                    break;
                case AnimationEntryMode.Fade:
                    FadeOut();
                    break;
                case AnimationEntryMode.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SlideIn()
        {
            _animationCoroutine =
                StartCoroutine(AnimationHelper.SlideIn(_rectTransform, entryDirection, animationTimeSeconds,
                    null));
        }

        private void SlideOut()
        {
            _animationCoroutine =
                StartCoroutine(AnimationHelper.SlideOut(_rectTransform, exitDirection, animationTimeSeconds,
                    () => _rectTransform.gameObject.SetActive(false)));
        }

        private void ZoomIn()
        {
            _animationCoroutine =
                StartCoroutine(AnimationHelper.ZoomIn(_rectTransform, animationTimeSeconds, null));
        }

        private void ZoomOut()
        {
            _animationCoroutine =
                StartCoroutine(AnimationHelper.ZoomOut(_rectTransform, animationTimeSeconds,
                    () => _rectTransform.gameObject.SetActive(false)));
        }

        private void FadeIn()
        {
            _rectTransform.gameObject.SetActive(true);
            _animationCoroutine =
                StartCoroutine(AnimationHelper.FadeIn(_canvasGroup, animationTimeSeconds, null));
        }

        private void FadeOut()
        {
            _animationCoroutine =
                StartCoroutine(AnimationHelper.FadeOut(_canvasGroup, animationTimeSeconds,
                    () => _rectTransform.gameObject.SetActive(false)));
        }
    }
}