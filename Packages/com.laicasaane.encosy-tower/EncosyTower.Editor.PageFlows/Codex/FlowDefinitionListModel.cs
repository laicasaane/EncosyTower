#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.PageFlows.Codex;
using UnityEditor;

namespace EncosyTower.Editor.PageFlows
{
    /// <summary>
    /// Holds the identifiers of the flow definitions of a codex and checks them against its scope collection.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A model is a snapshot: <see cref="FlowDefinitionsSection"/> builds a new one with <see cref="Create"/> on every
    /// change of the list. The constructor validates the identifiers with
    /// <see cref="EncosyTower.PageFlows.Codex.PageFlowCodexValidator.Validate"/>.
    /// </para>
    /// <para>
    /// While the scope collection is not found, the model reports no errors, offers no Sync and has no scopes to
    /// compare with. Identifiers are compared ordinally. A row index is the 0-based index in the serialized array.
    /// </para>
    /// </remarks>
    /// <seealso cref="FlowDefinitionError"/>
    /// <seealso cref="FlowDefinitionsError"/>
    /// <seealso cref="PageFlowScopeCollectionInfo"/>
    internal sealed class FlowDefinitionListModel
    {
        /// <summary>
        /// The serialized name of the identifier field of a flow definition.
        /// </summary>
        /// <remarks>
        /// Must equal the name of that field in the runtime flow definition of the codex.
        /// </remarks>
        public const string IDENTIFIER = "identifier";

        private readonly string[] _identifiers;
        private readonly PageFlowScopeCollectionInfo _info;
        private readonly HashSet<string> _scopeSet;
        private readonly PageFlowCodexValidation _validation;

        /// <summary>
        /// Creates a model and validates the identifiers.
        /// </summary>
        /// <param name="identifiers">The identifiers in list order. <see langword="null"/> is treated as empty.</param>
        /// <param name="info">The result of the scope collection lookup.</param>
        /// <remarks>
        /// The model keeps the <paramref name="identifiers"/> array without copying it.
        /// </remarks>
        public FlowDefinitionListModel(string[] identifiers, in PageFlowScopeCollectionInfo info)
        {
            _identifiers = identifiers ?? Array.Empty<string>();
            _info = info;
            _scopeSet = new HashSet<string>(info.ScopeIdentifiers ?? Array.Empty<string>(), StringComparer.Ordinal);
            _validation = PageFlowCodexValidator.Validate(_identifiers, _info.ScopeIdentifiers);
        }

