using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Overlays
{
    /// <summary>What a toast reports, which sets its color.</summary>
    internal enum ToastKind
    {
        /// <summary>A failed call.</summary>
        Failure,

        /// <summary>An aborted call.</summary>
        Cancelled,

        /// <summary>A plain notice.</summary>
        Info,

        /// <summary>The core unlocked.</summary>
        Core,

        /// <summary>A rune toast: shows the rune that was discovered.</summary>
        /// <remarks>
        /// Uses the failure colour (<c>COLOR_TOAST_FAILURE</c>) on purpose: the design gives rune toasts the same
        /// tint as failures.
        /// </remarks>
        Rune,
    }

    /// <summary>The toast: a short message above the screen.</summary>
    /// <remarks>
    /// The only page of the Toast flow. The app shows it by calling <c>Show</c> and dismisses it after a few seconds.
    /// It demonstrates a page that stays shown and changes its content, instead of being shown and hidden by the
    /// flow.
    /// </remarks>
    public sealed class ToastPage : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        /// <summary>The background image of the toast.</summary>
        [Tooltip("The background image of the toast.")]
        [SerializeField] private Image _background;

        /// <summary>The text of the toast.</summary>
        [Tooltip("The text of the toast.")]
        [SerializeField] private TMP_Text _label;

        /// <summary>The rune view of the toast.</summary>
        [Tooltip("The rune view of the toast.")]
        [SerializeField] private RuneView _rune;

        /// <summary>The canvas group that shows or hides the toast.</summary>
        [Tooltip("The canvas group that shows or hides the toast.")]
        [SerializeField] private CanvasGroup _group;

        private AppState _state;
        private ToastKind _kind;
        private bool _hasRune;
        private Rune _shownRune;
        private int _number;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            UnsubscribeState();

            _state = data.State;
            _state.ThemeChanged += OnThemeChanged;
            _state.Attachments.Toast = this;

            _group.blocksRaycasts = false;
            _group.interactable = false;

            Dismiss();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            UnsubscribeState();
        }

        private void OnDestroy()
        {
            UnsubscribeState();
        }

        internal void Show(string text, ToastKind kind)
        {
            Show(text, kind, hasRune: false, rune: default, number: 0);
        }

        internal void Show(string text, ToastKind kind, Rune rune, int number)
        {
            Show(text, kind, hasRune: true, rune: rune, number: number);
        }

        private void Show(string text, ToastKind kind, bool hasRune, Rune rune, int number)
        {
            if (_state == null)
            {
                return;
            }

            _kind = kind;
            _hasRune = hasRune;
            _shownRune = rune;
            _number = number;

            _group.alpha = 1f;
            _label.text = text;
            Render();
        }

        internal void Dismiss()
        {
            _group.alpha = 0f;
        }

        private void UnsubscribeState()
        {
            if (_state != null)
            {
                _state.ThemeChanged -= OnThemeChanged;

                if (ReferenceEquals(_state.Attachments.Toast, this))
                {
                    _state.Attachments.Toast = null;
                }

                _state = null;
            }
        }

        private void Render()
        {
            var config = _state.Config;
            var ink = config.Color(ConfigColorType.Ink);

            _label.color = ink;
            _background.color = _kind switch {
                ToastKind.Failure => config.Color(ConfigColorType.ToastFailure),
                ToastKind.Cancelled => config.Color(ConfigColorType.ToastCancelled),
                ToastKind.Core => config.Color(DimensionCatalog.Core.Surface),
                // Same tint as a failure on purpose; see ToastKind.Rune.
                ToastKind.Rune => config.Color(ConfigColorType.ToastFailure),
                _ => config.Color(ConfigColorType.ToastInfo),
            };

            BookView.SetBorders(this, config);

            if (_hasRune)
            {
                _rune.gameObject.SetActive(true);
                _rune.SetRune(_shownRune, decoded: false, number: _number);
                _rune.SetInk(ink);
            }
            else
            {
                _rune.gameObject.SetActive(false);
            }
        }

        private void OnThemeChanged()
        {
            Render();
        }
    }
}
