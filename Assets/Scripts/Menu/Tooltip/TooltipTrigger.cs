using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace VirtualShowcase.Menu.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private const float DELAY_SECONDS = 0.3f;

        #region Serialized Fields

        [SerializeField]
        private string header;

        [Multiline]
        [SerializeField]
        private string tooltip;

        #endregion

        private bool _mouseOver;

        #region IPointerEnterHandler Members

        public void OnPointerEnter(PointerEventData eventData)
        {
            StartCoroutine(StartTimer());
        }

        #endregion

        #region IPointerExitHandler Members

        public void OnPointerExit(PointerEventData eventData)
        {
            StopAllCoroutines();
            _mouseOver = false;
            Tooltip.Hide();
        }

        #endregion

        #region IPointerMoveHandler Members

        public void OnPointerMove(PointerEventData eventData)
        {
            if (_mouseOver)
            {
                Tooltip.UpdatePosition(eventData.position);
            }
        }

        #endregion

        public void SetTooltip(string text)
        {
            tooltip = text;
        }

        private IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(DELAY_SECONDS);
            _mouseOver = true;
            Tooltip.Show(tooltip, header);

            // The size of the rect doesn't update immediately, waiting for a frame.
            Tooltip.UpdatePosition(Mouse.current.position.ReadValue());
            yield return null;
            Tooltip.UpdatePosition(Mouse.current.position.ReadValue());
        }
    }
}