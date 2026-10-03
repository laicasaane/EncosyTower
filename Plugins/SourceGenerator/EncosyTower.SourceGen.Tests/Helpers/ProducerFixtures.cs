namespace EncosyTower.SourceGen.Tests;

/// <summary>
/// Declarations that make an EncosyTower generator produce a type that other generators cannot see.
/// </summary>
internal static class ProducerFixtures
{
    /// <summary>Makes <c>EnumTemplateGenerator</c> emit <c>enum ScreenType : byte</c> with Lobby and Shop.</summary>
    internal const string SCREEN_TYPE_TEMPLATE = """
        [EncosyTower.EnumExtensions.EnumTemplate]
        public readonly partial struct ScreenType_EnumTemplate { }

        [EncosyTower.EnumExtensions.EnumMembersForTemplate(typeof(ScreenType_EnumTemplate), 0)]
        public enum MenuScreen : byte
        {
            Lobby,
            Shop,
        }
        """;

    internal const string ENUM_TEMPLATE_TOOL = "EncosyTower.Core.Generators.EnumTemplates.EnumTemplateGenerator";

    internal const string SCREEN_TYPE_PRODUCER_PATH = "ScreenTypeProducer.cs";

    internal const string SCREEN_TYPE_PRODUCER = $$"""
        namespace TestProject
        {
        {{SCREEN_TYPE_TEMPLATE}}
        }
        """;

    /// <summary>Attribute classes whose arguments can name a member of a generated type.</summary>
    internal const string TAG_ATTRIBUTES = """
        [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
        public sealed class TagAttribute : System.Attribute
        {
            public TagAttribute(object value) { }
        }

        [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]
        public sealed class MultiTagAttribute : System.Attribute
        {
            public MultiTagAttribute(object value) { }

            public MultiTagAttribute(string name) { }
        }
        """;

    internal static NamedSource ScreenTypeProducer => new(SCREEN_TYPE_PRODUCER_PATH, SCREEN_TYPE_PRODUCER);

    internal const string STAT_SYSTEM_TOOL = "EncosyTower.Entities.Stats.Generators.StatSystemGenerator";

    internal const string STATS_API_PRODUCER_PATH = "StatsApiProducer.cs";

    /// <summary>
    /// Makes <c>StatSystemGenerator</c> emit <c>StatsApi.Stat</c>, <c>StatObserver</c>, and <c>StatModifier</c>.
    /// </summary>
    internal const string STATS_API_PRODUCER = """
        using System;
        using EncosyTower.Entities.Stats;
        using Unity.Entities;

        namespace TestProject;

        [StatSystem(StatDataSize.Size8)]
        public static partial class StatsApi { }
        """;

    internal static NamedSource StatsApiProducer => new(STATS_API_PRODUCER_PATH, STATS_API_PRODUCER);
}
