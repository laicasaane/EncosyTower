using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Attachments;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Cards
{
    /// <summary>The concept card of a dimension screen.</summary>
    /// <remarks>
    /// A page of the Cards flow, a multi-page list attached to the screen. The player can show it with the other card.
    /// </remarks>
    [UxmlElement]
    public partial class ConceptCard : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string TITLE_LABEL = "label-title";
        private const string BODY_LABEL = "label-body";
        private const string CLOSE_BUTTON = "button-close";
        private const float PERCENT = 100f;

        private static readonly CustomStyleProperty<float> s_splitFraction
            = ConfigNameAPI.AmountProperty(ConfigAmountType.CardSplitFraction);
        private static readonly CustomStyleProperty<float> s_gap
            = ConfigNameAPI.AmountProperty(ConfigAmountType.CardGap);

        private readonly PaletteStyle _palette = new();

        private Label _title;
        private Label _body;
        private Button _close;
        private AttachmentSet _set;
        private float _splitFraction;
        private float _gap;

        /// <summary>Creates the page element.</summary>
        public ConceptCard()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetAttachment(in context, out var set) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_body == null)
            {
                _title = this.Q<Label>(TITLE_LABEL);
                _body = this.Q<Label>(BODY_LABEL);
                _close = this.Q<Button>(CLOSE_BUTTON);

                _close.clicked += OnCloseClicked;
            }

            _set = set;
            _set.Changed += OnSetChanged;

            Render();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_set != null)
            {
                _set.Changed -= OnSetChanged;
                _set = null;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root over the auxiliary transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot) {
                ShowDurationMs = _set.State.Timings.AuxiliaryTransitionMs,
                HideDurationMs = _set.State.Timings.AuxiliaryTransitionMs,
            };

        private void Render()
        {
            if (_set.InUse == false || DimensionCatalog.TryGet(_set.Location, out var dimension) == false)
            {
                return;
            }

            var state = _set.State;
            var view = state.ViewOf(dimension.Id);
            var tones = ToneMath.Analogous(_palette.SurfaceOf(dimension.Index), _palette);
            var ink = _palette.Color(ConfigColorType.Ink);
            var borderMix = _palette.Amount(ConfigAmountType.ButtonOutlineMix);

            style.left = 0f;
            style.right = new Length(view.InPracticeVisible ? _splitFraction * PERCENT : 0f, LengthUnit.Percent);
            style.marginRight = view.InPracticeVisible ? _gap : 0f;
            style.backgroundColor = tones.ComplementA;

            _title.text = state.Resources.Get(ConfigTextType.CardConceptTitle);
            _body.text = dimension.Concept;
            BookView.AttachStates(this, _palette);
            BookView.SetColors(_close, tones.Button, ink, borderMix);

            var hover = new ButtonLook(
                  Color.Lerp(tones.Button, ink, _palette.Amount(ConfigAmountType.DimensionHoverMix))
                , ink
                , ink
            );

            var press = new ButtonLook(
                  Color.Lerp(tones.Button, ink, _palette.Amount(ConfigAmountType.DimensionPressMix))
                , ink
                , ink
            );

            BookView.SetStateLooks(_close, in hover, in press, ink);
            BookView.SetEnabled(_close, state.IsPopupActive == false);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var customStyle = evt.customStyle;

            _palette.Read(customStyle, ThemeAPI.IsDark(this));
            customStyle.Read(s_splitFraction, ref _splitFraction);
            customStyle.Read(s_gap, ref _gap);

            if (_set != null)
            {
                Render();
            }
        }

        private void OnSetChanged()
        {
            Render();
        }

        private void OnCloseClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.CloseCard, 0);
        }
    }
}
