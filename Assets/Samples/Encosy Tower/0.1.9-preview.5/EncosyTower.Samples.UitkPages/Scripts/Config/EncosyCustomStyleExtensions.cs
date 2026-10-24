using System;
using System.Collections.Generic;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>Reads USS custom properties and reports each missing one once.</summary>
    internal static class EncosyCustomStyleExtensions
    {
        private static readonly HashSet<string> s_reported = new(StringComparer.Ordinal);
        private static readonly HashSet<ConfigId> s_reportedIds = new();

        public static void ResetReported()
        {
            s_reported.Clear();
            s_reportedIds.Clear();
        }

        public static void ReportMissing(string propertyName)
        {
            if (s_reported.Add(propertyName))
            {
                ThrowHelper.LogError_PropertyMissing(propertyName);
            }
        }

        public static void ReportUnknown(ConfigId id)
        {
            if (s_reportedIds.Add(id))
            {
                ThrowHelper.LogError_IdUnknown(id);
            }
        }

        public static void Read(this ICustomStyle style, CustomStyleProperty<Color> property, ref Color value)
        {
            if (style.TryGetValue(property, out var read))
            {
                value = read;
            }
            else
            {
                ReportMissing(property.name);
            }
        }

        public static void Read(this ICustomStyle style, CustomStyleProperty<float> property, ref float value)
        {
            if (style.TryGetValue(property, out var read))
            {
                value = read;
            }
            else
            {
                ReportMissing(property.name);
            }
        }

        public static void Read(this ICustomStyle style, CustomStyleProperty<int> property, ref int value)
        {
            if (style.TryGetValue(property, out var read))
            {
                value = read;
            }
            else
            {
                ReportMissing(property.name);
            }
        }
    }
}
