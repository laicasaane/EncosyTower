#if UNITY_EDITOR

using System.Runtime.CompilerServices;
using EncosyTower.PolyEnumStructs;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Describes one problem found on a flow definition row, with the texts that explain it and the fix.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="FlowDefinitionListModel.GetErrors"/> produces the identifier cases.
    /// <see cref="EncosyTower.Editor.PageFlows.UitkPages.LayoutAssetContainerModel.TryGetError"/> produces the
    /// container cases, which only the UI Toolkit codex inspector uses. <see cref="FlowDefinitionsSection"/> and
    /// <see cref="TooltipAPI"/> show them. Nothing logs or throws a problem, and a problem never blocks saving.
    /// </para>
    /// <para>
    /// A problem has two texts: <see cref="IEnumCase.ToProblem"/> says what is wrong and
    /// <see cref="IEnumCase.ToFix"/> says how to resolve it. <see cref="TooltipAPI"/> joins them as
    /// <c>"problem\nFix: fix"</c>. <see cref="IEnumCase.IsIdentifierProblem"/> decides the column the row marks as
    /// invalid; the status icon at the end of the row always shows every problem of the row. The default value of this
    /// type is <see cref="Undefined"/>.
    /// </para>
    /// <list type="table">
    /// <listheader>
    /// <term>Case</term>
    /// <description>Column and occurrence</description>
    /// </listheader>
    /// <item>
    /// <term><see cref="Undefined"/></term>
    /// <description>None. No problem.</description>
    /// </item>
    /// <item>
    /// <term><see cref="EmptyIdentifier"/></term>
    /// <description>Identifier. The identifier is empty.</description>
    /// </item>
    /// <item>
    /// <term><see cref="UnknownIdentifier"/></term>
    /// <description>Identifier. The identifier is not a scope of the scope collection.</description>
    /// </item>
    /// <item>
    /// <term><see cref="DuplicateIdentifier"/></term>
    /// <description>Identifier. Another row has the same identifier.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ContainerNotInLayout"/></term>
    /// <description>Container Name. No element of the layout asset has the container name.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ContainerMatchesMany"/></term>
    /// <description>Container Name. More than one element of the layout asset has the container name.</description>
    /// </item>
    /// <item>
    /// <term><see cref="ContainerWithoutLayout"/></term>
    /// <description>Container Name. A container name is set but no layout asset is available.</description>
    /// </item>
    /// </list>
    /// </remarks>
    /// <seealso cref="FlowDefinitionsError"/>
    [PolyEnumStruct]
    internal readonly partial struct FlowDefinitionError
    {
        /// <summary>
        /// The members that every case of <see cref="FlowDefinitionError"/> provides.
        /// </summary>
        partial interface IEnumCase
        {
            /// <summary>
            /// Gets whether the problem belongs to the Identifier column instead of the Container Name column.
            /// </summary>
            /// <value>
            /// <see langword="false"/> unless the case overrides it. The identifier cases return
            /// <see langword="true"/>.
            /// </value>
            bool IsIdentifierProblem => false;

            /// <summary>
            /// Gets the text that says what is wrong.
            /// </summary>
            /// <returns>The localized sentence, built from the texts of the editor module.</returns>
            string ToProblem();

            /// <summary>
            /// Gets the text that says how to resolve the problem.
            /// </summary>
            /// <returns>The localized sentence, built from the texts of the editor module.</returns>
            string ToFix();
        }

        /// <summary>
        /// The row has no problem.
        /// </summary>
        /// <remarks>
        /// This is the default value of <see cref="FlowDefinitionError"/>. A try method that finds no problem
        /// returns it in its out parameter. It is never shown, and there is nothing to fix.
        /// </remarks>
        public readonly partial struct Undefined
        {
            /// <inheritdoc/>
            /// <remarks>
            /// Returns an empty string.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => string.Empty;

            /// <inheritdoc/>
            /// <remarks>
            /// Returns an empty string.
            /// </remarks>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => string.Empty;
        }

        /// <summary>
        /// The identifier of the flow definition is empty.
        /// </summary>
        /// <remarks>
        /// Occurs only while the scope collection is found. The codex cannot match the flow definition to a scope.
        /// Fix: choose a scope in the Identifier dropdown, or press Sync Scopes.
        /// </remarks>
        public readonly partial struct EmptyIdentifier
        {
            /// <inheritdoc/>
            public readonly bool IsIdentifierProblem => true;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => PageFlowsViewResources.Get().Problems.EmptyIdentifier;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => PageFlowsViewResources.Get().Problems.EmptyIdentifierFix;
        }

        /// <summary>
        /// The identifier of the flow definition is not a scope of the scope collection.
        /// </summary>
        /// <param name="Identifier">The unknown identifier.</param>
        /// <param name="ScopeCollectionType">The short name of the scope collection type.</param>
        /// <param name="KnownScopes">The scopes of the scope collection. Must not be <see langword="null"/>.</param>
        /// <remarks>
        /// Names are compared ordinally. Fix: choose one of the known scopes, add a
        /// <see cref="EncosyTower.PageFlows.PubSub.PageFlowScope"/> property with that name to the scope collection
        /// type, or press Sync Scopes.
        /// </remarks>
        public readonly partial record struct UnknownIdentifier(
              string Identifier
            , string ScopeCollectionType
            , string[] KnownScopes
        )
        {
            /// <inheritdoc/>
            public readonly bool IsIdentifierProblem => true;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => PageFlowsViewResources.Get().Problems.UnknownIdentifier(Identifier, ScopeCollectionType);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => PageFlowsViewResources.Get().Problems.UnknownIdentifierFix(
                      string.Join(", ", KnownScopes)
                    , Identifier
                    , ScopeCollectionType
                );
        }

        /// <summary>
        /// The identifier of the flow definition is a scope, but another flow definition uses it too.
        /// </summary>
        /// <param name="Identifier">The repeated identifier.</param>
        /// <param name="OtherDefinitionIndices">
        /// The 0-based indices of the other rows with the same identifier. The problem text shows them 1-based.
        /// </param>
        /// <remarks>
        /// Every row of the group has this problem. Fix: choose another scope for one of the flow definitions, or press
        /// Sync Scopes, which keeps the first flow definition of the scope.
        /// </remarks>
        public readonly partial record struct DuplicateIdentifier(string Identifier, int[] OtherDefinitionIndices)
        {
            /// <inheritdoc/>
            public readonly bool IsIdentifierProblem => true;

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => PageFlowsViewResources.Get().Problems.DuplicateIdentifier(
                      Identifier
                    , JoinDefinitionNumbers(OtherDefinitionIndices)
                );

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => PageFlowsViewResources.Get().Problems.DuplicateIdentifierFix;

            private static string JoinDefinitionNumbers(int[] definitionIndices)
            {
                var numbers = new string[definitionIndices.Length];

                for (var i = 0; i < definitionIndices.Length; i++)
                {
                    numbers[i] = (definitionIndices[i] + 1).ToString();
                }

                return string.Join(", ", numbers);
            }
        }

        /// <summary>
        /// The container name of the flow definition is not the name of an element of the layout asset.
        /// </summary>
        /// <param name="Container">The container name.</param>
        /// <param name="LayoutAsset">The name of the layout asset.</param>
        /// <remarks>
        /// UI Toolkit codex only. Occurs when a layout asset is available and no element of it has the name. Unless
        /// code creates the element before the codex initializes, the flow goes under the codex root. Fix: pick an
        /// element with the picker, or name an element of the layout asset with the container name.
        /// </remarks>
        public readonly partial record struct ContainerNotInLayout(string Container, string LayoutAsset)
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => PageFlowsViewResources.Get().Problems.ContainerNotInLayout(Container, LayoutAsset);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => PageFlowsViewResources.Get().Problems.ContainerNotInLayoutFix(LayoutAsset, Container);
        }

        /// <summary>
        /// More than one element of the layout asset has the container name of the flow definition.
        /// </summary>
        /// <param name="Container">The container name.</param>
        /// <param name="MatchCount">The number of elements with that name.</param>
        /// <param name="LayoutAsset">The name of the layout asset.</param>
        /// <param name="FirstPath">The path of the first matching element.</param>
        /// <remarks>
        /// UI Toolkit codex only. The first matching element in document order is used. Fix: give the intended
        /// element a unique name in the layout asset, then pick it.
        /// </remarks>
        public readonly partial record struct ContainerMatchesMany(
              string Container
            , int MatchCount
            , string LayoutAsset
            , string FirstPath
        )
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => PageFlowsViewResources.Get().Problems.ContainerMatchesMany(
                      Container
                    , MatchCount
                    , LayoutAsset
                    , FirstPath
                );

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => PageFlowsViewResources.Get().Problems.ContainerMatchesManyFix(LayoutAsset);
        }

        /// <summary>
        /// The flow definition has a container name, but no layout asset is available.
        /// </summary>
        /// <param name="Container">The container name.</param>
        /// <remarks>
        /// UI Toolkit codex only. The container must be created by code before the codex initializes, or the flow goes
        /// under the codex root. Fix: assign a layout asset that has an element with that name, or clear the
        /// container name to use the codex root.
        /// </remarks>
        public readonly partial record struct ContainerWithoutLayout(string Container)
        {
            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToProblem()
                => PageFlowsViewResources.Get().Problems.ContainerWithoutLayout(Container);

            /// <inheritdoc/>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public readonly string ToFix()
                => PageFlowsViewResources.Get().Problems.ContainerWithoutLayoutFix(Container);
        }
    }

    /// <summary>
    /// Describes the list-level problem of the flow definitions: scopes that have no flow definition.
    /// </summary>
    /// <param name="MissingScopes">The scopes of the scope collection without a flow definition.</param>
    /// <remarks>
    /// Not a case of <see cref="FlowDefinitionError"/>. The section header shows it on a warning icon. The codex
    /// cannot create the flow of a missing scope. Fix: press Sync Scopes, which adds a flow definition for each missing
    /// scope. Call <see cref="ToProblem"/> and <see cref="ToFix"/> only when <see cref="HasProblem"/> is
    /// <see langword="true"/>.
    /// </remarks>
    internal readonly record struct FlowDefinitionsError(string[] MissingScopes)
    {
        /// <summary>
        /// Gets whether at least one scope has no flow definition.
        /// </summary>
        public bool HasProblem => MissingScopes != null && MissingScopes.Length > 0;

        /// <summary>
        /// Gets the text that says which scopes have no flow definition.
        /// </summary>
        /// <returns>The localized sentence. It is singular for one scope and plural for more.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToProblem()
            => PageFlowsViewResources.Get().Problems.MissingScopes(
                  string.Join(", ", MissingScopes)
                , MissingScopes.Length
            );

        /// <summary>
        /// Gets the text that says how to resolve the problem.
        /// </summary>
        /// <returns>The localized sentence.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public string ToFix()
            => PageFlowsViewResources.Get().Problems.MissingScopesFix;
    }

    /// <summary>
    /// Describes one entry of the identifier dropdown of a flow definition.
    /// </summary>
    /// <param name="Value">The identifier that the entry writes when picked.</param>
    /// <param name="Checked">Whether the entry is the current identifier.</param>
    /// <param name="Disabled">Whether the entry is shown but cannot be picked.</param>
    /// <param name="Note">The text shown in parentheses after the value. Empty for no note.</param>
    /// <param name="SeparatorAfter">Whether a separator follows the entry.</param>
    /// <remarks>
    /// <see cref="FlowDefinitionListModel.GetIdentifierChoices"/> produces the entries.
    /// <see cref="FlowDefinitionsSection"/> shows them.
    /// </remarks>
    internal readonly record struct IdentifierChoice(
          string Value
        , bool Checked
        , bool Disabled
        , string Note
        , bool SeparatorAfter
    );
}

#endif
