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
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Dialogs
{
    /// <summary>The Echo dialog: the lore of the current dimension.</summary>
    /// <remarks>
    /// A page of the Popup flow. The player can abort its opening or closing, and open the lost appendix, a page that
    /// does not exist, to show a failed call. It keeps the page below it with
    /// <see cref="PopupPageOptions.KeepLowerPage"/>.
    /// </remarks>
    [UxmlElement]
    public partial class EchoDialog : UitkPageElement<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string ROOT = "root";
        private const string TITLE_LABEL = "label-title";
        private const string BODY_LABEL = "label-body";
        private const string NOTE_LABEL = "label-note";
        private const string APPENDIX_BUTTON = "button-appendix";
        private const string DONE_BUTTON = "button-done";
        private const string NESTED_CLASS = "tower-book-popup--nested";

        private readonly PaletteStyle _palette = new();

        private VisualElement _root;
        private Label _title;
        private Label _body;
        private Label _note;
        private Button _appendix;
        private Button _done;
        private readonly DialogBackdrop _dismissal = new();

        private AppState _state;
        private Dimension? _textId;
        private string _titleText;
        private string _noteText;

        /// <summary>Creates the page element.</summary>
        public EchoDialog()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <inheritdoc/>
        public override PageFocusMode FocusMode => PageFocusMode.FirstFocusable;

        /// <inheritdoc/>
        public override PageOptions PageOptions => PopupPageOptions.KeepLowerPage;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || DimensionCatalog.TryGet(data.Location, out var dimension) == false
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_title == null)
            {
                _root = this.Q(ROOT);
                _title = this.Q<Label>(TITLE_LABEL);
                _body = this.Q<Label>(BODY_LABEL);
                _note = this.Q<Label>(NOTE_LABEL);
                _appendix = this.Q<Button>(APPENDIX_BUTTON);
                _done = this.Q<Button>(DONE_BUTTON);

                _appendix.clicked += OnAppendixClicked;
                _done.clicked += OnDoneClicked;
            }

            _state = data.State;
            _dismissal.Bind(this, data.Dismiss);

            var resources = _state.Resources;

            if (_textId != dimension.Id)
            {
                _textId = dimension.Id;
                _titleText = resources.Format(ConfigTextType.EchoTitle, dimension.Title);
                _noteText = resources.Format(ConfigTextType.EchoNote, dimension.Motif);
            }

            _title.text = _titleText;
            _body.text = dimension.Lore;
            _note.text = _noteText;
            _root.style.backgroundColor = _palette.Color(ConfigColorType.PageEcho);
            BookView.AttachStates(this, _palette);
            EnableInClassList(NESTED_CLASS, data.State.PopupDepth > 0);

            return UnityTask.FromResult(true);
        }

        public void OnReturnToPool(PageContext context)
        {
            _dismissal.Unbind();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the backdrop and the panel with a <c>PopupTransition</c> over the echo transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new PopupTransition(slot, _state.Timings.EchoTransitionMs);

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            if (_root != null)
            {
                _root.style.backgroundColor = _palette.Color(ConfigColorType.PageEcho);
                BookView.AttachStates(this, _palette);
            }
        }

        private void OnAppendixClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.ReadAppendix);
        }

        private void OnDoneClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Done);
        }
    }
}
