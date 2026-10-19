using EncosyTower.Core;

namespace EncosyTower.Editor
{
    /// <summary>
    /// A labelled choice available in all builds so a choices member compiles everywhere.
    /// Only the Editor drawer reads it.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the field value, or the array or list type when the attribute sets
    /// <c>applyToCollection</c> to <c>true</c>.
    /// </typeparam>
    /// <remarks>
    /// A member named by <see cref="ValueChoicesAttribute"/> returns these choices as
    /// <c>IEnumerable&lt;ValueChoice&lt;T&gt;&gt;</c>. A choice is not serialized and does nothing at run time.
    /// When the label is <c>null</c> or empty, the Editor uses the <c>[InspectorName]</c> of an enum member,
    /// otherwise the display text of the value.
    /// </remarks>
    /// <seealso cref="ValueChoicesAttribute"/>
    /// <seealso cref="ValueChoice"/>
    [ApiForEditor]
    public readonly struct ValueChoice<T>
    {
        /// <summary>
        /// Creates a choice without a label; the Editor derives the label from the value.
        /// </summary>
        /// <param name="value">The value written to the field when the choice is picked.</param>
        [ApiForEditor]
        public ValueChoice(T value)
        {
            Label = null;
            Value = value;
        }

        /// <summary>
        /// Creates a choice with a label.
        /// </summary>
        /// <param name="label">The text shown for the choice.</param>
        /// <param name="value">The value written to the field when the choice is picked.</param>
        [ApiForEditor]
        public ValueChoice(string label, T value)
        {
            Label = label;
            Value = value;
        }

        /// <summary>
        /// The text shown for the choice, or <c>null</c> when the Editor derives it from <see cref="Value"/>.
        /// </summary>
        [ApiForEditor]
        public string Label { get; }

        /// <summary>
        /// The value written to the field when the choice is picked.
        /// </summary>
        [ApiForEditor]
        public T Value { get; }
    }

    /// <summary>
    /// A type-erased labelled choice available in all builds so a choices member compiles everywhere.
    /// Only the Editor drawer reads it.
    /// </summary>
    /// <remarks>
    /// Use it when the choices have mixed or unknown value types. Each <see cref="Value"/> must be assignable to
    /// the field value type. Otherwise the Editor logs a warning and draws the field normally.
    /// The label rules are the same as for <see cref="ValueChoice{T}"/>.
    /// </remarks>
    /// <seealso cref="ValueChoicesAttribute"/>
    [ApiForEditor]
    public readonly struct ValueChoice
    {
        /// <summary>
        /// Creates a choice without a label; the Editor derives the label from the value.
        /// </summary>
        /// <param name="value">The value written to the field when the choice is picked.</param>
        [ApiForEditor]
        public ValueChoice(object value)
        {
            Label = null;
            Value = value;
        }

        /// <summary>
        /// Creates a choice with a label.
        /// </summary>
        /// <param name="label">The text shown for the choice.</param>
        /// <param name="value">The value written to the field when the choice is picked.</param>
        [ApiForEditor]
        public ValueChoice(string label, object value)
        {
            Label = label;
            Value = value;
        }

        /// <summary>
        /// The text shown for the choice, or <c>null</c> when the Editor derives it from <see cref="Value"/>.
        /// </summary>
        [ApiForEditor]
        public string Label { get; }

        /// <summary>
        /// The value written to the field when the choice is picked.
        /// </summary>
        [ApiForEditor]
        public object Value { get; }
    }
}
