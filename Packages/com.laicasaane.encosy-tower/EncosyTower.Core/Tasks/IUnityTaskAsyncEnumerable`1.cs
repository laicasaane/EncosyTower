using System.Threading;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Exposes an asynchronous sequence whose elements are produced by <see cref="UnityTask"/> operations.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    /// <remarks>
    /// The sequence is lazy: no input is observed until the first
    /// <see cref="IUnityTaskAsyncEnumerator{T}.MoveNextAsync"/> call.
    /// </remarks>
    public interface IUnityTaskAsyncEnumerable<out T>
    {
        /// <summary>
        /// Returns an enumerator that iterates the sequence asynchronously.
        /// </summary>
        /// <param name="token">The token that cancels the enumeration.</param>
        /// <returns>An enumerator for the sequence.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> cancelling <paramref name="token"/> makes the pending and every later
        /// <see cref="IUnityTaskAsyncEnumerator{T}.MoveNextAsync"/> call throw
        /// <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// After <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/>, further
        /// <see cref="IUnityTaskAsyncEnumerator{T}.MoveNextAsync"/> calls return <c>false</c>,
        /// <see cref="IUnityTaskAsyncEnumerator{T}.Current"/> returns <c>default</c>, and further
        /// <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/> calls do nothing.
        /// </para>
        /// </remarks>
        IUnityTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default);
    }
}
