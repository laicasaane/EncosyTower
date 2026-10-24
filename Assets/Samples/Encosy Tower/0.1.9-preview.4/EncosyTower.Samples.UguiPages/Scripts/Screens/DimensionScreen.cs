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
    /// <summary>
    /// The screen of one dimension, filled from the configuration definition named by its page user data.
    /// </summary>
    /// <remarks>
    /// A page of the Screen flow; one template page serves every dimension. It rents an attachment set, which
    /// attaches a navigation bar page, two tab pages and two card pages, each in a flow of its own.
    /// </remarks>
    public class DimensionScreen
        : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnAttachToFlowAsync
        , IPageOnReturnToPool
    {
        /// <summary>The background image of the screen.</summary>
        [Tooltip("The background image of the screen.")]
        [SerializeField] private Image _background;

        /// <summary>The title of the dimension.</summary>
        [Tooltip("The title of the dimension.")]
        [SerializeField] private TMP_Text _title;

        /// <summary>The motif text of the dimension.</summary>
        [Tooltip("The motif text of the dimension.")]
        [SerializeField] private TMP_Text _motif;

        /// <summary>The button that opens the Echo dialog.</summary>
        [Tooltip("The button that opens the Echo dialog.")]
        [SerializeField] private Button _echo;

        /// <summary>The button that opens the More dialog.</summary>
        [Tooltip("The button that opens the More dialog.")]
        [SerializeField] private Button _more;

        /// <summary>The button that shows the Chronicle tab.</summary>
        [Tooltip("The button that shows the Chronicle tab.")]
        [SerializeField] private Button _chronicle;

        /// <summary>The button that shows the Inscription tab.</summary>
        [Tooltip("The button that shows the Inscription tab.")]
        [SerializeField] private Button _inscription;

        /// <summary>The button that toggles the concept card.</summary>
        [Tooltip("The button that toggles the concept card.")]
        [SerializeField] private Button _runes;

        /// <summary>The button that toggles the in-practice card.</summary>
        [Tooltip("The button that toggles the in-practice card.")]
        [SerializeField] private Button _compass;

        /// <summary>The rect that holds the navigation bar page.</summary>
        [Tooltip("The rect that holds the navigation bar page.")]
        [SerializeField] private RectTransform _navigationBarHost;

        /// <summary>The rect that holds the tab pages.</summary>
        [Tooltip("The rect that holds the tab pages.")]
        [SerializeField] private RectTransform _detailHost;

        /// <summary>The rect that holds the card pages.</summary>
        [Tooltip("The rect that holds the card pages.")]
        [SerializeField] private RectTransform _cardsHost;

        private DimensionData _data;
        private AppState _state;
        private AttachmentSet _set;
        private Dimension? _titleId;
        private string _titleText;
        private bool _isRegistered;

        /// <inheritdoc/>
        /// <remarks>
        /// Rejects the creation when the page user data names no outer dimension.
        /// </remarks>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested
                || PageUserDataAPI.TryGetPage(in context, out var data) == false
                || DimensionCatalog.TryGet(data.Location, out _data) == false
                || _data.Index >= DimensionCatalog.OUTER_COUNT
            )
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _echo.onClick.AddListener(OnEchoClicked);
                _more.onClick.AddListener(OnMoreClicked);
                _chronicle.onClick.AddListener(OnChronicleClicked);
                _inscription.onClick.AddListener(OnInscriptionClicked);
                _runes.onClick.AddListener(OnConceptClicked);
                _compass.onClick.AddListener(OnInPracticeClicked);
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
                  location: BookLocation.Of(_data.Id)
                , navigationBarHost: _navigationBarHost
                , detailHost: _detailHost
                , cardsHost: _cardsHost
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

            _titleId = null;
            _titleText = null;
        }

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _echo.onClick.RemoveListener(OnEchoClicked);
                _more.onClick.RemoveListener(OnMoreClicked);
                _chronicle.onClick.RemoveListener(OnChronicleClicked);
                _inscription.onClick.RemoveListener(OnInscriptionClicked);
                _runes.onClick.RemoveListener(OnConceptClicked);
                _compass.onClick.RemoveListener(OnInPracticeClicked);
            }
        }

        private void Render()
        {
            var config = _state.Config;
            var surface = config.Color(_data.Surface);
            var tones = ToneMath.Analogous(config, surface);
            var ink = config.Color(ConfigColorType.Ink);

            _background.color = surface;

            if (_titleId != _data.Id)
            {
                _titleId = _data.Id;
                _titleText = string.Format(config.Text(ConfigTextType.DimensionPageTitle), _data.Title);
            }

            _title.text = _titleText;
            _title.color = ink;
            _motif.text = _data.Motif;
            _motif.color = ink;

            BookView.SetColors(_echo, tones.Button, ink, config);
            BookView.SetColors(_more, tones.Button, ink, config);
            BookView.SetColors(_chronicle, tones.NearA, ink, config);
            BookView.SetColors(_inscription, tones.NearB, ink, config);
            BookView.SetColors(_runes, tones.ComplementA, ink, config);
            BookView.SetColors(_compass, tones.ComplementB, ink, config);

            var view = _state.ViewOf(_data.Id);
            var height = config.Amount(ConfigAmountType.ButtonSelectionHeight);

            BookView.SetSelected(_chronicle, view.Tab == 0, SelectionEdge.Top, height);
            BookView.SetSelected(_inscription, view.Tab == 1, SelectionEdge.Top, height);
            BookView.SetSelected(_runes, view.ConceptVisible, SelectionEdge.Bottom, height);
            BookView.SetSelected(_compass, view.InPracticeVisible, SelectionEdge.Bottom, height);

            BookView.SetDimensionLooks(_echo, tones.Button, tones.Button, ink, config);
            BookView.SetDimensionLooks(_more, tones.Button, tones.Button, ink, config);
            BookView.SetDimensionLooks(_chronicle, tones.NearA, tones.Button, ink, config);
            BookView.SetDimensionLooks(_inscription, tones.NearB, tones.Button, ink, config);
            BookView.SetDimensionLooks(_runes, tones.ComplementA, tones.Button, ink, config);
            BookView.SetDimensionLooks(_compass, tones.ComplementB, tones.Button, ink, config);

            var interactive = _state.IsPopupActive == false;

            BookView.SetInteractable(_echo, interactive || _state.IsMoreOnTop, config);
            BookView.SetInteractable(_more, interactive, config);
            BookView.SetInteractable(_chronicle, interactive, config);
            BookView.SetInteractable(_inscription, interactive, config);
            BookView.SetInteractable(_runes, interactive, config);
            BookView.SetInteractable(_compass, interactive, config);
        }

        private void OnStateChanged()
        {
            Render();
        }

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }

        private void Publish(AppAction action, int index)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action, index);
        }

        private void OnEchoClicked()
        {
            Publish(AppAction.ReadEcho);
        }

        private void OnMoreClicked()
        {
            Publish(AppAction.More);
        }

        private void OnChronicleClicked()
        {
            Publish(AppAction.SelectTab, 0);
        }

        private void OnInscriptionClicked()
        {
            Publish(AppAction.SelectTab, 1);
        }

        private void OnConceptClicked()
        {
            Publish(AppAction.ToggleCard, 0);
        }

        private void OnInPracticeClicked()
        {
            Publish(AppAction.ToggleCard, 1);
        }
    }
}
