using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Attachments;
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

namespace EncosyTower.Samples.UguiPages.Screens
{
    /// <summary>The entrance screen: pick one of the six dimensions or the core.</summary>
    /// <remarks>
    /// A page of the Screen flow. It rents an attachment set, which gives it a navigation bar page. The core card
    /// blinks when the core unlocks.
    /// </remarks>
    public class EntranceScreen
        : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnReturnToPool
    {
        private const string CORE_GLOW = "core-glow";
        private const string DASHED_NAME = "dashed";
        private const string CORE_SLOT = "core-slot";
        private const int ENTRY_COUNT = DimensionCatalog.OUTER_COUNT + 1;

        /// <summary>The background image of the screen.</summary>
        [Tooltip("The background image of the screen.")]
        [SerializeField] private Image _background;

        /// <summary>The title of the screen.</summary>
        [Tooltip("The title of the screen.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The instruction text of the screen.</summary>
        [Tooltip("The instruction text of the screen.")]
        [SerializeField] private TMP_Text _instruction;

        /// <summary>The button that reads the name of the tower, once the incantation is cast.</summary>
        [Tooltip("The button that reads the name of the tower, once the incantation is cast.")]
        [SerializeField] private Button _readName;

        /// <summary>The cards of the six dimensions and the core, in order.</summary>
        [Tooltip("The cards of the six dimensions and the core, in order.")]
        [SerializeField] private Button[] _dimensions;

        /// <summary>The titles of the cards, in order.</summary>
        [Tooltip("The titles of the cards, in order.")]
        [SerializeField] private TMP_Text[] _entryTitles;

        /// <summary>The rune views of the cards, in order.</summary>
        [Tooltip("The rune views of the cards, in order.")]
        [SerializeField] private RuneView[] _entryRunes;

        /// <summary>The rune views that show the runes gathered so far.</summary>
        [Tooltip("The rune views that show the runes gathered so far.")]
        [SerializeField] private RuneView[] _runes;

        /// <summary>The hint text of the core card.</summary>
        [Tooltip("The hint text of the core card.")]
        [SerializeField] private TMP_Text _coreHint;

        /// <summary>The glyph of the core card.</summary>
        [Tooltip("The glyph of the core card.")]
        [SerializeField] private Image _coreGlyph;

        /// <summary>The rect that holds the navigation bar page.</summary>
        [Tooltip("The rect that holds the navigation bar page.")]
        [SerializeField] private RectTransform _navigationBarHost;

        private AppState _state;
        private AttachmentSet _set;
        private Image _coreGlow;
        private Image _coreDashed;
        private Image _coreBorder;
        private RectTransform _coreGlowRect;
        private RectTransform _coreCardRect;
        private RectTransform _coreSlot;
        private Color _coreGlowColor;
        private Color _coreGlowMiddleColor;
        private Color _coreGlowFarColor;
        private Vector2 _glowCardMin;
        private Vector2 _glowCardMax;
        private Vector2 _glowCardSize;
        private float _blinkStart;
        private HaloVfx _coreHaloVfx;
        private bool _isBlinking;
        private bool _isGlowPlaced;
        private bool _isRegistered;

        /// <inheritdoc/>
        /// <remarks>
        /// Rejects the creation when the call has no page user data.
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
                _readName.onClick.AddListener(OnReadNameClicked);

                for (var i = 0; i < ENTRY_COUNT; i++)
                {
                    var index = i;

                    void OnClick()
                    {
                        OnEntryClicked(index);
                    }

                    _dimensions[i].onClick.AddListener(OnClick);
                }

                var coreCard = _dimensions[DimensionCatalog.OUTER_COUNT];

                _coreDashed = coreCard.transform.Find(DASHED_NAME).GetComponent<Image>();
                _coreBorder = BookView.FindBorder(coreCard);

                LiftCoreCard(coreCard);
                CreateCoreGlow();
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

                if (_set != null)
                {
                    _state.Attachments.Return(_set);
                    _set = null;
                }

                _state = null;
            }

            HideCoreGlow();
        }

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                Canvas.willRenderCanvases -= SyncCoreCard;
                _readName.onClick.RemoveAllListeners();

                for (var i = 0; i < ENTRY_COUNT; i++)
                {
                    _dimensions[i].onClick.RemoveAllListeners();
                }
            }
        }

        private void Update()
        {
            if (_state == null)
            {
                return;
            }

            if (_state.CoreBlink == false)
            {
                if (_isBlinking)
                {
                    HideCoreGlow();
                }

                return;
            }

            if (_isBlinking == false)
            {
                _isBlinking = true;
                _blinkStart = Time.unscaledTime;
            }

            var cardMin = _coreCardRect.offsetMin;
            var cardMax = _coreCardRect.offsetMax;

            if (_isGlowPlaced == false || cardMin != _glowCardMin || cardMax != _glowCardMax
                || _coreCardRect.rect.size != _glowCardSize)
            {
                PlaceCoreGlow(cardMin, cardMax);
            }

            var elapsed = Time.unscaledTime - _blinkStart;

            _coreHaloVfx?.SetRotationSpeeds(new Vector3(
                  _state.Config.Amount(ConfigAmountType.CoreVfxNearestDegreesPerSecond)
                , _state.Config.Amount(ConfigAmountType.CoreVfxMiddleDegreesPerSecond)
                , _state.Config.Amount(ConfigAmountType.CoreVfxFarDegreesPerSecond)
            ));
            _coreHaloVfx.Render(_coreGlowColor, _coreGlowMiddleColor, _coreGlowFarColor,
                elapsed, HaloVfx.Intensity(elapsed));
        }

        private void PlaceCoreGlow(Vector2 cardMin, Vector2 cardMax)
        {
            var size = HaloVfx.CoreSize(_coreCardRect.rect.size);

            _isGlowPlaced = true;
            _glowCardMin = cardMin;
            _glowCardMax = cardMax;
            _glowCardSize = _coreCardRect.rect.size;
            _coreGlowRect.anchorMin = _coreGlowRect.anchorMax = new Vector2(0.5f, 0.5f);
            _coreGlowRect.pivot = new Vector2(0.5f, 0.5f);
            _coreGlowRect.sizeDelta = size;
            _coreGlowRect.position = _coreCardRect.TransformPoint(_coreCardRect.rect.center);
        }

        // The glow must draw under the core card but over every other card. A card draws after the rows above it and
        // before the rows below it, so the core card leaves its row for the end of the board, behind a slot that keeps
        // its place in the layout, and the glow takes the place just before it.
        private void LiftCoreCard(Button card)
        {
            var cardRect = (RectTransform)card.transform;
            var row = cardRect.parent;
            var board = row.parent;
            var slotObject = new GameObject(CORE_SLOT, typeof(RectTransform), typeof(LayoutElement));
            var slotLayout = slotObject.GetComponent<LayoutElement>();

            _coreSlot = (RectTransform)slotObject.transform;
            _coreSlot.SetParent(row, worldPositionStays: false);
            _coreSlot.SetSiblingIndex(cardRect.GetSiblingIndex());
            _coreSlot.anchorMin = cardRect.anchorMin;
            _coreSlot.anchorMax = cardRect.anchorMax;
            _coreSlot.pivot = cardRect.pivot;
            _coreSlot.sizeDelta = cardRect.sizeDelta;
            slotObject.layer = card.gameObject.layer;

            if (card.TryGetComponent<LayoutElement>(out var cardLayout))
            {
                slotLayout.minWidth = cardLayout.minWidth;
                slotLayout.minHeight = cardLayout.minHeight;
                slotLayout.preferredWidth = cardLayout.preferredWidth;
                slotLayout.preferredHeight = cardLayout.preferredHeight;
                slotLayout.flexibleWidth = cardLayout.flexibleWidth;
                slotLayout.flexibleHeight = cardLayout.flexibleHeight;
                slotLayout.layoutPriority = cardLayout.layoutPriority;
                cardLayout.ignoreLayout = true;
            }

            if (slotLayout.minWidth < 0f && slotLayout.preferredWidth < 0f && slotLayout.flexibleWidth <= 0f)
            {
                slotLayout.preferredWidth = cardRect.sizeDelta.x;
            }

            if (slotLayout.minHeight < 0f && slotLayout.preferredHeight < 0f && slotLayout.flexibleHeight <= 0f)
            {
                slotLayout.preferredHeight = cardRect.sizeDelta.y;
            }

            cardRect.SetParent(board, worldPositionStays: false);
            cardRect.anchorMin = cardRect.anchorMax = cardRect.pivot = new Vector2(0.5f, 0.5f);
            Canvas.willRenderCanvases += SyncCoreCard;
        }

        // Runs after the layout of the frame, so the card never trails its slot.
        private void SyncCoreCard()
        {
            if (this == null || _coreSlot == null || _coreSlot.gameObject.activeInHierarchy == false)
            {
                return;
            }

            var rect = _coreSlot.rect;

            if (_coreCardRect.sizeDelta != rect.size)
            {
                _coreCardRect.sizeDelta = rect.size;
            }

            _coreCardRect.position = _coreSlot.TransformPoint(rect.center);
        }

        private void CreateCoreGlow()
        {
            var card = _dimensions[DimensionCatalog.OUTER_COUNT];
            var glowObject = new GameObject(CORE_GLOW, typeof(RectTransform), typeof(Image), typeof(LayoutElement));

            _coreCardRect = (RectTransform)card.transform;
            _coreGlowRect = (RectTransform)glowObject.transform;
            _coreGlowRect.SetParent(_coreCardRect.parent, worldPositionStays: false);
            _coreGlowRect.SetSiblingIndex(_coreCardRect.GetSiblingIndex());
            glowObject.layer = card.gameObject.layer;
            glowObject.GetComponent<LayoutElement>().ignoreLayout = true;

            _coreGlow = glowObject.GetComponent<Image>();
            _coreHaloVfx = HaloVfx.Bind(_coreGlow, rectangular: true);
            HideCoreGlow();
        }

        private void HideCoreGlow()
        {
            var color = _coreGlowColor;

            color.a = 0f;
            _isBlinking = false;
            _isGlowPlaced = false;

            if (_coreGlow != null)
            {
                _coreHaloVfx?.Render(color, Color.clear, Color.clear, 0f, 0f);
            }
        }

        private void Render()
        {
            var config = _state.Config;

            var cast = _state.IncantationCast;
            var outer = DimensionCatalog.Outer;
            var ink = config.Color(ConfigColorType.InkEntrance);
            var locked = config.Color(ConfigColorType.Locked);
            var interactive = _state.IsPopupActive == false;
            var lockedTitle = config.Text(ConfigTextType.DimensionLockedTitle);
            var titleId = cast ? ConfigTextType.EntranceTitleCast : ConfigTextType.EntranceTitleUncast;

            var page = config.Color(ConfigColorType.PageEntrance);
            var slotMix = config.Amount(ConfigAmountType.EntranceRuneSlotBorderMix);
            var slotBorder = Color.Lerp(page, ink, slotMix);

            _background.color = page;
            _title.text = config.Text(titleId);
            _title.color = ink;
            _instruction.gameObject.SetActive(cast == false);
            _instruction.color = ink;
            _readName.gameObject.SetActive(cast);
            BookView.SetGenericLooks(_readName, config);
            BookView.SetInteractable(_readName, interactive, config);

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var rune = (Rune)i;
                var hasRune = cast || _state.TryGetAcquiredRune(i, out rune);

                _runes[i].SetBorderColor(slotBorder);

                if (hasRune == false)
                {
                    _runes[i].SetBlank();
                }
                else
                {
                    _runes[i].SetRune(rune, decoded: cast, number: i + 1);
                }

                _runes[i].SetInk(ink);
            }

            for (var i = 0; i < DimensionCatalog.OUTER_COUNT; i++)
            {
                var found = _state.IsDiscovered(i);

                _entryTitles[i].text = found ? outer[i].Title : lockedTitle;
                var cardFill = found ? config.Color(outer[i].Surface) : locked;

                BookView.SetColors(_dimensions[i], cardFill, ink, config);
                _entryTitles[i].color = ink;
                SetCardLooks(_dimensions[i], cardFill, found == false, ink, config);
                BookView.SetInteractable(button: _dimensions[i], interactable: interactive, disabledAlpha: 1f);

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
            var visited = _state.CoreVisited;
            var coreLocked = _state.CoreUnlocked == false;
            var coreFill = visited ? config.Color(DimensionCatalog.Core.Surface) : locked;

            _entryTitles[core].text = visited ? DimensionCatalog.Core.Title : lockedTitle;
            BookView.SetColors(_dimensions[core], coreFill, ink, config);
            _entryTitles[core].color = ink;
            SetCardLooks(_dimensions[core], coreFill, visited == false, ink, config);
            BookView.SetInteractable(
                  button: _dimensions[core]
                , interactable: _state.CoreUnlocked && interactive
                , disabledAlpha: 1f
            );
            _coreBorder.gameObject.SetActive(coreLocked == false);
            _coreDashed.color = config.Color(ConfigColorType.EntranceCardBorderLocked);
            _coreDashed.gameObject.SetActive(coreLocked);
            _coreHint.gameObject.SetActive(coreLocked);
            _coreHint.color = ink;
            _coreGlyph.gameObject.SetActive(visited);
            _coreGlyph.color = ink;
            _coreGlowColor = config.Color(ConfigColorType.CoreVfxNearest);
            _coreGlowMiddleColor = config.Color(ConfigColorType.CoreVfxMiddle);
            _coreGlowFarColor = config.Color(ConfigColorType.CoreVfxFar);
        }

        private static void SetCardLooks(Button card, Color fill, bool isLocked, Color ink, AppConfig config)
        {
            var hoverFill = isLocked
                ? config.Color(ConfigColorType.EntranceCardLockedHover)
                : Color.Lerp(fill, ink, config.Amount(ConfigAmountType.EntranceCardHoverMix));

            var hover = new ButtonLook(hoverFill, config.Color(ConfigColorType.CardHoverBorder), ink);
            var press = new ButtonLook(config.Color(ConfigColorType.ButtonPress), ink, ink);

            BookView.SetStateLooks(card, in hover, in press, ink);
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
