using System;
using EncosyTower.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace EncosyTower.Tests.Core.Collections
{
    public sealed class ClassCollectionSerializationHost : ScriptableObject, IDisposable
    {
        [SerializeField] internal ArrayMap<int, int> _arrayMap = new();
        [SerializeField] internal ArraySet<int> _arraySet = new();
        [SerializeField] internal SharedArray<int> _sharedArray = new(0);
        [SerializeField] internal SharedReference<int> _sharedReference = new();
        [SerializeField] internal SharedList<int> _sharedList = new();
        [SerializeField] internal SharedQueue<int> _sharedQueue = new();
        [SerializeField] internal SharedStack<int> _sharedStack = new();
        [SerializeField] internal SharedArrayMap<int, int> _sharedArrayMap = new();
        [SerializeField] internal SharedArraySet<int> _sharedArraySet = new();

        public void Dispose()
        {
            _arrayMap?.Dispose();
            _arraySet?.Dispose();
            _sharedArray?.Dispose();
            _sharedReference?.Dispose();
            _sharedList?.Dispose();
            _sharedQueue?.Dispose();
            _sharedStack?.Dispose();
            _sharedArrayMap?.Dispose();
            _sharedArraySet?.Dispose();
        }
    }

    internal static class ClassCollectionSerializationTestUtility
    {
        public static void AssertAssetRoundTrip(
              Action<ClassCollectionSerializationHost> initialize
            , Action<ClassCollectionSerializationHost> verify
        )
        {
            var suffix = Guid.NewGuid().ToString("N");
            var sourceAssetPath = $"Assets/__EncosyTowerClassCollectionSerialization{suffix}Source.asset";
            var copyAssetPath = $"Assets/__EncosyTowerClassCollectionSerialization{suffix}Copy.asset";
            var source = ScriptableObject.CreateInstance<ClassCollectionSerializationHost>();
            ClassCollectionSerializationHost copy = null;

            try
            {
                initialize(source);
                AssetDatabase.CreateAsset(source, sourceAssetPath);
                EditorUtility.SetDirty(source);
                AssetDatabase.SaveAssets();

                Assert.That(AssetDatabase.CopyAsset(sourceAssetPath, copyAssetPath), Is.True);
                AssetDatabase.ImportAsset(
                      copyAssetPath
                    , ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate
                );

                copy = AssetDatabase.LoadAssetAtPath<ClassCollectionSerializationHost>(copyAssetPath);

                Assert.That(copy, Is.Not.Null);
                Assert.That(copy, Is.Not.SameAs(source));
                verify(copy);
            }
            finally
            {
                copy?.Dispose();
                source?.Dispose();
                AssetDatabase.DeleteAsset(copyAssetPath);
                AssetDatabase.DeleteAsset(sourceAssetPath);
            }
        }
    }
}
