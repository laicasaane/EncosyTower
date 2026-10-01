#if UNITY_EDITOR

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;
using EncosyTower.CodeGen;
using EncosyTower.Editor.CodeGen;
using NUnit.Framework;
using UnityEngine;

namespace EncosyTower.Tests.Editor.CodeGen
{
    [Category("Editor.CodeGen")]
    public sealed class UnityBackendTests
    {
        [Test]
        public void Discover_ValidInternalClassAndStruct_SortsByPathThenType()
        {
            var corePath = GetProjectPath(
                  "Packages"
                , "com.laicasaane.encosy-tower"
                , "EncosyTower.Core"
                , "CodeGen"
                , "CodeGeneratorAttribute.cs"
            );
            var testPath = GetCurrentSourcePath();
            var classType = CreateGeneratorType("Tests.P20.ZClass", corePath);
            var structType = CreateGeneratorType("Tests.P20.AStruct", corePath, isStruct: true);
            var secondPathType = CreateGeneratorType("Tests.P20.ASecondPath", testPath);
            var interfaceOnlyType = CreateGeneratorType("Tests.P20.InterfaceOnly", testPath, addAttribute: false);

            var result = UnityCodeGenBackend.Discover(new[] {
                secondPathType,
                classType,
                interfaceOnlyType,
                structType,
                classType,
            });

            Assert.That(result.Diagnostics, Is.Empty);
            Assert.That(result.Candidates, Has.Length.EqualTo(3));
            Assert.That(result.Candidates[0].Type, Is.SameAs(structType));
            Assert.That(result.Candidates[1].Type, Is.SameAs(classType));
            Assert.That(result.Candidates[2].Type, Is.SameAs(secondPathType));
            Assert.That(result.Candidates[0].SourcePath, Is.EqualTo(corePath));
            Assert.That(result.Candidates[2].SourcePath, Is.EqualTo(testPath));
            Assert.That(classType.IsNotPublic, Is.True);
            Assert.That(structType.IsNotPublic, Is.True);
            Assert.That(structType.IsValueType, Is.True);
        }

        [Test]
        public void Discover_InvalidAttributedTypes_ReturnsDiagnostics()
        {
            var sourcePath = GetCurrentSourcePath();
            var abstractType = CreateGeneratorType("Tests.P20.AAbstract", sourcePath, isAbstract: true);
            var missingInterfaceType = CreateGeneratorType(
                  "Tests.P20.BMissingInterface"
                , sourcePath
                , implementInterface: false
            );
            var openGenericType = CreateGeneratorType("Tests.P20.COpenGeneric", sourcePath, isOpenGeneric: true);
            var unconstructableType = CreateGeneratorType(
                  "Tests.P20.DUnconstructable"
                , sourcePath
                , hasParameterlessConstructor: false
            );

            var result = UnityCodeGenBackend.Discover(new[] {
                unconstructableType,
                openGenericType,
                missingInterfaceType,
                abstractType,
            });

            Assert.That(result.Candidates, Is.Empty);
            Assert.That(result.Diagnostics, Has.Length.EqualTo(4));

            for (var i = 0; i < result.Diagnostics.Length; i++)
            {
                Assert.That(result.Diagnostics[i].Severity, Is.EqualTo(CodeGenDiagnosticSeverity.Error));
                Assert.That(result.Diagnostics[i].Code, Is.EqualTo("ENCOSY_CODEGEN_UNITY_0001"));
                Assert.That(result.Diagnostics[i].FilePath, Is.EqualTo(sourcePath));
                Assert.That(result.Diagnostics[i].Line, Is.Zero);
                Assert.That(result.Diagnostics[i].Column, Is.Zero);
            }

            Assert.That(result.Diagnostics[0].Message, Does.Contain(abstractType.FullName));
            Assert.That(result.Diagnostics[1].Message, Does.Contain(missingInterfaceType.FullName));
            Assert.That(result.Diagnostics[2].Message, Does.Contain(openGenericType.FullName));
            Assert.That(result.Diagnostics[3].Message, Does.Contain(unconstructableType.FullName));
        }

        [Test]
        public void Discover_SourceOutsideAssetsOrPackages_ReturnsDiagnostic()
        {
            var sourcePath = GetProjectPath("ProjectSettings", "ProjectVersion.txt");
            var type = CreateGeneratorType("Tests.P20.OutsideProjectSource", sourcePath);

            var result = UnityCodeGenBackend.Discover(new[] { type });

            Assert.That(result.Candidates, Is.Empty);
            Assert.That(result.Diagnostics, Has.Length.EqualTo(1));
            Assert.That(result.Diagnostics[0].Code, Is.EqualTo("ENCOSY_CODEGEN_UNITY_0002"));
            Assert.That(result.Diagnostics[0].FilePath, Is.EqualTo(sourcePath));
        }

