using System;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.Common;
using EncosyTower.PageFlows.Codex;
using EncosyTower.PageFlows.PubSub;

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

        /// <summary>Gets the identifiers of the scopes of <see cref="CollectionType"/>.</summary>
        ReadOnlyMemory<string> ScopeIdentifiers { get; }

        /// <summary>
        /// Builds the scope collection from <paramref name="scopes"/>; on failure the previous collection stays.
        /// </summary>
        /// <param name="scopes">The scope of each flow, by the identifier of the flow.</param>
        /// <returns>
        /// Success when the collection holds a scope for every identifier; otherwise a failure; see
        /// <see cref="PageFlowCodexError"/>.
        /// </returns>
        Success<PageFlowCodexError> Build(ArrayMap<string, PageFlowScope> scopes);

        /// <summary>
        /// Applies a value of <see cref="IPageFlowScopeCollection"/> to an <see cref="IPage"/>
        /// if that page also implements <see cref="IPageNeedsFlowScopeCollection{TCollection}"/>.
        /// </summary>
        /// <param name="page">The page that receives the collection.</param>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public Success<PageFlowCodexError> Build(ArrayMap<string, PageFlowScope> scopes)
        {
            var value = new TCollection();
            var identifiers = value.ScopeIdentifiers.Span;

            for (var i = 0; i < identifiers.Length; i++)
            {
                var identifier = identifiers[i];

                if (scopes.TryGetValue(identifier, out var scope) == false)
                {
                    return PageFlowCodexError.ScopeNotFound(identifier);
                }

                if (value.TrySetScope(identifier, scope) == false)
                {
                    return PageFlowCodexError.ScopeNotAssigned(identifier);
                }
            }

            _value = value;
            return Success.Yes;
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
