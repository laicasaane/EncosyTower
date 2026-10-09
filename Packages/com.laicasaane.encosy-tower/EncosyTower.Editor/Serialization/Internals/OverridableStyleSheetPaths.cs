#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;

namespace EncosyTower.Editor.Serialization.Internals
{
    internal static class OverridableStyleSheetPaths
    {
        public const string ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/Serialization/Internals";
        public const string USS_PATH = $"{ROOT}/StyleSheets/OverridableField.uss";
        public const string DARK_USS_PATH = $"{ROOT}/StyleSheets/OverridableFieldDark.uss";
        public const string LIGHT_USS_PATH = $"{ROOT}/StyleSheets/OverridableFieldLight.uss";
    }
}

#endif
