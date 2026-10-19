#if UNITY_EDITOR

using System;
using EncosyTower.PageFlows;
using EncosyTower.UnityExtensions;
using UnityEngine;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Finds the scope collection of a codex from the initializer component on its GameObject.
    /// </summary>
    /// <remarks>
    /// Uses reflection and caches nothing. The codex inspectors call it once when they create the inspector, so a
    /// change of the initializer component shows only after the inspector is created again. It never calls the
    /// <c>OnInitializeAsync</c> of the initializer, but it creates the scope collection and can run the
    /// <c>PageFlowScopeCollectionApplier</c> getter of the initializer.
    /// </remarks>
    internal static class PageFlowScopeCollectionResolver
    {
        private const string APPLIER_PROPERTY = "PageFlowScopeCollectionApplier";

        /// <summary>
        /// Looks up the scope collection of a codex.
        /// </summary>
        /// <param name="codex">The codex component. Its GameObject is searched.</param>
        /// <param name="initializerInterface">The interface that the initializer component implements.</param>
        /// <param name="initializerBaseDefinition">
        /// The open generic base type of the initializers. Its first type argument is the scope collection type.
        /// </param>
        /// <returns>
        /// <see cref="PageFlowScopeCollectionInfo.Found"/> with the scopes, or the case that says why they could not be
        /// read: <see cref="PageFlowScopeCollectionInfo.NoInitializer"/>,
        /// <see cref="PageFlowScopeCollectionInfo.NullApplier"/> or
        /// <see cref="PageFlowScopeCollectionInfo.ScopesUnreadable"/>.
        /// </returns>
        /// <remarks>
        /// <para>The lookup takes these steps:</para>
        /// <list type="number">
        /// <item><description>
        /// Find the first valid component that implements <paramref name="initializerInterface"/>. Without one, the
        /// result is <see cref="PageFlowScopeCollectionInfo.NoInitializer"/>.
        /// </description></item>
        /// <item><description>
        /// When the component derives from <paramref name="initializerBaseDefinition"/>, create the scope collection
        /// of its first type argument and read its scopes.
        /// </description></item>
        /// <item><description>
        /// Otherwise read the <c>PageFlowScopeCollectionApplier</c> property of the component by reflection. An
        /// unusable value gives <see cref="PageFlowScopeCollectionInfo.NullApplier"/>.
        /// </description></item>
        /// </list>
        /// <para>
        /// An exception from step 2 or 3, for example from a getter of the user, is caught. The result is
        /// <see cref="PageFlowScopeCollectionInfo.ScopesUnreadable"/> with the innermost exception.
        /// </para>
        /// </remarks>
        public static PageFlowScopeCollectionInfo Resolve(
              Component codex
            , Type initializerInterface
            , Type initializerBaseDefinition
        )
        {
            var initializer = FindInitializer(codex, initializerInterface);

            if (initializer == null)
            {
                return new PageFlowScopeCollectionInfo.NoInitializer(initializerInterface.Name);
            }

            try
            {
                return ReadScopes(initializer, initializerInterface, initializerBaseDefinition);
            }
            catch (Exception exception)
            {
                var cause = exception.GetBaseException();

                return new PageFlowScopeCollectionInfo.ScopesUnreadable(
                      initializer.GetType().Name
                    , cause.GetType().Name
                    , cause.Message
                );
            }
        }

        private static PageFlowScopeCollectionInfo ReadScopes(
              Component initializer
            , Type initializerInterface
            , Type initializerBaseDefinition
        )
        {
            var baseType = FindClosedBase(initializer.GetType(), initializerBaseDefinition);

            if (baseType != null)
            {
                var scopesType = baseType.GetGenericArguments()[0];
                var scopes = (IPageFlowScopeCollection)Activator.CreateInstance(scopesType);
                return new PageFlowScopeCollectionInfo.Found(scopesType.Name, scopes.ScopeIdentifiers.ToArray());
            }

            var property = initializerInterface.GetProperty(APPLIER_PROPERTY);

            if (property?.GetValue(initializer) is not IPageFlowScopeCollectionApplier applier
                || applier.CollectionType == null
            )
            {
                return new PageFlowScopeCollectionInfo.NullApplier(initializerInterface.Name);
            }

            return new PageFlowScopeCollectionInfo.Found(
                  applier.CollectionType.Name
                , applier.ScopeIdentifiers.ToArray()
            );
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
