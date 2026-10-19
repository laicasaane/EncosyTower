#if UNITY_EDITOR

namespace EncosyTower.Editor.Serialization.Internals
{
    /// <summary>
    /// How a row draws a value type; see <see cref="OverridableEditorAPI.GetMode"/>.
    /// </summary>
    internal enum OverridableMode
    {
        /// <summary>
        /// A dropdown whose first item selects the default source. Used for a non-flags enum.
        /// </summary>
        Dropdown,

        /// <summary>
        /// An override switch next to a value field. Used for every other value type.
        /// </summary>
        Toggle,
    }
}

#endif
