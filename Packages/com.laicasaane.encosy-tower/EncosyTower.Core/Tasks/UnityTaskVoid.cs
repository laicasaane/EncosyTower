using System.Runtime.CompilerServices;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// The return type of a fire-and-forget async method that runs without a task to await.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Behaviour:</b> an <c>async UnityTaskVoid</c> method starts synchronously and runs to completion on its own.
    /// An exception other than <see cref="System.OperationCanceledException"/> is logged through the Encosy
    /// logger; cancellation is ignored. The method's state machine is pooled after it completes.
    /// </para>
    /// <para>
    /// <b>Thread:</b> each await inside the method resumes according to the awaited task's own rules.
    /// </para>
    /// <para>
    /// <b>Counterparts:</b> UniTask: <c>Cysharp.Threading.Tasks.UniTaskVoid</c>; Unity: none.
    /// </para>
    /// </remarks>
    [AsyncMethodBuilder(typeof(UnityTaskVoidMethodBuilder))]
    public readonly struct UnityTaskVoid
    {
        /// <summary>
        /// Does nothing. Call it to state that the result is discarded on purpose.
        /// </summary>
        public void Forget()
        {
        }
    }
}
