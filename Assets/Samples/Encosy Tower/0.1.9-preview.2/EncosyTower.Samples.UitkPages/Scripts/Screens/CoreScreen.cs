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
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Screens
{
    /// <summary>The core screen: it shows the six runes and casts the incantation.</summary>
    /// <remarks>
    /// A page of the Screen flow. It rejects the creation until the core is unlocked.
    /// </remarks>
    [UxmlElement]
    public partial class CoreScreen
        : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnReturnToPool
    {
        private const string TITLE_LABEL = "label-title";
        private const string LORE_LABEL = "label-lore";
        private const string SPELL_LABEL = "label-spell";
        private const string SEAL_IMAGE = "image-seal";
        private const string CAST_BUTTON = "button-cast";
        private const string RIBBON_RUNE = "rune-";
        private const string NAVIGATION_BAR_HOST = "navigation-bar-host";

        private readonly RuneView[] _ribbon = new RuneView[RuneCatalog.COUNT];
        private readonly PaletteStyle _palette = new();

        private Label _title;
        private Label _lore;
        private Label _spell;
        private Button _cast;
        private Image _seal;
        private VisualElement _navigationBarHost;
        private AttachmentSet _set;
        private AppState _state;

        /// <summary>Creates the page element.</summary>
        public CoreScreen()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || data.State.CoreUnlocked == false
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_title == null)
            {
                _navigationBarHost = this.Q(NAVIGATION_BAR_HOST);
                _title = this.Q<Label>(TITLE_LABEL);
                _lore = this.Q<Label>(LORE_LABEL);
                _spell = this.Q<Label>(SPELL_LABEL);
                _cast = this.Q<Button>(CAST_BUTTON);
                _seal = this.Q<Image>(SEAL_IMAGE);

                _seal.image = Resources.Load<Texture2D>(DimensionCatalog.Core.MotifKey.Value);

                for (var i = 0; i < RuneCatalog.COUNT; i++)
                {
                    _ribbon[i] = this.Q<RuneView>($"{RIBBON_RUNE}{i}");
                }

                _cast.clicked += OnCastClicked;
            }

            _state = data.State;
            _state.Changed += OnStateChanged;

            Render();

            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public async UnityTask<bool> OnAttachToFlowAsync(IPageFlow flow, PageContext context, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            _set = await _state.Attachments.RentAsync(
                  location: BookLocation.Of(Dimension.Core)
                , navigationBarHost: _navigationBarHost
                , detailHost: null
                , cardsHost: null
                , token: token
            );

            return true;
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_state != null)
            {
                _state.Changed -= OnStateChanged;
                _state.Attachments.Return(_set);
                _state = null;
            }

            _set = null;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root over the screen transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot) {
                ShowDurationMs = _state.Timings.ScreenTransitionMs,
                HideDurationMs = _state.Timings.ScreenTransitionMs,
            };

        private void Render()
        {
            var resources = _state.Resources;
            var cast = _state.IncantationCast;
            var visited = _state.CoreVisited;
            var ink = _palette.Color(ConfigColorType.Ink);
            var navigationBar = _palette.Color(ConfigColorType.NavigationBar);
            var borderMix = _palette.Amount(ConfigAmountType.ButtonOutlineMix);
            var spellName = visited ? ConfigTextType.CoreSpellLearned : ConfigTextType.CoreSpellWaiting;
            var castName = cast ? ConfigTextType.CoreReadNameButton : ConfigTextType.CoreCastButton;

            style.backgroundColor = _palette.SurfaceOf(DimensionCatalog.Core.Index);
            _seal.tintColor = ink;
            _title.text = DimensionCatalog.Core.Title;
            _lore.text = DimensionCatalog.Core.Lore;
            _spell.text = resources.Get(spellName);
            _cast.text = resources.Get(castName);

            BookView.SetDisplay(_cast, visited);

            BookView.AttachStates(this, _palette);
            BookView.SetColors(_cast, navigationBar, ink, borderMix);

            var hover = new ButtonLook(_palette.Color(ConfigColorType.ButtonHover), ink, ink);
            var press = new ButtonLook(_palette.Color(ConfigColorType.ButtonPress), ink, ink);

            BookView.SetStateLooks(_cast, in hover, in press, ink);

            var interactive = _state.IsPopupActive == false;

            BookView.SetEnabled(_cast, interactive);

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var rune = (Rune)i;
                var hasRune = cast || _state.TryGetAcquiredRune(i, out rune);

                if (hasRune == false)
                {
                    _ribbon[i].SetBlank();
                }
                else
                {
                    _ribbon[i].SetRune(rune, decoded: cast, number: i + 1);
                }

                _ribbon[i].SetInk(ink);
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            if (_state != null)
            {
                Render();
            }
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnCastClicked()
        {
            var action = _state.IncantationCast ? AppAction.ReadName : AppAction.CastIncantation;
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }
    }
}
