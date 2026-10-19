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
    /// <summary>The incantation dialog: cast the spell with the six runes, then read the name of the tower.</summary>
    /// <remarks>
    /// A page of the Popup flow with its own transition,
    /// <see cref="EncosyTower.Samples.UitkPages.Transitions.TowerIncantationTransition"/>, which moves the runes
    /// while it casts. It keeps the page below it with <see cref="PopupPageOptions.KeepLowerPage"/>.
    /// </remarks>
    [UxmlElement]
    public partial class IncantationDialog : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        private const string BACKDROP = "backdrop";
        private const string ROOT = "root";
        private const string RUNE_PREFIX = "rune-";
        private const string WRITE_IN_THE_BOOK_BUTTON = "button-write-in-the-book";
        private const string NESTED_CLASS = "tower-book-popup--nested";

        private const float FACE_FLIP_DEGREES = 90f;

        private static readonly CustomStyleProperty<float> s_slotPitch
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationSlotPitch);
        private static readonly CustomStyleProperty<float> s_rowWidth
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationRowWidth);
        private static readonly CustomStyleProperty<float> s_rowTop
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationRowTop);
        private static readonly CustomStyleProperty<float> s_arcHeight
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationArcHeight);
        private static readonly CustomStyleProperty<float> s_fadeEnd
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationFadeEnd);
        private static readonly CustomStyleProperty<float> s_moveStart
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationMoveStart);
        private static readonly CustomStyleProperty<float> s_moveStagger
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationMoveStagger);
        private static readonly CustomStyleProperty<float> s_moveSpan
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationMoveSpan);
        private static readonly CustomStyleProperty<float> s_turnStart
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationTurnStart);
        private static readonly CustomStyleProperty<float> s_turnStagger
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationTurnStagger);
        private static readonly CustomStyleProperty<float> s_turnSpan
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationTurnSpan);
        private static readonly CustomStyleProperty<float> s_turnDegrees
            = ConfigNameAPI.AmountProperty(ConfigAmountType.IncantationTurnDegrees);

        private readonly RuneView[] _runes = new RuneView[RuneCatalog.COUNT];
        private readonly Rune[] _sourceRunes = new Rune[RuneCatalog.COUNT];
        private readonly bool[] _decoded = new bool[RuneCatalog.COUNT];
        private readonly PaletteStyle _palette = new();

        private VisualElement _backdrop;
        private VisualElement _root;
        private Button _writeInTheBook;
        private AppState _state;
        private float _slotPitch;
        private float _rowWidth;
        private float _rowTop;
        private float _arcHeight;
        private float _fadeEnd;
        private float _moveStart;
        private float _moveStagger;
        private float _moveSpan;
        private float _turnStart;
        private float _turnStagger;
        private float _turnSpan;
        private float _turnDegrees;
        private float _pose;
        private bool _casting;
        private bool _terminal;

        /// <summary>Creates the page element.</summary>
        public IncantationDialog()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public override PageOptions PageOptions => PopupPageOptions.KeepLowerPage;

        internal bool IsCommitted => _state != null && _state.IncantationCast;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || data.State.CoreVisited == false
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_root == null)
            {
                _backdrop = this.Q(BACKDROP);
                _root = this.Q(ROOT);
                _writeInTheBook = this.Q<Button>(WRITE_IN_THE_BOOK_BUTTON);

                for (var i = 0; i < RuneCatalog.COUNT; i++)
                {
                    _runes[i] = this.Q<RuneView>($"{RUNE_PREFIX}{i}");
                }

                _writeInTheBook.clicked += OnWriteInTheBookClicked;
            }

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                if (data.State.TryGetAcquiredRune(i, out var rune) == false)
                {
                    return UnityTask.FromResult(false);
                }

                _sourceRunes[i] = rune;
            }

            _state = data.State;
            _state.Changed += OnStateChanged;

            _root.style.backgroundColor = _palette.Color(ConfigColorType.PageEcho);
            BookView.AttachStates(this, _palette);
            ResetFaces();
            EnableInClassList(NESTED_CLASS, _state.PopupDepth > 0);
            SetMotionMode(casting: _state.IncantationCast == false);

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
        /// Uses a <c>TowerIncantationTransition</c>.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new TowerIncantationTransition(
                  slot
                , this
                , _state.Timings.IncantationCastMs
                , _state.Timings.IncantationFadeMs
            );

        internal void SetMotionMode(bool casting)
        {
            _casting = casting;

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
            ApplyOpacity(_fadeEnd <= 0f ? 1f : Mathf.Clamp01(clamped / _fadeEnd));
        }

        internal void ApplyOpacity(float value)
        {
            _backdrop.style.opacity = value;
            _root.style.opacity = value;
        }

        private void LayoutPose(float p)
        {
            var terminal = p >= 1f;
            var pitch = _slotPitch * _rowWidth;

            _pose = p;
            _terminal = terminal;

            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var rune = _sourceRunes[i];
                var id = (int)rune;
                var moveBegin = _moveStart + _moveStagger * i;
                var moveSpan = _moveSpan - _moveStagger * i;
                var turnBegin = _turnStart + _turnStagger * id;
                var t = moveSpan <= 0f ? 1f : Mathf.Clamp01((p - moveBegin) / moveSpan);
                var f = _turnSpan <= 0f ? 1f : Mathf.Clamp01((p - turnBegin) / _turnSpan);
                var turn = f * _turnDegrees;
                var decoded = terminal || turn >= FACE_FLIP_DEGREES;
                var squash = terminal ? 1f : Mathf.Abs(Mathf.Cos(turn * Mathf.Deg2Rad));
                var view = _runes[i];

                view.style.left = (i + (id - i) * t) * pitch;
                view.style.top = _rowTop - Mathf.Sin(Mathf.PI * t) * _arcHeight;
                view.style.scale = new Scale(new Vector3(squash, 1f, 1f));

                if (_decoded[i] != decoded)
                {
                    _decoded[i] = decoded;
                    view.SetRune(rune: rune, decoded: decoded, number: i + 1);
                }
            }

            BookView.SetDisplay(_writeInTheBook, terminal && IsCommitted);
        }

        private void ResetFaces()
        {
            for (var i = 0; i < RuneCatalog.COUNT; i++)
            {
                var view = _runes[i];

                _decoded[i] = false;
                view.SetRune(rune: _sourceRunes[i], decoded: false, number: i + 1);
                view.SetInk(_palette.Color(ConfigColorType.CastGold));
            }
        }

        private void RenderWriteInTheBook()
        {
            BookView.SetDisplay(_writeInTheBook, _terminal && IsCommitted);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var customStyle = evt.customStyle;

            _palette.Read(customStyle, ThemeAPI.IsDark(this));
            customStyle.Read(s_slotPitch, ref _slotPitch);
            customStyle.Read(s_rowWidth, ref _rowWidth);
            customStyle.Read(s_rowTop, ref _rowTop);
            customStyle.Read(s_arcHeight, ref _arcHeight);
            customStyle.Read(s_fadeEnd, ref _fadeEnd);
            customStyle.Read(s_moveStart, ref _moveStart);
            customStyle.Read(s_moveStagger, ref _moveStagger);
            customStyle.Read(s_moveSpan, ref _moveSpan);
            customStyle.Read(s_turnStart, ref _turnStart);
            customStyle.Read(s_turnStagger, ref _turnStagger);
            customStyle.Read(s_turnSpan, ref _turnSpan);
            customStyle.Read(s_turnDegrees, ref _turnDegrees);

            if (_state != null)
            {
                _root.style.backgroundColor = _palette.Color(ConfigColorType.PageEcho);
                BookView.AttachStates(this, _palette);
                ResetFaces();
                LayoutPose(_pose);
            }
        }

        private void OnStateChanged()
        {
            RenderWriteInTheBook();
        }

        private void OnWriteInTheBookClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.WriteInTheBook);
        }
    }
}
