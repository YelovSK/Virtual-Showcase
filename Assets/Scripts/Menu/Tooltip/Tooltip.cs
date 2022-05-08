using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VirtualVitrine.Menu.Tooltip
{
    public class Tooltip : MonoBehaviour
    {
        private static Tooltip instance;

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
            if (instance != null)
                return;
            instance = this;
            gameObject.SetActive(false);
        }

        #endregion

        public static void Show(string content, string header = "")
        {
            instance.gameObject.SetActive(true);
            if (string.IsNullOrEmpty(header))
                instance.headerField.gameObject.SetActive(false);
            else
            {
                instance.headerField.gameObject.SetActive(true);
                instance.headerField.text = header;
            }

            instance.contentField.text = content;
            instance.layoutElement.enabled = Math.Max(instance.headerField.preferredWidth, instance.contentField.preferredWidth) >=
                                             instance.layoutElement.preferredWidth;
            instance.contentField.ForceMeshUpdate();
        }

        public static void Hide()
        {
            instance.gameObject.SetActive(false);
        }

        public static void UpdatePosition(Vector2 position)
        {
            // Position on the canvas.
            Vector2 anchoredPosition = position / instance.canvasRect.localScale.x;
            float tooltipRight = anchoredPosition.x + instance.rectTransform.sizeDelta.x;
            float tooltipBottom = anchoredPosition.y + instance.rectTransform.sizeDelta.y;

            // Check if the tooltip is out of the screen and adjust the position.
            if (tooltipRight > instance.canvasRect.sizeDelta.x)
                position.x -= (tooltipRight - instance.canvasRect.sizeDelta.x) * instance.canvasRect.localScale.x;
            if (tooltipBottom > instance.canvasRect.sizeDelta.y)
                position.y -= (tooltipBottom - instance.canvasRect.sizeDelta.y) * instance.canvasRect.localScale.y;

            // Update position.
            instance.transform.position = position;
        }
    }
}