using UnityEngine;
using UnityEngine.EventSystems;

namespace Labyrinthino.UI
{
    /// <summary>
    /// Shrinks an element slightly while it is pressed to give tactile feedback.
    /// </summary>
    public sealed class PressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private const float PressedScale = 0.94f;

        /// <inheritdoc />
        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = Vector3.one * PressedScale;
        }

        /// <inheritdoc />
        public void OnPointerUp(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }

        /// <inheritdoc />
        public void OnPointerExit(PointerEventData eventData)
        {
            transform.localScale = Vector3.one;
        }

        private void OnDisable()
        {
            transform.localScale = Vector3.one;
        }
    }
}
