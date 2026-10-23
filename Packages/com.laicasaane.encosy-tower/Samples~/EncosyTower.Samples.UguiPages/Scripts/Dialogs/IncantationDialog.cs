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
    /// <summary>The incantation dialog: cast the spell with the six runes, then read the name of the tower.</summary>
    /// <remarks>
    /// A page of the Popup flow with its own transition,
    /// <see cref="EncosyTower.Samples.UguiPages.Transitions.TowerIncantationTransition"/>, which moves the runes
    /// while it casts. It rejects the creation unless the player visited the core and holds six runes.
    /// </remarks>
    public class IncantationDialog : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        /// <summary>The backdrop behind the dialog, shown when it is the first popup.</summary>
        [Tooltip("The backdrop behind the dialog, shown when it is the first popup.")]
        [SerializeField] private GameObject _backdrop;

        /// <summary>The canvas group that fades the dialog.</summary>
        [Tooltip("The canvas group that fades the dialog.")]
        [SerializeField] private CanvasGroup _canvasGroup;

        /// <summary>The panel of the dialog.</summary>
        [Tooltip("The panel of the dialog.")]
        [SerializeField] private Image _panel;

        /// <summary>The rune views of the six runes.</summary>
        [Tooltip("The rune views of the six runes.")]
        [SerializeField] private RuneView[] _runes;

        /// <summary>The body text of the dialog.</summary>
        [Tooltip("The body text of the dialog.")]
        [SerializeField] private TMP_Text _body;

        /// <summary>The button that closes the dialog and returns to the entrance.</summary>
        [Tooltip("The button that closes the dialog and returns to the entrance.")]
        [SerializeField] private Button _writeInTheBook;

        private const float FACE_FLIP_DEGREES = 90f;
        private const string TITLE_NAME = "label-title";

        private readonly Rune[] _sourceRunes = new Rune[RuneCatalog.COUNT];
        private readonly bool[] _decoded = new bool[RuneCatalog.COUNT];

        private AppState _state;
        private IncantationAmounts _incantation;
        private Color _ink;
        private Color _castGold;
        private TMP_Text[] _titles;
        private bool _terminal;
        private bool _isRegistered;

        internal bool IsCommitted => _state != null && _state.IncantationCast;

        internal float CastSeconds => _incantation.CastSeconds;

        internal float FadeSeconds => _incantation.FadeSeconds;

        /// <inheritdoc/>
        /// <remarks>
        /// It rejects the creation unless the player visited the core and holds six runes.
        /// </remarks>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || data.State.CoreVisited == false
            )
            {
                return UnityTask.FromResult(false);
            }

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                if (data.State.TryGetAcquiredRune(i, out var rune) == false)
                {
                    return UnityTask.FromResult(false);
                }

                _sourceRunes[i] = rune;
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _writeInTheBook.onClick.AddListener(OnWriteInTheBookClicked);

                _titles = BookView.FindChildren<TMP_Text>(this, TITLE_NAME);
            }

            UnsubscribeState();

            _state = data.State;
            _state.Changed += OnStateChanged;
            _state.ThemeChanged += OnThemeChanged;

            var config = _state.Config;

            _incantation = IncantationAmounts.Read(config);

            DialogBackdrop.Bind(_backdrop, data.Dismiss, _state.PopupDepth > 0);
            RefreshColors();
            ResetFaces();
            SetMotionMode(casting: _state.IncantationCast == false);

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

            if (_isRegistered)
            {
                _writeInTheBook.onClick.RemoveListener(OnWriteInTheBookClicked);
            }
        }

        private void UnsubscribeState()
        {
            if (_state != null)
            {
                _state.Changed -= OnStateChanged;
                _state.ThemeChanged -= OnThemeChanged;
                _state = null;
            }
        }

        internal void SetMotionMode(bool casting)
        {
            RefreshColors();

            if (casting)
            {
                ApplyCastPose(0f);
            }
            else
            {
                LayoutPose(1f);
                ApplyOpacity(1f);
            }

            RenderWriteInTheBook();
        }

        internal void ApplyCastPose(float p)
        {
            var clamped = Mathf.Clamp01(p);

            LayoutPose(clamped);
            ApplyOpacity(Mathf.Clamp01(clamped / _incantation.FadeEnd));
        }

        internal void ApplyOpacity(float value)
        {
            _canvasGroup.alpha = value;
        }

        private void LayoutPose(float p)
        {
            var terminal = p >= 1f;
            var pitch = _incantation.SlotPitch * _incantation.RowWidth;

            _terminal = terminal;

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var rune = _sourceRunes[i];
                var runeIndex = (int)rune;
                var moveBegin = _incantation.MoveStart + _incantation.MoveStagger * i;
                var moveSpan = _incantation.MoveSpan - _incantation.MoveStagger * i;
                var turnBegin = _incantation.TurnStart + _incantation.TurnStagger * runeIndex;
                var t = Mathf.Clamp01((p - moveBegin) / moveSpan);
                var f = Mathf.Clamp01((p - turnBegin) / _incantation.TurnSpan);
                var turn = f * _incantation.TurnDegrees;
                var decoded = terminal || turn >= FACE_FLIP_DEGREES;
                var squash = terminal ? 1f : Mathf.Abs(Mathf.Cos(turn * Mathf.Deg2Rad));
                var left = (i + (runeIndex - i) * t) * pitch;
                var top = _incantation.RowTop - Mathf.Sin(Mathf.PI * t) * _incantation.ArcHeight;
                var view = (RectTransform)_runes[i].transform;
                var rect = view.rect;

                view.anchoredPosition = new Vector2(left + rect.width * 0.5f, -(top + rect.height * 0.5f));
                view.localScale = new Vector3(squash, 1f, 1f);

                if (_decoded[i] != decoded)
                {
                    _decoded[i] = decoded;
                    _runes[i].SetRune(rune: rune, decoded: decoded, number: i + 1);
                }
            }

            _writeInTheBook.gameObject.SetActive(terminal && IsCommitted);
        }

        private void RefreshColors()
        {
            if (_state == null)
            {
                return;
            }

            var config = _state.Config;

            _ink = config.Color(ConfigColorType.Ink);
            _castGold = config.Color(ConfigColorType.CastGold);

            _panel.color = config.Color(ConfigColorType.PageEcho);
            _body.color = _ink;
            BookView.SetColors(_titles, _ink);
            BookView.SetBackdrop(_backdrop, config);
            BookView.SetBorders(this, config);
            BookView.SetColors(_writeInTheBook, config.Color(ConfigColorType.NavigationBar), _ink, config);

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                _runes[i].SetInk(_castGold);
            }
        }

        private void ResetFaces()
        {
            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var view = _runes[i];

                _decoded[i] = false;
                view.SetRune(rune: _sourceRunes[i], decoded: false, number: i + 1);
                view.SetSolidBorder(solid: true);
                view.SetInk(_castGold);
            }
        }

        private void RenderWriteInTheBook()
        {
            _writeInTheBook.gameObject.SetActive(_terminal && IsCommitted);
        }

        private void OnStateChanged()
        {
            RenderWriteInTheBook();
        }

        private void OnThemeChanged()
        {
            RefreshColors();
        }

        private void OnWriteInTheBookClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.WriteInTheBook);
        }

        /// <summary>The configured amounts that control the incantation layout and animation.</summary>
        private readonly record struct IncantationAmounts(
              float SlotPitch
            , float RowWidth
            , float RowTop
            , float ArcHeight
            , float FadeEnd
            , float MoveStart
            , float MoveStagger
            , float MoveSpan
            , float TurnStart
            , float TurnStagger
            , float TurnSpan
            , float TurnDegrees
            , float CastSeconds
            , float FadeSeconds
        )
        {
            public static IncantationAmounts Read(AppConfig config)
                => new(
                      SlotPitch: config.Amount(ConfigAmountType.IncantationSlotPitch)
                    , RowWidth: config.Amount(ConfigAmountType.IncantationRowWidth)
                    , RowTop: config.Amount(ConfigAmountType.IncantationRowTop)
                    , ArcHeight: config.Amount(ConfigAmountType.IncantationArcHeight)
                    , FadeEnd: config.Amount(ConfigAmountType.IncantationFadeEnd)
                    , MoveStart: config.Amount(ConfigAmountType.IncantationMoveStart)
                    , MoveStagger: config.Amount(ConfigAmountType.IncantationMoveStagger)
                    , MoveSpan: config.Amount(ConfigAmountType.IncantationMoveSpan)
                    , TurnStart: config.Amount(ConfigAmountType.IncantationTurnStart)
                    , TurnStagger: config.Amount(ConfigAmountType.IncantationTurnStagger)
                    , TurnSpan: config.Amount(ConfigAmountType.IncantationTurnSpan)
                    , TurnDegrees: config.Amount(ConfigAmountType.IncantationTurnDegrees)
                    , CastSeconds: config.Amount(ConfigAmountType.IncantationCastSeconds)
                    , FadeSeconds: config.Amount(ConfigAmountType.IncantationFadeSeconds)
                );
        }
    }
}
