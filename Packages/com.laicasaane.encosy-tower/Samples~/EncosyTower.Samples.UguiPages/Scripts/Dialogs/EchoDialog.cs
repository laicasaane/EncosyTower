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
    /// <summary>The Echo dialog: the lore of the current dimension.</summary>
    /// <remarks>
    /// A page of the Popup flow. The player can abort its opening or closing, and open the lost appendix, a page that
    /// does not exist, to show a failed call.
    /// </remarks>
    public class EchoDialog : UguiPageBehaviour<AppFlowScopes>, IPageOnCreateAsync, IPageOnReturnToPool
    {
        /// <summary>The backdrop behind the dialog, shown when it is the first popup.</summary>
        [Tooltip("The backdrop behind the dialog, shown when it is the first popup.")]
        [SerializeField] private GameObject _backdrop;

        /// <summary>The panel of the dialog.</summary>
        [Tooltip("The panel of the dialog.")]
        [SerializeField] private Image _panel;

        /// <summary>The title of the dialog.</summary>
        [Tooltip("The title of the dialog.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The lore text of the dialog.</summary>
        [Tooltip("The lore text of the dialog.")]
        [SerializeField] private TMP_Text _body;

        /// <summary>The note text of the dialog.</summary>
        [Tooltip("The note text of the dialog.")]
        [SerializeField] private TMP_Text _note;

        /// <summary>The button that opens the lost appendix.</summary>
        [Tooltip("The button that opens the lost appendix.")]
        [SerializeField] private Button _appendix;

        /// <summary>The button that closes the dialog.</summary>
        [Tooltip("The button that closes the dialog.")]
        [SerializeField] private Button _done;

        private AppState _state;
        private Dimension? _textId;
        private string _titleText;
        private string _noteText;
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
                _appendix.onClick.AddListener(OnAppendixClicked);
                _done.onClick.AddListener(OnDoneClicked);
            }

            UnsubscribeState();

            _state = data.State;
            _state.ThemeChanged += OnThemeChanged;

            var config = _state.Config;

            if (_textId != dimension.Id)
            {
                _textId = dimension.Id;
                _titleText = string.Format(config.Text(ConfigTextType.EchoTitle), dimension.Title);
                _noteText = string.Format(config.Text(ConfigTextType.EchoNote), dimension.Motif);
            }

            _backdrop.SetActive(_state.PopupDepth == 0);
            _title.text = _titleText;
            _body.text = dimension.Lore;
            _note.text = _noteText;
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
                _appendix.onClick.RemoveListener(OnAppendixClicked);
                _done.onClick.RemoveListener(OnDoneClicked);
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

            _panel.color = config.Color(ConfigColorType.PageEcho);
            _title.color = ink;
            _body.color = ink;
            _note.color = ink;

            BookView.SetBackdrop(_backdrop, config);
            BookView.SetBorders(this, config);
            BookView.SetColors(_appendix, navigationBar, ink, config);
            BookView.SetColors(_done, navigationBar, ink, config);
        }

        private void OnThemeChanged()
        {
            RefreshColors();
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
