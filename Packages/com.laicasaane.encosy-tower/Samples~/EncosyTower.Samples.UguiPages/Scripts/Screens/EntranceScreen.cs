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
        private Color _coreGlowColor;
        private Vector2 _glowCardMin;
        private Vector2 _glowCardMax;
        private float _blinkStart;
        private float _glowOutset;
        private float _glowPulseSeconds;
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
                _glowOutset = _state.Config.Amount(ConfigAmountType.EntranceGlowOutset);
                _glowPulseSeconds = _state.Config.Amount(ConfigAmountType.EntranceGlowPulseSeconds);
            }

            var cardMin = _coreCardRect.offsetMin;
            var cardMax = _coreCardRect.offsetMax;

            if (_isGlowPlaced == false || cardMin != _glowCardMin || cardMax != _glowCardMax)
            {
                PlaceCoreGlow(cardMin, cardMax);
            }

            var phase = Mathf.Repeat((Time.unscaledTime - _blinkStart) / _glowPulseSeconds, 1f);
            var color = _coreGlowColor;

            color.a *= 1f - Mathf.Abs((phase * 2f) - 1f);

            _coreGlow.color = color;
        }

        private void PlaceCoreGlow(Vector2 cardMin, Vector2 cardMax)
        {
            var outset = new Vector2(_glowOutset, _glowOutset);

            _isGlowPlaced = true;
            _glowCardMin = cardMin;
            _glowCardMax = cardMax;

            _coreGlowRect.anchorMin = _coreCardRect.anchorMin;
            _coreGlowRect.anchorMax = _coreCardRect.anchorMax;
            _coreGlowRect.pivot = _coreCardRect.pivot;
            _coreGlowRect.offsetMin = cardMin - outset;
            _coreGlowRect.offsetMax = cardMax + outset;
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
            _coreGlow.sprite = card.image.sprite;
            _coreGlow.type = card.image.type;
            _coreGlow.raycastTarget = false;
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
                _coreGlow.color = color;
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
            _coreGlowColor = config.Color(ConfigColorType.CoreGlow);
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
