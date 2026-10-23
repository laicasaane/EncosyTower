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
    /// <summary>The More dialog: more actions for the current dimension.</summary>
    /// <remarks>A page of the Popup flow. The Echo dialog opens above it.</remarks>
    public class MoreDialog : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        private const string BODY_NAME = "label-body";

        /// <summary>The backdrop behind the dialog, shown when it is the first popup.</summary>
        [Tooltip("The backdrop behind the dialog, shown when it is the first popup.")]
        [SerializeField] private GameObject _backdrop;

        /// <summary>The panel of the dialog.</summary>
        [Tooltip("The panel of the dialog.")]
        [SerializeField] private Image _panel;

        /// <summary>The title of the dialog.</summary>
        [Tooltip("The title of the dialog.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The button that opens the Echo dialog.</summary>
        [Tooltip("The button that opens the Echo dialog.")]
        [SerializeField] private Button _echo;

        /// <summary>The button that closes the dialog.</summary>
        [Tooltip("The button that closes the dialog.")]
        [SerializeField] private Button _buttonClose;

        private AppState _state;
        private TMP_Text[] _bodies;
        private Dimension? _titleId;
        private string _titleText;
        private bool _isRegistered;

        /// <inheritdoc/>
        /// <remarks>
        /// It rejects the creation when the page user data names no dimension.
        /// </remarks>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || DimensionCatalog.TryGet(data.Location, out var dimension) == false
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _echo.onClick.AddListener(OnEchoClicked);
                _buttonClose.onClick.AddListener(OnCloseClicked);

                _bodies = BookView.FindChildren<TMP_Text>(this, BODY_NAME);
            }

            UnsubscribeState();

            _state = data.State;
            _state.ThemeChanged += OnThemeChanged;

            var config = _state.Config;

            if (_titleId != dimension.Id)
            {
                _titleId = dimension.Id;
                _titleText = string.Format(config.Text(ConfigTextType.DimensionPageTitle), dimension.Title);
            }

            DialogBackdrop.Bind(_backdrop, data.Dismiss, _state.PopupDepth > 0);
            _title.text = _titleText;
            RefreshColors();

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
                _echo.onClick.RemoveListener(OnEchoClicked);
                _buttonClose.onClick.RemoveListener(OnCloseClicked);
            }
        }

        private void UnsubscribeState()
        {
            if (_state != null)
            {
                _state.ThemeChanged -= OnThemeChanged;
                _state = null;
            }
        }

        private void RefreshColors()
        {
            var config = _state.Config;

            var ink = config.Color(ConfigColorType.Ink);
            var navigationBar = config.Color(ConfigColorType.NavigationBar);

            _panel.color = config.Color(ConfigColorType.PageMore);
            _title.color = ink;
            BookView.SetColors(_bodies, ink);

            BookView.SetBackdrop(_backdrop, config);
            BookView.SetBorders(this, config);
            BookView.SetColors(_echo, navigationBar, ink, config);
            BookView.SetColors(_buttonClose, navigationBar, ink, config);
        }

        private void OnThemeChanged()
        {
            RefreshColors();
        }

        private void OnEchoClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.ReadEcho);
        }

        private void OnCloseClicked()
        {
            BookActions.Publish(Publisher, FlowScopeCollection, AppAction.Done);
        }
    }
}
