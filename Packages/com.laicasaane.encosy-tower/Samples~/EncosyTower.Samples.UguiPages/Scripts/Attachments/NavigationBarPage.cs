using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UguiPages;
using EncosyTower.Samples.UguiPages.App;
using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.Messages;
using EncosyTower.Samples.UguiPages.PageUserData;
using EncosyTower.Samples.UguiPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Attachments
{
    /// <summary>Which buttons of the navigation bar the player can use.</summary>
    internal readonly record struct NavigationBarStatus(
          bool CanBack
        , bool CanForward
        , bool CanMap
        , bool CanSettings
        , bool CanPick
    );

    /// <summary>The navigation bar of a screen: book, back, forward, map and settings.</summary>
    /// <remarks>
    /// A page of the NavigationBar flow, a single-page stack attached to the screen and shown with it. The app renders
    /// its buttons from a <see cref="NavigationBarStatus"/>.
    /// </remarks>
    public sealed class NavigationBarPage : UguiPageBehaviour<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        private const string DOTTED_NAME = "dotted";

        /// <summary>The background image of the bar.</summary>
        [Tooltip("The background image of the bar.")]
        [SerializeField] private Image _background;

        /// <summary>The button that travels to the entrance.</summary>
        [Tooltip("The button that travels to the entrance.")]
        [SerializeField] private Button _book;

        /// <summary>The placeholder that stands in for the book button on the entrance.</summary>
        [Tooltip("The placeholder that stands in for the book button on the entrance.")]
        [SerializeField] private GameObject _bookPlaceholder;

        /// <summary>The button that goes back.</summary>
        [Tooltip("The button that goes back.")]
        [SerializeField] private Button _back;

        /// <summary>The button that goes forward.</summary>
        [Tooltip("The button that goes forward.")]
        [SerializeField] private Button _forward;

        /// <summary>The button that opens the map.</summary>
        [Tooltip("The button that opens the map.")]
        [SerializeField] private Button _map;

        /// <summary>The button that opens the settings.</summary>
        [Tooltip("The button that opens the settings.")]
        [SerializeField] private Button _settings;

        private AttachmentSet _set;
        private Image _placeholderFill;
        private Image _placeholderDotted;
        private bool _isRegistered;

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetSet(in context, out var set) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;
                _book.onClick.AddListener(OnBookClicked);
                _back.onClick.AddListener(OnBackClicked);
                _forward.onClick.AddListener(OnForwardClicked);
                _map.onClick.AddListener(OnMapClicked);
                _settings.onClick.AddListener(OnSettingsClicked);

                _placeholderFill = _bookPlaceholder.GetComponent<Image>();
                _placeholderDotted = _bookPlaceholder.transform.Find(DOTTED_NAME).GetComponent<Image>();
            }

            _set = set;
            _set.NavigationBar = this;

            BookView.SetAccessibleName(_back, set.State.Config.Text(ConfigTextType.NavigationBarBack));
            BookView.SetAccessibleName(_forward, set.State.Config.Text(ConfigTextType.NavigationBarForward));
            BookView.SetLabel(_settings, set.State.Config.Text(ConfigTextType.NavigationBarSettings));

            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_set != null)
            {
                if (ReferenceEquals(_set.NavigationBar, this))
                {
                    _set.NavigationBar = null;
                }

                _set = null;
            }
        }

        private void OnDestroy()
        {
            if (_isRegistered)
            {
                _book.onClick.RemoveListener(OnBookClicked);
                _back.onClick.RemoveListener(OnBackClicked);
                _forward.onClick.RemoveListener(OnForwardClicked);
                _map.onClick.RemoveListener(OnMapClicked);
                _settings.onClick.RemoveListener(OnSettingsClicked);
            }
        }

        internal void Render(in NavigationBarStatus status)
        {
            if (_set == null)
            {
                return;
            }

            var isEntrance = _set.Location.Screen == ScreenPage.Entrance;

            var config = _set.State.Config;
            var navigation = config.Color(ConfigColorType.NavigationBar);
            var ink = config.Color(ConfigColorType.Ink);

            if (DimensionCatalog.TryGet(_set.Location, out var data))
            {
                navigation = config.Color(data.Navigation);
                ink = config.Color(ConfigColorType.InkNavigation);
            }

            _background.color = navigation;
            ApplyButtonColors(_book, navigation, ink);
            ApplyButtonColors(_back, navigation, ink);
            ApplyButtonColors(_forward, navigation, ink);
            ApplyButtonColors(_map, navigation, ink);
            ApplyButtonColors(_settings, navigation, ink);

            _book.gameObject.SetActive(isEntrance == false);
            _bookPlaceholder.SetActive(isEntrance);

            _placeholderFill.color = Color.clear;
            _placeholderDotted.color = config.Color(ConfigColorType.EntranceNavigationBorder);

            BookView.SetBorders(this, config);
            BookView.SetInteractable(_book, status.CanPick, config);
            BookView.SetInteractable(_back, status.CanBack, config);
            BookView.SetInteractable(_forward, status.CanForward, config);
            BookView.SetInteractable(_map, status.CanMap, config);
            BookView.SetInteractable(_settings, status.CanSettings, config);
        }

        private void ApplyButtonColors(Button button, Color navigation, Color ink)
        {
            var config = _set.State.Config;
            var mix = config.Amount(ConfigAmountType.NavigationBarButtonFillMix);
            var background = Color.Lerp(navigation, ink, mix);

            BookView.SetColors(button, background, ink, config);

            var hoverFill = Color.Lerp(navigation, ink, config.Amount(ConfigAmountType.NavigationBarHoverMix));
            var pressFill = Color.Lerp(navigation, ink, config.Amount(ConfigAmountType.NavigationBarPressMix));
            var hover = new ButtonLook(hoverFill, ink, ink);
            var press = new ButtonLook(pressFill, ink, ink);

            BookView.SetStateLooks(button, in hover, in press, ink);
        }

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
        }

        private void Publish(AppAction action, BookLocation location)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action, location);
        }

        private void OnBookClicked()
        {
            Publish(AppAction.Travel, BookLocation.Entrance);
        }

        private void OnBackClicked()
        {
            Publish(AppAction.Back);
        }

        private void OnForwardClicked()
        {
            Publish(AppAction.Forward);
        }

        private void OnMapClicked()
        {
            Publish(AppAction.OpenMap);
        }

        private void OnSettingsClicked()
        {
            Publish(AppAction.OpenSettings);
        }
    }
}
