#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EncosyTower.Editor;
using EncosyTower.Editor.Internals;
using NUnit.Framework;
using UnityEngine;

using Object = UnityEngine.Object;

namespace EncosyTower.Tests.Editor.Serialization
{
    internal sealed class ValueChoicesEditorAPITests
    {
        [Test]
        public void Attribute_PreservesSourceMemberAndExclusivity()
        {
            var attribute = new ValueChoicesAttribute("Choices");

            Assert.IsNull(attribute.SourceType);
            Assert.AreEqual("Choices", attribute.MemberName);
            Assert.IsFalse(attribute.IsExclusive);

            attribute = new(typeof(ChoiceSource), "Choices") { IsExclusive = true };

            Assert.AreEqual(typeof(ChoiceSource), attribute.SourceType);
            Assert.AreEqual("Choices", attribute.MemberName);
            Assert.IsTrue(attribute.IsExclusive);

            attribute = new(
                  sourceTypeName: typeof(ChoiceSource).FullName
                , memberName: "Choices"
                , applyToCollection: true
            );

            Assert.IsNull(attribute.SourceType);
            Assert.AreEqual(typeof(ChoiceSource).FullName, attribute.SourceTypeName);
            Assert.AreEqual("Choices", attribute.MemberName);
            Assert.IsTrue(attribute.applyToCollection);
        }

        [Test]
        public void ChoiceStructs_PreserveLabelsAndValues()
        {
            var typed = new ValueChoice<int>(label: "Three", value: 3);
            var erased = new ValueChoice(label: "Five", value: 5);

            Assert.AreEqual("Three", typed.Label);
            Assert.AreEqual(expected: 3, actual: typed.Value);
            Assert.AreEqual("Five", erased.Label);
            Assert.AreEqual(expected: 5, actual: erased.Value);
        }

        [Test]
        public void ChoiceStructs_ValueOnlyConstructorsLeaveLabelsNull()
        {
            var typed = new ValueChoice<int>(value: 3);
            var erased = new ValueChoice(ChoiceEnum.Second);

            Assert.IsNull(typed.Label);
            Assert.AreEqual(expected: 3, actual: typed.Value);
            Assert.IsNull(erased.Label);
            Assert.AreEqual(ChoiceEnum.Second, erased.Value);
        }

        [Test]
        public void TryGetChoices_SameClassUsesTheFieldDeclaringType()
        {
            var field = typeof(ChoiceSource).GetField(nameof(ChoiceSource.selectedValue));
            var attribute = field.GetCustomAttribute<ValueChoicesAttribute>();
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , field.DeclaringType
                , typeof(int)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());
            Assert.IsTrue(error.Is(ValueChoicesError.Type.Undefined));
            Assert.IsEmpty(error.ToMessage());
            CollectionAssert.AreEqual(new[] { ("3", (object)3), ("5", (object)5) }, choices);
        }

        [TestCase("s_fieldChoices")]
        [TestCase("PropertyChoices")]
        [TestCase("MethodChoices")]
        [TestCase("EnumerableChoices")]
        [TestCase("InheritedChoices")]
        [TestCase("ProtectedChoices")]
        public void TryGetChoices_ExplicitAndNamedSourcesReadStaticMembersAndCollections(string memberName)
        {
            var attributes = new[] {
                new ValueChoicesAttribute(typeof(ChoiceSource), memberName),
                new ValueChoicesAttribute(typeof(ChoiceSource).FullName, memberName),
                new ValueChoicesAttribute(typeof(ChoiceSource).AssemblyQualifiedName, memberName),
            };

            for (var i = 0; i < attributes.Length; i++)
            {
                for (var attempt = 0; attempt < 2; attempt++)
                {
                    var success = ValueChoicesEditorAPI.TryGetChoices(
                          attributes[i]
                        , typeof(ValueChoicesEditorAPITests)
                        , typeof(int)
                        , out var choices
                        , out var error
                    );

                    Assert.IsTrue(success, error.ToMessage());
                    Assert.IsTrue(error.Is(ValueChoicesError.Type.Undefined));
                    Assert.IsEmpty(error.ToMessage());
                    CollectionAssert.AreEqual(new[] { ("3", (object)3), ("5", (object)5) }, choices);
                }
            }
        }

