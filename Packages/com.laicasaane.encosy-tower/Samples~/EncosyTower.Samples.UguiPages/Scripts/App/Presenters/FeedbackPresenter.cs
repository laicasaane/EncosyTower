using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Overlays;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Samples.UguiPages.App
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
            ShowToast(_ctx.Config.Text(text), kind);
        }

        public void ShowToast(string text, ToastKind kind)
        {
            var toast = _ctx.State.Attachments.Toast;

            if (toast.IsValid())
            {
                toast.Show(text, kind);
            }

            _toastExpiry = Time.unscaledTime + _ctx.Config.Amount(ConfigAmountType.ToastSeconds);
        }

        public void ShowRuneToast(DimensionData data)
        {
            var text = string.Format(_ctx.Config.Text(ConfigTextType.ToastRuneDiscovered), data.Title);
            var toast = _ctx.State.Attachments.Toast;

            if (toast.IsValid())
            {
                toast.Show(text, ToastKind.Rune, _ctx.State.RuneOf(data.Index), _ctx.State.NumberOf(data.Index));
            }

            _toastExpiry = Time.unscaledTime + _ctx.Config.Amount(ConfigAmountType.ToastSeconds);
        }

        public void StartCoreBlink()
        {
            _blinkExpiry = Time.unscaledTime + _ctx.Config.Amount(ConfigAmountType.EntranceBlinkSeconds);
        }

        public void Tick(float now)
        {
            if (_toastExpiry > 0f && now >= _toastExpiry)
            {
                _toastExpiry = 0f;
                DismissToast();
            }

            if (_blinkExpiry > 0f && now >= _blinkExpiry)
            {
                _blinkExpiry = 0f;
                _ctx.State.ClearCoreBlink();
            }
        }

        private void DismissToast()
        {
            var toast = _ctx.State.Attachments.Toast;

            if (toast.IsValid())
            {
                toast.Dismiss();
            }
        }
    }
}
