using EncosyTower.PolyEnumStructs;

namespace Samples.PolyEnumStructs.ObservableLists
{
    [PolyEnumFactoryFor(typeof(ObservableListChange<>.Change))]
    public readonly partial struct ObservableListChange<T>
    {
        private readonly Change _change;

        private ObservableListChange(Change change)
        {
            _change = change;
        }

        public ObservableListChange.Type Kind => (ObservableListChange.Type)_change.GetEnumCase();

        public int OldIndex => _change.OldIndex;

        public int NewIndex => _change.NewIndex;

        public T OldValue => _change.OldValue;

        public T NewValue => _change.NewValue;

        public int Version => _change.Version;

        public bool IsAdd => Is(ObservableListChange.Type.Add);

        public bool IsRemove => Is(ObservableListChange.Type.Remove);

        public bool IsReplace => Is(ObservableListChange.Type.Replace);

        public bool IsMove => Is(ObservableListChange.Type.Move);

        public bool IsClear => Is(ObservableListChange.Type.Clear);

        public bool IsReset => Is(ObservableListChange.Type.Reset);

        [PolyEnumStruct(Container = typeof(ObservableListChangeCases))]
        public readonly partial struct Change
        {
        }
    }

    public static partial class ObservableListChangeCases
    {
        internal partial interface IEnumCase
        {
            public int OldIndex => -1;

            public int NewIndex => -1;

            public int Version => default;
        }

        internal partial interface IEnumCase<T> : IEnumCase
        {
            public T OldValue => default;

            public T NewValue => default;
        }

        public readonly partial record struct Add<T>(int NewIndex, T NewValue, int Version);

        public readonly partial record struct Remove<T>(int OldIndex, T OldValue, int Version);

        public readonly partial record struct Replace<T>(
              int OldIndex
            , int NewIndex
            , T OldValue
            , T NewValue
            , int Version
        );

        public readonly partial record struct Move<T>(
              int OldIndex
            , int NewIndex
            , T OldValue
            , T NewValue
            , int Version
        );

        public readonly partial record struct Clear<T>(int Version);

        public readonly partial record struct Reset<T>(int Version);
    }
}
