using System;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Receives the outcome of one input of a fixed-arity combinator such as <c>WhenAll</c> or <c>WhenAny</c> from a
    /// <see cref="PooledUnityTaskObserver{TPosition, TSink}"/>.
    /// </summary>
    /// <typeparam name="TPosition">The <c>UnityTaskPositionN</c> tag type of the input.</typeparam>
    /// <remarks>
    /// A sink implements this interface once for each tag, so each input reaches its own <c>Complete</c> overload and
    /// no index is passed. <c>Detach</c> is called after the observer has returned to its pool.
    /// </remarks>
    internal interface IUnityTaskSink<TPosition>
    {
        void Complete(TPosition position, Exception exception);

        void Detach();
    }

    /// <summary>
    /// Receives the outcome of one <see cref="UnityTask{T}"/> input of a fixed-arity combinator from a
    /// <see cref="PooledUnityTaskObserver{T, TPosition, TSink}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    /// <typeparam name="TPosition">The <c>UnityTaskPositionN</c> tag type of the input.</typeparam>
    /// <remarks>
    /// Works as <see cref="IUnityTaskSink{TPosition}"/> does, and also receives the result.
    /// </remarks>
    internal interface IUnityTaskResultSink<T, TPosition>
    {
        void Complete(TPosition position, T result, Exception exception);

        void Detach();
    }

    /// <summary>
    /// Receives the outcome of one input of a combinator over a sequence from a
    /// <see cref="PooledIndexedUnityTaskObserver{TSink}"/>; the input is identified by its index.
    /// </summary>
    internal interface IIndexedUnityTaskSink
    {
        void Complete(int index, Exception exception);

        void Detach();
    }

    /// <summary>
    /// Receives the outcome of one <see cref="UnityTask{T}"/> input of a combinator over a sequence from a
    /// <see cref="PooledIndexedUnityTaskObserver{T, TSink}"/>; the input is identified by its index.
    /// </summary>
    /// <typeparam name="T">The type of the result.</typeparam>
    internal interface IIndexedUnityTaskResultSink<T>
    {
        void Complete(int index, T result, Exception exception);

        void Detach();
    }

    // Tag types: UnityTaskPosition1 to UnityTaskPosition15 only select an interface implementation; they hold no data.
    internal readonly struct UnityTaskPosition1 { }

    internal readonly struct UnityTaskPosition2 { }

    internal readonly struct UnityTaskPosition3 { }

    internal readonly struct UnityTaskPosition4 { }

    internal readonly struct UnityTaskPosition5 { }

    internal readonly struct UnityTaskPosition6 { }

    internal readonly struct UnityTaskPosition7 { }

    internal readonly struct UnityTaskPosition8 { }

    internal readonly struct UnityTaskPosition9 { }

    internal readonly struct UnityTaskPosition10 { }

    internal readonly struct UnityTaskPosition11 { }

    internal readonly struct UnityTaskPosition12 { }

    internal readonly struct UnityTaskPosition13 { }

    internal readonly struct UnityTaskPosition14 { }

    internal readonly struct UnityTaskPosition15 { }
}
