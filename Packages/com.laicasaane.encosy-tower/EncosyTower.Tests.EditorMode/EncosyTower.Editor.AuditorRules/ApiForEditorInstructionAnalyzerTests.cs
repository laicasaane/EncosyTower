#if UNITY_EDITOR && UNITY_TEST_FRAMEWORK && ENCOSY_TESTS_ENABLED

using EncosyTower.Editor.AuditorRules;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using Unity.ProjectAuditor.Editor;
using Unity.ProjectAuditor.Editor.Core;
using static EncosyTower.Tests.Editor.AuditorRules.ApiMarkerTestHelper;

namespace EncosyTower.Tests.Editor.AuditorRules
{
    public sealed class ApiForEditorInstructionAnalyzerTests
    {
        [Test]
        public void MarkedMethod_ReportsExpectedRule()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var target = AddMethod(AddType(module, "Api"), "Use");
            Mark(module, target, ApiForEditorInstructionAnalyzer.MARKER);

            var reported = CreateRules().TryGetViolation(
                  caller: caller
                , target: target
                , authoringAllowed: false
                , api: out var actualApi
            );

            Assert.That(reported, Is.True);
            Assert.That(actualApi, Is.SameAs(target));
        }

        [TestCase(ApiForEditorInstructionAnalyzer.MARKER, false)]
        [TestCase(ApiForAuthoringInstructionAnalyzer.MARKER, true)]
        public void CallingMember_UsesAsymmetricExemptions(string callerMarker, bool expected)
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var target = AddMethod(AddType(module, "Api"), "Use");
            Mark(module, caller, callerMarker);
            Mark(module, target, ApiForEditorInstructionAnalyzer.MARKER);
            AssertViolation(caller, target, expected);
        }

        [Test]
        public void MarkedContainingType_ExemptsNestedCaller()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var owner = AddType(module, "Owner");
            var nested = AddNested(owner, "Nested");
            var caller = AddMethod(nested, "Run");
            var target = AddMethod(AddType(module, "Api"), "Use");
            Mark(module, owner, ApiForEditorInstructionAnalyzer.MARKER);
            Mark(module, target, ApiForEditorInstructionAnalyzer.MARKER);
            AssertViolation(caller: caller, target: target, expected: false);
        }

        [Test]
        public void MarkedType_RestrictsUnmarkedNestedMembersAndTypeTokens()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var owner = AddType(module, "Owner");
            var nested = AddNested(owner, "Nested");
            var method = AddMethod(nested, "Use");
            var field = new FieldDefinition("Value", FieldAttributes.Public, module.TypeSystem.Int32);
            nested.Fields.Add(field);
            Mark(module, owner, ApiForEditorInstructionAnalyzer.MARKER);

            AssertViolation(caller: caller, target: method, expected: true);
            AssertViolation(caller: caller, target: field, expected: true);
            AssertViolation(caller: caller, target: nested, expected: true);
        }

        [Test]
        public void PropertyAndEventMarkers_ApplyToAccessorsAndCallingAccessors()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var owner = AddType(module, "Api");
            var getter = AddMethod(owner, "get_Value");
            getter.IsSpecialName = true;
            var property = new PropertyDefinition("Value", PropertyAttributes.None, module.TypeSystem.Int32) {
                GetMethod = getter,
            };

            owner.Properties.Add(property);
            Mark(module, property, ApiForEditorInstructionAnalyzer.MARKER);
            var add = AddMethod(owner, "add_Changed");
            add.IsSpecialName = true;
            var eventDefinition = new EventDefinition("Changed", EventAttributes.None, module.TypeSystem.Object) {
                AddMethod = add,
            };

            owner.Events.Add(eventDefinition);
            Mark(module, eventDefinition, ApiForEditorInstructionAnalyzer.MARKER);
            AssertViolation(caller: caller, target: getter, expected: true);
            AssertViolation(caller: caller, target: add, expected: true);
            AssertViolation(caller: getter, target: add, expected: false);
        }

        [Test]
        public void GenericArgumentsAndArrayElements_AreChecked()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var restricted = AddType(module, "Restricted");
            Mark(module, restricted, ApiForEditorInstructionAnalyzer.MARKER);
            var genericType = AddType(module, "Generic");
            genericType.GenericParameters.Add(new GenericParameter("T", genericType));
            var constructedType = new GenericInstanceType(genericType);
            constructedType.GenericArguments.Add(restricted);
            var method = AddMethod(genericType, "Use");
            method.GenericParameters.Add(new GenericParameter("U", method));
            var constructedMethod = new GenericInstanceMethod(method);
            constructedMethod.GenericArguments.Add(restricted);

            AssertViolation(caller: caller, target: constructedType, expected: true);
            AssertViolation(caller: caller, target: constructedMethod, expected: true);
            AssertViolation(caller: caller, target: new ArrayType(restricted), expected: true);
        }

        [Test]
        public void StateMachineAndLambda_KeepSourceMemberExemption()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var owner = AddType(module, "Consumer");
            var source = AddMethod(owner, "Run");
            Mark(module, source, ApiForEditorInstructionAnalyzer.MARKER);
            var state = AddNested(owner, "State");
            Mark(module, state, "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            var moveNext = AddMethod(state, "MoveNext");
            var stateAttribute = Mark(module, source, "System.Runtime.CompilerServices.AsyncStateMachineAttribute");

            stateAttribute.ConstructorArguments.Add(new CustomAttributeArgument(
                  module.ImportReference(typeof(System.Type))
                , state
            ));

            var lambda = AddMethod(owner, "Lambda");
            Mark(module, lambda, "System.Runtime.CompilerServices.CompilerGeneratedAttribute");
            source.Body.Instructions.Add(Instruction.Create(OpCodes.Ldftn, lambda));
            var target = AddMethod(AddType(module, "Api"), "Use");
            Mark(module, target, ApiForEditorInstructionAnalyzer.MARKER);
            AssertViolation(caller: moveNext, target: target, expected: false);
            AssertViolation(caller: lambda, target: target, expected: false);
            source.CustomAttributes.RemoveAt(0);
            AssertViolation(caller: moveNext, target: target, expected: true);
            AssertViolation(caller: lambda, target: target, expected: true);
        }

        [Test]
        public void UnmarkedAndUnresolvableApis_DoNotReport()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var target = AddMethod(AddType(module, "Api"), "Use");
            AssertViolation(caller: caller, target: target, expected: false);
            var unresolved = new TypeReference("Missing", "Api", module, module);
            AssertViolation(caller: caller, target: unresolved, expected: false);
        }

        [Test]
        public void AuthoringDefine_AllowsAuthoringButNotEditorApis()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var owner = AddType(module, "Api");
            var authoring = AddMethod(owner, "Authoring");
            var editor = AddMethod(owner, "Editor");
            Mark(module, authoring, ApiForAuthoringInstructionAnalyzer.MARKER);
            Mark(module, editor, ApiForEditorInstructionAnalyzer.MARKER);

            Assert.That(CreateRules().TryGetViolation(
                  caller: caller
                , target: authoring
                , authoringAllowed: true
                , api: out _
            ), Is.False);

            Assert.That(CreateRules().TryGetViolation(
                  caller: caller
                , target: editor
                , authoringAllowed: true
                , api: out _
            ), Is.True);
        }

        [TestCase(CompilationMode.Editor)]
        [TestCase(CompilationMode.EditorPlayMode)]
        public void EditorCompilation_DoesNotReport(CompilationMode mode)
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var target = AddMethod(AddType(module, "Api"), "Use");
            Mark(module, target, ApiForEditorInstructionAnalyzer.MARKER);
            var context = new InstructionAnalysisContext {
                Params = new AnalysisParams(copyParamsFromGlobal: false) { CompilationMode = mode },
                MethodDefinition = caller,
                Instruction = Instruction.Create(OpCodes.Call, target),
            };

            Assert.That(new ApiForEditorInstructionAnalyzer().Analyze(context), Is.Empty);
        }

        [Test]
        public void AuthoringOnlyApis_AreIgnored()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var owner = AddType(module, "Api");
            var target = AddMethod(owner, "Use");
            Mark(module, target, ApiForAuthoringInstructionAnalyzer.MARKER);
            AssertViolation(caller: caller, target: target, expected: false);

            Mark(module, owner, ApiForAuthoringInstructionAnalyzer.MARKER);
            AssertViolation(caller: caller, target: owner, expected: false);
        }

        [Test]
        public void BothMarkers_PreserveEditorErrorPrecedence()
        {
            using var module = ModuleDefinition.CreateModule("ApiMarkerTests", ModuleKind.Dll);
            var caller = AddMethod(AddType(module, "Consumer"), "Run");
            var target = AddMethod(AddType(module, "Api"), "Use");
            Mark(module, target, ApiForEditorInstructionAnalyzer.MARKER);
            Mark(module, target, ApiForAuthoringInstructionAnalyzer.MARKER);
            AssertViolation(caller: caller, target: target, expected: true);
        }

        private static ApiForEditorInstructionAnalyzer CreateRules()
            => new();

        private static void AssertViolation(MethodDefinition caller, MemberReference target, bool expected)
            => ApiMarkerTestHelper.AssertViolation(CreateRules().TryGetViolation, caller, target, expected);
    }
}

#endif
