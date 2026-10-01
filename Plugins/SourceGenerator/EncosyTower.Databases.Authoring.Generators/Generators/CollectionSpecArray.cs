namespace EncosyTower.Databases.Authoring.Generators
{
    public readonly struct CollectionSpecArray : IEquatable<CollectionSpecArray>
    {
        private readonly CollectionSpec[] _items;

        public CollectionSpecArray(CollectionSpec item)
        {
            _items = new[] { item };
        }

        public CollectionSpecArray(CollectionSpec first, CollectionSpec second)
        {
            _items = new[] { first, second };
        }

        public ref readonly CollectionSpec this[int index]
        {
            get => ref _items[index];
        }

        public int Count => _items?.Length ?? 0;

        public readonly bool Equals(CollectionSpecArray other)
            => AsReadOnlySpan().SequenceEqual(other.AsReadOnlySpan());

        public readonly override bool Equals(object obj)
            => obj is CollectionSpecArray other && Equals(other);

        public readonly override int GetHashCode()
        {
            HashValue hash = default;
            var count = Count;

            for (var i = 0; i < count; i++)
            {
                hash = hash.Add(_items[i]);
            }

            return hash.ToHashCode();
        }

        public readonly ReadOnlySpan<CollectionSpec>.Enumerator GetEnumerator()
            => AsReadOnlySpan().GetEnumerator();

        private readonly ReadOnlySpan<CollectionSpec> AsReadOnlySpan()
            => _items;
    }
}
