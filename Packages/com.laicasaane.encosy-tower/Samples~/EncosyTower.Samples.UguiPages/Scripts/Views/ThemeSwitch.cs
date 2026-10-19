using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.Samples.UguiPages.State;
using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Views
{
    /// <summary>The switch that picks the light or the dark theme.</summary>
    public class ThemeSwitch : MonoBehaviour
    {
        /// <summary>The button that picks the dark theme.</summary>
        [Tooltip("The button that picks the dark theme.")]
        [SerializeField] private Button _dark;

        /// <summary>The button that picks the light theme.</summary>
        [Tooltip("The button that picks the light theme.")]
        [SerializeField] private Button _light;

        /// <summary>The label of the dark button.</summary>
        [Tooltip("The label of the dark button.")]
        [SerializeField] private TMP_Text _darkLabel;

        /// <summary>The label of the light button.</summary>
        [Tooltip("The label of the light button.")]
        [SerializeField] private TMP_Text _lightLabel;

        /// <summary>The fill image of the dark button.</summary>
        [Tooltip("The fill image of the dark button.")]
        [SerializeField] private Image _darkFill;

        /// <summary>The fill image of the light button.</summary>
        [Tooltip("The fill image of the light button.")]
        [SerializeField] private Image _lightFill;

        /// <summary>The pill that surrounds the two buttons.</summary>
        [Tooltip("The pill that surrounds the two buttons.")]
        [SerializeField] private Image _pill;

        /// <summary>The border image of the pill.</summary>
        [Tooltip("The border image of the pill.")]
        [SerializeField] private Image _pillBorder;

        private AppState _state;
        private ThemeSwitchSegment _darkSegment;
        private ThemeSwitchSegment _lightSegment;

        internal void Bind(AppState state)
        {
            _darkSegment = AttachSegment(_dark);
            _lightSegment = AttachSegment(_light);

            if (ReferenceEquals(_state, state))
            {
                Render();
                return;
            }

            Unbind();

            _state = state;
            _state.ThemeChanged += OnThemeChanged;
            _dark.onClick.AddListener(OnDarkClicked);
            _light.onClick.AddListener(OnLightClicked);

            Render();
        }

        internal void Unbind()
        {
            if (_state == null)
            {
                return;
            }

            _state.ThemeChanged -= OnThemeChanged;
            _dark.onClick.RemoveListener(OnDarkClicked);
            _light.onClick.RemoveListener(OnLightClicked);
            _state = null;

            ClearSegment(_darkSegment);
            ClearSegment(_lightSegment);
        }

        internal void RenderSegments()
        {
            if (_state == null)
            {
                return;
            }

            var config = _state.Config;
            var isDark = _state.Theme == Theme.Dark;

            var unselectedInk = config.Color(ConfigColorType.Ink);

            unselectedInk.a = config.Amount(ConfigAmountType.SettingsThemeUnselectedAlpha);

            ApplySegment(_darkFill, _darkLabel, _darkSegment, isDark, unselectedInk, config);
            ApplySegment(_lightFill, _lightLabel, _lightSegment, isDark == false, unselectedInk, config);
        }

        private void OnDestroy()
        {
            Unbind();
        }

        private void Render()
        {
            var config = _state.Config;

            var ink = config.Color(ConfigColorType.Ink);
            var fill = config.Color(ConfigColorType.NavigationBar);

            _pill.color = config.Color(ConfigColorType.PageConfirmation);

            if (_pillBorder.IsValid())
            {
                _pillBorder.color = Color.Lerp(fill, ink, config.Amount(ConfigAmountType.ButtonOutlineMix));
            }

            _darkLabel.text = config.Text(ConfigTextType.SettingsThemeDark);
            _lightLabel.text = config.Text(ConfigTextType.SettingsThemeLight);

            RenderSegments();
        }

        private ThemeSwitchSegment AttachSegment(Button button)
        {
            if (button.TryGetComponent(out ThemeSwitchSegment segment) == false)
            {
                segment = button.gameObject.AddComponent<ThemeSwitchSegment>();
            }

            segment.SetOwner(this);
            return segment;
        }

        private static void ClearSegment(ThemeSwitchSegment segment)
        {
            if (segment.IsValid())
            {
                segment.Clear();
            }
        }

        private static void ApplySegment(
              Image segment
            , TMP_Text label
            , ThemeSwitchSegment hover
            , bool selected
            , Color unselectedInk
            , AppConfig config
        )
        {
            if (selected)
            {
                segment.color = config.Color(ConfigColorType.SettingsThemeSelectedBackground);
                label.color = config.Color(ConfigColorType.SettingsThemeSelectedInk);
                return;
            }

            var isHovered = hover.IsValid() && hover.IsHovered;

            segment.color = isHovered ? config.Color(ConfigColorType.SettingsThemeUnselectedHover) : Color.clear;
            label.color = unselectedInk;
        }

        private void OnThemeChanged()
        {
            Render();
        }

        private void OnDarkClicked()
        {
            _state.SetTheme(Theme.Dark);
        }

        private void OnLightClicked()
        {
            _state.SetTheme(Theme.Light);
        }
    }

    internal sealed class ThemeSwitchSegment : MonoBehaviour
        , IPointerEnterHandler
        , IPointerExitHandler
    {
        private ThemeSwitch _owner;

        public bool IsHovered { get; private set; }

        public void SetOwner(ThemeSwitch owner)
        {
            _owner = owner;
        }

        public void Clear()
        {
            IsHovered = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            IsHovered = true;
            Notify();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsHovered = false;
            Notify();
        }

        private void OnDisable()
        {
            Clear();
            Notify();
        }

        private void Notify()
        {
            if (_owner.IsValid())
            {
                _owner.RenderSegments();
            }
        }
    }
}
