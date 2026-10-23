using System.Threading;
using EncosyTower.PageFlows;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.PageFlows.UitkPages.Transitions;
using EncosyTower.Samples.UitkPages.App;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.Messages;
using EncosyTower.Samples.UitkPages.PageUserData;
using EncosyTower.Samples.UitkPages.Views;
using EncosyTower.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Attachments
{
    /// <summary>Which buttons of the navigation bar the player can use.</summary>
    internal readonly record struct NavigationBarStatus(
          bool CanBack
        , bool CanForward
        , bool CanMap
        , bool CanSettings
    );

    /// <summary>The navigation bar of a screen: back, forward, map and settings.</summary>
    /// <remarks>
    /// A page of the NavigationBar flow, a single-page stack attached to the screen and shown with it. The app renders
    /// its buttons from a <see cref="NavigationBarStatus"/>.
    /// </remarks>
    [UxmlElement]
    public partial class NavigationBarPage
        : UitkPageElement<AppFlowScopes>
        , IPageOnCreateAsync
        , IPageOnReturnToPool
    {
        private const string BACK_BUTTON = "button-back";
        private const string FORWARD_BUTTON = "button-forward";
        private const string MAP_BUTTON = "button-map";
        private const string SETTINGS_BUTTON = "button-settings";
        private const string BACK_ICON_CLASS = "tower-book-navigation-bar__icon--back";
        private const string FORWARD_ICON_CLASS = "tower-book-navigation-bar__icon--forward";

        private readonly PaletteStyle _palette = new();

        private Button _back;
        private Button _forward;
        private VisualElement _backIcon;
        private VisualElement _forwardIcon;
        private Button _map;
        private Button _settings;
        private AttachmentSet _set;
        private NavigationBarStatus _status;
        private bool _isRegistered;
        private bool _hasStatus;

        /// <summary>Creates the page element.</summary>
        public NavigationBarPage()
        {
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private static void ApplyButtonColors(
              Button button
            , Color navigation
            , Color ink
            , float fillMix
            , float borderMix
            , float hoverMix
            , float pressMix
        )
        {
            button.style.backgroundColor = Color.Lerp(navigation, ink, fillMix);
            button.style.color = ink;

            var border = Color.Lerp(navigation, ink, borderMix);

            button.style.borderTopColor = button.style.borderRightColor = border;
            button.style.borderBottomColor = button.style.borderLeftColor = border;

            var hover = new ButtonLook(Color.Lerp(navigation, ink, hoverMix), ink, ink);
            var press = new ButtonLook(Color.Lerp(navigation, ink, pressMix), ink, ink);

            BookView.SetStateLooks(button, in hover, in press, ink);
        }

        /// <inheritdoc/>
        public UnityTask<bool> OnCreateAsync(PageContext context, CancellationToken token)
        {
            if (token.IsCancellationRequested || PageUserDataAPI.TryGetAttachment(in context, out var set) == false)
            {
                return UnityTask.FromResult(false);
            }

            if (_back == null)
            {
                _back = this.Q<Button>(BACK_BUTTON);
                _forward = this.Q<Button>(FORWARD_BUTTON);
                _backIcon = _back.Q(className: BACK_ICON_CLASS);
                _forwardIcon = _forward.Q(className: FORWARD_ICON_CLASS);
                _map = this.Q<Button>(MAP_BUTTON);
                _settings = this.Q<Button>(SETTINGS_BUTTON);
            }

            if (_isRegistered == false)
            {
                _isRegistered = true;

                _back.clicked += OnBackClicked;
                _forward.clicked += OnForwardClicked;
                _map.clicked += OnMapClicked;
                _settings.clicked += OnSettingsClicked;
            }

            _set = set;
            _set.Bar = this;
            _settings.text = set.State.Resources.Get(ConfigTextType.NavigationBarSettings);
            _back.tooltip = set.State.Resources.Get(ConfigTextType.NavigationBarBack);
            _forward.tooltip = set.State.Resources.Get(ConfigTextType.NavigationBarForward);

            return UnityTask.FromResult(true);
        }

        /// <inheritdoc/>
        public void OnReturnToPool(PageContext context)
        {
            if (_isRegistered)
            {
                _isRegistered = false;

                _back.clicked -= OnBackClicked;
                _forward.clicked -= OnForwardClicked;
                _map.clicked -= OnMapClicked;
                _settings.clicked -= OnSettingsClicked;
            }

            if (_set != null)
            {
                _set.Bar = null;
                _set = null;
            }

            _hasStatus = false;
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Fades the page root with the default duration.
        /// </remarks>
        protected override IPageTransition CreateTransition(VisualElement slot)
            => new UitkValueAnimationTransition(slot);

        internal void Render(in NavigationBarStatus status)
        {
            _status = status;
            _hasStatus = true;

            Render();
        }

        private void Render()
        {
            if (_set == null || _hasStatus == false || _palette.IsResolved == false)
            {
                return;
            }

            var status = _status;
            var navigation = _palette.Color(ConfigColorType.NavigationBar);
            var ink = _palette.Color(ConfigColorType.Ink);

            if (DimensionCatalog.TryGet(_set.Location, out var data))
            {
                navigation = _palette.NavigationOf(data.Index);
                ink = _palette.Color(ConfigColorType.InkNavigation);
            }

            var fillMix = _palette.Amount(ConfigAmountType.NavigationBarButtonFillMix);
            var borderMix = _palette.Amount(ConfigAmountType.NavigationBarBorderMix);
            var hoverMix = _palette.Amount(ConfigAmountType.NavigationBarHoverMix);
            var pressMix = _palette.Amount(ConfigAmountType.NavigationBarPressMix);

            style.backgroundColor = navigation;

            BookView.AttachStates(this, _palette);

            ApplyButtonColors(_back, navigation, ink, fillMix, borderMix, hoverMix, pressMix);
            ApplyButtonColors(_forward, navigation, ink, fillMix, borderMix, hoverMix, pressMix);
            ApplyButtonColors(_map, navigation, ink, fillMix, borderMix, hoverMix, pressMix);
            ApplyButtonColors(_settings, navigation, ink, fillMix, borderMix, hoverMix, pressMix);

            _backIcon.style.unityBackgroundImageTintColor = ink;
            _forwardIcon.style.unityBackgroundImageTintColor = ink;

            BookView.SetEnabled(_back, status.CanBack);
            BookView.SetEnabled(_forward, status.CanForward);
            BookView.SetEnabled(_map, status.CanMap);
            BookView.SetEnabled(_settings, status.CanSettings);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            _palette.Read(evt.customStyle, ThemeAPI.IsDark(this));

            Render();
        }

        private void Publish(AppAction action)
        {
            BookActions.Publish(Publisher, FlowScopeCollection, action);
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
