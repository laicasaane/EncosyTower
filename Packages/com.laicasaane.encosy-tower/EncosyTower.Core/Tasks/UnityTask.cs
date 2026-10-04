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
        public async UnityTask AsUnityTask()
            => _ = await this;
    }
}