        /// <summary>
        /// Gets whether <see cref="Sync"/> would change the list.
        /// </summary>
        /// <value>
        /// <see langword="false"/> while the scope collection is not found. Otherwise <see langword="true"/> when a
        /// scope has no flow definition, or when a row has an identifier error.
        /// </value>
        public bool CanSync
        {
            get
            {
                if (_info.IsValid == false)
                {
                    return false;
                }

                if (_validation.ScopesWithoutDefinition.Length > 0)
                {
                    return true;
                }

                for (var i = 0; i < _identifiers.Length; i++)
                {
                    if (TryGetIdentifierError(i, out _))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// Reads the identifiers from the serialized flow definitions and creates a model.
        /// </summary>
        /// <param name="flows">
        /// The serialized array of flow definitions. Each element must have a string field named
        /// <see cref="IDENTIFIER"/>.
        /// </param>
        /// <param name="info">The result of the scope collection lookup.</param>
        /// <returns>The model for the current content of <paramref name="flows"/>.</returns>
        /// <remarks>
        /// Only reads. The caller refreshes the <see cref="SerializedObject"/> first, so Undo, Redo and external edits
        /// are included.
        /// </remarks>
        public static FlowDefinitionListModel Create(SerializedProperty flows, in PageFlowScopeCollectionInfo info)
        {
            var count = flows.arraySize;
            var identifiers = new string[count];

            for (var i = 0; i < count; i++)
            {
                identifiers[i] = flows.GetArrayElementAtIndex(i).FindPropertyRelative(IDENTIFIER).stringValue;
            }

            return new FlowDefinitionListModel(identifiers, info);
        }

        /// <summary>
        /// Gets the identifier error of a row.
        /// </summary>
        /// <param name="definitionIndex">The 0-based index of the row.</param>
        /// <returns>
        /// An array with one error, or an empty array when the row has no identifier error, the scope collection is not
        /// found, or <paramref name="definitionIndex"/> is out of range.
        /// </returns>
        /// <remarks>
        /// The error is the first that applies of <see cref="FlowDefinitionError.EmptyIdentifier"/>,
        /// <see cref="FlowDefinitionError.UnknownIdentifier"/> and
        /// <see cref="FlowDefinitionError.DuplicateIdentifier"/>. Container errors are not produced here; the caller
        /// adds them.
        /// </remarks>
        public FlowDefinitionError[] GetErrors(int definitionIndex)
            => TryGetIdentifierError(definitionIndex, out var error)
                ? new[] { error }
                : Array.Empty<FlowDefinitionError>();

        /// <summary>
        /// Gets the list-level error: the scopes that have no flow definition.
        /// </summary>
        /// <returns>
        /// The error. <see cref="FlowDefinitionsError.MissingScopes"/> is empty when no scope is missing or the scope
        /// collection is not found.
        /// </returns>
        public FlowDefinitionsError GetDefinitionsError()
            => new(_info.IsValid ? _validation.ScopesWithoutDefinition : Array.Empty<string>());

        /// <summary>
        /// Gets the entries of the identifier dropdown of a row.
        /// </summary>
        /// <param name="definitionIndex">The 0-based index of the row. An index out of range counts as empty.</param>
        /// <returns>The entries in display order.</returns>
        /// <remarks>
        /// <para>
        /// When the current identifier is not empty and not a scope, it comes first, checked and disabled, with a
        /// separator after it.
        /// </para>
        /// <para>
        /// Then come all scopes in collection order. The current identifier is checked. A scope that another row
        /// already uses is disabled.
        /// </para>
        /// </remarks>
        public IdentifierChoice[] GetIdentifierChoices(int definitionIndex)
        {
            var scopes = _info.ScopeIdentifiers ?? Array.Empty<string>();

            var current = (uint)definitionIndex < (uint)_identifiers.Length
                ? _identifiers[definitionIndex]
                : string.Empty;

            var choices = new List<IdentifierChoice>(scopes.Length + 1);

            if (string.IsNullOrEmpty(current) == false && _scopeSet.Contains(current) == false)
            {
                choices.Add(new IdentifierChoice(
                      Value: current
                    , Checked: true
                    , Disabled: true
                    , Note: PageFlowsViewResources.Get().Flows.NoteNotAScope
                    , SeparatorAfter: true
                ));
            }

            for (var i = 0; i < scopes.Length; i++)
            {
                var scope = scopes[i];
                var isChecked = string.Equals(scope, current, StringComparison.Ordinal);
                var inUse = isChecked == false && IsUsedByOtherDefinition(scope, definitionIndex);

                choices.Add(new IdentifierChoice(
                      Value: scope
                    , Checked: isChecked
                    , Disabled: inUse
                    , Note: inUse ? PageFlowsViewResources.Get().Flows.NoteInUse : string.Empty
                    , SeparatorAfter: false
                ));
            }

            return choices.ToArray();
        }

        /// <summary>
        /// Edits the list so that it has exactly one flow definition for each scope.
        /// </summary>
        /// <param name="flows">The serialized array of flow definitions.</param>
        /// <param name="resetNewDefinition">
        /// Sets the fields other than the identifier of an added flow definition to their defaults. Can be
        /// <see langword="null"/>.
        /// </param>
        /// <remarks>
        /// <para>
        /// Does nothing while the scope collection is not found. The caller updates the
        /// <see cref="SerializedObject"/> first. The steps are:
        /// </para>
        /// <list type="number">
        /// <item><description>Keep the first row of each scope.</description></item>
        /// <item><description>
        /// Give each other row (unknown, empty or repeated identifier) the next missing scope in collection order. The
        /// other fields of the row are kept.
        /// </description></item>
        /// <item><description>
        /// Delete the rows that are still without a scope, from the last to the first.
        /// </description></item>
        /// <item><description>
        /// Add a row for each scope that is still missing, call <paramref name="resetNewDefinition"/> on it, then set
        /// its identifier.
        /// </description></item>
        /// <item><description>
        /// Apply the changes with one <c>ApplyModifiedProperties</c>, so Undo reverts all of them.
        /// </description></item>
        /// </list>
        /// </remarks>
        public void Sync(SerializedProperty flows, Action<SerializedProperty> resetNewDefinition)
        {
            if (_info.IsValid == false)
            {
                return;
            }

            var scopes = _info.ScopeIdentifiers;
            var count = flows.arraySize;
            var assigned = new HashSet<string>(StringComparer.Ordinal);
            var keep = new bool[count];

            for (var i = 0; i < count; i++)
            {
                var identifier = GetIdentifier(flows, i);

                if (_scopeSet.Contains(identifier) && assigned.Add(identifier))
                {
                    keep[i] = true;
                }
            }

            var missing = new Queue<string>();

            for (var i = 0; i < scopes.Length; i++)
            {
                if (assigned.Contains(scopes[i]) == false)
                {
                    missing.Enqueue(scopes[i]);
                }
            }

            for (var i = 0; i < count; i++)
            {
                if (keep[i] || missing.Count < 1)
                {
                    continue;
                }

                SetIdentifier(flows, i, missing.Dequeue());
                keep[i] = true;
            }

            for (var i = count - 1; i >= 0; i--)
            {
                if (keep[i] == false)
                {
                    flows.DeleteArrayElementAtIndex(i);
                }
            }

            while (missing.Count > 0)
            {
                var index = flows.arraySize;
                flows.arraySize = index + 1;

                var element = flows.GetArrayElementAtIndex(index);
                resetNewDefinition?.Invoke(element);
                element.FindPropertyRelative(IDENTIFIER).stringValue = missing.Dequeue();
            }

            flows.serializedObject.ApplyModifiedProperties();
        }

        private bool TryGetIdentifierError(int definitionIndex, out FlowDefinitionError error)
        {
            error = default;

            if (_info.IsValid == false || (uint)definitionIndex >= (uint)_identifiers.Length)
            {
                return false;
            }

            var identifier = _identifiers[definitionIndex];

            if (string.IsNullOrEmpty(identifier))
            {
                error = new FlowDefinitionError.EmptyIdentifier();
                return true;
            }

            if (_scopeSet.Contains(identifier) == false)
            {
                error = new FlowDefinitionError.UnknownIdentifier(identifier, _info.TypeName, _info.ScopeIdentifiers);
                return true;
            }

            var others = GetOtherDefinitionIndices(identifier, definitionIndex);

            if (others.Length > 0)
            {
                error = new FlowDefinitionError.DuplicateIdentifier(identifier, others);
                return true;
            }

            return false;
        }

        private int[] GetOtherDefinitionIndices(string identifier, int definitionIndex)
        {
            var indices = new List<int>();

            for (var i = 0; i < _identifiers.Length; i++)
            {
                if (i != definitionIndex && string.Equals(_identifiers[i], identifier, StringComparison.Ordinal))
                {
                    indices.Add(i);
                }
            }

            return indices.ToArray();
        }

        private bool IsUsedByOtherDefinition(string identifier, int definitionIndex)
        {
            for (var i = 0; i < _identifiers.Length; i++)
            {
                if (i != definitionIndex && string.Equals(_identifiers[i], identifier, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetIdentifier(SerializedProperty flows, int index)
            => flows.GetArrayElementAtIndex(index).FindPropertyRelative(IDENTIFIER).stringValue;

        private static void SetIdentifier(SerializedProperty flows, int index, string identifier)
            => flows.GetArrayElementAtIndex(index).FindPropertyRelative(IDENTIFIER).stringValue = identifier;
    }
}

#endif
