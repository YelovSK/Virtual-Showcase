using TMPro;
using UnityEngine;

namespace VirtualVitrine.Menu.Tooltip
{
    public class Tooltip : MonoBehaviour
    {
        private static Tooltip instance;

        #region Serialized Fields

        [SerializeField] private RectTransform canvasRect;

        #endregion

        private RectTransform backgroundRect;
        private TextMeshProUGUI tooltipText;

        #region Event Functions

        private void Awake()
        {
            if (instance != null)
                return;
            instance = this;
            tooltipText = transform.Find("Text").GetComponent<TextMeshProUGUI>();
            backgroundRect = transform.Find("Background").GetComponent<RectTransform>();
            gameObject.SetActive(false);
        }

        #endregion

        public static void ShowTooltip(string tooltip)
        {
            instance.gameObject.SetActive(true);
            instance.tooltipText.text = tooltip;
            instance.tooltipText.ForceMeshUpdate();
            var padding = new Vector2(9, 8);
            instance.backgroundRect.sizeDelta = instance.tooltipText.GetRenderedValues(false) + padding;
        }

        public static void HideTooltip()
        {
            instance.gameObject.SetActive(false);
        }

        public static void UpdatePosition(Vector2 position)
        {
            // Position on the canvas.
            Vector2 anchoredPosition = position / instance.canvasRect.localScale.x;
            float tooltipRight = anchoredPosition.x + instance.backgroundRect.sizeDelta.x;
            float tooltipBottom = anchoredPosition.y + instance.backgroundRect.sizeDelta.y;

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