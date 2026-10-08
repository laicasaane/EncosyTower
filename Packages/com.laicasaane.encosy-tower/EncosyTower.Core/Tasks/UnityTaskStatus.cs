namespace EncosyTower.Tasks
{
    /// <summary>
    /// Describes the state of a <see cref="UnityTask"/> or <see cref="UnityTask{T}"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The values match <c>Cysharp.Threading.Tasks.UniTaskStatus</c>.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.UniTaskStatus</c>; Unity: none.
    /// </para>
    /// </remarks>
    public enum UnityTaskStatus
    {
        /// <summary>The operation has not completed yet.</summary>
        Pending = 0,

        /// <summary>The operation completed successfully.</summary>
        Succeeded = 1,

        /// <summary>The operation completed with an exception other than
        /// <see cref="System.OperationCanceledException"/>.</summary>
        Faulted = 2,

        /// <summary>The operation completed with an <see cref="System.OperationCanceledException"/>.</summary>
        Canceled = 3,
    }
}
