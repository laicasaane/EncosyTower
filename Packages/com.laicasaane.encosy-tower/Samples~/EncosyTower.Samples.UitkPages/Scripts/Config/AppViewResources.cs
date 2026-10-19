using System;
using System.Collections.Generic;
using EncosyTower.Samples.UitkPages.Ids;
using EncosyTower.UnityExtensions;
using UnityEngine.UIElements;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>The resources layout of the Book: it holds the texts, the dimensions and the runes.</summary>
    /// <param name="Root">The root element of the instantiated resources layout.</param>
    internal readonly record struct AppViewResources(VisualElement Root)
    {
        private static readonly HashSet<string> s_reported = new(StringComparer.Ordinal);

        public bool IsMissing
        {
            get => Root == null || Root.childCount < 1;
        }

        public static implicit operator AppViewResources(VisualElement root)
            => new(root);

        public static AppViewResources Create(VisualTreeAsset asset)
        {
            if (asset.IsInvalid())
            {
                return new(new VisualElement());
            }

            return new(asset.CloneTree());
        }

        public static void ResetReported()
        {
            s_reported.Clear();
        }

        public string Get(ConfigTextType name)
            => GetLabel(ConfigNameAPI.NameOf(name));

        public string Format(ConfigTextType name, params object[] args)
            => string.Format(Get(name), args);

        public bool Has(int position, DimensionDetail detail)
            => HasLabel(ConfigNameAPI.DimensionLabelName(position, detail));

        public string Get(int position, DimensionDetail detail)
            => GetLabel(ConfigNameAPI.DimensionLabelName(position, detail));

        public bool HasCore(DimensionDetail detail)
            => HasLabel(ConfigNameAPI.CoreLabelName(detail));

        public string GetCore(DimensionDetail detail)
            => GetLabel(ConfigNameAPI.CoreLabelName(detail));

        public bool Has(int position, RuneDetail detail)
            => HasLabel(ConfigNameAPI.RuneLabelName(position, detail));

        public string Get(int position, RuneDetail detail)
            => GetLabel(ConfigNameAPI.RuneLabelName(position, detail));

        private bool HasLabel(string name)
            => Root != null && Root.childCount > 0 && Root.Q<Label>(name: name) != null;

        private string GetLabel(string name)
        {
            if (Root == null || Root.childCount < 1)
            {
                return string.Empty;
            }

            if (Root.Q<Label>(name: name)?.text is string value)
            {
                return value;
            }

            if (s_reported.Add(name))
            {
                ThrowHelper.LogError_LabelMissing(name);
            }

            return string.Empty;
        }
    }
}
