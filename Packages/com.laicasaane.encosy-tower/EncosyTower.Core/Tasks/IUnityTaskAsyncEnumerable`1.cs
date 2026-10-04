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
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Using the enumerator after the enumeration has ended or after
        /// <see cref="IUnityTaskAsyncEnumerator{T}.DisposeAsync"/>: may throw
        /// <see cref="System.ObjectDisposedException"/> because enumerators are pooled.
        /// </description></item>
        /// </list>
        /// </remarks>
        IUnityTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken token = default);
    }
}
