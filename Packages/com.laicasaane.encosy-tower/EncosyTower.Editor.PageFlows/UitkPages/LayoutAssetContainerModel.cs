#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using EncosyTower.UnityExtensions;
using UnityEngine.UIElements;

namespace EncosyTower.Editor.PageFlows.UitkPages
{
    internal readonly record struct LayoutAssetContainer(string Name, string Path);

    internal readonly record struct ContainerChoice(string Value, string Label, string Path, bool Checked, string Note);

    internal sealed class LayoutAssetContainers
    {
        public static readonly LayoutAssetContainers Empty = new(null, Array.Empty<LayoutAssetContainer>());

        public LayoutAssetContainers(string assetName, LayoutAssetContainer[] items)
        {
            AssetName = assetName;
            Items = items;
        }

        public string AssetName { get; }

        public LayoutAssetContainer[] Items { get; }

        public bool HasAsset => AssetName != null;

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

    internal static class LayoutAssetContainerModel
    {
        public const string CODEX_ROOT_LABEL = "(Codex Root)";

        private const string PATH_SEPARATOR = " / ";

        private static readonly Dictionary<VisualTreeAsset, CacheEntry> s_cache = new();

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

        public static bool TryGetProblem(string containerName, LayoutAssetContainers containers, out RowProblem problem)
        {
            problem = default;

            if (string.IsNullOrEmpty(containerName))
            {
                return false;
            }

            if (containers.HasAsset == false)
            {
                problem = CreateProblem(RowProblemKind.ContainerWithoutLayout, containerName, containers);
                return true;
            }

            var count = containers.CountOf(containerName, out var firstPath);

            if (count == 1)
            {
                return false;
            }

            var kind = count < 1 ? RowProblemKind.ContainerNotInLayout : RowProblemKind.ContainerMatchesMany;
            problem = CreateProblem(kind, containerName, containers) with { MatchCount = count, FirstPath = firstPath };
            return true;
        }

        public static ContainerChoice[] GetChoices(
              LayoutAssetContainers containers
            , string current
            , IReadOnlyList<(string Identifier, string ContainerName)> otherRows
        )
        {
            var items = containers.Items;
            var choices = new List<ContainerChoice>(items.Length + 1) {
                new(
                      Value: string.Empty
                    , Label: CODEX_ROOT_LABEL
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
                    , Note: GetUsedByNote(item.Name, otherRows)
                ));
            }

            return choices.ToArray();
        }

        internal static void ClearCache()
        {
            s_cache.Clear();
        }

        private static RowProblem CreateProblem(RowProblemKind kind, string value, LayoutAssetContainers containers)
            => new(
                  Kind: kind
                , Value: value
                , TypeName: string.Empty
                , Scopes: Array.Empty<string>()
                , OtherRows: Array.Empty<int>()
                , AssetName: containers.AssetName
            );

        private static string GetUsedByNote(
              string name
            , IReadOnlyList<(string Identifier, string ContainerName)> otherRows
        )
        {
            if (otherRows == null)
            {
                return string.Empty;
            }

            var users = new List<string>();
            var count = otherRows.Count;

            for (var i = 0; i < count; i++)
            {
                var row = otherRows[i];

                if (string.Equals(row.ContainerName, name, StringComparison.Ordinal))
                {
                    users.Add(string.IsNullOrEmpty(row.Identifier) ? "(no identifier)" : row.Identifier);
                }
            }

            return users.Count > 0 ? $"used by {string.Join(", ", users)}" : string.Empty;
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
