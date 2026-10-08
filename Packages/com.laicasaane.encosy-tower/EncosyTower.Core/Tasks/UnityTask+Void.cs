using System;
using System.Threading;
using UnityEngine.Events;

namespace EncosyTower.Tasks
{
    public readonly partial struct UnityTask
    {
        /// <summary>
        /// Runs <paramref name="asyncAction"/> without waiting for it.
        /// </summary>
        /// <param name="asyncAction">The fire-and-forget method to run.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> starts <paramref name="asyncAction"/> synchronously. Exceptions follow
        /// <see cref="UnityTaskVoid"/> rules.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Void</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static void Void(Func<UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            asyncAction().Forget();
        }

        /// <summary>
        /// Runs <paramref name="asyncAction"/> with <paramref name="token"/> without waiting for it.
        /// </summary>
        /// <param name="asyncAction">The fire-and-forget method to run.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> starts <paramref name="asyncAction"/> synchronously. Exceptions follow
        /// <see cref="UnityTaskVoid"/> rules.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Void</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static void Void(Func<CancellationToken, UnityTaskVoid> asyncAction, CancellationToken token)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            asyncAction(token).Forget();
        }

        /// <summary>
        /// Runs <paramref name="asyncAction"/> with <paramref name="state"/> without waiting for it.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="asyncAction"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="asyncAction"/>.</param>
        /// <param name="asyncAction">The fire-and-forget method to run.</param>
        /// <remarks>
        /// <para>
        /// <b>Behaviour:</b> starts <paramref name="asyncAction"/> synchronously. Exceptions follow
        /// <see cref="UnityTaskVoid"/> rules.
        /// </para>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Void</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static void Void<TState>(TState state, Func<TState, UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            asyncAction(state).Forget();
        }

        /// <summary>
        /// Creates a delegate that runs <paramref name="asyncAction"/> without waiting for it.
        /// </summary>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Action</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static Action Action(Func<UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke()
                => asyncAction().Forget();
        }

        /// <summary>
        /// Creates a delegate that runs <paramref name="asyncAction"/> with <paramref name="token"/> without waiting
        /// for it.
        /// </summary>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Action</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static Action Action(Func<CancellationToken, UnityTaskVoid> asyncAction, CancellationToken token)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke()
                => asyncAction(token).Forget();
        }

        /// <summary>
        /// Creates a delegate that runs <paramref name="asyncAction"/> with <paramref name="state"/> without waiting
        /// for it.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="asyncAction"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="asyncAction"/>.</param>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.Action</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static Action Action<TState>(TState state, Func<TState, UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke()
                => asyncAction(state).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityEngine.Events.UnityAction"/> that runs <paramref name="asyncAction"/> without
        /// waiting for it.
        /// </summary>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction UnityAction(Func<UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke()
                => asyncAction().Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityEngine.Events.UnityAction"/> that runs <paramref name="asyncAction"/> with
        /// <paramref name="token"/> without waiting for it.
        /// </summary>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction UnityAction(
              Func<CancellationToken, UnityTaskVoid> asyncAction
            , CancellationToken token
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke()
                => asyncAction(token).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityEngine.Events.UnityAction"/> that runs <paramref name="asyncAction"/> with
        /// <paramref name="state"/> without waiting for it.
        /// </summary>
        /// <typeparam name="TState">The type of the state passed to <paramref name="asyncAction"/>.</typeparam>
        /// <param name="state">The state passed to <paramref name="asyncAction"/>.</param>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction UnityAction<TState>(TState state, Func<TState, UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke()
                => asyncAction(state).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0}"/> that runs <paramref name="asyncAction"/> without waiting for it.
        /// </summary>
        /// <typeparam name="T">The type of the event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T> UnityAction<T>(Func<T, UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T arg)
                => asyncAction(arg).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0, T1}"/> that runs <paramref name="asyncAction"/> without waiting for
        /// it.
        /// </summary>
        /// <typeparam name="T0">The type of the first event argument.</typeparam>
        /// <typeparam name="T1">The type of the second event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T0, T1> UnityAction<T0, T1>(Func<T0, T1, UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T0 arg0, T1 arg1)
                => asyncAction(arg0, arg1).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0, T1, T2}"/> that runs <paramref name="asyncAction"/> without waiting
        /// for it.
        /// </summary>
        /// <typeparam name="T0">The type of the first event argument.</typeparam>
        /// <typeparam name="T1">The type of the second event argument.</typeparam>
        /// <typeparam name="T2">The type of the third event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T0, T1, T2> UnityAction<T0, T1, T2>(Func<T0, T1, T2, UnityTaskVoid> asyncAction)
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T0 arg0, T1 arg1, T2 arg2)
                => asyncAction(arg0, arg1, arg2).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0, T1, T2, T3}"/> that runs <paramref name="asyncAction"/> without
        /// waiting for it.
        /// </summary>
        /// <typeparam name="T0">The type of the first event argument.</typeparam>
        /// <typeparam name="T1">The type of the second event argument.</typeparam>
        /// <typeparam name="T2">The type of the third event argument.</typeparam>
        /// <typeparam name="T3">The type of the fourth event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T0, T1, T2, T3> UnityAction<T0, T1, T2, T3>(
              Func<T0, T1, T2, T3, UnityTaskVoid> asyncAction
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
                => asyncAction(arg0, arg1, arg2, arg3).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0}"/> that runs <paramref name="asyncAction"/> with
        /// <paramref name="token"/> without waiting for it.
        /// </summary>
        /// <typeparam name="T">The type of the event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T> UnityAction<T>(
              Func<T, CancellationToken, UnityTaskVoid> asyncAction
            , CancellationToken token
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T arg)
                => asyncAction(arg, token).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0, T1}"/> that runs <paramref name="asyncAction"/> with
        /// <paramref name="token"/> without waiting for it.
        /// </summary>
        /// <typeparam name="T0">The type of the first event argument.</typeparam>
        /// <typeparam name="T1">The type of the second event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T0, T1> UnityAction<T0, T1>(
              Func<T0, T1, CancellationToken, UnityTaskVoid> asyncAction
            , CancellationToken token
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T0 arg0, T1 arg1)
                => asyncAction(arg0, arg1, token).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0, T1, T2}"/> that runs <paramref name="asyncAction"/> with
        /// <paramref name="token"/> without waiting for it.
        /// </summary>
        /// <typeparam name="T0">The type of the first event argument.</typeparam>
        /// <typeparam name="T1">The type of the second event argument.</typeparam>
        /// <typeparam name="T2">The type of the third event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T0, T1, T2> UnityAction<T0, T1, T2>(
              Func<T0, T1, T2, CancellationToken, UnityTaskVoid> asyncAction
            , CancellationToken token
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T0 arg0, T1 arg1, T2 arg2)
                => asyncAction(arg0, arg1, arg2, token).Forget();
        }

        /// <summary>
        /// Creates a <see cref="UnityAction{T0, T1, T2, T3}"/> that runs <paramref name="asyncAction"/> with
        /// <paramref name="token"/> without waiting for it.
        /// </summary>
        /// <typeparam name="T0">The type of the first event argument.</typeparam>
        /// <typeparam name="T1">The type of the second event argument.</typeparam>
        /// <typeparam name="T2">The type of the third event argument.</typeparam>
        /// <typeparam name="T3">The type of the fourth event argument.</typeparam>
        /// <param name="asyncAction">The fire-and-forget method to run on each call.</param>
        /// <param name="token">The token passed to <paramref name="asyncAction"/>.</param>
        /// <returns>A new delegate.</returns>
        /// <remarks>
        /// <para>
        /// <b>Counterparts:</b> UniTask: <c>UniTask.UnityAction</c>; Unity: none.
        /// </para>
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="asyncAction"/> is <c>null</c>.</exception>
        public static UnityAction<T0, T1, T2, T3> UnityAction<T0, T1, T2, T3>(
              Func<T0, T1, T2, T3, CancellationToken, UnityTaskVoid> asyncAction
            , CancellationToken token
        )
        {
            Debugging.ThrowHelper.ThrowIfNull(asyncAction);
            return Invoke;

            void Invoke(T0 arg0, T1 arg1, T2 arg2, T3 arg3)
                => asyncAction(arg0, arg1, arg2, arg3, token).Forget();
        }
    }
}
