using EncosyTower.Samples.UguiPages.Config;
using EncosyTower.Samples.UguiPages.Ids;
using EncosyTower.UnityExtensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EncosyTower.Samples.UguiPages.Views
{
    /// <summary>Shows one rune: its glyph, or its Latin letter once decoded.</summary>
    public class RuneView : MonoBehaviour
    {
        private const string DOTTED_NAME = "dotted";

        /// <summary>The frame image around the rune.</summary>
        [Tooltip("The frame image around the rune.")]
        [SerializeField] private Image _frame;

        /// <summary>The image of the rune glyph.</summary>
        [Tooltip("The image of the rune glyph.")]
        [SerializeField] private Image _glyph;

        /// <summary>The text of the Latin letter.</summary>
        [Tooltip("The text of the Latin letter.")]
        [SerializeField] private TMP_Text _latin;

        /// <summary>Whether the view hides its frame.</summary>
        [Tooltip("Whether the view hides its frame.")]
        [SerializeField] private bool _hideFrame;

        private Image[] _border;
        private Image _dotted;
        private Color _ink;
        private Color _borderColor;
        private bool _hasInk;
        private bool _hasBorderColor;
        private bool _isBlank;
        private bool _isDottedBound;
        private bool _solidBorder;

        /// <summary>Shows a rune.</summary>
        /// <param name="rune">The rune.</param>
        /// <param name="decoded">Whether to show the Latin letter instead of the glyph.</param>
        /// <param name="number">The number of the rune in the order it was found.</param>
        internal void SetRune(Rune rune, bool decoded, int number)
        {
            _glyph.sprite = decoded ? null : RuneCatalog.SpriteOf(rune);
            _glyph.enabled = decoded == false;
            _latin.text = decoded ? RuneCatalog.LetterOf(rune) : string.Empty;
            _latin.enabled = decoded;
            _frame.enabled = _hideFrame == false;
            _isBlank = false;
            RenderBorder();
        }

        /// <summary>Shows an empty rune slot.</summary>
        public void SetBlank()
        {
            _glyph.sprite = null;
            _glyph.enabled = false;
            _latin.text = string.Empty;
            _latin.enabled = false;
            _frame.enabled = _hideFrame == false;
            _isBlank = true;
            RenderBorder();
        }

        /// <summary>Sets the color of the glyph, the letter and the frame.</summary>
        /// <param name="color">The color.</param>
        public void SetInk(Color color)
        {
            _glyph.color = color;
            _latin.color = color;
            _frame.color = new Color(color.r, color.g, color.b, RuneCatalog.FrameAlpha);
            _ink = color;
            _hasInk = true;
            RenderBorder();
        }

        /// <summary>Sets the color of the border.</summary>
        /// <param name="color">The color.</param>
        public void SetBorderColor(Color color)
        {
            _borderColor = color;
            _hasBorderColor = true;
            RenderBorder();
        }

        /// <summary>Sets whether the border is solid instead of dotted.</summary>
        /// <param name="solid">Whether the border is solid.</param>
        public void SetSolidBorder(bool solid)
        {
            _solidBorder = solid;
            RenderBorder();
        }

        private void RenderBorder()
        {
            if (_hideFrame)
            {
                return;
            }

            if (_border == null)
            {
                _border = new Image[4];

                for (var i = 0; i < 4; i++)
                {
                    var go = new GameObject("border", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                    var rt = (RectTransform)go.transform;

                    rt.SetParent(transform, false);

                    var horizontal = i < 2;
                    var near = (i & 1) == 0;

                    rt.anchorMin = horizontal ? new Vector2(0f, near ? 1f : 0f) : new Vector2(near ? 0f : 1f, 0f);
                    rt.anchorMax = horizontal ? new Vector2(1f, near ? 1f : 0f) : new Vector2(near ? 0f : 1f, 1f);
                    rt.pivot = horizontal ? new Vector2(0.5f, near ? 1f : 0f) : new Vector2(near ? 0f : 1f, 0.5f);
                    rt.offsetMin = rt.offsetMax = Vector2.zero;
                    rt.sizeDelta = horizontal ? new Vector2(0f, 1f) : new Vector2(1f, 0f);
                    rt.anchoredPosition = Vector2.zero;

                    var image = go.GetComponent<Image>();

                    image.raycastTarget = false;
                    _border[i] = image;
                }
            }

            if (_isDottedBound == false)
            {
                _isDottedBound = true;

                var dotted = transform.Find(DOTTED_NAME);

                if (dotted.IsValid())
                {
                    dotted.TryGetComponent(out _dotted);
                }
            }

            var hasDotted = _dotted.IsValid();
            var ink = _hasInk ? _ink : RuneCatalog.DefaultInk;
            var alpha = _isBlank ? RuneCatalog.BorderAlphaBlank : RuneCatalog.BorderAlpha;

            if (_solidBorder)
            {
                alpha = 1f;
            }

            var color = _hasBorderColor ? _borderColor : new Color(ink.r, ink.g, ink.b, alpha);
            var showSolid = hasDotted == false || _isBlank == false;

            for (var i = 0; i < 4; i++)
            {
                _border[i].color = color;
                _border[i].enabled = showSolid;
            }

            if (hasDotted)
            {
                _dotted.color = color;
                _dotted.gameObject.SetActive(_isBlank);
            }
        }
    }
}
