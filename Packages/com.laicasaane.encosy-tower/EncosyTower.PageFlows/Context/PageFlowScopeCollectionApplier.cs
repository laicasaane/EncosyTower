using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;

namespace EncosyTower.PageFlows
{
    /// <summary>
    /// Provides the necessary mechanism to create and set
    /// the value of a type that implements <see cref="IPageFlowScopeCollection"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="PageFlowScopeCollectionApplier{TFlowScopes}"/> should be used
    /// instead to minimize the user code.
    /// </remarks>
    public interface IPageFlowScopeCollectionApplier
    {
        /// <summary>
        /// The type of struct that implements <see cref="IPageFlowScopeCollection"/>.
        /// </summary>
        Type CollectionType { get; }

        ReadOnlyMemory<string> ScopeIdentifiers { get; }

        bool TryBuild(ArrayMap<string, PageFlowScope> scopes, out string missingIdentifier);

        /// <summary>
        /// Applies a value of <see cref="IPageFlowScopeCollection"/> to an <see cref="IPage"/>
        /// if that page also implements <see cref="IPageNeedsFlowScopeCollection{TCollection}"/>.
        /// </summary>
        /// <param name="page"></param>
        void ApplyTo(IPage page);
    }

    /// <summary>
    /// Provides a ready-to-use implementation of <see cref="IPageFlowScopeCollectionApplier"/>.
    /// </summary>
    /// <typeparam name="TCollection">
    /// The struct implements <see cref="IPageFlowScopeCollection"/>.
    /// </typeparam>
    /// <remarks>
    /// To simplify user code, this type should be used instead.
    /// </remarks>
    public sealed class PageFlowScopeCollectionApplier<TCollection>
        : IPageFlowScopeCollectionApplier
        , ITryGet<TCollection>
        where TCollection : struct, IPageFlowScopeCollection
    {
        private Option<TCollection> _value;

        public ReadOnlyMemory<string> ScopeIdentifiers => new TCollection().ScopeIdentifiers;

        /// <inheritdoc/>
        Type IPageFlowScopeCollectionApplier.CollectionType => typeof(TCollection);

        /// <summary>
        /// Attempts to retrieve a value of <typeparamref name="TCollection"/>.
        /// </summary>
        /// <remarks>
        /// A codex system should be responsible to create this value and
        /// assign a <see cref="PageFlowScope"/> to each of its properties.
        /// </remarks>
        /// <param name="result">The value of <typeparamref name="TCollection"/> created by a codex system.</param>
        /// <returns>True if the value exists, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGet(out TCollection result)
            => _value.TryGetValue(out result);

        public bool TryBuild(ArrayMap<string, PageFlowScope> scopes, out string missingIdentifier)
        {
            var value = new TCollection();
            var identifiers = value.ScopeIdentifiers.Span;

            for (var i = 0; i < identifiers.Length; i++)
            {
                var identifier = identifiers[i];

                if (scopes.TryGetValue(identifier, out var scope) == false)
                {
                    missingIdentifier = identifier;
                    return false;
                }

                value.TrySetScope(identifier, scope);
            }

            _value = value;
            missingIdentifier = null;
            return true;
        }

        /// <inheritdoc/>
        void IPageFlowScopeCollectionApplier.ApplyTo(IPage page)
        {
            if (page is IPageNeedsFlowScopeCollection<TCollection> collection)
            {
                collection.FlowScopeCollection = _value;
            }
        }
    }
}
