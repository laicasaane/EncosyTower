using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Overlays;
using UnityEngine;

namespace EncosyTower.Samples.UitkPages.App
{
    /// <summary>Shows the transient feedback: toasts, and the blink of the core entry.</summary>
    internal sealed class FeedbackPresenter
    {
        private readonly CodexContext _ctx;
        private float _toastExpiry;
        private float _blinkExpiry;

        public FeedbackPresenter(CodexContext ctx)
        {
            _ctx = ctx;
        }

        public void ShowToast(ConfigTextType text, ToastKind kind)
        {
            ShowToast(_ctx.ViewResources.Get(text), kind);
        }

        public void ShowToast(string text, ToastKind kind)
        {
            var toast = _ctx.Attachments.Toast;

            if (toast != null)
            {
                toast.Show(text, kind);
            }

            _toastExpiry = Time.unscaledTime + _ctx.Timings.ToastSeconds;
        }

        public void StartCoreBlink()
        {
            _blinkExpiry = Time.unscaledTime + _ctx.Timings.BlinkSeconds;
        }

        public void Tick(float now)
        {
            if (_toastExpiry > 0f && now >= _toastExpiry)
            {
                _toastExpiry = 0f;
                _ctx.Attachments.Toast?.Dismiss();
            }

            if (_blinkExpiry > 0f && now >= _blinkExpiry)
            {
                _blinkExpiry = 0f;
                _ctx.State.ClearCoreBlink();
            }
        }
    }
}
