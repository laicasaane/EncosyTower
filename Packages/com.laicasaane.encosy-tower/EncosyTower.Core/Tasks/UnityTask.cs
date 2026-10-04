using System.Runtime.CompilerServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Represents an asynchronous operation that behaves the same on the UniTask and <c>Awaitable</c> backends.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Backend selection: UniTask when <c>com.cysharp.unitask</c> is installed (<c>UNITASK</c>), unless
    /// <c>ENCOSY_UNITYTASK_AWAITABLE</c> is defined, which forces Unity's <c>Awaitable</c>.
    /// </para>
    /// <para>
    /// The default value is a successfully completed task.
    /// </para>
    /// </remarks>
    [AsyncMethodBuilder(typeof(UnityTaskAsyncMethodBuilder))]
    public readonly partial struct UnityTask
    {
    }

    /// <summary>
    /// Represents an asynchronous operation that produces a result of type <typeparamref name="T"/> and behaves the
    /// same on the UniTask and <c>Awaitable</c> backends.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <remarks>
    /// <para>
    /// Backend selection: UniTask when <c>com.cysharp.unitask</c> is installed (<c>UNITASK</c>), unless
    /// <c>ENCOSY_UNITYTASK_AWAITABLE</c> is defined, which forces Unity's <c>Awaitable</c>.
    /// </para>
    /// <para>
    /// The default value is a successfully completed task whose result is <c>default</c>.
    /// </para>
    /// </remarks>
    [AsyncMethodBuilder(typeof(UnityTaskAsyncMethodBuilder<>))]
    public readonly partial struct UnityTask<T>
    {
        /// <summary>
        /// Returns a <see cref="UnityTask"/> that completes when this task completes and discards its result.
        /// </summary>
        /// <returns>A task without a result.</returns>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> a fault or cancellation of this task propagates unchanged, as the same exception
        /// instance.
        /// </para>
        /// <para>
        /// <b>Thread:</b> the returned task resumes its awaiter on the kind of thread that called this method: the
        /// main thread when called on the main thread, a thread-pool thread otherwise.
        /// </para>
        /// <para><b>Undefined behaviour:</b></para>
        /// <list type="bullet">
        /// <item><description>
        /// Awaiting this task elsewhere as well, or awaiting the returned task or a copy more than once: may throw
        /// <see cref="System.InvalidOperationException"/>, return a stale result, or observe another operation's
        /// result because sources are pooled.
        /// </description></item>
        /// </list>
        /// </remarks>
        public async UnityTask AsUnityTask()
            => _ = await this;
    }
}
