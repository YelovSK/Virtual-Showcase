using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualShowcase.Menu
{
    public class LoadingScreen : MonoBehaviour
    {
        private const string MODEL_LOADING_TEXT = "LOADING MODELS...";
        private const string SCENE_LOADING_TEXT = "LOADING SCENE...";

        #region Serialized Fields

        [SerializeField]
        private TMP_Text text;

        [SerializeField]
        private Slider slider;

        #endregion

        private LoadingType _currentType = LoadingType.Models;

        /// <param name="progress">0.0 - 1.0</param>
        public void ShowProgress(LoadingType type, float progress)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            if (type != _currentType)
            {
                SetType(type);
            }

            slider.value = progress;
        }

        /// <summary>
        ///     Hides the loading screen.
        /// </summary>
        public void Finish()
        {
            slider.value = 1f;
            gameObject.SetActive(false);
        }

        private void SetType(LoadingType type)
        {
            _currentType = type;

            text.text = type switch
            {
                LoadingType.Models => MODEL_LOADING_TEXT,
                LoadingType.Scene => SCENE_LOADING_TEXT,
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public enum LoadingType
    {
        Models,
        Scene,
    }
}