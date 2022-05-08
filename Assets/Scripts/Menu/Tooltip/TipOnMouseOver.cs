using UnityEngine;
using UnityEngine.EventSystems;

namespace VirtualVitrine.Menu.Tooltip
{
    public class TipOnMouseOver : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        #region Serialized Fields

        [SerializeField] private string tooltip;

        #endregion

        private bool mouseOver;

        #region Event Functions

        private void Awake()
        {
            tooltip = tooltip.Replace("\\n", "\r\n");
        }

        #endregion

        #region IPointerEnterHandler Members

        public void OnPointerEnter(PointerEventData eventData)
        {
            mouseOver = true;
            Tooltip.ShowTooltip(tooltip);
        }

        #endregion

        #region IPointerExitHandler Members

        public void OnPointerExit(PointerEventData eventData)
        {
            mouseOver = false;
            Tooltip.HideTooltip();
        }

        #endregion

        #region IPointerMoveHandler Members

        public void OnPointerMove(PointerEventData eventData)
        {
            if (mouseOver)
                Tooltip.UpdatePosition(eventData.position);
        }

        #endregion
    }
}