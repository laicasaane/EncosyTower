#if UNITY_EDITOR

using System;

namespace EncosyTower.Editor.PageFlows
{
    internal enum ScopeCollectionProblem
    {
        None,
        NoInitializer,
        NullApplier,
    }

    internal readonly record struct PageFlowScopeCollectionInfo(
          string TypeName
        , string[] ScopeIdentifiers
        , ScopeCollectionProblem Problem
        , string InitializerInterfaceName
    )
    {
        public bool IsValid => Problem == ScopeCollectionProblem.None;

        public static PageFlowScopeCollectionInfo Found(Type collectionType, string[] scopeIdentifiers)
            => new(collectionType.Name, scopeIdentifiers, ScopeCollectionProblem.None, string.Empty);

        public static PageFlowScopeCollectionInfo Failed(ScopeCollectionProblem problem, Type initializerInterface)
            => new(string.Empty, Array.Empty<string>(), problem, initializerInterface.Name);

        public string ToProblemText()
            => Problem switch {
                ScopeCollectionProblem.NoInitializer => "No initializer component found",
                ScopeCollectionProblem.NullApplier => "Initializer returns no applier",
                _ => string.Empty,
            };

        public string ToTooltip()
            => Problem switch {
                ScopeCollectionProblem.NoInitializer => $"No component on this GameObject implements "
                    + $"{InitializerInterfaceName}, so the codex has no scope collection: it cannot initialize, "
                    + $"and identifiers cannot be checked.\nFix: Add a component that implements "
                    + $"{InitializerInterfaceName} and returns a PageFlowScopeCollectionApplier<T> for a "
                    + "[PageFlowScopeCollection] struct.",
                ScopeCollectionProblem.NullApplier => $"The {InitializerInterfaceName} component returns a null "
                    + "PageFlowScopeCollectionApplier, so the codex has no scope collection.\nFix: Return a "
                    + "PageFlowScopeCollectionApplier<T> instance from its PageFlowScopeCollectionApplier property.",
                _ => string.Empty,
            };
    }
}

#endif
