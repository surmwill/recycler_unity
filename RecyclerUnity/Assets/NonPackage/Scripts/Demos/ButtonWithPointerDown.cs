using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
    /// <summary>
    /// A button with an additional OnPointerDown callback.
    /// </summary>
    public class ButtonWithPointerDown : Button
    {
        [SerializeField]
        private UnityEvent _onPointerDownEvent = null;

        /// <summary>
        /// Event called when the button is pressed.
        /// </summary>
        public UnityEvent OnPointerDownEvent => _onPointerDownEvent;

        /// <summary>
        /// Call the corresponding event when the button is pressed.
        /// </summary>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            OnPointerDownEvent.Invoke();
        }
    }
}
