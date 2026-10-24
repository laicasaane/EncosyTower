using System;
using EncosyTower.Samples.UguiPages.Ids;

namespace EncosyTower.Samples.UguiPages.Config
{
    /// <summary>The six outer dimensions and the core, built from the definitions in the configuration.</summary>
    internal static class DimensionCatalog
    {
        public const int OUTER_COUNT = 6;
        public const int FULL_MASK = (1 << OUTER_COUNT) - 1;

        private static DimensionData[] s_outer = Array.Empty<DimensionData>();

        private static DimensionData s_core;

        public static ReadOnlySpan<DimensionData> Outer => s_outer;

        public static DimensionData Core => s_core;

        public static bool IsValid(AppConfig config)
        {
            var definitions = config.Dimensions;
            var count = definitions.Length;
            var seen = 0;

            for (var i = 0; i < count; i++)
            {
                var id = (int)definitions[i].Id;

                if ((uint)id > (uint)Dimension.Core || (seen & (1 << id)) != 0)
                {
                    return false;
                }

                seen |= 1 << id;
            }

            return seen == (1 << (OUTER_COUNT + 1)) - 1;
        }

        public static void Initialize(AppConfig config)
        {
            var definitions = config.Dimensions;
            var count = definitions.Length;
            var outer = new DimensionData[OUTER_COUNT];
            var outerCount = 0;
            var core = default(DimensionData);

            for (var i = 0; i < count; i++)
            {
                var definition = definitions[i];

                if (definition.Id == Dimension.Core)
                {
                    core = CreateData(in definition, OUTER_COUNT);
                }
                else
                {
                    outer[outerCount] = CreateData(in definition, outerCount);
                    outerCount++;
                }
            }

            s_outer = outer;
            s_core = core;
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

        private static DimensionData CreateData(in DimensionDefinition definition, int index)
            => new(
                  Id: definition.Id
                , Title: definition.title
                , Lore: definition.lore
                , Motif: definition.motif
                , InscriptionObject: definition.inscriptionObject
                , Concept: definition.concept
                , Practice: definition.practice
                , Watch: definition.watch
                , Surface: definition.surface
                , Navigation: definition.navigation
                , Index: index
            );
    }
}
