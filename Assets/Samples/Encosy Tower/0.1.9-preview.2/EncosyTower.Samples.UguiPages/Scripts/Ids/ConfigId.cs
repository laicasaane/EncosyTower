using System.Runtime.CompilerServices;
using EncosyTower.EnumExtensions;
using EncosyTower.UnionIds;

namespace EncosyTower.Samples.UguiPages.Ids
{
    /// <summary>
    /// The id of a configuration value: the kind is its category (color, amount or text) and the id is the value's
    /// name in that category.
    /// </summary>
    /// <remarks>
    /// Each category enum, such as <see cref="ConfigColorType"/>, adds itself as a kind with
    /// <see cref="KindForUnionIdAttribute"/> and adds its members to <see cref="ConfigType"/> with
    /// <see cref="EnumMembersForTemplateAttribute"/>, at the same order, so a kind's value is the offset of its
    /// members in <see cref="ConfigType"/>. The default id has the <see cref="IdKind.None"/> kind and names no value.
    /// <see cref="Config.AppConfig"/> keys its values by this id.
    /// </remarks>
    [UnionId(KindSettings = UnionIdKindSettings.PreserveOrder)]
    [UnionIdKind(typeof(NoneType), 0, nameof(NoneType.None))]
    internal readonly partial struct ConfigId
    {
        /// <summary>Initializes the id of a configuration value from its member in <see cref="ConfigType"/>.</summary>
        /// <param name="configType">
        /// The value; <see cref="ConfigType.None"/> or an undefined value gives the default id.
        /// </param>
        public ConfigId(ConfigType configType)
            : this()
        {
            if (configType.TryConvert(out var order, out var value))
            {
                Kind = (IdKind)order;
                IdUnsigned = (byte)value;
            }
        }

        /// <summary>Converts an id to its member in <see cref="ConfigType"/>.</summary>
        /// <param name="id">The id.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ConfigType(ConfigId id)
        {
            var value = (ushort)id.IdUnsigned;
            var order = (ushort)id.Kind;
            return (ConfigType)(value + order);
        }

        /// <summary>Converts an id to its member in <see cref="ConfigType"/>, with the enum extensions.</summary>
        /// <param name="id">The id.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ConfigTypeExtended(ConfigId id)
            => (ConfigType)id;
    }
}