        [Test]
        public void Generate_ValidClassAndStruct_InvokesOnCallingThread()
        {
            var sourcePath = GetCurrentSourcePath();
            var classType = CreateGeneratorType(
                  "Tests.P20.AClass"
                , sourcePath
                , outputPath: "A.gen.cs"
                , content: "class-output"
            );
            var structType = CreateGeneratorType(
                  "Tests.P20.BStruct"
                , sourcePath
                , outputPath: "B.gen.cs"
                , content: "struct-output"
                , isStruct: true
            );
            var callingThreadId = Thread.CurrentThread.ManagedThreadId;

            var batch = UnityCodeGenBackend.Generate(
                  new[] { structType, classType }
                , CancellationToken.None
            );

            Assert.That(batch.Diagnostics, Is.Empty);
            Assert.That(batch.GeneratedCodes, Has.Length.EqualTo(2));
            Assert.That(batch.GeneratedCodes[0].filePath, Is.EqualTo("A.gen.cs"));
            Assert.That(batch.GeneratedCodes[0].content, Is.EqualTo("class-output"));
            Assert.That(batch.GeneratedCodes[1].filePath, Is.EqualTo("B.gen.cs"));
            Assert.That(batch.GeneratedCodes[1].content, Is.EqualTo("struct-output"));
            Assert.That(batch.AllCandidatesSkipped, Is.False);
            Assert.That(GetThreadId(classType, "ConstructionThreadId"), Is.EqualTo(callingThreadId));
            Assert.That(GetThreadId(classType, "InvocationThreadId"), Is.EqualTo(callingThreadId));
            Assert.That(GetThreadId(structType, "InvocationThreadId"), Is.EqualTo(callingThreadId));
        }

        [Test]
        public void Generate_FailingType_ContinuesRemainingTypes()
        {
            var sourcePath = GetCurrentSourcePath();
            var failingType = CreateGeneratorType("Tests.P20.AFailing", sourcePath, throwOnGenerate: true);
            var validType = CreateGeneratorType(
                  "Tests.P20.BValid"
                , sourcePath
                , outputPath: "Valid.gen.cs"
                , content: "valid"
            );

            var batch = UnityCodeGenBackend.Generate(
                  new[] { validType, failingType }
                , CancellationToken.None
            );

            Assert.That(batch.GeneratedCodes, Has.Length.EqualTo(1));
            Assert.That(batch.GeneratedCodes[0].filePath, Is.EqualTo("Valid.gen.cs"));
            Assert.That(batch.Diagnostics, Has.Length.EqualTo(1));
            Assert.That(batch.Diagnostics[0].Code, Is.EqualTo("ENCOSY_CODEGEN_UNITY_0003"));
            Assert.That(batch.Diagnostics[0].Message, Does.Contain(failingType.FullName));
            Assert.That(batch.Diagnostics[0].Message, Does.Contain("Generator failure."));
        }

        [Test]
        public void Generate_CanceledToken_ThrowsOperationCanceledException()
        {
            var source = new CancellationTokenSource();
            source.Cancel();

            Assert.Throws<OperationCanceledException>(
                () => UnityCodeGenBackend.Generate(Array.Empty<Type>(), source.Token)
            );
        }

