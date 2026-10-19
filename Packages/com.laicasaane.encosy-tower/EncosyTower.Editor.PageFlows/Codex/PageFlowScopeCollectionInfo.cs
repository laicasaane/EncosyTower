#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using EncosyTower.PolyEnumStructs;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Describes the result of looking up the scope collection of a codex.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="PageFlowScopeCollectionResolver.Resolve"/> produces it. <see cref="FlowDefinitionListModel"/>,
    /// <see cref="FlowDefinitionsSection"/> and the codex inspectors consume it. Only <see cref="Found"/> is valid.
    /// With any other case, the section does not check identifiers, Sync does nothing, and the identifier column is a
    /// text field. The default value of this type is a case without scopes, so it is not valid.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>Occurrence</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Found"/></term>
    /// <description>The scope collection was found. Not a problem.</description>
    /// </item>
    /// <item>
    /// <term><see cref="NoInitializer"/></term>
    /// <description>No component of the codex GameObject implements the initializer interface.</description>
    /// </item>
    /// <item>
    /// <term><see cref="NullApplier"/></term>
    /// <description>The initializer gives no usable scope collection applier.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ScopesUnreadable"/></term>
    /// <description>Reading the scope collection threw an exception.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [PolyEnumStruct]
    internal readonly partial struct PageFlowScopeCollectionInfo
    {
        /// <summary>
        /// The members that every case of <see cref="PageFlowScopeCollectionInfo"/> provides.
        /// </summary>
        /// <remarks>
        /// The default members describe a result without scopes.
        /// </remarks>
        partial interface IEnumCase
        {
            /// <summary>
            /// Gets whether the scope collection was found.
            /// </summary>
            bool IsValid => false;

            /// <summary>
            /// Gets the short name of the scope collection type, without its namespace.
            /// </summary>
            /// <value>An empty string when the scope collection was not found.</value>
            string TypeName => string.Empty;

            /// <summary>
            /// Gets the scope names in the order of the scope collection.
            /// </summary>
            /// <value>An empty array when the scope collection was not found.</value>
            string[] ScopeIdentifiers => Array.Empty<string>();

            /// <summary>
            /// Gets the short localized label that the Scope Collection row shows in place of the type name.
            /// </summary>
            /// <returns>The label. An empty string for <see cref="Found"/>.</returns>
            string ToProblemText()
                => string.Empty;

            /// <summary>
            /// Gets the localized explanation, with the fix, that the warning icon of the Scope Collection row shows.
            /// </summary>
            /// <returns>The tooltip. An empty string for <see cref="Found"/>.</returns>
            string ToTooltip()
                => string.Empty;
        }

        /// <summary>
        /// The scope collection was found.
        /// </summary>
        /// <param name="TypeName">The short name of the scope collection type.</param>
        /// <param name="ScopeIdentifiers">The scope names in the order of the scope collection.</param>
        /// <remarks>
        /// The lookup finds the type through the generic initializer base, or through the scope collection applier.
        /// This case is not a problem.
        /// </remarks>
        public readonly partial record struct Found(string TypeName, string[] ScopeIdentifiers)
        {
            /// <inheritdoc/>
            public readonly bool IsValid => true;
        }

        /// <summary>
        /// No component on the codex GameObject implements the initializer interface.
        /// </summary>
        /// <param name="InitializerInterfaceName">The short name of the initializer interface.</param>
        /// <remarks>
        /// The codex has no scope collection, so it cannot initialize and the identifiers cannot be checked. Fix: add a
        /// component that implements the initializer interface (<c>IUguiPageCodexOnInitialize</c> for the uGUI codex,
        /// <see cref="EncosyTower.PageFlows.UitkPages.Codex.IUitkPageCodexOnInitialize"/> for the UI Toolkit codex) and
        /// returns a <see cref="EncosyTower.PageFlows.PageFlowScopeCollectionApplier{TCollection}"/> for a struct
        /// marked with <see cref="EncosyTower.PageFlows.PageFlowScopeCollectionAttribute"/>.
        /// </remarks>
        public readonly partial record struct NoInitializer(string InitializerInterfaceName)
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblemText()
                => PageFlowsViewResources.Get().Scope.NoInitializer;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToTooltip()
                => PageFlowsViewResources.Get().Scope.NoInitializerTooltip(InitializerInterfaceName);
        }

        /// <summary>
        /// An initializer exists, but it gives no usable scope collection applier.
        /// </summary>
        /// <param name="InitializerInterfaceName">The short name of the initializer interface.</param>
        /// <remarks>
        /// Occurs when the initializer does not derive from the generic initializer base, and its
        /// <c>PageFlowScopeCollectionApplier</c> property returns <see langword="null"/>, a value that is not a
        /// <see cref="EncosyTower.PageFlows.IPageFlowScopeCollectionApplier"/>, or an applier without a collection
        /// type. Fix: return a <see cref="EncosyTower.PageFlows.PageFlowScopeCollectionApplier{TCollection}"/> instance
        /// from that property.
        /// </remarks>
        public readonly partial record struct NullApplier(string InitializerInterfaceName)
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblemText()
                => PageFlowsViewResources.Get().Scope.NullApplier;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToTooltip()
                => PageFlowsViewResources.Get().Scope.NullApplierTooltip(InitializerInterfaceName);
        }

        /// <summary>
        /// Reading the scope collection of the initializer threw an exception.
        /// </summary>
        /// <param name="InitializerTypeName">The short name of the type of the initializer component.</param>
        /// <param name="ExceptionTypeName">The short name of the type of the innermost exception.</param>
        /// <param name="ExceptionMessage">The message of the innermost exception.</param>
        /// <remarks>
        /// The inspector shows it instead of failing to draw. Fix: make the <c>PageFlowScopeCollectionApplier</c>
        /// property of the initializer, and the <c>ScopeIdentifiers</c> of its scope collection, return without
        /// throwing in Edit mode.
        /// </remarks>
        public readonly partial record struct ScopesUnreadable(
              string InitializerTypeName
            , string ExceptionTypeName
            , string ExceptionMessage
        )
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblemText()
                => PageFlowsViewResources.Get().Scope.ScopesUnreadable;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToTooltip()
                => PageFlowsViewResources.Get().Scope.ScopesUnreadableTooltip(
                      InitializerTypeName
                    , ExceptionTypeName
                    , ExceptionMessage
                );
        }
    }
}

#endif
