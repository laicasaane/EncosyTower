using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.UnityExtensions;
using UnityEngine.UIElements;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.UIElements
{
    public static class EncosyUIElementExtensions
    {
        /// <summary>
        /// Adds <c>unity-base-field__aligned</c> to the class list of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithAlignFieldClass<T>([NotNull] this T self)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            return WithClass(self, TextField.alignedFieldUssClassName);
        }

        /// <summary>
        /// Adds <paramref name="className"/> to the class list of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithClass<T>([NotNull] this T self, [NotNull] string className)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(className);
            self.AddToClassList(className);
            return self;
        }

        /// <summary>
        /// Set <paramref name="name"/> to <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithName<T>([NotNull] this T self, [NotNull] string name)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(name);
            self.name = name;
            return self;
        }

        /// <summary>
        /// Adds a cloned tree of <paramref name="asset"/> into <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        public static T WithCloneTree<T>([NotNull] this T self, VisualTreeAsset asset)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            if (asset.IsValid())
            {
                asset.CloneTree(self);
            }

            return self;
        }

        /// <summary>
        /// Adds <paramref name="child"/> to <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithChild<T>([NotNull] this T self, [NotNull] VisualElement child)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            DebuggingThrowHelper.ThrowIfNull(child);
            self.Add(child);
            return self;
        }

        /// <summary>
        /// Sets <paramref name="display"/> to the style of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithDisplay<T>([NotNull] this T self, DisplayStyle display)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            self.style.display = display;
            return self;
        }

        /// <summary>
        /// Adds <paramref name="styleSheet"/> to the style sheets of <paramref name="self"/>
        /// then returns <paramref name="self"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T WithStyleSheet<T>([NotNull] this T self, StyleSheet styleSheet)
            where T : VisualElement
        {
            DebuggingThrowHelper.ThrowIfNull(self);
            if (styleSheet.IsValid())
            {
                self.styleSheets.Add(styleSheet);
            }

            return self;
        }
    }
}
