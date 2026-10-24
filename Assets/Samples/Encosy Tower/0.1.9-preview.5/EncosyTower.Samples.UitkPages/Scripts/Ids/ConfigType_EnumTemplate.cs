using EncosyTower.EnumExtensions;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>
    /// The template of <see cref="ConfigType"/>, the one enum of every configuration value the Book looks up by name.
    /// </summary>
    /// <remarks>
    /// <see cref="NoneType"/> gives <see cref="ConfigType.None"/>; each category enum adds its members at its own
    /// offset.
    /// </remarks>
    [EnumTemplate]
    [EnumTemplateMembersFromEnum(typeof(NoneType), 0)]
    internal readonly partial struct ConfigType_EnumTemplate { }
}
