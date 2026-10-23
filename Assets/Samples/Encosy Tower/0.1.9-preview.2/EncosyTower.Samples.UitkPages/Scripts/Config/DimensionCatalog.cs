using System;
using EncosyTower.Samples.UitkPages.Ids;
using UnityEngine;
using UnityEngine.UIElements;
using ThrowHelper = EncosyTower.Samples.UitkPages.Diagnostics.ThrowHelper;

namespace EncosyTower.Samples.UitkPages.Config
{
    /// <summary>The six outer dimensions and the core, built from the resources.</summary>
    internal static class DimensionCatalog
    {
        public const int OUTER_COUNT = 6;
        public const int FULL_MASK = (1 << OUTER_COUNT) - 1;

        private static DimensionData[] s_outer = Array.Empty<DimensionData>();

        private static DimensionData s_core;

        public static ReadOnlySpan<DimensionData> Outer => s_outer;

        public static DimensionData Core => s_core;

        public static bool Initialize(in AppViewResources resources, ICustomStyle style)
        {
            if (TryValidate(in resources, style, out var reason) == false)
            {
                ThrowHelper.LogError_ConfigInvalid(reason);
                return false;
            }

            var outer = new DimensionData[OUTER_COUNT];

            for (var i = 0; i < OUTER_COUNT; i++)
            {
                DimensionExtensions.TryParse(resources.Get(i, DimensionDetail.Id), out var id);

                outer[i] = new DimensionData(
                      Id: id
                    , Title: resources.Get(i, DimensionDetail.Title)
                    , Lore: resources.Get(i, DimensionDetail.Lore)
                    , Motif: resources.Get(i, DimensionDetail.Motif)
                    , InscriptionObject: resources.Get(i, DimensionDetail.InscriptionObject)
                    , Concept: resources.Get(i, DimensionDetail.Concept)
                    , Practice: resources.Get(i, DimensionDetail.Practice)
                    , Watch: resources.Get(i, DimensionDetail.Watch)
                    , Index: i
                );
            }

            s_outer = outer;
            s_core = new DimensionData(
                  Id: Dimension.Core
                , Title: resources.GetCore(DimensionDetail.Title)
                , Lore: resources.GetCore(DimensionDetail.Lore)
                , Motif: string.Empty
                , InscriptionObject: string.Empty
                , Concept: string.Empty
                , Practice: string.Empty
                , Watch: string.Empty
                , Index: OUTER_COUNT
            );

            return true;
        }

        public static void Reset()
        {
            s_outer = Array.Empty<DimensionData>();
            s_core = null;
        }

        public static bool TryGet(Dimension dimension, out DimensionData data)
        {
            if (dimension == Dimension.Core)
            {
                data = s_core;
                return data != null;
            }

            var count = s_outer.Length;

            for (var i = 0; i < count; i++)
            {
                if (s_outer[i].Id == dimension)
                {
                    data = s_outer[i];
                    return true;
                }
            }

            data = null;
            return false;
        }

        public static bool TryGet(BookLocation location, out DimensionData data)
        {
            if (location.TryGetDimension(out var dimension))
            {
                return TryGet(dimension, out data);
            }

            data = null;
            return false;
        }

        public static bool TryGetOuterId(int index, out Dimension dimension)
        {
            if ((uint)index < (uint)s_outer.Length)
            {
                dimension = s_outer[index].Id;
                return true;
            }

            dimension = default;
            return false;
        }

        private static bool TryValidate(in AppViewResources resources, ICustomStyle style, out string reason)
        {
            var seen = 0;
            var details = DimensionDetailExtensions.Values.AsSpan();

            for (var i = 0; i < OUTER_COUNT; i++)
            {
                for (var detail = 0; detail < details.Length; detail++)
                {
                    if (resources.Has(i, details[detail]) == false)
                    {
                        var label = ConfigNameAPI.DimensionLabelName(i, details[detail]);
                        reason = $"the label '{label}' is missing";
                        return false;
                    }
                }

                var text = resources.Get(i, DimensionDetail.Id);

                if (DimensionExtensions.TryParse(text, out var dimension) == false
                    || (uint)dimension >= (uint)Dimension.Core
                )
                {
                    reason = $"the dimension {i} has the id '{text}', which is not an outer dimension";
                    return false;
                }

                var bit = 1 << (int)dimension;

                if ((seen & bit) != 0)
                {
                    reason = $"the dimension id '{text}' is repeated";
                    return false;
                }

                seen |= bit;
            }

            if (resources.Has(OUTER_COUNT, DimensionDetail.Id))
            {
                reason = $"there are more than {OUTER_COUNT} dimension definitions";
                return false;
            }

            if (resources.HasCore(DimensionDetail.Title) == false || resources.HasCore(DimensionDetail.Lore) == false)
            {
                var coreTitle = ConfigNameAPI.CoreLabelName(DimensionDetail.Title);
                var coreLore = ConfigNameAPI.CoreLabelName(DimensionDetail.Lore);

                reason = $"the labels '{coreTitle}' and '{coreLore}' must both exist";
                return false;
            }

            return TryValidateColors(style, false, out reason)
                && TryValidateColors(style, true, out reason);
        }

        private static bool TryValidateColors(ICustomStyle style, bool navigation, out string reason)
        {
            for (var position = 0; position <= OUTER_COUNT; position++)
            {
                var name = ConfigNameAPI.DimensionColorName(position, navigation);

                if (style.TryGetValue(new CustomStyleProperty<Color>(name), out _) == false)
                {
                    reason = $"the style property '{name}' does not resolve";
                    return false;
                }
            }

            reason = null;
            return true;
        }
    }
}
