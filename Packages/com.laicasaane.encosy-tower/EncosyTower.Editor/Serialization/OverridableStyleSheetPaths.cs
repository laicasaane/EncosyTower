#if UNITY_EDITOR

using EncosyTower.Editor.UIElements;

namespace EncosyTower.Editor.Serialization
{
    internal static class OverridableStyleSheetPaths
    {
        public const string ROOT = $"{EditorStyleSheetPaths.ROOT}/EncosyTower.Editor/Serialization";
        public const string USS_PATH = $"{ROOT}/StyleSheets/OverridableField.uss";
    }
}

#endif
