using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EncosyTower.Collections;
using EncosyTower.PageFlows.History;

namespace EncosyTower.PageFlows.Internals
{
    /// <summary>The entries and the current index that a navigation history keeps.</summary>
    /// <remarks>
    /// The record keeps at most <see cref="MaxEntries"/> entries and drops the oldest first. <see cref="Version"/>
    /// changes whenever the entries or the current index change; a history compares it before and after a replay to
    /// see whether the record changed meanwhile.
    /// </remarks>
    internal sealed class PageHistoryRecord
    {
        public const int DEFAULT_MAX_ENTRIES = 100;

        private readonly ListFast<PageHistoryEntry> _entries = new();
        private int _maxEntries = DEFAULT_MAX_ENTRIES;

        public ListFast<PageHistoryEntry>.ReadOnly Entries => _entries.AsReadOnly();

        public int CurrentIndex { get; private set; } = -1;

        public int Version { get; private set; }

        public int MaxEntries
        {
            get => _maxEntries;

            set
            {
                _maxEntries = Math.Max(1, value);
                Trim();
            }
        }

        /// <summary>Applies the change that a recording strategy returned.</summary>
        /// <param name="change">The change to apply.</param>
        /// <param name="shouldApplyBackAndForwardChanges">
        /// Whether <c>Navigate</c> drops the entries after the current index and <c>GoBack</c> moves the index. A log
        /// passes <c>false</c>: it only appends.
        /// </param>
        public void Apply(in PageHistoryChange change, bool shouldApplyBackAndForwardChanges)
        {
            if (change.TryGetValue(out PageHistoryChange.Navigate navigate))
            {
                if (shouldApplyBackAndForwardChanges)
                {
                    var start = CurrentIndex + 1;

                    if (start < _entries.Count)
                    {
                        _entries.RemoveRange(start, _entries.Count - start);
                    }
                }

                AddAsCurrent(navigate.Entry);
            }
            else if (change.TryGetValue(out PageHistoryChange.Append append))
            {
                AddAsCurrent(append.Entry);
            }
            else if (shouldApplyBackAndForwardChanges
                && change.TryGetValue(out PageHistoryChange.GoBack goBack)
                && goBack.Steps > 0
            )
            {
                MoveBack(goBack.Steps);
            }

            Trim();
        }

        public void Trim()
        {
            var excess = _entries.Count - _maxEntries;

            if (excess < 1)
            {
                return;
            }

            _entries.RemoveRange(0, excess);
            CurrentIndex = Math.Max(CurrentIndex - excess, -1);
            Version++;
        }

        public void MoveTo(int index)
        {
            if (index < -1 || index >= _entries.Count)
            {
                ThrowIndexOutOfRange();
            }

            CurrentIndex = index;
            Version++;
        }

        public void Clear()
        {
            _entries.Clear();
            CurrentIndex = -1;
            Version++;
        }

        private void AddAsCurrent(in PageHistoryEntry entry)
        {
            _entries.Add(entry);
            CurrentIndex = _entries.Count - 1;
            Version++;
        }

        private void MoveBack(int steps)
        {
            var index = Math.Max(CurrentIndex - steps, -1);

            if (index == CurrentIndex)
            {
                return;
            }

            CurrentIndex = index;
            Version++;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [UnityEngine.HideInCallstack, StackTraceHidden, DoesNotReturn]
        private static void ThrowIndexOutOfRange()
            => throw new ArgumentOutOfRangeException("index");
    }
}
