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
    /// <summary>The screen of one dimension, filled from the dimension named by its page user data.</summary>
    /// <remarks>
    /// A page of the Screen flow; one template page serves every dimension. It rents an attachment set, which
    /// attaches a navigation bar page, two tab pages and two card pages, each in a flow of its own.
    /// </remarks>
    [UxmlElement]
    public partial class DimensionScreen
        : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnReturnToPool
    {
        private const string NAVIGATION_BAR_HOST = "navigation-bar-host";
        private const string DETAIL_HOST = "detail-host";
        private const string CARDS_HOST = "cards-host";
        private const string TITLE_LABEL = "label-title";
        private const string MOTIF_LABEL = "label-motif";
        private const string ECHO_BUTTON = "button-echo";
        private const string MORE_BUTTON = "button-more";
        private const string CHRONICLE_BUTTON = "button-chronicle";
        private const string INSCRIPTION_BUTTON = "button-inscription";
        private const string RUNES_BUTTON = "button-concept";
        private const string COMPASS_BUTTON = "button-in-practice";
        private const string SELECTED_CLASS = "tower-book-button--selected";
        private const string PRESSED_CLASS = "tower-book-button--pressed";

        private readonly PaletteStyle _palette = new();

        private Label _title;
        private Label _motif;
        private Button _echo;
        private Button _more;
        private Button _chronicle;
        private Button _inscription;
        private Button _runes;
        private Button _compass;
        private VisualElement _navigationBarHost;
        private VisualElement _detailHost;
        private VisualElement _cardsHost;
        private AttachmentSet _set;
        private DimensionData _data;
        private AppState _state;
        private Dimension? _titleId;
        private string _titleText;

        /// <summary>Creates the page element.</summary>
        public DimensionScreen()
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
                || DimensionCatalog.TryGet(data.Location, out var dimension) == false
                || dimension.Index >= DimensionCatalog.OUTER_COUNT
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_title == null)
            {
                Query();
            }

            _data = dimension;
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
                  location: BookLocation.Of(_data.Id)
                , navigationBarHost: _navigationBarHost
                , detailHost: _detailHost
                , cardsHost: _cardsHost
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
            _titleId = null;
            _titleText = null;
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

        private void Query()
        {
            _navigationBarHost = this.Q(NAVIGATION_BAR_HOST);
            _detailHost = this.Q(DETAIL_HOST);
            _cardsHost = this.Q(CARDS_HOST);
            _title = this.Q<Label>(TITLE_LABEL);
            _motif = this.Q<Label>(MOTIF_LABEL);
            _echo = this.Q<Button>(ECHO_BUTTON);
            _more = this.Q<Button>(MORE_BUTTON);
            _chronicle = this.Q<Button>(CHRONICLE_BUTTON);
            _inscription = this.Q<Button>(INSCRIPTION_BUTTON);
            _runes = this.Q<Button>(RUNES_BUTTON);
            _compass = this.Q<Button>(COMPASS_BUTTON);

            _echo.clicked += OnEchoClicked;
            _more.clicked += OnMoreClicked;
            _chronicle.clicked += OnChronicleClicked;
            _inscription.clicked += OnInscriptionClicked;
            _runes.clicked += OnConceptClicked;
            _compass.clicked += OnInPracticeClicked;
        }

        private void Render()
        {
            var surface = _palette.SurfaceOf(_data.Index);
            var tones = ToneMath.Analogous(surface, _palette);
            var ink = _palette.Color(ConfigColorType.Ink);
            var borderMix = _palette.Amount(ConfigAmountType.ButtonOutlineMix);

            if (_titleId != _data.Id)
            {
                _titleId = _data.Id;
                _titleText = _state.Resources.Format(ConfigTextType.DimensionPageTitle, _data.Title);
            }

            style.backgroundColor = surface;
            _title.text = _titleText;
            _motif.text = _data.Motif;

            BookView.AttachStates(this, _palette);

            BookView.SetColors(_echo, tones.Button, ink, borderMix);
            BookView.SetColors(_more, tones.Button, ink, borderMix);
            BookView.SetColors(_chronicle, tones.NearA, ink, borderMix);
            BookView.SetColors(_inscription, tones.NearB, ink, borderMix);
            BookView.SetColors(_runes, tones.ComplementA, ink, borderMix);
            BookView.SetColors(_compass, tones.ComplementB, ink, borderMix);

            var view = _state.ViewOf(_data.Id);

            _chronicle.EnableInClassList(SELECTED_CLASS, view.Tab == 0);
            _inscription.EnableInClassList(SELECTED_CLASS, view.Tab == 1);
            _runes.EnableInClassList(PRESSED_CLASS, view.ConceptVisible);
            _compass.EnableInClassList(PRESSED_CLASS, view.InPracticeVisible);

            if (view.Tab == 0)
            {
                BookView.SetBorder(_chronicle, ink);
            }
            else
            {
                BookView.SetBorder(_inscription, ink);
            }

            if (view.ConceptVisible)
            {
                BookView.SetBorder(_runes, ink);
            }

            if (view.InPracticeVisible)
            {
                BookView.SetBorder(_compass, ink);
            }

            var hoverMix = _palette.Amount(ConfigAmountType.DimensionHoverMix);
            var pressMix = _palette.Amount(ConfigAmountType.DimensionPressMix);
            var press = Color.Lerp(tones.Button, ink, pressMix);

            SetLooks(_echo, tones.Button, press, ink, hoverMix);
            SetLooks(_more, tones.Button, press, ink, hoverMix);
            SetLooks(_chronicle, tones.NearA, press, ink, hoverMix);
            SetLooks(_inscription, tones.NearB, press, ink, hoverMix);
            SetLooks(_runes, tones.ComplementA, press, ink, hoverMix);
            SetLooks(_compass, tones.ComplementB, press, ink, hoverMix);

            var barHeight = _palette.Amount(ConfigAmountType.ButtonSelectionHeight);

            SetTopBar(_chronicle, view.Tab == 0, barHeight);
            SetTopBar(_inscription, view.Tab == 1, barHeight);
            SetBottomBar(_runes, view.ConceptVisible, barHeight);
            SetBottomBar(_compass, view.InPracticeVisible, barHeight);

            var interactive = _state.IsPopupActive == false;

            BookView.SetEnabled(_echo, interactive || _state.IsMoreOnTop);
            BookView.SetEnabled(_more, interactive);
            BookView.SetEnabled(_chronicle, interactive);
            BookView.SetEnabled(_inscription, interactive);
            BookView.SetEnabled(_runes, interactive);
            BookView.SetEnabled(_compass, interactive);
        }

        private static void SetLooks(Button button, Color fill, Color pressFill, Color ink, float hoverMix)
        {
            var hover = new ButtonLook(Color.Lerp(fill, ink, hoverMix), ink, ink);
            var press = new ButtonLook(pressFill, ink, ink);

            BookView.SetStateLooks(button, in hover, in press, ink);
        }

        private static void SetTopBar(Button button, bool isOn, float height)
        {
            if (isOn)
            {
                button.style.borderTopWidth = height;
            }
            else
            {
                button.style.borderTopWidth = StyleKeyword.Null;
            }
        }

        private static void SetBottomBar(Button button, bool isOn, float height)
        {
            if (isOn)
            {
                button.style.borderBottomWidth = height;
            }
            else
            {
                button.style.borderBottomWidth = StyleKeyword.Null;
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

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }

        private void Publish(AppAction action, int index)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action, index);
        }

        private void OnEchoClicked()
        {
            Publish(AppAction.ReadEcho);
        }

        private void OnMoreClicked()
        {
            Publish(AppAction.More);
        }

        private void OnChronicleClicked()
        {
            Publish(AppAction.SelectTab, 0);
        }

        private void OnInscriptionClicked()
        {
            Publish(AppAction.SelectTab, 1);
        }

        private void OnConceptClicked()
        {
            Publish(AppAction.ToggleCard, 0);
        }

        private void OnInPracticeClicked()
        {
            Publish(AppAction.ToggleCard, 1);
        }
    }
}
