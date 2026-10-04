using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;

namespace EncosyTower.Serialization
{
    /// <summary>
    /// A serializable value that either uses a default source or overrides it with <see cref="value"/>.
    /// </summary>
    /// <remarks>
    /// The default value of this type is not overridden, which means "use the default source".
    /// The default source is declared on the field with <see cref="OverridableDefaultAttribute"/>.
    /// </remarks>
    [Serializable]
    public struct Overridable<T> : IEquatable<Overridable<T>>
    {
        /// <summary>
        /// The overriding value. It is used only while <see cref="isOverridden"/> is true.
        /// </summary>
        public T value;

        /// <summary>
        /// Whether <see cref="value"/> overrides the default source.
        /// </summary>
        public bool isOverridden;

        /// <summary>
        /// Creates an <see cref="Overridable{T}"/> with a value and whether it overrides the default source.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Overridable(T value, bool isOverridden)
        {
            this.value = value;
            this.isOverridden = isOverridden;
        }

        /// <summary>
        /// Converts the overriding value to <see cref="Option{T}"/>; not overridden becomes <see cref="Option.None"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Option<T>(Overridable<T> overridable)
            => overridable.isOverridden ? Option.Some(overridable.value) : Option.None;

        /// <summary>
        /// Overrides with the value of <paramref name="option"/>; <see cref="Option.None"/> uses the default source.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Overridable<T>(Option<T> option)
            => option.TryGetValue(out var value) ? new(value, true) : default;

        /// <summary>
        /// Keeps <paramref name="value"/> as the local value without overriding.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Overridable<T>(T value)
            => new(value, false);

        /// <summary>
        /// Returns true when both values have the same override state and the same value.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(Overridable<T> other)
            => isOverridden == other.isOverridden
            && EqualityComparer<T>.Default.Equals(value, other.value)
            ;

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
            => obj is Overridable<T> other && Equals(other);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
            => HashValue.Combine(isOverridden, value);

        /// <summary>
        /// Gets the overriding value when <see cref="isOverridden"/> is true.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(out T value)
            => ((Option<T>)this).TryGetValue(out value);

        /// <summary>
        /// Returns the overriding value, or <paramref name="defaultValue"/> when not overridden.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValueOrDefault(T defaultValue = default)
            => isOverridden ? value : defaultValue;
    }
}
