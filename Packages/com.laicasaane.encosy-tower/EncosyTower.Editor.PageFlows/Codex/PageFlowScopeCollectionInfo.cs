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
        {
            var scope = PageFlowsViewResources.Get().Scope;

            return Problem switch {
                ScopeCollectionProblem.NoInitializer => scope.NoInitializer,
                ScopeCollectionProblem.NullApplier => scope.NullApplier,
                _ => string.Empty,
            };
        }

        public string ToTooltip()
        {
            var scope = PageFlowsViewResources.Get().Scope;

            return Problem switch {
                ScopeCollectionProblem.NoInitializer => scope.NoInitializerTooltip(InitializerInterfaceName),
                ScopeCollectionProblem.NullApplier => scope.NullApplierTooltip(InitializerInterfaceName),
                _ => string.Empty,
            };
        }
    }
}

#endif
