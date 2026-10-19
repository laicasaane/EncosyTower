using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Messages;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Dialogs
{
    /// <summary>The map popup: travel to a dimension, the core or the entrance.</summary>
    /// <remarks>A page of the Popup flow. A travel closes the popup and shows the screen.</remarks>
    public class MapPopup : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const int NODE_COUNT = DimensionCatalog.OUTER_COUNT + 1;
        private const int ENTRANCE_MARK = NODE_COUNT;
        private const int MARK_COUNT = NODE_COUNT + 1;
        private const string MAP_LINE_NAME = "line";
        private const string OUTLINE_NAME = "current-outline";
        private const string HALO_NAME = "current-halo";
        private const string DOTTED_NAME = "dotted";
        private const string SOLID_NAME = "solid";
        private const string TITLE_NAME = "label-title";

        /// <summary>The background image of the map.</summary>
        [Tooltip("The background image of the map.")]
        [SerializeField] private Image _background;

        /// <summary>The text that names the current dimension.</summary>
        [Tooltip("The text that names the current dimension.")]
        [SerializeField] private TMP_Text _status;

        /// <summary>The text shown while the core is locked.</summary>
        [Tooltip("The text shown while the core is locked.")]
        [SerializeField] private TMP_Text _coreLock;

        /// <summary>The glyph of the core node.</summary>
        [Tooltip("The glyph of the core node.")]
        [SerializeField] private Image _coreGlyph;

        /// <summary>The button that closes the map.</summary>
        [Tooltip("The button that closes the map.")]
        [SerializeField] private Button _return;

        /// <summary>The button that travels to the entrance.</summary>
        [Tooltip("The button that travels to the entrance.")]
        [SerializeField] private Button _entrance;

        /// <summary>The backing images of the nodes, in order.</summary>
        [Tooltip("The backing images of the nodes, in order.")]
        [SerializeField] private Image[] _backings;

        /// <summary>The buttons of the dimension nodes and the core node, in order.</summary>
        [Tooltip("The buttons of the dimension nodes and the core node, in order.")]
        [SerializeField] private Button[] _nodes;

        /// <summary>The labels of the nodes, in order.</summary>
        [Tooltip("The labels of the nodes, in order.")]
        [SerializeField] private TMP_Text[] _labels;

        /// <summary>The rune views of the nodes, in order.</summary>
        [Tooltip("The rune views of the nodes, in order.")]
        [SerializeField] private RuneView[] _runes;

        private AppState _state;
        private Image[] _borders;
        private Image[] _outlines;
        private Image[] _halos;
        private Image[] _lines;
        private TMP_Text[] _titles;
        private Image _entranceDotted;
        private Image _entranceSolid;
        private TMP_Text _entranceLabel;
        private RectTransform _coreBacking;
        private RectTransform _coreNode;
        private Vector2 _coreBackingHintSize;
        private Vector2 _coreBackingOpenSize;
        private Vector2 _coreNodeHintPosition;
        private Dimension? _statusId;
        private string _statusDimension;
        private bool _isRegistered;

        /// <inheritdoc/>
        /// <remarks>
        /// It rejects the creation when the call has no page user data.
        /// </remarks>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _return.onClick.AddListener(OnReturnClicked);
                _entrance.onClick.AddListener(OnEntranceClicked);

                for (var i = 0; i < NODE_COUNT; i++)
                {
                    var index = i;

                    void OnClick()
                    {
                        OnNodeClicked(index);
                    }

                    _nodes[i].onClick.AddListener(OnClick);
                }

                BindChildren();
            }

            _state = data.State;
            _state.Changed += OnStateChanged;

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

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _return.onClick.RemoveListener(OnReturnClicked);
                _entrance.onClick.RemoveListener(OnEntranceClicked);

                for (var i = 0; i < NODE_COUNT; i++)
                {
                    _nodes[i].onClick.RemoveAllListeners();
                }
            }
        }

        private void BindChildren()
        {
            var core = DimensionCatalog.OUTER_COUNT;

            _borders = new Image[NODE_COUNT];
            _outlines = new Image[MARK_COUNT];
            _halos = new Image[MARK_COUNT];

            for (var i = 0; i < NODE_COUNT; i++)
            {
                var node = _nodes[i];

                _borders[i] = BookView.FindBorder(node);
                _outlines[i] = node.transform.Find(OUTLINE_NAME).GetComponent<Image>();
                _halos[i] = node.transform.Find(HALO_NAME).GetComponent<Image>();
            }

            var entrance = _entrance.transform;

            _outlines[ENTRANCE_MARK] = entrance.Find(OUTLINE_NAME).GetComponent<Image>();
            _halos[ENTRANCE_MARK] = entrance.Find(HALO_NAME).GetComponent<Image>();
            _entranceDotted = entrance.Find(DOTTED_NAME).GetComponent<Image>();
            _entranceSolid = entrance.Find(SOLID_NAME).GetComponent<Image>();
            _entranceLabel = _entrance.GetComponentInChildren<TMP_Text>();

            _coreBacking = (RectTransform)_backings[core].transform;
            _coreNode = (RectTransform)_nodes[core].transform;
            _coreBackingHintSize = _coreBacking.sizeDelta;
            _coreNodeHintPosition = _coreNode.anchoredPosition;

            var nodeSize = _coreNode.sizeDelta;
            var padding = _coreBackingHintSize.x - nodeSize.x;

            _coreBackingOpenSize = new Vector2(_coreBackingHintSize.x, nodeSize.y + padding);

            _lines = BookView.FindChildren<Image>(this, MAP_LINE_NAME);
            _titles = BookView.FindChildren<TMP_Text>(this, TITLE_NAME);
        }

        private void SetMarks(int mark, bool current, Color ink, Color halo)
        {
            _outlines[mark].color = ink;
            _outlines[mark].gameObject.SetActive(current);
            _halos[mark].color = halo;
            _halos[mark].gameObject.SetActive(current);
        }

        private void Render()
        {
            var config = _state.Config;

            var outer = DimensionCatalog.Outer;
            var current = _state.CurrentLocation;
            var ink = config.Color(ConfigColorType.Ink);
            var pageMap = config.Color(ConfigColorType.PageMap);
            var navigationBar = config.Color(ConfigColorType.NavigationBar);
            var buttonBorder = config.Color(ConfigColorType.ButtonBorder);
            var borderMix = config.Amount(ConfigAmountType.MapNodeBorderMix);
            var lockedTitle = config.Text(ConfigTextType.DimensionLockedTitle);
            var halo = ink;

            halo.a = config.Amount(ConfigAmountType.MapCurrentHaloAlpha);

            _background.color = pageMap;
            BookView.SetColors(_lines, config.Color(ConfigColorType.MapLine));
            BookView.SetColors(_titles, ink);

            for (var i = 0; i < NODE_COUNT; i++)
            {
                _backings[i].color = pageMap;
            }

            for (var i = 0; i < DimensionCatalog.OUTER_COUNT; i++)
            {
                var found = _state.IsDiscovered(i);
                var isCurrent = current == BookLocation.Of(outer[i].Id);
                var fill = found ? config.Color(outer[i].Surface) : navigationBar;

                _labels[i].text = found ? outer[i].Title : lockedTitle;
                _labels[i].color = ink;
                _labels[i].fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;
                BookView.SetColors(_nodes[i], fill, ink, config);
                _borders[i].color = found ? Color.Lerp(fill, ink, borderMix) : buttonBorder;
                SetNodeLooks(_nodes[i], fill, found, ink, config);
                BookView.SetInteractable(button: _nodes[i], interactable: isCurrent == false, disabledAlpha: 1f);
                SetMarks(i, isCurrent, ink, halo);

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
            var coreCurrent = current == BookLocation.Of(Dimension.Core);
            var open = _state.CoreUnlocked;
            var visited = _state.CoreVisited;
            var coreFill = navigationBar;
            var coreBorder = buttonBorder;

            if (open == false)
            {
                coreFill = pageMap;
            }
            else if (visited)
            {
                var coreSurface = config.Color(DimensionCatalog.Core.Surface);

                coreFill = coreSurface;
                coreBorder = Color.Lerp(coreSurface, ink, borderMix);
            }

            var coreAlignment = visited ? HorizontalAlignmentOptions.Left : HorizontalAlignmentOptions.Center;

            _labels[core].text = visited ? DimensionCatalog.Core.Title : lockedTitle;
            _labels[core].color = ink;
            _labels[core].fontStyle = coreCurrent ? FontStyles.Bold : FontStyles.Normal;
            _labels[core].horizontalAlignment = coreAlignment;
            BookView.SetColors(_nodes[core], coreFill, ink, config);
            _borders[core].color = coreBorder;
            BookView.SetGenericLooks(_nodes[core], config);
            BookView.SetInteractable(
                  button: _nodes[core]
                , interactable: open && coreCurrent == false
                , disabledAlpha: open ? 1f : config.Amount(ConfigAmountType.MapCoreLockedAlpha)
            );
            SetMarks(core, coreCurrent, ink, halo);
            _coreBacking.sizeDelta = open ? _coreBackingOpenSize : _coreBackingHintSize;
            _coreNode.anchoredPosition = open ? _coreBacking.anchoredPosition : _coreNodeHintPosition;
            _coreLock.gameObject.SetActive(open == false);
            _coreLock.color = ink;
            _coreGlyph.gameObject.SetActive(visited);
            _coreGlyph.color = ink;

            var isEntrance = _state.CurrentScreen == ScreenPage.Entrance;
            var entranceId = isEntrance ? ConfigColorType.PageEntrance : ConfigColorType.NavigationBar;
            var entranceBorder = config.Color(ConfigColorType.MapEntranceBorder);

            BookView.SetColors(_entrance, config.Color(entranceId), ink, config);
            BookView.SetInteractable(button: _entrance, interactable: isEntrance == false, disabledAlpha: 1f);
            _entranceLabel.fontStyle = isEntrance ? FontStyles.Bold : FontStyles.Normal;
            _entranceDotted.color = entranceBorder;
            _entranceDotted.gameObject.SetActive(isEntrance == false);
            _entranceSolid.color = entranceBorder;
            _entranceSolid.gameObject.SetActive(isEntrance);
            SetMarks(ENTRANCE_MARK, isEntrance, ink, halo);
            BookView.SetColors(_return, navigationBar, ink, config);

            _status.color = ink;
            _status.text = _state.CurrentScreen switch {
                ScreenPage.Entrance => config.Text(ConfigTextType.MapStatusEntrance),
                ScreenPage.CoreDimension => config.Text(ConfigTextType.MapStatusCore),
                ScreenPage.Dimension => DimensionStatus(config),
                _ => string.Empty,
            };
        }

        private static void SetNodeLooks(Button node, Color fill, bool found, Color ink, AppConfig config)
        {
            if (found == false)
            {
                BookView.SetGenericLooks(node, config);
                return;
            }

            var hoverFill = Color.Lerp(fill, ink, config.Amount(ConfigAmountType.MapNodeHoverMix));
            var hover = new ButtonLook(hoverFill, ink, ink);
            var press = new ButtonLook(config.Color(ConfigColorType.ButtonPress), ink, ink);

            BookView.SetStateLooks(node, in hover, in press, ink);
        }

        private string DimensionStatus(AppConfig config)
        {
            var dimension = _state.CurrentDimensionData;

            if (_statusId != dimension.Id)
            {
                _statusId = dimension.Id;
                _statusDimension = string.Format(config.Text(ConfigTextType.MapStatusDimension), dimension.Title);
            }

            return _statusDimension;
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
