using System.Globalization;
using EncosyTower.Naming;
using EncosyTower.Samples.UitkPages.Config;
using EncosyTower.Vaults;
using UnityEngine;
using UnityEngine.UIElements;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>Composes the USS property and UXML label names of the Book's configuration from their ids.</summary>
    /// <remarks>
    /// One rule for every name, each piece in kebab case: <c>--tb-{kind}-{name}</c> for colors and amounts,
    /// <c>text.{name}</c> for texts, <c>--tb-dimension-{position}-{color}</c>, <c>dimensions__{position}-{detail}</c>
    /// and <c>runes__{position}-{detail}</c>, where the core's position is <c>core</c>. The USS and UXML files declare
    /// exactly these names. Call on the Unity main thread.
    /// </remarks>
    internal static class ConfigNameAPI
    {
        private const string PROPERTY_PREFIX = "--tb-";

        private static readonly ObjectVault<ConfigId> s_names = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            // Entering Play Mode without a domain reload keeps the statics; Dispose only clears the vault.
            s_names.Dispose();
        }
#endif

        /// <summary>Gets the USS property name of a color or amount, or the UXML label name of a text.</summary>
        /// <param name="id">
        /// The value; a category enum member converts to it, such as <c>ConfigColorType.Ink</c> or
        /// <c>ConfigTextType.EchoTitle</c>.
        /// </param>
        /// <returns>The name, or an empty string for the default id, which names no value.</returns>
        public static string NameOf(ConfigId id)
        {
            if (id.Kind == ConfigId.IdKind.None)
            {
                return string.Empty;
            }

            if (s_names.TryGet<string>(id, out var cached) && cached.TryGetValue(out var name))
            {
                return name;
            }

            var kind = Kebab(id.Kind.ToStringFast());
            var member = Kebab(id.GetIdStringFast());

            name = id.Kind == ConfigId.IdKind.Text
                ? string.Concat(kind, ".", member)
                : string.Concat(PROPERTY_PREFIX, kind, "-", member);

            s_names.TryAdd(id, name);
            return name;
        }

        /// <summary>Gets the USS custom property of a color.</summary>
        /// <param name="name">The name of the color.</param>
        public static CustomStyleProperty<Color> ColorProperty(ConfigColorType name)
            => new(NameOf(name));

        /// <summary>Gets the USS custom property of an amount.</summary>
        /// <param name="name">The name of the amount.</param>
        public static CustomStyleProperty<float> AmountProperty(ConfigAmountType name)
            => new(NameOf(name));

        /// <summary>Gets the name of the USS custom property of a dimension's color.</summary>
        /// <param name="position">
        /// The position of the dimension in the catalog; <see cref="DimensionCatalog.OUTER_COUNT"/> is the core.
        /// </param>
        /// <param name="navigation"><c>true</c> for the navigation color; <c>false</c> for the surface color.</param>
        public static string DimensionColorName(int position, bool navigation)
            => string.Concat(
                  PROPERTY_PREFIX
                , "dimension-"
                , PositionOf(position)
                , navigation ? "-navigation" : "-surface"
            );

        /// <summary>Gets the name of the UXML label of a detail of an outer dimension.</summary>
        /// <param name="position">The position of the dimension in the resources layout.</param>
        /// <param name="detail">The detail.</param>
        public static string DimensionLabelName(int position, DimensionDetail detail)
            => string.Concat(
                  "dimensions__"
                , position.ToString(CultureInfo.InvariantCulture)
                , "-"
                , Kebab(detail.ToStringFast())
            );

        /// <summary>Gets the name of the UXML label of a detail of the core.</summary>
        /// <param name="detail">The detail.</param>
        public static string CoreLabelName(DimensionDetail detail)
            => string.Concat("dimensions__", Kebab(Dimension.Core.ToStringFast()), "-", Kebab(detail.ToStringFast()));

        /// <summary>Gets the name of the UXML label of a detail of a rune.</summary>
        /// <param name="position">The position of the rune in the resources layout.</param>
        /// <param name="detail">The detail.</param>
        public static string RuneLabelName(int position, RuneDetail detail)
            => string.Concat(
                  "runes__"
                , position.ToString(CultureInfo.InvariantCulture)
                , "-"
                , Kebab(detail.ToStringFast())
            );

        private static string PositionOf(int position)
            => position == DimensionCatalog.OUTER_COUNT
                ? Kebab(Dimension.Core.ToStringFast())
                : position.ToString(CultureInfo.InvariantCulture);

        private static string Kebab(string name)
            => NamingPolicy.KebabCaseLower.ConvertName(name);
    }
}
