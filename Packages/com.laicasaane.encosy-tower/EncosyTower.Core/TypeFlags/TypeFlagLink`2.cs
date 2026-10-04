using System.Runtime.CompilerServices;
using EncosyTower.Types;

namespace EncosyTower.TypeFlags
{
    /// <summary>
    /// The link from type flag owner <typeparamref name="TOwner"/> to <typeparamref name="TLinked"/>. It keys at
    /// most one global object or value of <typeparamref name="TLinked"/> stored for <typeparamref name="TOwner"/>.
    /// </summary>
    /// <remarks>
    /// Has no instance state: every copy addresses the same entry.
    /// </remarks>
    public readonly struct TypeFlagLink<TOwner, TLinked>
    {
        /// <summary>
        /// Gets the <see cref="TypeId{T}"/> of <typeparamref name="TOwner"/>.
        /// </summary>
        public TypeId<TOwner> OwnerId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<TOwner>.Id;
        }

        /// <summary>
        /// Gets the <see cref="TypeId{T}"/> of <typeparamref name="TLinked"/>.
        /// </summary>
        public TypeId<TLinked> LinkedId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type<TLinked>.Id;
        }
    }
}
