using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EncosyTower.AssetKeys;
using EncosyTower.Naming;
using EncosyTower.Vaults;
using UnityEngine;

namespace EncosyTower.Samples.UitkPages.Ids
{
    /// <summary>
    /// Composes the Resources paths of the Book's assets from their ids, and keeps each composed path in a vault so a
    /// key is built once.
    /// </summary>
    /// <remarks>
    /// The rule is the layout of the sample's <c>Resources</c> folder: <c>Pages/{kind}s/uitk-{page}</c>,
    /// <c>Motifs/uitk-motif-{dimension}</c>, <c>Runes/uitk-rune-{rune}</c>, where the kind and member names come from
    /// <see cref="AssetId"/> and the category enums, in kebab case. A new asset is a member of its category enum and
    /// a file at its composed path. The page keys are what the sample passes to
    /// <see cref="EncosyTower.PageFlows.PubSub.ShowPageMessage"/> and the flows' show and add calls, loaded with
    /// <see cref="EncosyTower.PageFlows.Loading.PageLoaderStrategy.Resources"/>; a key whose asset is missing fails the
    /// load with <c>PageFlowError.AssetLoadFailed</c>, which the caller logs. The image keys go to
    /// <see cref="Resources.Load{T}(string)"/>. Call on the Unity main thread.
    /// </remarks>
    internal static class AssetKeyAPI
    {
        private static readonly AssetKey s_pageFormat = new("Pages/{0}s/uitk-{1}");
        private static readonly AssetKey s_motifFormat = new("Motifs/uitk-motif-{0}");
        private static readonly AssetKey s_runeFormat = new("Runes/uitk-rune-{0}");
        private static readonly ValueVault<AssetId, AssetKey> s_keys = new();
        private static readonly Dictionary<AssetKey, AssetId> s_ids = new();

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            // Entering Play Mode without a domain reload keeps the statics.
            s_keys.Clear();
            s_ids.Clear();
        }
#endif

        /// <summary>Gets the key of an asset.</summary>
        /// <param name="id">
        /// The asset; a category enum member converts to it, such as <c>DialogPage.Map</c> or <c>Rune.Kaun</c>; a
        /// <see cref="Dimension"/> member gives the dimension's motif image.
        /// </param>
        /// <returns>The key, or the default (invalid) key for the default id, which names no asset.</returns>
        public static AssetKey Key(AssetId id)
        {
            if (id.Kind == AssetId.IdKind.None)
            {
                return default;
            }

            if (s_keys.TryGet(id, out var key))
            {
                return key;
            }

            key = Compose(id);
            s_keys.TryAdd(id, key);
            s_ids.TryAdd(key, id);
            return key;
        }

        /// <summary>Gets the id of an asset from a key that <see cref="Key"/> composed.</summary>
        /// <param name="key">The key, such as the asset key of a navigated page.</param>
        /// <param name="id">The id, or the default id when no composed key matches.</param>
        /// <returns><c>true</c> when a composed key matches; otherwise <c>false</c>.</returns>
        public static bool TryGetId(AssetKey key, out AssetId id)
            => s_ids.TryGetValue(key, out id);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static AssetKey Compose(AssetId id)
        {
            var name = NamingPolicy.KebabCaseLower.ConvertName(id.GetIdStringFast());

            return id.Kind switch {
                AssetId.IdKind.Dimension => s_motifFormat.Format(name),
                AssetId.IdKind.Rune => s_runeFormat.Format(name),
                _ => s_pageFormat.Format(id.Kind.ToStringFast(), name),
            };
        }
    }
}
