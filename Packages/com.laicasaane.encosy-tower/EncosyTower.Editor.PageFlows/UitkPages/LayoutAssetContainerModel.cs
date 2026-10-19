#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.UnityExtensions;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages
{
    /// <summary>
    /// Describes a named element of a layout asset, which a flow can use as its container.
    /// </summary>
    /// <param name="Name">The <c>name</c> of the element. A container name must equal it, compared ordinally.</param>
    /// <param name="Path">
    /// The names of the elements from the root to this element, joined by <c> / </c>. An ancestor without a name shows
    /// its C# type name.
    /// </param>
    internal readonly record struct LayoutAssetContainer(string Name, string Path);

    /// <summary>
    /// Describes one entry of the container picker.
    /// </summary>
    /// <param name="Value">The container name that the entry writes when picked. Empty for the codex root.</param>
    /// <param name="Label">The name that the entry shows.</param>
    /// <param name="Path">The tooltip and secondary text of the entry.</param>
    /// <param name="Checked">Whether the entry is the current container.</param>
    /// <param name="Note">More text for the entry, for example which other flow definitions use it.</param>
    internal readonly record struct ContainerChoice(string Value, string Label, string Path, bool Checked, string Note);

    /// <summary>
    /// A snapshot of the named elements of one layout asset.
    /// </summary>
    /// <remarks>
    /// <see cref="Empty"/> stands for no layout asset: its <see cref="AssetName"/> is <see langword="null"/> and
    /// <see cref="HasAsset"/> is <see langword="false"/>. <see cref="LayoutAssetContainerModel.Get"/> creates and
    /// caches the snapshots.
    /// </remarks>
    internal sealed class LayoutAssetContainers
    {
        /// <summary>
        /// The snapshot for no layout asset.
        /// </summary>
        public static readonly LayoutAssetContainers Empty = new(null, Array.Empty<LayoutAssetContainer>());

        public LayoutAssetContainers(string assetName, LayoutAssetContainer[] items)
        {
            AssetName = assetName;
            Items = items;
        }

        /// <summary>
        /// Gets the name of the layout asset.
        /// </summary>
        /// <value><see langword="null"/> when there is no layout asset.</value>
        public string AssetName { get; }

        /// <summary>
        /// Gets the named elements in depth-first document order.
        /// </summary>
        /// <value>Elements with the same name are all kept. Elements without a name are not listed.</value>
        public LayoutAssetContainer[] Items { get; }

        /// <summary>
        /// Gets whether a layout asset exists.
        /// </summary>
        public bool HasAsset => AssetName != null;

        /// <summary>
        /// Counts the elements that have a name.
        /// </summary>
        /// <param name="name">The name to look for, compared ordinally.</param>
        /// <param name="firstPath">The path of the first match, or <see langword="null"/> when there is none.</param>
        /// <returns>The number of elements named <paramref name="name"/>.</returns>
        public int CountOf(string name, out string firstPath)
        {
            var count = 0;
            firstPath = null;

            for (var i = 0; i < Items.Length; i++)
            {
                var item = Items[i];

                if (string.Equals(item.Name, name, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                firstPath ??= item.Path;
                count++;
            }

            return count;
        }
    }

    /// <summary>
    /// Reads, caches, validates and lists the container choices of a layout asset.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The model instantiates the asset to read its element tree. Only elements with a name are containers. It caches
    /// the result for each asset and reads the asset again when its content hash changes. A cache entry stays until the
    /// next domain reload or <see cref="ClearCache"/>; destroyed assets are not removed.
    /// </para>
    /// <para>
    /// Only the UI Toolkit codex inspector uses it.
    /// </para>
    /// </remarks>
    internal static class LayoutAssetContainerModel
    {
        private const string PATH_SEPARATOR = " / ";

        private static readonly Dictionary<VisualTreeAsset, CacheEntry> s_cache = new();

        /// <summary>
        /// Gets the named elements of a layout asset.
        /// </summary>
        /// <param name="asset">The layout asset. Can be <see langword="null"/>.</param>
        /// <returns>
        /// The snapshot of the asset, or <see cref="LayoutAssetContainers.Empty"/> when <paramref name="asset"/> is
        /// <see langword="null"/> or destroyed.
        /// </returns>
        public static LayoutAssetContainers Get(VisualTreeAsset asset)
        {
            if (asset.IsInvalid())
            {
                return LayoutAssetContainers.Empty;
            }

            var hash = asset.contentHash;

            if (s_cache.TryGetValue(asset, out var entry) && entry.ContentHash == hash)
            {
                return entry.Containers;
            }

            var items = new List<LayoutAssetContainer>();
            var root = asset.Instantiate();
            var children = root.Children();

            foreach (var child in children)
            {
                Collect(child, string.Empty, items);
            }

            var containers = new LayoutAssetContainers(asset.name, items.ToArray());
            s_cache[asset] = new CacheEntry(hash, containers);
            return containers;
        }

        /// <summary>
        /// Checks a container name against the named elements of a layout asset.
        /// </summary>
        /// <param name="containerName">The container name of a flow definition.</param>
        /// <param name="containers">The snapshot to check against.</param>
        /// <param name="error">
        /// The problem when the method returns <see langword="true"/>, otherwise the default
        /// <see cref="FlowDefinitionError"/>.
        /// </param>
        /// <returns><see langword="true"/> when the container name has a problem.</returns>
        /// <remarks>
        /// <para>
        /// An empty name is valid and means the codex root. For a name that is not empty, the problem is
        /// <see cref="FlowDefinitionError.ContainerWithoutLayout"/> when there is no layout asset,
        /// <see cref="FlowDefinitionError.ContainerNotInLayout"/> when no element has the name, and
        /// <see cref="FlowDefinitionError.ContainerMatchesMany"/> when more than one element has it. Exactly one
        /// match is valid.
        /// </para>
        /// </remarks>
        public static bool TryGetError(
              string containerName
            , LayoutAssetContainers containers
            , out FlowDefinitionError error
        )
        {
            error = default;

            if (string.IsNullOrEmpty(containerName))
            {
                return false;
            }

            if (containers.HasAsset == false)
            {
                error = new FlowDefinitionError.ContainerWithoutLayout(containerName);
                return true;
            }

            var count = containers.CountOf(containerName, out var firstPath);

            if (count == 1)
            {
                return false;
            }

            if (count < 1)
            {
                error = new FlowDefinitionError.ContainerNotInLayout(containerName, containers.AssetName);
                return true;
            }

            error = new FlowDefinitionError.ContainerMatchesMany(
                  containerName
                , count
                , containers.AssetName
                , firstPath
            );

            return true;
        }

        /// <summary>
        /// Lists the entries of the container picker.
        /// </summary>
        /// <param name="containers">The snapshot to list.</param>
        /// <param name="current">The current container name of the flow definition.</param>
        /// <param name="otherDefinitions">
        /// The identifier and container name of the other flow definitions, used only for the used-by note. Can be
        /// <see langword="null"/>. A flow definition without an identifier shows as no identifier.
        /// </param>
        /// <returns>
        /// The entries. The first one is always the codex root, with an empty value, checked when
        /// <paramref name="current"/> is empty. One entry for each named element follows in document order. Only the
        /// first element named <paramref name="current"/> is checked.
        /// </returns>
        /// <remarks>
        /// Does not read the layout asset again.
        /// </remarks>
        public static ContainerChoice[] GetChoices(
              LayoutAssetContainers containers
            , string current
            , IReadOnlyList<(string Identifier, string ContainerName)> otherDefinitions
        )
        {
            var items = containers.Items;
            var choices = new List<ContainerChoice>(items.Length + 1) {
                new(
                      Value: string.Empty
                    , Label: PageFlowsViewResources.Get().Picker.CodexRoot
                    , Path: string.Empty
                    , Checked: string.IsNullOrEmpty(current)
                    , Note: string.Empty
                ),
            };

            var checkedName = false;

            for (var i = 0; i < items.Length; i++)
            {
                var item = items[i];
                var isChecked = checkedName == false && string.Equals(item.Name, current, StringComparison.Ordinal);
                checkedName |= isChecked;

                choices.Add(new ContainerChoice(
                      Value: item.Name
                    , Label: item.Name
                    , Path: item.Path
                    , Checked: isChecked
                    , Note: GetUsedByNote(item.Name, otherDefinitions)
                ));
            }

            return choices.ToArray();
        }

        /// <summary>
        /// Empties the cache.
        /// </summary>
        /// <remarks>
        /// Tests call it before and after each case. Production code does not need it, because a change of the asset
        /// is found by its content hash.
        /// </remarks>
        internal static void ClearCache()
        {
            s_cache.Clear();
        }

        private static string GetUsedByNote(
              string name
            , IReadOnlyList<(string Identifier, string ContainerName)> otherDefinitions
        )
        {
            if (otherDefinitions == null)
            {
                return string.Empty;
            }

            var picker = PageFlowsViewResources.Get().Picker;
            var users = new List<string>();
            var count = otherDefinitions.Count;

            for (var i = 0; i < count; i++)
            {
                var (identifier, containerName) = otherDefinitions[i];

                if (string.Equals(containerName, name, StringComparison.Ordinal))
                {
                    users.Add(string.IsNullOrEmpty(identifier) ? picker.NoIdentifier : identifier);
                }
            }

            return users.Count > 0 ? picker.UsedBy(string.Join(", ", users)) : string.Empty;
        }

        private static void Collect(VisualElement element, string parentPath, List<LayoutAssetContainer> items)
        {
            var segment = string.IsNullOrEmpty(element.name) ? element.GetType().Name : element.name;
            var path = string.IsNullOrEmpty(parentPath) ? segment : parentPath + PATH_SEPARATOR + segment;

            if (string.IsNullOrEmpty(element.name) == false)
            {
                items.Add(new LayoutAssetContainer(element.name, path));
            }

            foreach (var child in element.Children())
            {
                Collect(child, path, items);
            }
        }

        private readonly record struct CacheEntry(int ContentHash, LayoutAssetContainers Containers);
    }
}

#endif