        [TestCase("LabelledArray")]
        [TestCase("LabelledEnumerable")]
        [TestCase("ErasedArray")]
        [TestCase("ErasedEnumerable")]
        public void TryGetChoices_LabelledAndErasedChoicesPreserveLabelsAndUseDisplayText(string memberName)
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), memberName);
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ValueChoicesEditorAPITests)
                , typeof(int)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());
            CollectionAssert.AreEqual(new[] { ("Three", (object)3), ("5", (object)5), ("7", (object)7) }, choices);
        }

        [Test]
        public void TryGetChoices_UnlabelledEnumsUseInspectorNamesAndNicifiedNames()
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), nameof(ChoiceSource.EnumChoices));
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(ChoiceEnum)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());

            CollectionAssert.AreEqual(
                  new[] { ("First Value", (object)ChoiceEnum.FirstValue), ("Second choice", (object)ChoiceEnum.Second) }
                , choices
            );
        }

        [TestCase(nameof(ChoiceSource.TypedEnumChoices))]
        [TestCase(nameof(ChoiceSource.ErasedEnumChoices))]
        public void TryGetChoices_EmptyEnumLabelsUseInspectorNamesAndExplicitLabelsWin(string memberName)
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), memberName);

            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(ChoiceEnum)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());

            CollectionAssert.AreEqual(
                  new[] {
                      ("Second choice", (object)ChoiceEnum.Second),
                      ("Second choice", (object)ChoiceEnum.Second),
                      ("Custom label", (object)ChoiceEnum.Second),
                      ("First Value", (object)ChoiceEnum.FirstValue),
                  }
                , choices
            );
        }

        [TestCase(nameof(ChoiceSource.TypedFlagsChoices))]
        [TestCase(nameof(ChoiceSource.ErasedFlagsChoices))]
        public void TryGetChoices_FlagsLabelsUseInspectorNamesAndCombinationDisplayText(string memberName)
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), memberName);

            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(ChoiceFlags)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());

            CollectionAssert.AreEqual(
                  new[] {
                      ("Ground layer", (object)ChoiceFlags.Ground),
                      ("Ground layer", (object)ChoiceFlags.Ground),
                      ("Custom label", (object)ChoiceFlags.Ground),
                      ("Ground, Air", (object)(ChoiceFlags.Ground | ChoiceFlags.Air)),
                  }
                , choices
            );
        }

        [Test]
        public void TryGetChoices_ErasedReferenceValuesAcceptNullAndDerivedInstances()
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), nameof(ChoiceSource.ReferenceChoices));
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(Payload)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());
            Assert.AreEqual(expected: 2, actual: choices.Count);
            Assert.AreEqual(("", (object)null), choices[0]);
            Assert.AreEqual("Derived", choices[1].label);
            Assert.AreSame(ChoiceSource.ReferenceValue, choices[1].value);
        }

        [Test]
        public void TryGetChoices_ErasedNullableValuesAcceptNullAndUnderlyingValue()
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), nameof(ChoiceSource.NullableChoices));
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(int?)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());
            CollectionAssert.AreEqual(new[] { ("", (object)null), ("3", (object)3) }, choices);
        }

        [Test]
        public void TryGetChoices_EmptyCollectionSucceeds()
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), nameof(ChoiceSource.EmptyChoices));
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(int)
                , out var choices
                , out var error
            );

            Assert.IsTrue(success, error.ToMessage());
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(ValueChoicesError.Type.Undefined));
            Assert.IsEmpty(error.ToMessage());
        }

        [TestCase("MissingChoices", ValueChoicesError.Type.MissingMember)]
        [TestCase("_instanceChoices", ValueChoicesError.Type.InstanceMember)]
        [TestCase("InstanceProperty", ValueChoicesError.Type.InstanceMember)]
        [TestCase("InstanceMethod", ValueChoicesError.Type.InstanceMember)]
        [TestCase("UnsupportedChoices", ValueChoicesError.Type.UnsupportedMemberType)]
        [TestCase("NonGenericChoices", ValueChoicesError.Type.UnsupportedMemberType)]
        [TestCase("WrongTypedChoices", ValueChoicesError.Type.UnsupportedMemberType)]
        [TestCase("InvalidErasedChoices", ValueChoicesError.Type.IncompatibleValue)]
        [TestCase("NullIntChoices", ValueChoicesError.Type.IncompatibleValue)]
        [TestCase("NullCollection", ValueChoicesError.Type.InvalidCollection)]
        [TestCase("ParameterizedChoices", ValueChoicesError.Type.InvalidMethod)]
        [TestCase("GenericChoices", ValueChoicesError.Type.InvalidMethod)]
        [TestCase("WriteOnlyChoices", ValueChoicesError.Type.InvalidProperty)]
        [TestCase("ThrowingChoices", ValueChoicesError.Type.ReadFailure)]
        [TestCase("ThrowingEnumerable", ValueChoicesError.Type.ReadFailure)]
        public void TryGetChoices_InvalidSourcesReturnTypedErrorAndExactMessageWithNoPartialChoices(
              string memberName
            , ValueChoicesError.Type expectedType
        )
        {
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), memberName);
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(int)
                , out var choices
                , out var error
            );

            Assert.IsFalse(success);
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(expectedType), error.ToMessage());

            var reason = memberName switch {
                "MissingChoices" => "the member was not found",
                "_instanceChoices" or "InstanceProperty" or "InstanceMethod"
                    => "the member is an instance member; a static member is required",
                "UnsupportedChoices" => $"unsupported member type '{typeof(int)}'",
                "NonGenericChoices" => $"unsupported member type '{typeof(IEnumerable)}'",
                "WrongTypedChoices" => $"unsupported member type '{typeof(ValueChoice<string>[])}'",
                "InvalidErasedChoices" => $"value 'Text' is not assignable to '{typeof(int)}'",
                "NullIntChoices" => $"value '' is not assignable to '{typeof(int)}'",
                "NullCollection" => "the member returned a null or unsupported collection",
                "ParameterizedChoices" or "GenericChoices"
                    => "the method must be parameterless and have no unbound generic parameters",
                "WriteOnlyChoices" => "the property must have a static getter and no index parameters",
                _ => $"reading choices failed: {Assert.Throws<FormatException>(ParseInvalidChoices).Message}",
            };

            var message = $"Value choices member '{typeof(ChoiceSource).FullName}.{memberName}': {reason}.";
            Assert.AreEqual(message, error.ToMessage());
            Assert.AreEqual(message, error.ToString());

            static void ParseInvalidChoices()
            {
                int.Parse("Invalid choices");
            }
        }

        [Test]
        public void TryGetChoices_MissingAttributeReturnsTypedErrorAndExactMessage()
        {
            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute: null
                , declaringType: typeof(ChoiceSource)
                , valueType: typeof(int)
                , choices: out var choices
                , error: out var error
            );

            Assert.IsFalse(success);
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(ValueChoicesError.Type.MissingMemberName));

            Assert.AreEqual(
                  $"Value choices member '{typeof(ChoiceSource).FullName}.<unspecified>': "
                    + "a choices attribute with a member name is required."
                , error.ToMessage()
            );
        }

        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void TryGetChoices_InvalidSourceTypesReturnTypedErrorsAndExactMessages(
              bool missingSource
            , bool missingValue
        )
        {
            var sourceType = missingSource ? null : typeof(ChoiceSource);
            var valueType = missingValue ? null : typeof(int);
            var attribute = new ValueChoicesAttribute(nameof(ChoiceSource.EmptyChoices));

            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , sourceType
                , valueType
                , out var choices
                , out var error
            );

            Assert.IsFalse(success);
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(ValueChoicesError.Type.MissingType));

            Assert.AreEqual(
                  $"Value choices member '{sourceType?.FullName}.{attribute.MemberName}': "
                    + "the source type and value type are required."
                , error.ToMessage()
            );

            if (missingValue)
            {
                return;
            }

            var missingName = typeof(ChoiceSource).FullName + ".MissingSource";
            AssertSourceError(missingName, ValueChoicesError.Type.SourceTypeNotFound);

            AssertSourceError(
                  missingName + ", Missing.ValueChoices.Assembly"
                , ValueChoicesError.Type.SourceTypeNotFound
            );

            var ambiguousName = "EncosyTower.Tests.ValueChoices.AmbiguousSource" + Guid.NewGuid().ToString("N");
            var firstSource = CreateSource(ambiguousName);
            var secondSource = CreateSource(ambiguousName);
            Assert.AreNotEqual(firstSource, secondSource);
            AssertSourceError(ambiguousName, ValueChoicesError.Type.AmbiguousSourceType);

            attribute = new(firstSource.AssemblyQualifiedName, "Choices");

            success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(int)
                , out choices
                , out error
            );

            Assert.IsTrue(success, error.ToMessage());
            CollectionAssert.AreEqual(new[] { ("3", (object)3), ("5", (object)5) }, choices);

            static void AssertSourceError(string typeName, ValueChoicesError.Type expected)
            {
                var attribute = new ValueChoicesAttribute(typeName, "PropertyChoices");

                for (var attempt = 0; attempt < 2; attempt++)
                {
                    var success = ValueChoicesEditorAPI.TryGetChoices(
                          attribute
                        , typeof(ChoiceSource)
                        , typeof(int)
                        , out var choices
                        , out var error
                    );

                    Assert.IsFalse(success);
                    Assert.IsEmpty(choices);
                    Assert.IsTrue(error.Is(expected), error.ToMessage());

                    var reason = expected == ValueChoicesError.Type.SourceTypeNotFound
                        ? "the type was not found."
                        : "the name matches multiple loaded types; use an assembly-qualified name.";

                    Assert.AreEqual($"Value choices source type '{typeName}': {reason}", error.ToMessage());
                }
            }

            static Type CreateSource(string typeName)
            {
                var name = new AssemblyName("ValueChoicesSource" + Guid.NewGuid().ToString("N"));
                var assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
                var module = assembly.DefineDynamicModule(name.Name);
                var builder = module.DefineType(typeName, TypeAttributes.Public);
                builder.DefineField("Choices", typeof(int[]), FieldAttributes.Public | FieldAttributes.Static);
                var type = builder.CreateType();
                type.GetField("Choices").SetValue(obj: null, value: new[] { 3, 5 });
                return type;
            }
        }

        [Test]
        public void TryGetChoices_IncompatibleValueDefersFormattingUntilMessageIsRequested()
        {
            DeferredValue.FormatCount = 0;
            var attribute = new ValueChoicesAttribute(typeof(ChoiceSource), nameof(ChoiceSource.DeferredChoices));

            var success = ValueChoicesEditorAPI.TryGetChoices(
                  attribute
                , typeof(ChoiceSource)
                , typeof(int)
                , out var choices
                , out var error
            );

            Assert.IsFalse(success);
            Assert.IsEmpty(choices);
            Assert.IsTrue(error.Is(ValueChoicesError.Type.IncompatibleValue));
            Assert.AreEqual(expected: 0, actual: DeferredValue.FormatCount);

            Assert.AreEqual(
                  $"Value choices member '{typeof(ChoiceSource).FullName}.{attribute.MemberName}': "
                    + $"value 'Deferred' is not assignable to '{typeof(int)}'."
                , error.ToMessage()
            );

            Assert.AreEqual(expected: 1, actual: DeferredValue.FormatCount);
        }

        [Test]
        public void Matches_IntStringAndEnumUseValueEquality()
        {
            Assert.IsTrue(ValueChoicesEditorAPI.Matches(a: 3, b: 3));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: 3, b: 5));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: 3, b: 3L));
            Assert.IsTrue(ValueChoicesEditorAPI.Matches("Text", new string("Text".ToCharArray())));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches("Text", "text"));
            Assert.IsTrue(ValueChoicesEditorAPI.Matches(ChoiceEnum.FirstValue, ChoiceEnum.FirstValue));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(ChoiceEnum.FirstValue, ChoiceEnum.Second));
        }

        [Test]
        public void Matches_MaterialsUseReferenceEqualityEvenAfterDestruction()
        {
            var shader = Shader.Find("Hidden/InternalErrorShader");
            Assert.IsNotNull(shader);
            var first = new Material(shader);
            var second = new Material(shader);

            try
            {
                Assert.IsTrue(ValueChoicesEditorAPI.Matches(first, first));
                Assert.IsFalse(ValueChoicesEditorAPI.Matches(first, second));

                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);

                Assert.IsTrue(ValueChoicesEditorAPI.Matches(first, first));
                Assert.IsFalse(ValueChoicesEditorAPI.Matches(first, second));
                Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: first, b: null));
                Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: null, b: first));
            }
            finally
            {
                Object.DestroyImmediate(first);
                Object.DestroyImmediate(second);
            }
        }

        [Test]
        public void Matches_SerializableClassesUseSerializedContent()
        {
            var first = new Payload { number = 3, text = "Text" };
            var same = new Payload { number = 3, text = "Text" };
            var differentNumber = new Payload { number = 5, text = "Text" };
            var differentText = new Payload { number = 3, text = "Other" };

            Assert.IsTrue(ValueChoicesEditorAPI.Matches(first, first));
            Assert.IsTrue(ValueChoicesEditorAPI.Matches(first, same));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(first, differentNumber));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(first, differentText));
            Assert.IsTrue(ValueChoicesEditorAPI.Matches(a: null, b: null));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: first, b: null));
            Assert.IsFalse(ValueChoicesEditorAPI.Matches(a: null, b: first));
        }

        [TestCase(typeof(ChoiceEnum), false, true, false)]
        [TestCase(typeof(ChoiceEnum), true, true, true)]
        [TestCase(typeof(OverridableTestFlags), false, false, false)]
        [TestCase(typeof(OverridableTestFlags), true, true, true)]
        [TestCase(typeof(int), false, false, false)]
        [TestCase(typeof(int), true, true, true)]
        public void ChoicePolicy_PlainEnumsUseDropdownAndFlagsRespectExclusivity(
              Type valueType
            , bool isExclusive
            , bool usesDropdown
            , bool listsOnlyChoices
        )
        {
            var attribute = new ValueChoicesAttribute("Choices") { IsExclusive = isExclusive };

            Assert.AreEqual(usesDropdown, ValueChoicesEditorAPI.UsesDropdown(attribute, valueType));
            Assert.AreEqual(listsOnlyChoices, ValueChoicesEditorAPI.ListsOnlyChoices(attribute, valueType));
        }

        [Test]
        public void ChoicePolicy_WithoutAttributePreservesEnumDropdownAndFreeInput()
        {
            Assert.IsTrue(ValueChoicesEditorAPI.UsesDropdown(attribute: null, valueType: typeof(ChoiceEnum)));
            Assert.IsFalse(ValueChoicesEditorAPI.UsesDropdown(attribute: null, valueType: typeof(int)));
            Assert.IsFalse(ValueChoicesEditorAPI.UsesDropdown(
                  attribute: null
                , valueType: typeof(OverridableTestFlags)
            ));
            Assert.IsFalse(ValueChoicesEditorAPI.ListsOnlyChoices(attribute: null, valueType: typeof(ChoiceEnum)));
            Assert.IsFalse(ValueChoicesEditorAPI.ListsOnlyChoices(attribute: null, valueType: typeof(int)));

            Assert.IsFalse(ValueChoicesEditorAPI.ListsOnlyChoices(
                  attribute: null
                , valueType: typeof(OverridableTestFlags)
            ));
        }

        private enum ChoiceEnum
        {
            FirstValue,

            [InspectorName("Second choice")]
            Second,
        }

        [Flags]
        private enum ChoiceFlags
        {
            [InspectorName("Ground layer")]
            Ground = 1,

            Air = 2,
        }

        [Serializable]
        private class Payload
        {
            public int number;
            public string text;
        }

        [Serializable]
        private sealed class DerivedPayload : Payload
        {
        }

        private sealed class DeferredValue
        {
            public static int FormatCount;

            public override string ToString()
            {
                FormatCount++;
                return "Deferred";
            }
        }

        private class ChoiceSourceBase
        {
            public static int[] InheritedChoices => new[] { 3, 5 };

            protected static int[] ProtectedChoices => new[] { 3, 5 };
        }

        private sealed class ChoiceSource : ChoiceSourceBase
        {
            public static readonly Payload ReferenceValue = new DerivedPayload { number = 3, text = "Text" };

            private static readonly int[] s_fieldChoices = { 3, 5 };

            [ValueChoices(nameof(s_fieldChoices))]
            public int selectedValue = 0;

            private readonly int[] _instanceChoices = { 3, 5 };

            public static ChoiceEnum[] EnumChoices => new[] { ChoiceEnum.FirstValue, ChoiceEnum.Second };

            public static ValueChoice<ChoiceEnum>[] TypedEnumChoices => new ValueChoice<ChoiceEnum>[] {
                new(ChoiceEnum.Second),
                new(string.Empty, ChoiceEnum.Second),
                new("Custom label", ChoiceEnum.Second),
                new(ChoiceEnum.FirstValue),
            };

            public static ValueChoice[] ErasedEnumChoices => new ValueChoice[] {
                new(ChoiceEnum.Second),
                new(string.Empty, ChoiceEnum.Second),
                new("Custom label", ChoiceEnum.Second),
                new(ChoiceEnum.FirstValue),
            };

            public static ValueChoice<ChoiceFlags>[] TypedFlagsChoices => new ValueChoice<ChoiceFlags>[] {
                new(ChoiceFlags.Ground),
                new(string.Empty, ChoiceFlags.Ground),
                new("Custom label", ChoiceFlags.Ground),
                new(ChoiceFlags.Ground | ChoiceFlags.Air),
            };

            public static ValueChoice[] ErasedFlagsChoices => new ValueChoice[] {
                new(ChoiceFlags.Ground),
                new(string.Empty, ChoiceFlags.Ground),
                new("Custom label", ChoiceFlags.Ground),
                new(ChoiceFlags.Ground | ChoiceFlags.Air),
            };

            public static ValueChoice[] ReferenceChoices
                => new ValueChoice[] { new(label: null, value: null), new("Derived", ReferenceValue) };

            public static ValueChoice[] NullableChoices
                => new ValueChoice[] { new(label: null, value: null), new(label: null, value: 3) };

            public static ValueChoice[] DeferredChoices => new[] { new ValueChoice(new DeferredValue()) };

            public static int[] EmptyChoices => Array.Empty<int>();

            public static int UnsupportedChoices => 3;

            public static IEnumerable NonGenericChoices => new ArrayList { 3, 5 };

            public static ValueChoice<string>[] WrongTypedChoices => new[] { new ValueChoice<string>("Text", "Text") };

            public static ValueChoice[] InvalidErasedChoices
                => new ValueChoice[] { new(label: "Three", value: 3), new("Wrong", "Text") };

            public static ValueChoice[] NullIntChoices => new[] { new ValueChoice(label: null, value: null) };

            public static int[] NullCollection => null;

            public static int[] WriteOnlyChoices
            {
                set => Array.Copy(value, s_fieldChoices, s_fieldChoices.Length);
            }

            public static int[] ThrowingChoices => new[] { int.Parse("Invalid choices") };

            private static int[] PropertyChoices => s_fieldChoices;

            private static ValueChoice<int>[] LabelledArray
                => new ValueChoice<int>[] {
                    new(label: "Three", value: 3),
                    new(value: 5),
                    new(label: "", value: 7),
                };

            private static ValueChoice[] ErasedArray
                => new ValueChoice[] {
                    new(label: "Three", value: 3),
                    new(value: 5),
                    new(label: "", value: 7),
                };

            public int[] InstanceProperty => _instanceChoices;

            public static int[] ParameterizedChoices(int count)
                => new int[count];

            public static int[] GenericChoices<T>()
                => Array.Empty<int>();

            public static IEnumerable<int> ThrowingEnumerable()
            {
                yield return 3;
                yield return int.Parse("Invalid choices");
            }

            private static int[] MethodChoices()
                => s_fieldChoices;

            private static IEnumerable<int> EnumerableChoices()
            {
                yield return 3;
                yield return 5;
            }

            private static IEnumerable<ValueChoice<int>> LabelledEnumerable()
            {
                yield return new(label: "Three", value: 3);
                yield return new(label: null, value: 5);
                yield return new(label: "", value: 7);
            }

            private static IEnumerable<ValueChoice> ErasedEnumerable()
            {
                yield return new(label: "Three", value: 3);
                yield return new(label: null, value: 5);
                yield return new(label: "", value: 7);
            }

            public int[] InstanceMethod()
                => _instanceChoices;
        }
    }
}

#endif
