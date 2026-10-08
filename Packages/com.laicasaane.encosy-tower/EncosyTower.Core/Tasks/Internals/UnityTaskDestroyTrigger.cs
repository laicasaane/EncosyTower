using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EncosyTower.Tasks
{
    /// <summary>
    /// Hidden component that cancels registered <see cref="CancellationTokenSource"/> objects when its
    /// <see cref="GameObject"/> is destroyed.
    /// </summary>
    /// <remarks>
    /// <see cref="ExecuteAlwaysAttribute"/> makes <c>OnDestroy</c> run in edit mode as well. A source registered after
    /// the destruction is canceled at once.
    /// </remarks>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ExecuteAlways]
    internal sealed class UnityTaskDestroyTrigger : MonoBehaviour
    {
        private readonly List<CancellationTokenSource> _sources = new();
        private bool _destroyed;

        /// <summary>
        /// Adds <paramref name="source"/> to the trigger of <paramref name="gameObject"/>, creating the trigger if
        /// needed.
        /// </summary>
        internal static void Register(GameObject gameObject, CancellationTokenSource source)
        {
            if (gameObject.TryGetComponent<UnityTaskDestroyTrigger>(out var trigger) == false)
            {
                trigger = gameObject.AddComponent<UnityTaskDestroyTrigger>();
                trigger.hideFlags = HideFlags.HideAndDontSave;
            }

            if (trigger._destroyed)
            {
                source.Cancel();
                return;
            }

            trigger._sources.Add(source);
        }

        /// <summary>
        /// Marks the trigger as destroyed and cancels every registered source.
        /// </summary>
        private void OnDestroy()
        {
            _destroyed = true;

            var count = _sources.Count;

            for (var i = 0; i < count; i++)
            {
                _sources[i].Cancel();
            }

            _sources.Clear();
        }
    }
}
