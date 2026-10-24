using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Views
{
    /// <summary>Shows one rune: its glyph, or its Latin letter once decoded.</summary>
    [UxmlElement]
    public partial class RuneView : VisualElement
    {
        private const string RUNE_CLASS = "tower-book-rune";
        private const string BLANK_CLASS = "tower-book-rune--blank";
        private const string LATIN_CLASS = "tower-book-rune--latin";
        private const float DOT_SIZE = 1f;
        private const float DOT_RADIUS = 4f;

        private static readonly CustomStyleProperty<Color> s_defaultInk
            = ConfigNameAPI.ColorProperty(ConfigColorType.RuneDefaultInk);

        private readonly DottedBorder _dots;
        private readonly Image _glyph;
        private readonly Label _latin;

        private Color _defaultInk;
        private bool _isInkSet;

        /// <summary>Creates an empty rune view.</summary>
        public RuneView()
        {
            pickingMode = PickingMode.Ignore;
            AddToClassList(RUNE_CLASS);

            _dots = new DottedBorder {
                name = "rune-dots",
                Width = DOT_SIZE,
                Radius = DOT_RADIUS,
                Dash = DOT_SIZE,
                Gap = DOT_SIZE,
            };

            _glyph = new Image {
                name = "rune-glyph",
                scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore,
            };

            _latin = new Label {
                name = "rune-latin",
                pickingMode = PickingMode.Ignore,
            };

            Add(_dots);
            Add(_glyph);
            Add(_latin);
            SetBlank();
            RegisterCallback<CustomStyleResolvedEvent>(OnCustomStyleResolved);
        }

        /// <summary>Shows a rune.</summary>
        /// <param name="rune">The rune.</param>
        /// <param name="decoded">Whether to show the Latin letter instead of the glyph.</param>
        /// <param name="number">The number of the rune in the order it was found.</param>
        internal void SetRune(Rune rune, bool decoded, int number)
        {
            EnableInClassList(BLANK_CLASS, false);
            EnableInClassList(LATIN_CLASS, decoded);

            _glyph.image = decoded ? null : RuneCatalog.TextureOf(rune);
            _glyph.style.display = decoded ? DisplayStyle.None : DisplayStyle.Flex;
            _latin.text = decoded ? RuneCatalog.LetterOf(rune) : string.Empty;
            _latin.style.display = decoded ? DisplayStyle.Flex : DisplayStyle.None;
            tooltip = decoded ? RuneCatalog.LetterOf(rune) : RuneCatalog.CollectedTooltip(number);
        }

        /// <summary>Shows an empty rune slot.</summary>
        public void SetBlank()
        {
            EnableInClassList(BLANK_CLASS, true);
            EnableInClassList(LATIN_CLASS, false);

            _glyph.image = null;
            _glyph.style.display = DisplayStyle.None;
            _latin.text = string.Empty;
            _latin.style.display = DisplayStyle.None;
            tooltip = RuneCatalog.EmptyTooltip;
        }

        /// <summary>Sets the color of the glyph, the letter and the slot.</summary>
        /// <param name="color">The color.</param>
        public void SetInk(Color color)
        {
            _isInkSet = true;
            ApplyInk(color);
        }

        /// <summary>Sets the color of the slot border and its dots.</summary>
        /// <param name="color">The color.</param>
        public void SetSlotColor(Color color)
        {
            BookView.SetBorder(this, color);
            _dots.Color = color;
        }

        private void ApplyInk(Color color)
        {
            _glyph.tintColor = color;
            _latin.style.color = color;
            SetSlotColor(color);
        }

        private void OnCustomStyleResolved(CustomStyleResolvedEvent evt)
        {
            evt.customStyle.Read(s_defaultInk, ref _defaultInk);

            if (_isInkSet == false)
            {
                ApplyInk(_defaultInk);
            }
        }
    }
}
