using System.Collections.Generic;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using NUnit.Framework;

using DictionaryAPI = EncosyTower.Collections.Extensions.Unsafe.DictionaryReadOnlyExtensionsUnsafe;
using HashSetAPI = EncosyTower.Collections.Extensions.Unsafe.HashSetReadOnlyExtensionsUnsafe;
using ListFastAPI = EncosyTower.Collections.Extensions.Unsafe.ListFastExtensionsUnsafe;

namespace EncosyTower.Tests.Core.Collections.Extensions.Unsafe
{
    public partial class ManagedViewExtensionsUnsafeTests
    {
        [Test]
        public void ReadOnlyViews_ReturnOriginalManagedCollections()
        {
            var dictionary = new Dictionary<int, string> { [1] = "one" };
            var set = new HashSet<int> { 1 };
            var list = new List<int> { 1 };
            var dictionaryView = new DictionaryReadOnly<int, string>(dictionary);
            var setView = new HashSetReadOnly<int>(set);
            var listView = list.AsListFast().AsReadOnly();

            var returnedDictionary = DictionaryAPI.GetDictionaryUnsafe(dictionaryView);
            var returnedSet = HashSetAPI.GetHashSetUnsafe(setView);
            var returnedList = ListFastAPI.GetListUnsafe(listView);

            Assert.AreSame(dictionary, returnedDictionary);
            Assert.AreSame(set, returnedSet);
            Assert.AreSame(list, returnedList);
        }

        [Test]
        public void ListFastBuffer_AliasesListStorageAndReportsCount()
        {
            var list = new List<int> { 1, 2, 3 };
            var listFast = list.AsListFast();

            ListFastAPI.GetBufferUnsafe(listFast, out var buffer, out var count);
            buffer[0] = 99;

            Assert.AreEqual(listFast.Count, count);
            Assert.AreEqual(99, list[0]);
        }
    }
}
