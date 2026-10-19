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
    /// <summary>The entrance screen: pick one of the six dimensions or the core.</summary>
    /// <remarks>
    /// A page of the Screen flow. It rents an attachment set, which gives it a navigation bar page. The core card
    /// blinks when the core unlocks.
    /// </remarks>
    [UxmlElement]
    public partial class EntranceScreen
        : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnReturnToPool
    {
        private const string TITLE_LABEL = "label-title";
        private const string TITLE_TAIL_LABEL = "label-title-tail";
        private const string CORE_DASHES = "core-dashes";
        private const string NAMED_CLASS = "tower-book-entrance--named";
        private const string DASHED_CLASS = "tower-book-entry--dashed";
        private const string INSTRUCTION_LABEL = "label-instruction";
        private const string READ_NAME_BUTTON = "button-read-name";
        private const string DIMENSION_BUTTON = "button-dimension-";
        private const string ENTRY_TITLE = "label-entry-";
        private const string ENTRY_HINT = "label-hint-";
        private const string ENTRY_RUNE = "entry-rune-";
        private const string RIBBON_RUNE = "rune-";
        private const string NAVIGATION_BAR_HOST = "navigation-bar-host";
        private const string GLOW_NAME = "core-glow";
        private const string GLOW_CLASS = "tower-book-entry__glow";
        private const string LOCKED_CLASS = "tower-book-entry--locked";
        private const int ENTRY_COUNT = DimensionCatalog.OUTER_COUNT + 1;
        private const float SECONDS_PER_MS = 0.001f;
        private const float CORE_DASH_WIDTH = 2f;
        private const float CORE_DASH_RADIUS = 4f;
        private const float CORE_DASH_LENGTH = 6f;
        private const float CORE_DASH_GAP = 4f;
        private const char LINE_BREAK = '\n';

        private readonly Button[] _entries = new Button[ENTRY_COUNT];
        private readonly Label[] _entryTitles = new Label[ENTRY_COUNT];
        private readonly Label[] _entryHints = new Label[ENTRY_COUNT];
        private readonly RuneView[] _entryRunes = new RuneView[ENTRY_COUNT];
        private readonly RuneView[] _ribbon = new RuneView[RuneCatalog.COUNT];
        private readonly PaletteStyle _palette = new();

        private Label _title;
        private Label _titleTail;
        private DottedBorder _coreDashes;
        private Label _instruction;
        private Button _readName;
        private Image _coreGlyph;
        private VisualElement _coreGlow;
        private VisualElement _navigationBarHost;
        private IVisualElementScheduledItem _blink;
        private string _castTitle;
        private string _castTitleTail;
        private string _uncastTitle;
        private string _uncastTitleTail;
        private float _blinkStart;
        private bool _isBlinking;
        private AttachmentSet _set;
        private AppState _state;

        /// <summary>Creates the page element.</summary>
        public EntranceScreen()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_title == null)
            {
                Query();
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
                  location: BookLocation.Entrance
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
            StopBlink();
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

        private static void SplitTitle(string title, out string head, out string tail)
        {
            var lineBreak = title.IndexOf(LINE_BREAK);

            if (lineBreak < 0)
            {
                head = title;
                tail = null;
                return;
            }

            head = title[..lineBreak];
            tail = title[(lineBreak + 1)..];
        }

        private void Query()
        {
            _navigationBarHost = this.Q(NAVIGATION_BAR_HOST);
            _title = this.Q<Label>(TITLE_LABEL);
            _titleTail = this.Q<Label>(TITLE_TAIL_LABEL);
            _coreDashes = this.Q<DottedBorder>(CORE_DASHES);
            _coreDashes.style.left = -CORE_DASH_WIDTH;
            _coreDashes.style.top = -CORE_DASH_WIDTH;
            _coreDashes.style.right = -CORE_DASH_WIDTH;
            _coreDashes.style.bottom = -CORE_DASH_WIDTH;
            _instruction = this.Q<Label>(INSTRUCTION_LABEL);
            _readName = this.Q<Button>(READ_NAME_BUTTON);
            _coreGlyph = this.Q<Image>("core-glyph");
            _coreGlyph.image = Resources.Load<Texture2D>(DimensionCatalog.Core.MotifKey.Value);
            _readName.clicked += OnReadNameClicked;

            for (var i = 0; i < ENTRY_COUNT; i++)
            {
                var button = this.Q<Button>($"{DIMENSION_BUTTON}{i}");

                _entries[i] = button;
                _entryTitles[i] = button.Q<Label>($"{ENTRY_TITLE}{i}");
                _entryHints[i] = button.Q<Label>($"{ENTRY_HINT}{i}");
                _entryRunes[i] = button.Q<RuneView>($"{ENTRY_RUNE}{i}");

                var index = i;

                void OnClick()
                {
                    OnEntryClicked(index);
                }

                button.clicked += OnClick;
            }

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                _ribbon[i] = this.Q<RuneView>($"{RIBBON_RUNE}{i}");
            }

            CreateCoreGlow();
        }

        private void Render()
        {
            var resources = _state.Resources;
            var cast = _state.IncantationCast;
            var interactive = _state.IsPopupActive == false;
            var ink = _palette.Color(ConfigColorType.InkEntrance);
            var locked = _palette.Color(ConfigColorType.Locked);
            var lockedTitle = resources.Get(ConfigTextType.DimensionLockedTitle);
            var pageColor = _palette.Color(ConfigColorType.PageEntrance);
            var slotMix = _palette.Amount(ConfigAmountType.EntranceRuneSlotBorderMix);
            var slotColor = Color.Lerp(pageColor, ink, slotMix);

            if (_castTitle == null)
            {
                SplitTitle(resources.Get(ConfigTextType.EntranceTitleCast), out _castTitle, out _castTitleTail);
                SplitTitle(resources.Get(ConfigTextType.EntranceTitleUncast), out _uncastTitle, out _uncastTitleTail);
            }

            var title = cast ? _castTitle : _uncastTitle;
            var titleTail = cast ? _castTitleTail : _uncastTitleTail;

            EnableInClassList(NAMED_CLASS, cast);
            BookView.SetDisplay(_titleTail, titleTail != null);
            _title.text = title;

            if (titleTail != null)
            {
                _titleTail.text = titleTail;
            }

            BookView.AttachStates(this, _palette);

            _coreGlyph.tintColor = ink;
            BookView.SetDisplay(_instruction, cast == false);
            BookView.SetDisplay(_readName, cast);
            BookView.SetEnabled(_readName, interactive);

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
                _ribbon[i].SetSlotColor(slotColor);
            }

            var outer = DimensionCatalog.Outer;

            for (var i = 0; i < DimensionCatalog.OUTER_COUNT; i++)
            {
                var found = _state.IsDiscovered(i);
                var entry = _entries[i];

                _entryTitles[i].text = found ? outer[i].Title : lockedTitle;
                BookView.SetDisplay(_entryHints[i], false);
                var fill = found ? _palette.SurfaceOf(outer[i].Index) : locked;
                BookView.SetEnabled(entry, interactive);
                entry.EnableInClassList(LOCKED_CLASS, found == false);
                SetEntryLooks(entry, fill, found, interactive, false, ink);

                if (found)
                {
                    _entryRunes[i].SetRune(_state.RuneOf(i), decoded: false, number: _state.NumberOf(i));
                }
                else
                {
                    _entryRunes[i].SetBlank();
                }

                _entryRunes[i].SetInk(ink);
            }

            var core = DimensionCatalog.OUTER_COUNT;
            var open = _state.CoreUnlocked;
            var visited = _state.CoreVisited;

            _entryTitles[core].text = visited ? DimensionCatalog.Core.Title : lockedTitle;
            var coreFill = visited ? _palette.SurfaceOf(core) : locked;
            BookView.SetEnabled(_entries[core], open && interactive);
            _entries[core].EnableInClassList(LOCKED_CLASS, visited == false);
            _entries[core].EnableInClassList(DASHED_CLASS, open == false);

            SetEntryLooks(
                  _entries[core]
                , coreFill
                , visited
                , open && interactive
                , open == false
                , ink
            );

            BookView.SetDisplay(_entryHints[core], open == false);
            BookView.SetDisplay(_coreGlyph, visited);

            _coreDashes.Color = _palette.Color(ConfigColorType.EntranceCardBorderLocked);
            _coreDashes.Width = CORE_DASH_WIDTH;
            _coreDashes.Radius = CORE_DASH_RADIUS;
            _coreDashes.Dash = CORE_DASH_LENGTH;
            _coreDashes.Gap = CORE_DASH_GAP;
            BookView.SetDisplay(_coreDashes, open == false);
            RenderBlink();
        }

        private void SetEntryLooks(Button entry, Color fill, bool found, bool isEnabled, bool isDashed, Color ink)
        {
            var border = isDashed ? Color.clear : _palette.Color(ConfigColorType.EntranceCardBorderLocked);
            var hoverFill = found
                ? Color.Lerp(fill, ink, _palette.Amount(ConfigAmountType.EntranceCardHoverMix))
                : _palette.Color(ConfigColorType.EntranceCardLockedHover);

            var labelInk = isEnabled ? ink : _palette.Color(ConfigColorType.EntranceCardInkDisabled);

            BookView.SetColors(entry, fill, labelInk, border);

            var hover = new ButtonLook(hoverFill, _palette.Color(ConfigColorType.CardHoverBorder), ink);
            var press = new ButtonLook(_palette.Color(ConfigColorType.ButtonPress), ink, ink);

            BookView.SetStateLooks(entry, in hover, in press, ink);
        }

        private void CreateCoreGlow()
        {
            if (_coreGlow != null)
            {
                return;
            }

            var card = this.Q<Button>($"{DIMENSION_BUTTON}{DimensionCatalog.OUTER_COUNT}");
            var row = card.parent;

            _coreGlow = new VisualElement { name = GLOW_NAME, pickingMode = PickingMode.Ignore };
            _coreGlow.AddToClassList(GLOW_CLASS);
            row.Insert(row.IndexOf(card), _coreGlow);
            card.RegisterCallback<GeometryChangedEvent>(OnCoreGeometryChanged);
        }

        private void PlaceCoreGlow()
        {
            var rect = _entries[DimensionCatalog.OUTER_COUNT].layout;

            if (float.IsNaN(rect.width))
            {
                return;
            }

            var outset = _palette.Amount(ConfigAmountType.EntranceGlowOutset);

            _coreGlow.style.left = rect.x - outset;
            _coreGlow.style.top = rect.y - outset;
            _coreGlow.style.width = rect.width + (outset * 2f);
            _coreGlow.style.height = rect.height + (outset * 2f);
        }

        private void RenderBlink()
        {
            if (_state.CoreBlink == false)
            {
                StopBlink();
                return;
            }

            if (_isBlinking == false)
            {
                _isBlinking = true;
                _blinkStart = Time.unscaledTime;
            }

            _blink ??= schedule.Execute(UpdateGlow).Every(0);
            _blink.Resume();
        }

        private void StopBlink()
        {
            _isBlinking = false;
            _blink?.Pause();

            if (_coreGlow != null)
            {
                _coreGlow.style.opacity = StyleKeyword.Null;
            }
        }

        private void UpdateGlow()
        {
            var period = _state.Timings.CoreGlowPulseMs * SECONDS_PER_MS;

            if (period <= 0f)
            {
                _coreGlow.style.opacity = StyleKeyword.Null;
                return;
            }

            var phase = Mathf.Repeat((Time.unscaledTime - _blinkStart) / period, 1f);

            _coreGlow.style.opacity = 1f - Mathf.Abs((phase * 2f) - 1f);
        }

        private void OnCoreGeometryChanged(GeometryChangedEvent evt)
        {
            PlaceCoreGlow();
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            if (_coreGlow != null)
            {
                PlaceCoreGlow();
            }

            if (_state != null)
            {
                Render();
            }
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnEntryClicked(int index)
        {
            var dimension = index < DimensionCatalog.OUTER_COUNT ? DimensionCatalog.Outer[index].Id : Dimension.Core;

            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Travel, BookLocation.Of(dimension));
        }

        private void OnReadNameClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.ReadName);
        }
    }
}
