namespace EncosyTower.Tasks
{
    /// <summary>
    /// Iterates an asynchronous sequence whose elements are produced by <see cref="UnityTask"/> operations.
    /// </summary>
    /// <typeparam name="T">The type of the elements.</typeparam>
    public interface IUnityTaskAsyncEnumerator<out T>
    {
        /// <summary>
        /// Gets the element at the current position of the enumerator, or <c>default</c> after
        /// <see cref="DisposeAsync"/>.
        /// </summary>
        T Current { get; }

        /// <summary>
        /// Advances the enumerator to the next element of the sequence.
        /// </summary>
        /// <returns>
        /// A task whose result is <c>true</c> if the enumerator advanced, or <c>false</c> if the sequence ended or the
        /// enumerator was disposed.
        /// </returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> after the token passed to <see cref="IUnityTaskAsyncEnumerable{T}.GetAsyncEnumerator"/>
        /// is cancelled, the pending and every later call throw <c>new OperationCanceledException(token)</c>.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the awaiter resumes on the kind of thread that called this method: the main thread when
        /// called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Calling this method again before the previous call completes: development builds (<c>UNITY_EDITOR</c>,
        /// <c>DEBUG</c>, <c>ENCOSY_RUNTIME_CHECKS</c>) throw <see cref="System.InvalidOperationException"/>. Release
        /// builds may lose a result or never complete.
        /// </description></item>
        /// <item><description>
        /// Awaiting the returned task or a copy more than once, or calling <c>GetResult</c> twice: may throw
        /// <see cref="System.InvalidOperationException"/>, return a stale result, or observe another operation's
        /// result because sources are pooled.
        /// </description></item>
        /// </list>
        /// </remarks>
        UnityTask<bool> MoveNextAsync();

        /// <summary>
        /// Stops the enumeration and releases its resources.
        /// </summary>
        /// <returns>A task that has already completed.</returns>
        /// <remarks>
        /// <b>Behaviour:</b> a pending <see cref="MoveNextAsync"/> call completes with <c>false</c>. Inputs that have
        /// not completed are still observed and their results are discarded. Calling this method again does nothing.
        /// </remarks>
        UnityTask DisposeAsync();
    }
}
