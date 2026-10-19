using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Logging;
using EncosyTower.PolyEnumStructs;
using EncosyTower.Pooling;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.PageFlows.Codex
{
    /// <summary>Describes why a page codex could not initialize.</summary>
    /// <remarks>
    /// <para>The failure is one of these cases:</para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>When it occurs</description>
    /// </listheader>
    /// <item>
    /// <term><c>Undefined</c></term>
    /// <description>The default value; the codex never returns it.</description>
    /// </item>
    /// <item>
    /// <term><c>InitializerMissing</c></term>
    /// <description>
    /// The GameObject of the codex has no component that implements the initializer interface of the codex.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>ScopeCollectionApplierMissing</c></term>
    /// <description>The initializer returned a <c>null</c> scope collection applier.</description>
    /// </item>
    /// <item>
    /// <term><c>ScopeCollectionTypeMissing</c></term>
    /// <description>The scope collection applier returned a <c>null</c> collection type.</description>
    /// </item>
    /// <item>
    /// <term><c>NoScopeProperties</c></term>
    /// <description>
    /// The scope collection has no <see cref="EncosyTower.PageFlows.PubSub.PageFlowScope"/> property.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>InvalidDefinitions</c></term>
    /// <description>
    /// The flow definitions of the codex do not match the scope properties one to one; the case holds a
    /// <see cref="PageFlowCodexValidation"/>.
    /// </description>
    /// </item>
    /// <item>
    /// <term><c>FlowCreationFailed</c></term>
    /// <description>The codex could not create the flow of a definition.</description>
    /// </item>
    /// <item>
    /// <term><c>ScopeNotFound</c></term>
    /// <description>No created flow has the identifier of a scope property.</description>
    /// </item>
    /// <item>
    /// <term><c>ScopeNotAssigned</c></term>
    /// <description>The scope collection did not accept the scope of a property.</description>
    /// </item>
    /// <item>
    /// <term><c>InitializerFailed</c></term>
    /// <description>The initializer threw an exception other than a cancellation.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumFactoryFor(typeof(Error))]
    public readonly partial struct PageFlowCodexError
    {
        internal readonly Error _error;

        private PageFlowCodexError(in Error error)
        {
            _error = error;
        }

        /// <summary>Gets a message that describes the failure.</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToMessage()
            => _error.ToMessage();

        /// <inheritdoc cref="ToMessage"/>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override string ToString()
            => ToMessage();

        /// <summary>Logs the failure as an error.</summary>
        /// <param name="logger">The logger to log with.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="logger"/> is <c>null</c>.</exception>
        public void Log([NotNull] ILogger logger)
        {
            DebuggingThrowHelper.ThrowIfNull(logger);
            _error.Log(logger);
        }

        [PolyEnumStruct]
        internal readonly partial struct Error
        {
            partial interface IEnumCase
            {
                string ToMessage();

                void Log(ILogger logger)
                    => logger.LogError(ToMessage());
            }

            /// <summary>The default value of <see cref="PageFlowCodexError"/>; the codex never returns it.</summary>
            public readonly partial struct Undefined
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "An unknown page codex error occurred.";
            }

            /// <summary>The GameObject of the codex has no component that implements its initializer.</summary>
            public readonly partial struct InitializerMissing
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "Cannot initialize the page codex without an initializer. "
                        + "Add a component that implements IUguiPageCodexOnInitialize or IUitkPageCodexOnInitialize "
                        + "to the GameObject of the codex.";
            }

            /// <summary>The codex initializer returned a <c>null</c> scope collection applier.</summary>
            public readonly partial struct ScopeCollectionApplierMissing
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "The codex initializer returned a null IPageFlowScopeCollectionApplier.";
            }

            /// <summary>The scope collection applier returned a <c>null</c> collection type.</summary>
            public readonly partial struct ScopeCollectionTypeMissing
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => "The IPageFlowScopeCollectionApplier returned a null flow scope collection type.";
            }

            /// <summary>The scope collection type has no <c>PageFlowScope</c> property.</summary>
            /// <param name="CollectionType">The scope collection type.</param>
            public readonly partial record struct NoScopeProperties(System.Type CollectionType)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The type '{CollectionType}' has no valid PageFlowScope property. "
                        + "Declare it as a partial struct with [PageFlowScopeCollection] "
                        + "and properties of type PageFlowScope with a private setter.";
            }

            /// <summary>The flow definitions of the codex do not match the scope properties one to one.</summary>
            /// <param name="Validation">What the definitions and the scope properties do not share.</param>
            public readonly partial record struct InvalidDefinitions(PageFlowCodexValidation Validation)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                {
                    using var _ = StringBuilderPool.Rent(out var sb);

                    sb.Append("The flow definitions do not match the properties of the scope collection.");

                    if (Validation.FirstEmptyIndex >= 0)
                    {
                        sb.Append(
                              $" The identifier of the definition at index {Validation.FirstEmptyIndex} "
                                  + "is null or empty."
                        );
                    }

                    if (Validation.DuplicateIdentifiers is { Length: > 0 } duplicates)
                    {
                        sb.Append($" Duplicate identifiers: {string.Join(", ", duplicates)}.");
                    }

                    if (Validation.ScopesWithoutDefinition is { Length: > 0 } scopes)
                    {
                        sb.Append($" Properties without a definition: {string.Join(", ", scopes)}.");
                    }

                    if (Validation.DefinitionsWithoutScope is { Length: > 0 } definitions)
                    {
                        sb.Append($" Definitions without a property: {string.Join(", ", definitions)}.");
                    }

                    return sb.ToString();
                }
            }

            /// <summary>
            /// The codex could not create the flow of a definition: its kind is unknown, its identifier repeats, or
            /// the flow could not be created.
            /// </summary>
            /// <param name="Index">The index of the definition.</param>
            /// <param name="Identifier">The identifier of the definition.</param>
            public readonly partial record struct FlowCreationFailed(int Index, string Identifier)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Cannot create the flow '{Identifier}' for the definition at index {Index}.";
            }

            /// <summary>No created flow has the identifier of a scope property.</summary>
            /// <param name="Identifier">The identifier of the scope property.</param>
            public readonly partial record struct ScopeNotFound(string Identifier)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"Cannot find a flow scope for the property '{Identifier}' of the scope collection.";
            }

            /// <summary>The scope collection did not accept the scope of a property.</summary>
            /// <param name="Identifier">The identifier of the scope property.</param>
            public readonly partial record struct ScopeNotAssigned(string Identifier)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The scope collection did not accept a flow scope for the property '{Identifier}'.";
            }

            /// <summary>The codex initializer threw an exception other than a cancellation.</summary>
            /// <param name="Ex">The exception.</param>
            public readonly partial record struct InitializerFailed(System.Exception Ex)
            {
                [MethodImpl(MethodImplOptions.NoInlining)]
                public string ToMessage()
                    => $"The codex initializer threw an exception: {Ex.Message}";

                public void Log(ILogger logger)
                    => logger.LogException(new System.InvalidOperationException(ToMessage(), Ex));
            }
        }
    }
}
