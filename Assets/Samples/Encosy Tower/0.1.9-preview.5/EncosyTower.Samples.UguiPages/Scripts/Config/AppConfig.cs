using System;
using System.Collections.Generic;
using EncosyTower.Samples.UguiPages.Ids;
using UnityEngine;
using UnityEngine.Serialization;
using ThrowHelper = EncosyTower.Samples.UguiPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>
    /// The configuration of the Book: its named colors, amounts and texts and the definitions of the dimensions and
    /// runes.
    /// </summary>
    /// <remarks>
    /// The colors, amounts and texts are named values keyed by <see cref="ConfigId"/>; the dimensions and runes are
    /// definitions, so the page prefabs and the scene hold no content.
    /// </remarks>
    public sealed class AppConfig : ScriptableObject, ISerializationCallbackReceiver
    {
        /// <summary>The named colors, each with a light and a dark value.</summary>
        [Tooltip("The named colors, each with a light and a dark value.")]
        [SerializeField] private ConfigColor[] _namedColors = Array.Empty<ConfigColor>();

        /// <summary>The named amounts.</summary>
        [Tooltip("The named amounts.")]
        [FormerlySerializedAs("_namedNumbers")]
        [SerializeField] private ConfigAmount[] _namedAmounts = Array.Empty<ConfigAmount>();

        /// <summary>The named texts.</summary>
        [Tooltip("The named texts.")]
        [SerializeField] private ConfigText[] _namedTexts = Array.Empty<ConfigText>();

        /// <summary>The definitions of the six dimensions and the core.</summary>
        [Tooltip("The definitions of the six dimensions and the core.")]
        [SerializeField] private DimensionDefinition[] _dimensions = Array.Empty<DimensionDefinition>();

        /// <summary>The definitions of the six runes.</summary>
        [Tooltip("The definitions of the six runes.")]
        [SerializeField] private RuneDefinition[] _runes = Array.Empty<RuneDefinition>();

        [NonSerialized] private Dictionary<ConfigId, ThemedColor> _colorLookup;
        [NonSerialized] private Dictionary<ConfigId, float> _amountLookup;
        [NonSerialized] private Dictionary<ConfigId, string> _textLookup;
        [NonSerialized] private HashSet<ConfigId> _reportedIds;

        /// <summary>Gets the definitions of the dimensions.</summary>
        public ReadOnlySpan<DimensionDefinition> Dimensions => _dimensions;

        /// <summary>Gets the definitions of the runes.</summary>
        public ReadOnlySpan<RuneDefinition> Runes => _runes;

        internal Theme ActiveTheme { get; set; } = Theme.Light;

        /// <summary>Gets the color of a name in the active theme.</summary>
        /// <param name="name">The name of the color.</param>
        /// <returns>The color, or the default color when the name is missing.</returns>
        internal UnityEngine.Color Color(ConfigColorType name)
        {
            ConfigId id = name;

            if (GetColorLookup().TryGetValue(id, out var value))
            {
                return Color(in value);
            }

            ReportMissingId(id);
            return default;
        }

        /// <summary>Gets the color of a themed color in the active theme.</summary>
        /// <param name="value">The themed color.</param>
        /// <returns>The light or the dark color.</returns>
        public UnityEngine.Color Color(in ThemedColor value)
            => ActiveTheme == Theme.Dark ? value.dark : value.light;

        /// <summary>Gets the text of a name.</summary>
        /// <param name="name">The name of the text.</param>
        /// <returns>The text, or empty when the name is missing.</returns>
        internal string Text(ConfigTextType name)
        {
            ConfigId id = name;

            if (GetTextLookup().TryGetValue(id, out var value))
            {
                return value;
            }

            ReportMissingId(id);
            return string.Empty;
        }

        /// <summary>Gets the amount of a name.</summary>
        /// <param name="name">The name of the amount.</param>
        /// <returns>The amount, or 0 when the name is missing.</returns>
        internal float Amount(ConfigAmountType name)
        {
            ConfigId id = name;

            if (GetAmountLookup().TryGetValue(id, out var value))
            {
                return value;
            }

            ReportMissingId(id);
            return default;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _colorLookup = null;
            _amountLookup = null;
            _textLookup = null;
        }

        private Dictionary<ConfigId, ThemedColor> GetColorLookup()
        {
            if (_colorLookup != null)
            {
                return _colorLookup;
            }

            var entries = _namedColors ?? Array.Empty<ConfigColor>();
            var count = entries.Length;
            var lookup = new Dictionary<ConfigId, ThemedColor>(count);

            for (var i = 0; i < count; i++)
            {
                ref var entry = ref entries[i];
                ConfigId id = entry.Type;

                if (lookup.TryAdd(id, entry.Value) == false)
                {
                    ThrowHelper.LogError_ConfigDuplicateId(id);
                }
            }

            _colorLookup = lookup;
            return lookup;
        }

        private Dictionary<ConfigId, float> GetAmountLookup()
        {
            if (_amountLookup != null)
            {
                return _amountLookup;
            }

            var entries = _namedAmounts ?? Array.Empty<ConfigAmount>();
            var count = entries.Length;
            var lookup = new Dictionary<ConfigId, float>(count);

            for (var i = 0; i < count; i++)
            {
                ref var entry = ref entries[i];
                ConfigId id = entry.Type;

                if (lookup.TryAdd(id, entry.Value) == false)
                {
                    ThrowHelper.LogError_ConfigDuplicateId(id);
                }
            }

            _amountLookup = lookup;
            return lookup;
        }

        private Dictionary<ConfigId, string> GetTextLookup()
        {
            if (_textLookup != null)
            {
                return _textLookup;
            }

            var entries = _namedTexts ?? Array.Empty<ConfigText>();
            var count = entries.Length;
            var lookup = new Dictionary<ConfigId, string>(count);

            for (var i = 0; i < count; i++)
            {
                ref var entry = ref entries[i];
                ConfigId id = entry.Type;

                if (lookup.TryAdd(id, entry.Value) == false)
                {
                    ThrowHelper.LogError_ConfigDuplicateId(id);
                }
            }

            _textLookup = lookup;
            return lookup;
        }

        private void ReportMissingId(ConfigId id)
        {
            _reportedIds ??= new HashSet<ConfigId>();

            if (_reportedIds.Add(id))
            {
                ThrowHelper.LogError_ConfigIdMissing(id);
            }
        }
    }
}
