using System;
using System.Collections.Generic;
using UnityEngine;

namespace EncosyTower.Tests.Core.Testing
{
    /// <summary>
    /// Creates temporary <see cref="GameObject"/>s for a test and destroys every one of them in <see cref="Dispose"/>.
    /// </summary>
    internal sealed class TempGameObjects : IDisposable
    {
        private readonly List<GameObject> _created = new();

        public GameObject Create(string name = "TempGameObject")
        {
            var gameObject = new GameObject(name);
            _created.Add(gameObject);
            return gameObject;
        }

        public void Dispose()
        {
            for (var i = _created.Count - 1; i >= 0; i--)
            {
                var gameObject = _created[i];

                if (gameObject != null)
                {
                    UnityEngine.Object.DestroyImmediate(gameObject);
                }
            }

            _created.Clear();
        }
    }
}
