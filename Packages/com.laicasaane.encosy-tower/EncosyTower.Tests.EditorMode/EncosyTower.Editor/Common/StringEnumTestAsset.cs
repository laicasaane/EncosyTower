#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.Serialization;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Common
{
    public enum StringEnumTestKind
    {
        None,
        Fire,
        [InspectorName("Cold Water")] Water,
        Wind,
    }

    public enum StringEnumTestAlias
    {
        First = 0,
        Second = 1,
        Other = 1,
    }

    public readonly struct StringEnumTestConverter : IStringEnumConverter<StringEnumTestKind>
    {
        public string Transform(StringEnumTestKind from)
            => from.ToString().ToLowerInvariant();

        public StringEnumTestKind Transform(string from)
            => Enum.TryParse(from, true, out StringEnumTestKind result) ? result : StringEnumTestKind.None;
    }

    public readonly struct StringEnumTestThrowingConverter : IStringEnumConverter<StringEnumTestKind>
    {
        public string Transform(StringEnumTestKind from)
            => throw new InvalidOperationException("test failure");

        public StringEnumTestKind Transform(string from)
            => default;
    }

    [Serializable]
    public struct StringEnumTestHolder
    {
        public StringEnum<StringEnumTestKind> kind;
    }

    public sealed class StringEnumTestAsset : ScriptableObject
    {
        public StringEnum<StringEnumTestKind> plain;
        public StringEnum<StringEnumTestKind, StringEnumTestConverter> converted;

        public StringEnum<StringEnumTestKind>[] list = new StringEnum<StringEnumTestKind>[2];

        public List<StringEnum<StringEnumTestKind, StringEnumTestConverter>> convertedList = new() {
            default, default,
        };

        public StringEnum<StringEnumTestAlias> alias;
        public StringEnum<StringEnumTestKind, StringEnumTestThrowingConverter> failing;

        public Overridable<StringEnumTestHolder> overridableHolder = new(value: default, isOverridden: false);
    }

    public sealed class StringEnumTestWindow : EditorWindow
    {
        public Action Draw { get; set; }

        private void OnGUI()
        {
            Draw?.Invoke();
        }
    }
}

#endif