        private static Type CreateGeneratorType(
              string name
            , string sourcePath
            , string outputPath = ""
            , string content = ""
            , bool isStruct = false
            , bool addAttribute = true
            , bool implementInterface = true
            , bool isAbstract = false
            , bool isOpenGeneric = false
            , bool hasParameterlessConstructor = true
            , bool throwOnGenerate = false
        )
        {
            var assemblyName = new AssemblyName($"EncosyTower.CodeGen.Tests.{Guid.NewGuid():N}");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            var module = assembly.DefineDynamicModule(assemblyName.Name);
            var typeAttributes = TypeAttributes.NotPublic | TypeAttributes.BeforeFieldInit;
            var baseType = typeof(object);

            if (isStruct)
            {
                typeAttributes |= TypeAttributes.Sealed | TypeAttributes.SequentialLayout;
                baseType = typeof(ValueType);
            }
            else
            {
                typeAttributes |= TypeAttributes.Class;
                typeAttributes |= isAbstract ? TypeAttributes.Abstract : TypeAttributes.Sealed;
            }

            var interfaces = implementInterface
                ? new[] { typeof(ICodeGenerator) }
                : Type.EmptyTypes;
            var builder = module.DefineType(name, typeAttributes, baseType, interfaces);

            if (isOpenGeneric)
            {
                builder.DefineGenericParameters("T");
            }

            if (addAttribute)
            {
                var attributeConstructor = typeof(CodeGeneratorAttribute).GetConstructor(new[] { typeof(string) });
                builder.SetCustomAttribute(new(attributeConstructor, new object[] { sourcePath }));
            }

            var constructionThreadField = builder.DefineField(
                  "ConstructionThreadId"
                , typeof(int)
                , FieldAttributes.Public | FieldAttributes.Static
            );
            var invocationThreadField = builder.DefineField(
                  "InvocationThreadId"
                , typeof(int)
                , FieldAttributes.Public | FieldAttributes.Static
            );

            if (isStruct == false)
            {
                DefineConstructor(
                      builder
                    , constructionThreadField
                    , hasParameterlessConstructor ? Type.EmptyTypes : new[] { typeof(int) }
                );
            }

            if (implementInterface)
            {
                DefineGenerateMethod(builder, invocationThreadField, outputPath, content, throwOnGenerate);
            }

            return builder.CreateTypeInfo().AsType();
        }

        private static void DefineConstructor(
              TypeBuilder builder
            , FieldBuilder constructionThreadField
            , Type[] parameterTypes
        )
        {
            var constructor = builder.DefineConstructor(
                  MethodAttributes.Public
                , CallingConventions.Standard
                , parameterTypes
            );
            var generator = constructor.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes));
            EmitCurrentThreadId(generator, constructionThreadField);
            generator.Emit(OpCodes.Ret);
        }

        private static void DefineGenerateMethod(
              TypeBuilder builder
            , FieldBuilder invocationThreadField
            , string outputPath
            , string content
            , bool throwOnGenerate
        )
        {
            var method = builder.DefineMethod(
                  nameof(ICodeGenerator.Generate)
                , MethodAttributes.Public
                    | MethodAttributes.Virtual
                    | MethodAttributes.Final
                    | MethodAttributes.HideBySig
                    | MethodAttributes.NewSlot
                , typeof(GeneratedCode[])
                , Type.EmptyTypes
            );
            var generator = method.GetILGenerator();
            EmitCurrentThreadId(generator, invocationThreadField);

            if (throwOnGenerate)
            {
                generator.Emit(OpCodes.Ldstr, "Generator failure.");
                generator.Emit(
                      OpCodes.Newobj
                    , typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })
                );
                generator.Emit(OpCodes.Throw);
            }
            else
            {
                generator.Emit(OpCodes.Ldc_I4_1);
                generator.Emit(OpCodes.Newarr, typeof(GeneratedCode));
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ldelema, typeof(GeneratedCode));
                generator.Emit(OpCodes.Dup);
                generator.Emit(OpCodes.Ldstr, outputPath);
                generator.Emit(OpCodes.Stfld, typeof(GeneratedCode).GetField(nameof(GeneratedCode.filePath)));
                generator.Emit(OpCodes.Ldstr, content);
                generator.Emit(OpCodes.Stfld, typeof(GeneratedCode).GetField(nameof(GeneratedCode.content)));
                generator.Emit(OpCodes.Ret);
            }

            builder.DefineMethodOverride(method, typeof(ICodeGenerator).GetMethod(nameof(ICodeGenerator.Generate)));
        }

        private static void EmitCurrentThreadId(ILGenerator generator, FieldBuilder targetField)
        {
            generator.Emit(OpCodes.Call, typeof(Thread).GetProperty(nameof(Thread.CurrentThread)).GetMethod);
            generator.Emit(OpCodes.Callvirt, typeof(Thread).GetProperty(nameof(Thread.ManagedThreadId)).GetMethod);
            generator.Emit(OpCodes.Stsfld, targetField);
        }

        private static string GetCurrentSourcePath([CallerFilePath] string filePath = "")
            => Path.GetFullPath(filePath);

        private static string GetProjectPath(params string[] parts)
        {
            var path = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            for (var i = 0; i < parts.Length; i++)
            {
                path = Path.Combine(path, parts[i]);
            }

            return Path.GetFullPath(path);
        }

        private static int GetThreadId(Type type, string fieldName)
            => (int)type.GetField(fieldName).GetValue(null);
    }
}

#endif
