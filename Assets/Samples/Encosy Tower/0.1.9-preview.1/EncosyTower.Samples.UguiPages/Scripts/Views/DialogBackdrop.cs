using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Views
{
    public sealed class DialogBackdrop : MonoBehaviour, IPointerClickHandler
    {
        private Action _dismiss;

        internal bool IsNested { get; private set; }

        internal static void Bind(GameObject backdrop, Action dismiss, bool nested)
        {
            backdrop.SetActive(true);

            if (backdrop.TryGetComponent<DialogBackdrop>(out var handler) == false)
            {
                handler = backdrop.AddComponent<DialogBackdrop>();
            }

            handler._dismiss = dismiss;
            handler.IsNested = nested;
            backdrop.GetComponent<Image>().raycastTarget = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            eventData.Use();
            _dismiss?.Invoke();
        }

        private void OnDisable()
        {
            _dismiss = null;
        }
    }
}
