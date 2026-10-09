using System;
using System.Collections.Generic;
using EncosyTower.Common;
using EncosyTower.PageFlows.UitkPages;
using EncosyTower.Serialization;
using UnityEngine;

namespace EncosyTower.Tests.Editor.Serialization
{
    public enum OverridableTestEnum
    {
        First,
        [InspectorName("Second Choice")] Second,
        ThirdValue,
    }

    [Flags]
    public enum OverridableTestFlags
    {
        None = 0,
        A = 1,
        B = 2,
    }

    public static class OverridableTestDefaults
    {
        public static OverridableTestEnum EnumValue = OverridableTestEnum.ThirdValue;
        public static int IntValue = 3;
        public static GameObject ObjectValue;

        public static bool BoolValue => true;

        public static SerializableGuid GuidValue => new(new Guid("01234567-89ab-4cde-8f01-23456789abcd"));

        public static List<SerializableGuid> GuidListValue => new() {
            GuidValue,
            new(new Guid("fedcba98-7654-4321-8fed-cba987654321")),
        };

        public static OverridableTestPayload PayloadValue => new() { number = 7, text = "Default" };

        public static List<int> ListValue => new() { 3, 5 };

        public static int[] ArrayValue => new[] { 3, 5 };

        public static Overridable<int>[] NestedArrayValue => new[] {
            new Overridable<int>(value: 2, isOverridden: true),
            new Overridable<int>(value: 9, isOverridden: false),
        };

        public static List<Overridable<int>> NestedListValue
            => new() {
                new(value: 2, isOverridden: true),
                new(value: 9, isOverridden: false),
            };
    }

    [Serializable]
    public sealed class OverridableTestPayload
    {
        public int number;
        public string text;
    }

    [Serializable]
    public struct OverridableGuidEnvelope
    {
        [field: SerializeField]
        public SerializableGuid Id { get; set; }

        [field: SerializeField]
        public int Count { get; set; }
    }

    [Serializable]
    public sealed class OverridableFixedBufferPayload
    {
        public OverridableGuidEnvelope envelope;
        public SerializableGuid[] array;
        public List<SerializableGuid> list;
        public GameObject target;

        [SerializeReference]
        public OverridableFixedBufferNode node;

        [SerializeReference]
        public OverridableFixedBufferNode alias;

        [SerializeReference]
        public OverridableFixedBufferNode absent;
    }

    [Serializable]
    public sealed class OverridableFixedBufferNode
    {
        public SerializableGuid id;
        public int number;
        public GameObject target;

        [SerializeReference]
        public OverridableFixedBufferNode next;

        public OverridableFixedBufferNode(int number)
        {
            this.number = number;
        }
    }

    public sealed class OverridableTestAsset : ScriptableObject
    {
        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.EnumValue))]
        public Overridable<OverridableTestEnum> enumWithDefault;

        public Overridable<OverridableTestEnum> enumWithoutDefault;

        [OverridableDefault(
              typeof(OverridableTestDefaults)
            , nameof(OverridableTestDefaults.BoolValue)
            , Label = "Global"
        )]
        public Overridable<bool> boolWithDefault;

        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.IntValue))]
        public Overridable<int> retryCount;

        public Overridable<int> intValue;

        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.GuidValue))]
        public Overridable<SerializableGuid> guidValue;

        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.GuidListValue))]
        public Overridable<List<SerializableGuid>> guidList;

        public Overridable<OverridableTestFlags> flagsValue;

        public Overridable<GameObject> objectValue;
        public Overridable<OverridableTestPayload> payloadValue;
        public Overridable<List<int>> listValue;
        public Overridable<int[]> arrayValue;

        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.NestedListValue))]
        public Overridable<List<Overridable<int>>> nestedList;

        public Overridable<Overridable<bool>[]> nestedArray;
        public Overridable<Overridable<int>[]> nestedIntArray;

        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.IntValue))]
        public Overridable<int>[] ordinaryArray;

        [OverridableDefault(typeof(OverridableTestDefaults), nameof(OverridableTestDefaults.IntValue))]
        public List<Overridable<int>> ordinaryList;
    }

    public sealed class OverridableSettingsTestAsset : ScriptableObject
    {
        [OverridableDefault(typeof(UitkPageFlowSettings), nameof(UitkPageFlowSettings.warnNoSubscriber))]
        public Overridable<bool> warnNoSubscriber;
    }
}
