using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Overlays
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
    [UxmlElement]
    public partial class ToastPage : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string TOAST_PILL = "toast-pill";
        private const string TOAST_LABEL = "label-toast";
        private const string RUNE_CLASS = "tower-book-rune--small";

        private readonly PaletteStyle _palette = new();

        private VisualElement _toastPill;
        private Label _toastLabel;
        private RuneView _toastRune;
        private AppState _state;
        private ToastKind _kind;
        private bool _hasRune;
        private bool _hasShown;

        /// <summary>Creates the page element.</summary>
        public ToastPage()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_toastPill == null)
            {
                _toastPill = this.Q(TOAST_PILL);
                _toastLabel = this.Q<Label>(TOAST_LABEL);

                _toastRune = new RuneView();
                _toastRune.AddToClassList(RUNE_CLASS);
                _toastPill.Insert(0, _toastRune);
            }

            Slot.pickingMode = PickingMode.Ignore;
            style.display = DisplayStyle.None;

            _state = data.State;
            _state.Attachments.Toast = this;

            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_state != null)
            {
                if (ReferenceEquals(_state.Attachments.Toast, this))
                {
                    _state.Attachments.Toast = null;
                }

                _state = null;
            }

            _hasShown = false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root with the default duration.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot);

        internal void Show(string text, ToastKind kind)
        {
            ShowCore(text, kind, hasRune: false, rune: default, number: 0);
        }

        internal void Show(string text, ToastKind kind, Rune rune, int number)
        {
            ShowCore(text, kind, hasRune: true, rune: rune, number: number);
        }

        private void ShowCore(string text, ToastKind kind, bool hasRune, Rune rune, int number)
        {
            if (_state == null)
            {
                return;
            }

            _kind = kind;
            _hasRune = hasRune;
            _hasShown = true;

            style.display = DisplayStyle.Flex;
            _toastLabel.text = text;

            if (_hasRune)
            {
                _toastRune.style.display = DisplayStyle.Flex;
                _toastRune.SetRune(rune, decoded: false, number: number);
            }
            else
            {
                _toastRune.style.display = DisplayStyle.None;
            }

            ApplyColors();
        }

        internal void Dismiss()
        {
            if (_state != null)
            {
                style.display = DisplayStyle.None;
            }
        }

        private void ApplyColors()
        {
            _toastPill.style.backgroundColor = _kind switch {
                ToastKind.Failure => _palette.Color(ConfigColorType.ToastFailure),
                ToastKind.Cancelled => _palette.Color(ConfigColorType.ToastCancelled),
                ToastKind.Core => _palette.SurfaceOf(DimensionCatalog.OUTER_COUNT),
                // Same tint as a failure on purpose; see ToastKind.Rune.
                ToastKind.Rune => _palette.Color(ConfigColorType.ToastFailure),
                _ => _palette.Color(ConfigColorType.ToastInfo),
            };

            if (_hasRune)
            {
                _toastRune.SetInk(_palette.Color(ConfigColorType.Ink));
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            if (_state != null && _hasShown)
            {
                ApplyColors();
            }
        }
    }
}
