using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace VirtualVitrine.Menu.Tooltip
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
    {
        private const float delay_seconds = 0.3f;

        #region Serialized Fields

        [SerializeField] private string header;
        [Multiline] [SerializeField] private string tooltip;

        #endregion

        private bool mouseOver;

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
            mouseOver = false;
            Tooltip.Hide();
        }

        #endregion

        #region IPointerMoveHandler Members

        public void OnPointerMove(PointerEventData eventData)
        {
            if (mouseOver)
                Tooltip.UpdatePosition(eventData.position);
        }

        #endregion

        private IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(delay_seconds);
            mouseOver = true;
            Tooltip.Show(tooltip, header);
            // The size of the rect doesn't update immediately, waiting for a frame.
            Tooltip.UpdatePosition(Mouse.current.position.ReadValue());
            yield return null;
            Tooltip.UpdatePosition(Mouse.current.position.ReadValue());
        }
    }
}