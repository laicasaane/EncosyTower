using System.Collections.Generic;

namespace EncosyTower.PageFlows.Internals
{
    /// <summary>Remembers which pages of a list an add, a remove or a transition is working on.</summary>
    /// <remarks>
    /// A list checks it to fail a call that would collide with another one: a duplicate add, a remove of a page that is
    /// being removed, or a remove of a page that is in a transition.
    /// </remarks>
    /// <typeparam name="TPage">The type of the pages.</typeparam>
    internal sealed class PageListCallTracker<TPage>
        where TPage : class
    {
        private readonly List<TPage> _adding = new();
        private readonly List<TPage> _removing = new();
        private readonly List<TPage> _inTransition = new();

        public bool IsAdding(TPage page)
            => Contains(_adding, page);

        public bool IsRemoving(TPage page)
            => Contains(_removing, page);

        public bool IsInTransition(TPage page)
            => Contains(_inTransition, page);

        public void BeginAdd(TPage page)
        {
            _adding.Add(page);
        }

        public void EndAdd(TPage page)
        {
            Remove(_adding, page);
        }

        public void BeginRemove(TPage page)
        {
            _removing.Add(page);
        }

        public void EndRemove(TPage page)
        {
            Remove(_removing, page);
        }

        public void BeginTransition(TPage pageToHide, TPage pageToShow)
        {
            if (pageToHide != null)
            {
                _inTransition.Add(pageToHide);
            }

            if (pageToShow != null)
            {
                _inTransition.Add(pageToShow);
            }
        }

        public void BeginTransition(TPage[] pages, int count)
        {
            for (var i = 0; i < count; i++)
            {
                _inTransition.Add(pages[i]);
            }
        }

        public void EndTransition(TPage pageToHide, TPage pageToShow)
        {
            if (pageToHide != null)
            {
                Remove(_inTransition, pageToHide);
            }

            if (pageToShow != null)
            {
                Remove(_inTransition, pageToShow);
            }
        }

        public void EndTransition(TPage[] pages, int count)
        {
            for (var i = 0; i < count; i++)
            {
                Remove(_inTransition, pages[i]);
            }
        }

        public void Clear()
        {
            _adding.Clear();
            _removing.Clear();
            _inTransition.Clear();
        }

        private static bool Contains(List<TPage> pages, TPage page)
        {
            var count = pages.Count;

            for (var i = 0; i < count; i++)
            {
                if (ReferenceEquals(pages[i], page))
                {
                    return true;
                }
            }

            return false;
        }

        private static void Remove(List<TPage> pages, TPage page)
        {
            var count = pages.Count;

            for (var i = 0; i < count; i++)
            {
                if (ReferenceEquals(pages[i], page))
                {
                    pages.RemoveAt(i);
                    return;
                }
            }
        }
    }
}
