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
    /// <summary>The More dialog: more actions for the current dimension.</summary>
    /// <remarks>
    /// A page of the Popup flow, defined by a prefab with a <see cref="UitkPageBehaviour"/> component and a layout.
    /// The Echo dialog opens above it.
    /// </remarks>
    public class MoreDialog : UitkPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string ROOT = "root";
        private const string TITLE_LABEL = "label-title";
        private const string ECHO_BUTTON = "button-echo";
        private const string CLOSE_BUTTON = "button-close";
        private const string NESTED_CLASS = "tower-book-popup--nested";

        private readonly PaletteStyle _palette = new();

        private Label _title;
        private Button _echo;
        private Button _close;
        private VisualElement _root;
        private readonly DialogBackdrop _dismissal = new();

        private AppState _state;
        private Dimension? _titleId;
        private string _titleText;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetPage(in context, out var data) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_title == null)
            {
                _title = Slot.Q<Label>(TITLE_LABEL);
                _echo = Slot.Q<Button>(ECHO_BUTTON);
                _close = Slot.Q<Button>(CLOSE_BUTTON);
                _root = Slot.Q(ROOT);

                _echo.clicked += OnEchoClicked;
                _close.clicked += OnCloseClicked;
                Slot.RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            }

            DimensionCatalog.TryGet(data.Location, out var dimension);

            _state = data.State;
            _dismissal.Bind(Slot, data.Dismiss);

            if (_titleId != dimension.Id)
            {
                _titleId = dimension.Id;
                _titleText = _state.Resources.Format(ConfigTextType.DimensionPageTitle, dimension.Title);
            }

            _title.text = _titleText;
            _root.style.backgroundColor = _palette.Color(ConfigColorType.PageMore);
            BookView.AttachStates(Slot, _palette);
            Slot.EnableInClassList(NESTED_CLASS, data.State.PopupDepth > 0);

            return UnityTask.FromResult(true);
        }

        public void OnReturnToPool(PageContext context)
        {
            _dismissal.Unbind();
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the backdrop and the panel with a <c>PopupTransition</c> over the popup transition time.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new PopupTransition(slot, _state.Timings.PopupTransitionMs);

        private void OnDestroy()
        {
            if (_echo != null)
            {
                _echo.clicked -= OnEchoClicked;
                _close.clicked -= OnCloseClicked;
                Slot.UnregisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
            }
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(Slot));

            if (_state != null)
            {
                _root.style.backgroundColor = _palette.Color(ConfigColorType.PageMore);
                BookView.AttachStates(Slot, _palette);
            }
        }

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }

        private void OnEchoClicked()
        {
            Publish(AppAction.ReadEcho);
        }

        private void OnCloseClicked()
        {
            Publish(AppAction.Done);
        }
    }
}
