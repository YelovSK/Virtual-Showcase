using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualShowcase.Menu.Tooltip
{
    public class Tooltip : MonoBehaviour
    {
        private static Tooltip _instance;

        #region Serialized Fields

        [Header("Parent canvas")]
        [SerializeField] private RectTransform canvasRect;

        [Header("Tooltip texts")]
        [SerializeField] private TextMeshProUGUI headerField;
        [SerializeField] private TextMeshProUGUI contentField;

        [Header("Tooltip components")]
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private RectTransform rectTransform;

        #endregion

        #region Event Functions

        private void Awake()
        {
            if (_instance != null)
                return;
            _instance = this;
            gameObject.SetActive(false);
        }

        #endregion

        public static void Show(string content, string header = "")
        {
            _instance.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(header))
                _instance.headerField.gameObject.SetActive(false);
            else
            {
                _instance.headerField.gameObject.SetActive(true);
                _instance.headerField.text = header;
            }

            _instance.contentField.text = content;
            _instance.layoutElement.enabled = Math.Max(_instance.headerField.preferredWidth, _instance.contentField.preferredWidth) >=
                                             _instance.layoutElement.preferredWidth;
            _instance.contentField.ForceMeshUpdate();
        }

        public static void Hide()
        {
            _instance.gameObject.SetActive(false);
        }

        public static void UpdatePosition(Vector2 position)
        {
            // Position on the canvas.
            Vector2 anchoredPosition = position / _instance.canvasRect.localScale.x;
            float tooltipRight = anchoredPosition.x + _instance.rectTransform.sizeDelta.x;
            float tooltipBottom = anchoredPosition.y + _instance.rectTransform.sizeDelta.y;

            // Check if the tooltip is out of the screen and adjust the position.
            if (tooltipRight > _instance.canvasRect.sizeDelta.x)
                position.x -= (tooltipRight - _instance.canvasRect.sizeDelta.x) * _instance.canvasRect.localScale.x;
            if (tooltipBottom > _instance.canvasRect.sizeDelta.y)
                position.y -= (tooltipBottom - _instance.canvasRect.sizeDelta.y) * _instance.canvasRect.localScale.y;

            // Update position.
            _instance.transform.position = position;
        }
    }
}