using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.State;
using EncosyTower.Samples.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Dialogs
{
    /// <summary>The map popup: travel to a dimension, the core or the entrance.</summary>
    /// <remarks>
    /// A page of the Popup flow. A travel closes the popup and shows the screen. It keeps the page below it with
    /// <see cref="PopupPageOptions.KeepLowerPage"/>.
    /// </remarks>
    [UxmlElement]
    public partial class MapPopup : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string RETURN_BUTTON = "button-close-map";
        private const string STATUS_LABEL = "label-current";
        private const string ENTRANCE_BUTTON = "button-node-entrance";
        private const string NODE = "node-";
        private const string NODE_LABEL = "label-node-";
        private const string NODE_RUNE = "node-rune-";
        private const string CORE_GLYPH = "core-glyph";
        private const string CORE_LOCK = "core-lock";
        private const string NODE_WRAP = "node-wrap-";
        private const string NODE_HALO = "node-halo-";
        private const string NODE_OUTLINE = "node-outline-";
        private const string ENTRANCE_WRAP = "entrance-wrap";
        private const string ENTRANCE_DOTS = "entrance-dots";
        private const string ENTRANCE_HALO = "entrance-halo";
        private const string ENTRANCE_OUTLINE = "entrance-outline";
        private const string CURRENT_CLASS = "tower-book-node--current";
        private const string LOCKED_CLASS = "tower-book-node--locked";
        private const string CORE_LOCKED_CLASS = "tower-book-node--core-locked";
        private const string CENTERED_CLASS = "tower-book-node--centered";
        private const string WRAP_CURRENT_CLASS = "tower-book-map__node--current";
        private const int NODE_COUNT = DimensionCatalog.OUTER_COUNT + 1;
        private const int ENTRANCE_SLOT = NODE_COUNT;
        private const int SLOT_COUNT = NODE_COUNT + 1;
        private const float CORE_INSET = 2f;
        private const float ENTRANCE_RADIUS = 8f;

        private readonly Button[] _nodes = new Button[NODE_COUNT];
        private readonly Label[] _labels = new Label[NODE_COUNT];
        private readonly RuneView[] _runes = new RuneView[DimensionCatalog.OUTER_COUNT];
        private readonly VisualElement[] _wraps = new VisualElement[SLOT_COUNT];
        private readonly VisualElement[] _halos = new VisualElement[SLOT_COUNT];
        private readonly VisualElement[] _outlines = new VisualElement[SLOT_COUNT];
        private readonly MapStyle _map = new();
        private readonly PaletteStyle _palette = new();

        private Button _return;
        private Button _entrance;
        private DottedBorder _entranceDots;
        private Label _status;
        private Label _coreLock;
        private Image _coreGlyph;
        private AppState _state;
        private Dimension? _statusId;
        private string _statusDimension;

        /// <summary>Creates the page element.</summary>
        public MapPopup()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public override PageOptions PageOptions => PopupPageOptions.KeepLowerPage;

        private void SetNodeColors(Button button, bool filled, Color fill, Color border, bool isMapNode)
        {
            if (filled)
            {
                var ink = _palette.Color(ConfigColorType.Ink);

                BookView.SetColors(button, fill, ink, border);

                var hoverFill = isMapNode
                    ? Color.Lerp(fill, ink, _palette.Amount(ConfigAmountType.MapNodeHoverMix))
                    : _palette.Color(ConfigColorType.ButtonHover);

                var hover = new ButtonLook(hoverFill, ink, ink);
                var press = new ButtonLook(_palette.Color(ConfigColorType.ButtonPress), ink, ink);

                BookView.SetStateLooks(button, in hover, in press, ink);
                return;
            }

            BookView.ClearStateLooks(button);

            button.style.backgroundColor = StyleKeyword.Null;
            button.style.color = StyleKeyword.Null;
            button.style.borderTopColor = StyleKeyword.Null;
            button.style.borderRightColor = StyleKeyword.Null;
            button.style.borderBottomColor = StyleKeyword.Null;
            button.style.borderLeftColor = StyleKeyword.Null;
        }

        private static void SetBorderWidth(VisualElement element, float width)
        {
            element.style.borderTopWidth = width;
            element.style.borderRightWidth = width;
            element.style.borderBottomWidth = width;
            element.style.borderLeftWidth = width;
        }

        private static void SetRing(VisualElement ring, Color color, float width, float offset, float inset)
        {
            var edge = inset - offset - width;

            ring.style.left = edge;
            ring.style.top = edge;
            ring.style.right = edge;
            ring.style.bottom = edge;
            SetBorderWidth(ring, width);
            BookView.SetBorder(ring, color);
        }

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_status == null)
            {
                Query();
            }

            _state = data.State;
            _state.Changed += OnStateChanged;

            PlaceNodes();
            Render();
            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_state != null)
            {
                _state.Changed -= OnStateChanged;
                _state = null;
            }
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the backdrop and the panel with a <c>PopupTransition</c> over the popup transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new PopupTransition(slot, _state.Timings.PopupTransitionMs);

        private void Query()
        {
            _return = this.Q<Button>(RETURN_BUTTON);
            _entrance = this.Q<Button>(ENTRANCE_BUTTON);
            _status = this.Q<Label>(STATUS_LABEL);
            _coreLock = this.Q<Label>(CORE_LOCK);
            _coreGlyph = this.Q<Image>(CORE_GLYPH);
            _coreGlyph.image = Resources.Load<Texture2D>(DimensionCatalog.Core.MotifKey.Value);

            _entranceDots = this.Q<DottedBorder>(ENTRANCE_DOTS);
            _wraps[ENTRANCE_SLOT] = this.Q<VisualElement>(ENTRANCE_WRAP);
            _halos[ENTRANCE_SLOT] = this.Q<VisualElement>(ENTRANCE_HALO);
            _outlines[ENTRANCE_SLOT] = this.Q<VisualElement>(ENTRANCE_OUTLINE);

            _return.clicked += OnReturnClicked;
            _entrance.clicked += OnEntranceClicked;

            for (var i = 0; i < NODE_COUNT; i++)
            {
                var button = this.Q<Button>($"{NODE}{i}");
                var wrap = this.Q<VisualElement>($"{NODE_WRAP}{i}");

                _nodes[i] = button;
                _labels[i] = button.Q<Label>($"{NODE_LABEL}{i}");
                _wraps[i] = wrap;
                _halos[i] = this.Q<VisualElement>($"{NODE_HALO}{i}");
                _outlines[i] = this.Q<VisualElement>($"{NODE_OUTLINE}{i}");

                wrap.style.translate = new Translate(
                      new Length(-50f, LengthUnit.Percent)
                    , new Length(-50f, LengthUnit.Percent)
                );

                if (i < DimensionCatalog.OUTER_COUNT)
                {
                    _runes[i] = button.Q<RuneView>($"{NODE_RUNE}{i}");
                }

                var index = i;

                void OnClick()
                {
                    OnNodeClicked(index);
                }

                button.clicked += OnClick;
            }
        }

        private void PlaceNodes()
        {
            for (var i = 0; i < NODE_COUNT; i++)
            {
                var position = _map.PositionOf(i);
                var wrap = _wraps[i];

                wrap.style.left = new Length(position.x, LengthUnit.Percent);
                wrap.style.top = new Length(position.y, LengthUnit.Percent);
            }
        }

        private void Render()
        {
            var resources = _state.Resources;
            var outer = DimensionCatalog.Outer;
            var currentLocation = _state.CurrentLocation;
            var lockedTitle = resources.Get(ConfigTextType.DimensionLockedTitle);
            var ink = _palette.Color(ConfigColorType.Ink);
            var mix = _palette.Amount(ConfigAmountType.MapNodeBorderMix);

            _coreGlyph.tintColor = ink;

            BookView.AttachStates(this, _palette);

            for (var i = 0; i < DimensionCatalog.OUTER_COUNT; i++)
            {
                var found = _state.IsDiscovered(i);
                var current = currentLocation == BookLocation.Of(outer[i].Id);
                var button = _nodes[i];
                var surface = _palette.SurfaceOf(outer[i].Index);

                _labels[i].text = found ? outer[i].Title : lockedTitle;
                SetNodeColors(button, found, surface, Color.Lerp(surface, ink, mix), isMapNode: true);
                button.EnableInClassList(CURRENT_CLASS, current);
                button.EnableInClassList(LOCKED_CLASS, found == false);
                BookView.SetEnabled(button, current == false);
                SetCurrent(slot: i, current: current, inset: 0f);

                if (found)
                {
                    _runes[i].SetRune(_state.RuneOf(i), decoded: false, number: _state.NumberOf(i));
                }
                else
                {
                    _runes[i].SetBlank();
                }

                _runes[i].SetInk(ink);
            }

            var core = DimensionCatalog.OUTER_COUNT;
            var coreCurrent = currentLocation == BookLocation.Of(Dimension.Core);
            var open = _state.CoreUnlocked;
            var visited = _state.CoreVisited;
            var coreSurface = _palette.SurfaceOf(core);
            var coreFill = coreSurface;

            _labels[core].text = visited ? DimensionCatalog.Core.Title : lockedTitle;
            SetNodeColors(_nodes[core], visited, coreFill, Color.Lerp(coreSurface, ink, mix), isMapNode: false);
            _nodes[core].EnableInClassList(CURRENT_CLASS, coreCurrent);
            _nodes[core].EnableInClassList(LOCKED_CLASS, visited == false);
            _nodes[core].EnableInClassList(CORE_LOCKED_CLASS, open == false);
            _nodes[core].EnableInClassList(CENTERED_CLASS, visited == false);
            BookView.SetEnabled(_nodes[core], open && coreCurrent == false);
            SetCurrent(slot: core, current: coreCurrent, inset: CORE_INSET);
            BookView.SetDisplay(_coreLock, open == false);
            BookView.SetDisplay(_coreGlyph, visited);

            var isEntrance = _state.CurrentScreen == ScreenPage.Entrance;
            var entranceBorder = _palette.Color(ConfigColorType.MapEntranceBorder);
            var entranceWidth = _palette.Amount(ConfigAmountType.MapEntranceBorderWidth);

            _entrance.EnableInClassList(CURRENT_CLASS, isEntrance);

            SetNodeColors(
                  _entrance
                , isEntrance
                , _palette.Color(ConfigColorType.PageEntrance)
                , entranceBorder
                , isMapNode: false
            );
            SetBorderWidth(_entrance, entranceWidth);
            BookView.SetEnabled(_entrance, isEntrance == false);

            _entranceDots.Color = entranceBorder;
            _entranceDots.Width = entranceWidth;
            _entranceDots.Radius = ENTRANCE_RADIUS;
            _entranceDots.Dash = entranceWidth;
            _entranceDots.Gap = entranceWidth;
            BookView.SetDisplay(_entranceDots, isEntrance == false);
            SetCurrent(slot: ENTRANCE_SLOT, current: isEntrance, inset: 0f);

            _status.text = _state.CurrentScreen switch {
                ScreenPage.Entrance => resources.Get(ConfigTextType.MapStatusEntrance),
                ScreenPage.CoreDimension => resources.Get(ConfigTextType.MapStatusCore),
                ScreenPage.Dimension => DimensionStatus(in resources),
                _ => string.Empty,
            };
        }

        private string DimensionStatus(in AppViewResources resources)
        {
            var dimension = _state.CurrentDimensionData;

            if (_statusId != dimension.Id)
            {
                _statusId = dimension.Id;
                _statusDimension = resources.Format(ConfigTextType.MapStatusDimension, dimension.Title);
            }

            return _statusDimension;
        }

        private void SetCurrent(int slot, bool current, float inset)
        {
            _wraps[slot].EnableInClassList(WRAP_CURRENT_CLASS, current);

            if (current == false)
            {
                return;
            }

            var ink = _palette.Color(ConfigColorType.Ink);
            var haloColor = ink;

            haloColor.a *= _palette.Amount(ConfigAmountType.MapCurrentHaloAlpha);

            SetRing(
                  _halos[slot]
                , haloColor
                , _palette.Amount(ConfigAmountType.MapCurrentHaloWidth)
                , _palette.Amount(ConfigAmountType.MapCurrentHaloOffset)
                , inset
            );

            SetRing(
                  _outlines[slot]
                , ink
                , _palette.Amount(ConfigAmountType.MapCurrentOutlineWidth)
                , _palette.Amount(ConfigAmountType.MapCurrentOutlineOffset)
                , inset
            );
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var customStyle = evt.customStyle;

            _palette.Read(customStyle, ThemeAPI.IsDark(this));
            _map.Read(customStyle);

            if (_state != null)
            {
                PlaceNodes();
                Render();
            }
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void OnReturnClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.CloseMap);
        }

        private void OnEntranceClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.MapTravel, BookLocation.Entrance);
        }

        private void OnNodeClicked(int index)
        {
            var dimension = index < DimensionCatalog.OUTER_COUNT ? DimensionCatalog.Outer[index].Id : Dimension.Core;

            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.MapTravel, BookLocation.Of(dimension));
        }
    }
}
