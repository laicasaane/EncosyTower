#if !UNITASK || ENCOSY_UNITYTASK_AWAITABLE

using System;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// A reusable completion source that a <see cref="UnityTask"/> reads through a version token on the
    /// <c>Awaitable</c> backend.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Mirrors <c>Cysharp.Threading.Tasks.IUniTaskSource</c> member for member, so every EncosyTower source class
    /// compiles on both backends through the file-local <c>ITaskSource</c> alias. On the UniTask backend the same
    /// classes implement <c>IUniTaskSource</c> instead and are wrapped in a <c>UniTask</c>.
    /// </para>
    /// <para>
    /// A task stores the source and the token it was created with. Pooled sources increment their version when they are
    /// reset, so a member called with an old token throws <see cref="System.InvalidOperationException"/> instead of
    /// reading another operation's outcome.
    /// </para>
    /// </remarks>
    internal interface IUnityTaskSource
    {
        /// <summary>
        /// Returns the state for <paramref name="token"/>; <see cref="UnityTaskStatus.Pending"/> while the current
        /// thread kind differs from the creator's, so the awaiter suspends and resumes on the creator's thread kind.
        /// </summary>
        UnityTaskStatus GetStatus(short token);

        /// <summary>
        /// Returns the state without checking the token or the current thread kind. Used for diagnostics such as
        /// <c>ToString</c>.
        /// </summary>
        UnityTaskStatus UnsafeGetStatus();

        /// <summary>
        /// Registers the single continuation of the operation. It runs once, with <paramref name="state"/>, when the
        /// operation completes; immediately when it has already completed.
        /// </summary>
        void OnCompleted(Action<object> continuation, object state, short token);

        /// <summary>
        /// Ends the operation and rethrows its exception. A pooled source returns to its pool here, which is why a task
        /// may be awaited only once.
        /// </summary>
        void GetResult(short token);
    }

    /// <summary>
    /// A <see cref="IUnityTaskSource"/> that produces a <typeparamref name="T"/>; mirrors
    /// <c>Cysharp.Threading.Tasks.IUniTaskSource{T}</c>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    internal interface IUnityTaskSource<out T> : IUnityTaskSource
    {
        /// <summary>
        /// Ends the operation and returns its result, or rethrows its exception.
        /// </summary>
        new T GetResult(short token);
    }
}

#endif
