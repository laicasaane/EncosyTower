using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace EncosyTower.UnityExtensions
{
    public static class EncosyUnityObjectExtensions
    {
        /// <summary>
        /// Returns true if <paramref name="self"/> is not null and not destroyed, otherwise returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValid([NotNullWhen(true)] this UnityEngine.Object self)
            => self != null && self != false;

        /// <summary>
        /// Returns true if <paramref name="self"/> is null or destroyed, otherwise returns false.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInvalid([NotNullWhen(false)] this UnityEngine.Object self)
            => self == null || self == false;

        /// <summary>
        /// Returns <paramref name="self"/> if it is valid, otherwise throws an <see cref="System.ArgumentException"/>.
        /// </summary>
        /// <remarks>
        /// Prevent warning CA1062 if the object is surely valid but the compiler still complains.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        /// Thrown if the object is invalid.
        /// </exception>
        /// <returns>The valid object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        public static T AssumeValid<T>(this T self)
            where T : UnityEngine.Object
        {
            ThrowHelper.ThrowIfObjectInvalid(self.IsValid());
            return self;
        }

        /// <summary>
        /// Returns <paramref name="self"/> if it is valid, otherwise returns <paramref name="defaultValue"/>.
        /// </summary>
        /// <remarks>
        /// This method assumes that <paramref name="defaultValue"/> is valid.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="defaultValue"/> is invalid.
        /// </exception>
        /// <param name="defaultValue">The default value to return if <paramref name="self"/> is invalid.</param>
        /// <returns>The valid object.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [return: NotNull]
        public static T ValidOrDefault<T>(this T self, [NotNull] T defaultValue)
            where T : UnityEngine.Object
            => self.IsValid() ? self : defaultValue.AssumeValid();

        /// <summary>
        /// Returns <paramref name="self"/> if it is valid, otherwise initializes <paramref name="backingField"/>
        /// using <paramref name="initializer"/> and returns the initialized value.
        /// </summary>
        /// <remarks>
        /// This method assumes that <paramref name="initializer"/> returns a valid object.
        /// </remarks>
        /// <exception cref="System.ArgumentException">
        /// Thrown if <paramref name="initializer"/> returns an invalid object.
        /// </exception>
        /// <param name="backingField">The backing field to initialize if <paramref name="self"/> is invalid.</param>
        /// <param name="initializer">The function to initialize the backing field.</param>
        /// <returns>The valid object.</returns>
        public static T ValidOrInitialize<T>(this T self, ref T backingField, [NotNull] Func<T> initializer)
            where T : UnityEngine.Object
            => self.IsValid() ? self : (backingField = initializer().AssumeValid());
    }
}
