#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.PageFlows;
using UnityEditor;

namespace EncosyTower.Editor.PageFlows
{
    internal sealed class FlowDefinitionListModel
    {
        public const string IDENTIFIER = "identifier";


        private readonly string[] _identifiers;
        private readonly PageFlowScopeCollectionInfo _info;
        private readonly HashSet<string> _scopeSet;
        private readonly PageFlowCodexValidation _validation;

        public FlowDefinitionListModel(string[] identifiers, in PageFlowScopeCollectionInfo info)
        {
            _identifiers = identifiers ?? Array.Empty<string>();
            _info = info;
            _scopeSet = new HashSet<string>(info.ScopeIdentifiers ?? Array.Empty<string>(), StringComparer.Ordinal);
            _validation = PageFlowCodexValidator.Validate(_identifiers, _info.ScopeIdentifiers);
        }

        public int Count => _identifiers.Length;

        public bool HasScopeCollection => _info.IsValid;

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
                    if (TryGetIdentifierProblem(i, out _))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

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

        public RowProblem[] GetRowProblems(int row)
            => TryGetIdentifierProblem(row, out var problem) ? new[] { problem } : Array.Empty<RowProblem>();

        public ListProblem GetListProblem()
            => new(_info.IsValid ? _validation.ScopesWithoutDefinition : Array.Empty<string>());

        public IdentifierChoice[] GetIdentifierChoices(int row)
        {
            var scopes = _info.ScopeIdentifiers ?? Array.Empty<string>();
            var current = (uint)row < (uint)_identifiers.Length ? _identifiers[row] : string.Empty;
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
                var inUse = isChecked == false && IsUsedByOtherRow(scope, row);

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

        public void Sync(SerializedProperty flows, Action<SerializedProperty> resetNewRow)
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
                resetNewRow?.Invoke(element);
                element.FindPropertyRelative(IDENTIFIER).stringValue = missing.Dequeue();
            }

            flows.serializedObject.ApplyModifiedProperties();
        }

        private bool TryGetIdentifierProblem(int row, out RowProblem problem)
        {
            problem = default;

            if (_info.IsValid == false || (uint)row >= (uint)_identifiers.Length)
            {
                return false;
            }

            var identifier = _identifiers[row];

            if (string.IsNullOrEmpty(identifier))
            {
                problem = CreateProblem(RowProblemKind.EmptyIdentifier, string.Empty, Array.Empty<int>());
                return true;
            }

            if (_scopeSet.Contains(identifier) == false)
            {
                problem = CreateProblem(RowProblemKind.UnknownIdentifier, identifier, Array.Empty<int>());
                return true;
            }

            var others = GetOtherRows(identifier, row);

            if (others.Length > 0)
            {
                problem = CreateProblem(RowProblemKind.DuplicateIdentifier, identifier, others);
                return true;
            }

            return false;
        }

        private RowProblem CreateProblem(RowProblemKind kind, string value, int[] otherRows)
            => new(kind, value, _info.TypeName, _info.ScopeIdentifiers, otherRows);

        private int[] GetOtherRows(string identifier, int row)
        {
            var rows = new List<int>();

            for (var i = 0; i < _identifiers.Length; i++)
            {
                if (i != row && string.Equals(_identifiers[i], identifier, StringComparison.Ordinal))
                {
                    rows.Add(i);
                }
            }

            return rows.ToArray();
        }

        private bool IsUsedByOtherRow(string identifier, int row)
        {
            for (var i = 0; i < _identifiers.Length; i++)
            {
                if (i != row && string.Equals(_identifiers[i], identifier, StringComparison.Ordinal))
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
