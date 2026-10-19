using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.Common;
using UnityEngine;

namespace EncosyTower.Serialization
{
    /// <summary>
    /// A serializable value that either uses a default source or overrides it with <see cref="value"/>.
    /// </summary>
    /// <typeparam name="T">The value type. Unity must be able to serialize it.</typeparam>
    /// <remarks>
    /// <para>
    /// The default value of this type is not overridden, which means "use the default source".
    /// The default source is declared on the field with <see cref="OverridableDefaultAttribute"/>.
    /// </para>
    /// <para>
    /// The type has two serialized fields, <see cref="value"/> then <see cref="isOverridden"/>; the Editor drawer
    /// finds them by name. <see cref="value"/> is stored even while not overridden, but
    /// <see cref="TryGetValue"/> and <see cref="GetValueOrDefault"/> ignore it.
    /// </para>
    /// <para>
    /// This type never reads the default source at run time. Runtime code passes its own fallback to
    /// <see cref="GetValueOrDefault"/>, or reads <see cref="TryGetValue"/>.
    /// </para>
    /// <para>
    /// In the Inspector (<c>OverridablePropertyDrawer</c>), a row shows an override switch and the value.
    /// While the switch is off, the row shows the resolved default source read-only.
    /// Turning the switch on copies the shown default into <see cref="value"/>; turning it off keeps
    /// <see cref="value"/>. Each switch change is one Undo step.
    /// A non-flags enum uses a dropdown instead of a switch; its first item selects the default source.
    /// Selecting several objects with different states shows a mixed state.
    /// </para>
    /// <para>
    /// An <see cref="Option{T}"/> holds no value for a <see langword="null"/> reference or a destroyed Unity object.
    /// So when <see cref="isOverridden"/> is true and <see cref="value"/> is such a value,
    /// <see cref="TryGetValue"/> returns false and the conversion to <see cref="Option{T}"/> is
    /// <see cref="Option{T}.None"/>, while <see cref="GetValueOrDefault"/> still returns <see cref="value"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="OverridableDefaultAttribute"/>
    /// <seealso cref="EncosyTower.Editor.ValueChoicesAttribute"/>
    [Serializable]
    public struct Overridable<T> : IEquatable<Overridable<T>>
    {
        /// <summary>
        /// The overriding value. It is used only while <see cref="isOverridden"/> is true.
        /// </summary>
        /// <remarks>
        /// The Editor keeps this value when the override is turned off.
        /// </remarks>
        [Tooltip("The overriding value.")]
        public T value;

        /// <summary>
        /// Whether the <see cref="value"/> overrides the default source.
        /// </summary>
        [Tooltip("Whether the value overrides the default source.")]
        public bool isOverridden;

        /// <summary>
        /// Creates an <see cref="Overridable{T}"/> with a value and whether it overrides the default source.
        /// </summary>
        /// <param name="value">The value to store.</param>
        /// <param name="isOverridden">Whether <paramref name="value"/> overrides the default source.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Overridable(T value, bool isOverridden)
        {
            this.value = value;
            this.isOverridden = isOverridden;
        }

        /// <summary>
        /// Converts the overriding value to <see cref="Option{T}"/>; not overridden becomes <see cref="Option.None"/>.
        /// </summary>
        /// <param name="overridable">The value to convert.</param>
        /// <returns>
        /// An <see cref="Option{T}"/> holding <see cref="value"/> when <see cref="isOverridden"/> is true;
        /// otherwise <see cref="Option{T}.None"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Option<T>(Overridable<T> overridable)
            => overridable.isOverridden ? Option.Some(overridable.value) : Option.None;

        /// <summary>
        /// Overrides with the value of <paramref name="option"/>; <see cref="Option.None"/> uses the default source.
        /// </summary>
        /// <param name="option">The option to convert.</param>
        /// <returns>
        /// An overridden value holding the value of <paramref name="option"/>, or a value that is not overridden
        /// when <paramref name="option"/> holds no value.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Overridable<T>(Option<T> option)
            => option.TryGetValue(out var value) ? new(value, true) : default;

        /// <summary>
        /// Keeps <paramref name="value"/> as the local value without overriding.
        /// </summary>
        /// <remarks>
        /// The result is not overridden, so <see cref="GetValueOrDefault"/> and <see cref="TryGetValue"/> ignore
        /// <paramref name="value"/>.
        /// </remarks>
        /// <param name="value">The value to store.</param>
        /// <returns>A value that stores <paramref name="value"/> and is not overridden.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Overridable<T>(T value)
            => new(value, false);

        /// <summary>
        /// Compares this value with another by override state and stored value.
        /// </summary>
        /// <remarks>
        /// The stored values are compared with <see cref="EqualityComparer{T}.Default"/>
        /// even when neither value is overridden.
        /// </remarks>
        /// <param name="other">The value to compare with.</param>
        /// <returns>
        /// <see langword="true"/> when both have the same <see cref="isOverridden"/> and equal
        /// <see cref="value"/>; otherwise <see langword="false"/>.
        /// </returns>
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
        /// <param name="value">Receives the overriding value, or the default of <typeparamref name="T"/>.</param>
        /// <returns>
        /// <see langword="true"/> when <see cref="isOverridden"/> is true and <see cref="value"/> is neither a
        /// <see langword="null"/> reference nor a destroyed Unity object; otherwise <see langword="false"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool TryGetValue(out T value)
            => ((Option<T>)this).TryGetValue(out value);

        /// <summary>
        /// Returns the overriding value, or <paramref name="defaultValue"/> when not overridden.
        /// </summary>
        /// <param name="defaultValue">The fallback returned while <see cref="isOverridden"/> is false.</param>
        /// <returns>
        /// <see cref="value"/> when <see cref="isOverridden"/> is true; otherwise <paramref name="defaultValue"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T GetValueOrDefault(T defaultValue = default)
            => isOverridden ? value : defaultValue;
    }
}
