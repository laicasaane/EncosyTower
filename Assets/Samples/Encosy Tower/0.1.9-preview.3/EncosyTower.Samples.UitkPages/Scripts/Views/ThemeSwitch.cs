using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.Samples.UitkPages.State;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>The switch that picks the light or the dark theme.</summary>
    [UxmlElement]
    public partial class ThemeSwitch : VisualElement
    {
        /// <summary>The USS class of the two buttons of the switch.</summary>
        internal const string SEGMENT_CLASS = "tower-book-theme-switch__segment";

        private const string SWITCH_CLASS = "tower-book-theme-switch";
        private const string SELECTED_CLASS = "tower-book-theme-switch__segment--selected";
        private const string DARK_BUTTON = "button-theme-dark";
        private const string LIGHT_BUTTON = "button-theme-light";

        private static readonly CustomStyleProperty<Color> s_inkProperty
            = ConfigNameAPI.ColorProperty(ConfigColorType.Ink);

        private static readonly CustomStyleProperty<Color> s_selectedInkProperty
            = ConfigNameAPI.ColorProperty(ConfigColorType.SettingsThemeSelectedInk);

        private static readonly CustomStyleProperty<float> s_unselectedAlphaProperty
            = ConfigNameAPI.AmountProperty(ConfigAmountType.SettingsThemeUnselectedAlpha);

        private readonly Button _dark;
        private readonly Button _light;

        private AppState _state;
        private Color _ink;
        private Color _selectedInk;
        private float _unselectedAlpha;
        private bool _isSubscribed;
        private bool _isStyleRead;

        /// <summary>Creates the switch.</summary>
        public ThemeSwitch()
        {
            AddToClassList(SWITCH_CLASS);

            _dark = CreateSegment(DARK_BUTTON);
            _light = CreateSegment(LIGHT_BUTTON);

            _dark.clicked += OnDarkClicked;
            _light.clicked += OnLightClicked;

            Add(_dark);
            Add(_light);

            RegisterCallback<AttachToPanelEvent>(OnAttachToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        private static Button CreateSegment(string segmentName)
        {
            var segment = new Button { name = segmentName };

            segment.AddToClassList(SEGMENT_CLASS);
            return segment;
        }

        internal void Bind(AppState state, string darkText, string lightText)
        {
            Unsubscribe();

            _state = state;
            _dark.text = darkText;
            _light.text = lightText;

            if (panel != null)
            {
                Subscribe();
            }

            Refresh();
        }

        private void Subscribe()
        {
            if (_isSubscribed || _state == null)
            {
                return;
            }

            _isSubscribed = true;
            _state.ThemeChanged += OnThemeChanged;
        }

        private void Unsubscribe()
        {
            if (_isSubscribed == false)
            {
                return;
            }

            _isSubscribed = false;
            _state.ThemeChanged -= OnThemeChanged;
        }

        private void Refresh()
        {
            var isDark = _state != null && _state.Theme == Theme.Dark;
            var isLight = _state != null && _state.Theme == Theme.Light;

            _dark.EnableInClassList(SELECTED_CLASS, isDark);
            _light.EnableInClassList(SELECTED_CLASS, isLight);

            ApplyInks();
        }

        private void ApplyInks()
        {
            if (_isStyleRead == false)
            {
                _isStyleRead = TryReadStyle(customStyle);
            }

            if (_isStyleRead == false)
            {
                return;
            }

            var isDark = _state != null && _state.Theme == Theme.Dark;
            var isLight = _state != null && _state.Theme == Theme.Light;
            var unselected = _ink;

            unselected.a *= _unselectedAlpha;

            _dark.style.color = isDark ? _selectedInk : unselected;
            _light.style.color = isLight ? _selectedInk : unselected;
        }

        private bool TryReadStyle(ICustomStyle style)
        {
            return style.TryGetValue(s_inkProperty, out _ink)
                && style.TryGetValue(s_selectedInkProperty, out _selectedInk)
                && style.TryGetValue(s_unselectedAlphaProperty, out _unselectedAlpha);
        }

        private void OnAttachToPanel(AttachToPanelEvent evt)
        {
            Subscribe();
            Refresh();
        }

        private void OnDetachFromPanel(DetachFromPanelEvent evt)
        {
            Unsubscribe();
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            var style = evt.customStyle;

            style.Read(s_inkProperty, ref _ink);
            style.Read(s_selectedInkProperty, ref _selectedInk);
            style.Read(s_unselectedAlphaProperty, ref _unselectedAlpha);

            _isStyleRead = true;

            ApplyInks();
        }

        private void OnThemeChanged()
        {
            Refresh();
        }

        private void OnDarkClicked()
        {
            _state?.SetTheme(Theme.Dark);
        }

        private void OnLightClicked()
        {
            _state?.SetTheme(Theme.Light);
        }
    }
}
