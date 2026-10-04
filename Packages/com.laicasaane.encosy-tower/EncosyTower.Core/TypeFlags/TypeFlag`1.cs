using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.Ids;
using EncosyTower.Tasks;
using EncosyTower.Types;
using EncosyTower.Vaults;

namespace EncosyTower.TypeFlags
{
    internal readonly struct TypeFlagState { }

    /// <summary>
    /// A process-wide readiness flag for the type <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// Has no instance state: <see langword="default"/>, <c>new()</c>, every copy, and every generated or hand-written
    /// declaration address the same type-wide state. Entering Play Mode with domain reload disabled clears it.
    /// </remarks>
    public readonly struct TypeFlag<T>
    {
        private static Id2 Key
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T>.Id.ToId2();
        }

        /// <summary>
        /// Gets the <see cref="TypeId{T}"/> of <typeparamref name="T"/>.
        /// </summary>
        public TypeId<T> TypeId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<T>.Id;
        }

        /// <summary>
        /// Gets whether the flag of <typeparamref name="T"/> is enabled.
        /// </summary>
        public bool IsEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => GlobalValueVault<TypeFlagState>.Contains(Key);
        }

        /// <summary>
        /// Enables the flag of <typeparamref name="T"/>.
        /// </summary>
        /// <returns><see langword="true"/> only when the flag changes from disabled to enabled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Enable()
            => GlobalValueVault<TypeFlagState>.TryAdd(id: Key, value: default);

        /// <summary>
        /// Disables the flag of <typeparamref name="T"/>.
        /// </summary>
        /// <returns><see langword="true"/> only when the flag changes from enabled to disabled.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Disable()
            => GlobalValueVault<TypeFlagState>.TryRemove(Key, out _);

        /// <summary>
        /// Waits until the flag of <typeparamref name="T"/> is enabled.
        /// </summary>
        /// <remarks>
        /// Completes without waiting for a frame when the flag is already enabled; otherwise checks once per frame.
        /// </remarks>
        /// <exception cref="System.OperationCanceledException">
        /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
        /// </exception>
        public UnityTask WaitUntilEnabledAsync(CancellationToken token = default)
            => GlobalValueVault<TypeFlagState>.WaitUntilContains(Key, token);

        /// <summary>
        /// Gets the link from <typeparamref name="T"/> to <typeparamref name="TLinked"/>, which keys the global object
        /// or value of <typeparamref name="TLinked"/> stored for <typeparamref name="T"/>.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TypeFlagLink<T, TLinked> GetLink<TLinked>()
            => default;

        /// <summary>
        /// Returns the name of <typeparamref name="T"/> and whether its flag is enabled.
        /// </summary>
        public override string ToString()
            => $"TypeFlag<{Type<T>.FriendlyName}> {(IsEnabled ? "Enabled" : "Disabled")}";

        /// <summary>
        /// A read-only view of the flag of <typeparamref name="T"/>.
        /// </summary>
        /// <remarks>
        /// Has no instance state and addresses the same type-wide state as <see cref="TypeFlag{T}"/>.
        /// </remarks>
        public readonly struct ReadOnly
        {
            /// <summary>
            /// Gets the <see cref="TypeId{T}"/> of <typeparamref name="T"/>.
            /// </summary>
            public TypeId<T> TypeId
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Type<T>.Id;
            }

            /// <summary>
            /// Gets whether the flag of <typeparamref name="T"/> is enabled.
            /// </summary>
            public bool IsEnabled
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => default(TypeFlag<T>).IsEnabled;
            }

            /// <summary>
            /// Converts a flag to its read-only view.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static implicit operator ReadOnly(TypeFlag<T> _)
                => default;

            /// <summary>
            /// Waits until the flag of <typeparamref name="T"/> is enabled.
            /// </summary>
            /// <remarks>
            /// Completes without waiting for a frame when the flag is already enabled; otherwise checks once per
            /// frame.
            /// </remarks>
            /// <exception cref="System.OperationCanceledException">
            /// Thrown when <paramref name="token"/> is cancelled before or during the wait.
            /// </exception>
            public UnityTask WaitUntilEnabledAsync(CancellationToken token = default)
                => default(TypeFlag<T>).WaitUntilEnabledAsync(token);

            /// <summary>
            /// Gets the link from <typeparamref name="T"/> to <typeparamref name="TLinked"/>, which keys the global
            /// object or value of <typeparamref name="TLinked"/> stored for <typeparamref name="T"/>.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public TypeFlagLink<T, TLinked> GetLink<TLinked>()
                => default;

            /// <summary>
            /// Returns the name of <typeparamref name="T"/> and whether its flag is enabled.
            /// </summary>
            public override string ToString()
                => default(TypeFlag<T>).ToString();
        }
    }
}
