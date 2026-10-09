using EncosyTower.Core;

namespace EncosyTower.Editor
{
    /// <summary>
    /// A labelled choice available in all builds so a choices member compiles everywhere.
    /// Only the Editor drawer reads it.
    /// </summary>
    [ApiForEditor]
    public readonly struct ValueChoice<T>
    {
        /// <summary>
        /// Creates a choice with an empty label, falling back to a supported attribute's label or the display text.
        /// </summary>
        [ApiForEditor]
        public ValueChoice(T value)
        {
            Label = null;
            Value = value;
        }

        [ApiForEditor]
        public ValueChoice(string label, T value)
        {
            Label = label;
            Value = value;
        }

        [ApiForEditor]
        public string Label { get; }

        [ApiForEditor]
        public T Value { get; }
    }

    /// <summary>
    /// A type-erased labelled choice available in all builds so a choices member compiles everywhere.
    /// Only the Editor drawer reads it.
    /// </summary>
    [ApiForEditor]
    public readonly struct ValueChoice
    {
        /// <summary>
        /// Creates a choice with an empty label, falling back to a supported attribute's label or the display text.
        /// </summary>
        [ApiForEditor]
        public ValueChoice(object value)
        {
            Label = null;
            Value = value;
        }

        [ApiForEditor]
        public ValueChoice(string label, object value)
        {
            Label = label;
            Value = value;
        }

        [ApiForEditor]
        public string Label { get; }

        [ApiForEditor]
        public object Value { get; }
    }
}
