using System;

namespace EncosyTower.Tasks
{
    internal interface IUnityTaskSink<TPosition>
    {
        void Complete(TPosition position, Exception exception);

        void Detach();
    }

    internal interface IUnityTaskResultSink<T, TPosition>
    {
        void Complete(TPosition position, T result, Exception exception);

        void Detach();
    }

    internal interface IIndexedUnityTaskSink
    {
        void Complete(int index, Exception exception);

        void Detach();
    }

    internal interface IIndexedUnityTaskResultSink<T>
    {
        void Complete(int index, T result, Exception exception);

        void Detach();
    }

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
