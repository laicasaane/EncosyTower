using System;
using System.Diagnostics.CodeAnalysis;

using DebuggingThrowHelper = EncosyTower.Debugging.ThrowHelper;

namespace EncosyTower.Annotations
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class AssetAttribute : Attribute
    {
        public string Guid { get; }

        public string Path { get; }

        public Type MainAssetType { get; }

        public AssetAttribute([NotNull] string guid, [NotNull] string path, [NotNull] Type mainAssetType)
        {
            DebuggingThrowHelper.ThrowIfNull(guid);
            DebuggingThrowHelper.ThrowIfNull(path);
            DebuggingThrowHelper.ThrowIfNull(mainAssetType);
            Guid = guid;
            Path = path;
            MainAssetType = mainAssetType;
        }
    }
}
