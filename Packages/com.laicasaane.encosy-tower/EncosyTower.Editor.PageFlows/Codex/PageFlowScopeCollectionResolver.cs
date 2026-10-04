#if UNITY_EDITOR

using System;
using EncosyTower.PageFlows;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows
{
    internal static class PageFlowScopeCollectionResolver
    {
        private const string APPLIER_PROPERTY = "PageFlowScopeCollectionApplier";

        public static PageFlowScopeCollectionInfo Resolve(
              Component codex
            , Type initializerInterface
            , Type initializerBaseDefinition
        )
        {
            var initializer = FindInitializer(codex, initializerInterface);

            if (initializer == null)
            {
                return PageFlowScopeCollectionInfo.Failed(ScopeCollectionProblem.NoInitializer, initializerInterface);
            }

            var baseType = FindClosedBase(initializer.GetType(), initializerBaseDefinition);

            if (baseType != null)
            {
                var scopesType = baseType.GetGenericArguments()[0];
                var scopes = (IPageFlowScopeCollection)Activator.CreateInstance(scopesType);
                return PageFlowScopeCollectionInfo.Found(scopesType, scopes.ScopeIdentifiers.ToArray());
            }

            var property = initializerInterface.GetProperty(APPLIER_PROPERTY);

            if (property?.GetValue(initializer) is not IPageFlowScopeCollectionApplier applier
                || applier.CollectionType == null
            )
            {
                return PageFlowScopeCollectionInfo.Failed(ScopeCollectionProblem.NullApplier, initializerInterface);
            }

            return PageFlowScopeCollectionInfo.Found(applier.CollectionType, applier.ScopeIdentifiers.ToArray());
        }

        private static Component FindInitializer(Component codex, Type initializerInterface)
        {
            var components = codex.GetComponents<Component>();

            for (var i = 0; i < components.Length; i++)
            {
                var component = components[i];

                if (component.IsValid() && initializerInterface.IsInstanceOfType(component))
                {
                    return component;
                }
            }

            return null;
        }

        private static Type FindClosedBase(Type type, Type genericDefinition)
        {
            for (var current = type; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition)
                {
                    return current;
                }
            }

            return null;
        }
    }
}

#endif
