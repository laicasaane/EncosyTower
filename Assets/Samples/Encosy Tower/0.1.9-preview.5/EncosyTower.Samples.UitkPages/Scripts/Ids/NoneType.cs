using EncosyTower.EnumExtensions;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>
    /// The kind of an <see cref="AssetId"/> that names no asset and of a <see cref="ConfigId"/> that names no value.
    /// </summary>
    /// <remarks>
    /// It is the default kind, so <c>default(AssetId)</c> means "no asset" and <c>default(ConfigId)</c> means "no
    /// value".
    /// </remarks>
    [EnumExtensions]
    internal enum NoneType : byte
    {
        None = 0,
    }
}
