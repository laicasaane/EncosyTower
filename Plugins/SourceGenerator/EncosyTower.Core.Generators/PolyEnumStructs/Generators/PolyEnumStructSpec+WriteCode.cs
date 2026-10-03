using EncosyTower.Core.Generators.EnumExtensions;

namespace EncosyTower.Core.Generators.PolyEnumStructs
{
    partial struct PolyEnumStructSpec
    {
        private const string METHOD_IMPL_OPTIONS = "g__SRCS.MethodImplOptions";
        private const string INLINING = $"{METHOD_IMPL_OPTIONS}.AggressiveInlining";
        private const string GENERATOR = "\"EncosyTower.Core.Generators.PolyEnumStructs.PolyEnumStructGenerator\"";

        private const string AGGRESSIVE_INLINING = "[g__SRCS.MethodImpl(INLINING)]";
        private const string EDITOR_BROWSABLE_NEVER = "[global::System.ComponentModel.EditorBrowsable(" +
            "global::System.ComponentModel.EditorBrowsableState.Never)]";
        private const string EXCLUDE_COVERAGE = "[g__SDCA.ExcludeFromCodeCoverage]";
        private const string GENERATED_CODE = $"[g__SCDC.GeneratedCode(GENERATOR, \"{SourceGenVersion.VALUE}\")]";
        private const string GENERATED_CODE_LITERAL = $"[g__SCDC.GeneratedCode({GENERATOR}, "
            + $"\"{SourceGenVersion.VALUE}\")]";
        private const string VALIDATION_ATTRIBUTES = "[g__UE.HideInCallstack, g__SD.StackTraceHidden, " +
            "g__SD.Conditional(g__ETDVD.UNITY_EDITOR), " +
            "g__SD.Conditional(g__ETDVD.DEBUG), g__SD.Conditional(g__ETDVD.RUNTIME_CHECKS)]";

        private const string UNDEFINED_NAME = "Undefined";
        private const string ENUM_CASE_NAME = "EnumCase";
        private const string FIELD_OFFSET = "g__SRIS.FieldOffset";

        private static readonly List<ConstructionValue> s_emptyValues = new();

        public readonly string WriteCode(
              in PolyEnumStructCompilationSpec compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            return separateContainer
                ? WriteTargetCode(compilation, token)
                : WriteLegacyCode(compilation, token);
        }

        private readonly string WriteLegacyCode(
              in PolyEnumStructCompilationSpec compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var dimCollection = new DimCollections();

            GenerateMerged(
                  out var structRefs
                , out var mergedStructRef
                , out var partialInterfaceRef
                , out var undefinedType
                , out var enumCaseType
                , token
            );

            var p = new Printer(0, 1024 * 512, token);

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p = p.IncreasedIndent();
            {
                p.Print("#region    ENUM CASE").PrintEndLine();
                p.Print("#endregion =========").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // EnumCase");
                WriteConstraintLines(ref p, typeConstraints);
                p.OpenScope();
                {
                    WriteEnumCaseEnum(ref p, enumCaseType.UnderlyingType, undefinedType);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region    INTERFACE ENUM CASE").PrintEndLine();
                p.Print("#endregion ===================").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // IEnumCase");
                WriteConstraintLines(ref p, typeConstraints);
                p.OpenScope();
                {
                    WriteInterface(ref p, partialInterfaceRef, mergedStructRef);
                }
                p.CloseScope();
                p.PrintEndLine();
                p.Print("#region    CASE STRUCTS").PrintEndLine();
                p.Print("#endregion ============").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // Case Structs");
                WriteConstraintLines(ref p, typeConstraints);
                p.OpenScope();
                {
                    WriteUndefinedStruct(
                          ref p
                        , interfaceDef
                        , undefinedType
                        , mergedStructRef
                        , definedUndefinedStruct
                        , autoEquatable
                        , typeName
                        , typeSelfName
                    );

                    WriteCaseStructs(ref p, structs, mergedStructRef, autoEquatable, typeName, typeSelfName);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region    ENUM STRUCT").PrintEndLine();
                p.Print("#endregion ===========").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial ").Print("struct ")
                    .Print(typeSelfName).Print(" : ")
                    .Print(typeSelfName).Print(".IEnumCase, ")
                    .Print("g__SRCS.IUnion, g__ET.IHasValue");

                if (autoEquatable)
                {
                    p.Print(", g__S.IEquatable<").Print(typeSelfName).Print(">");
                }

                p.PrintEndLine(" // Enum Struct");
                WriteConstraintLines(ref p, typeConstraints);
                p.OpenScope();
                {

                    if (isExplicitLayout)
                    {
                        WriteExplicitFields(ref p, structRefs, mergedStructRef.EnumCaseSize);
                    }
                    else
                    {
                        WriteMergedFields(ref p, mergedStructRef.FieldRefs);
                    }

                    WriteConstructors(ref p, structRefs);
                    WriteUnionPattern(
                          ref p
                        , structRefs
                        , compilation.EnableNullable
                        , hasExplicitHasValue: HasCommonValueProperty(mergedStructRef)
                    );
                    WriteMergedProperties(ref p, mergedStructRef.PropertyDimMap, dimCollection.Properties);
                    WriteMergedIndexers(ref p, mergedStructRef.IndexerDimMap, dimCollection.Indexers);
                    WriteImplicitOperators(ref p, structRefs, mergedStructRef);
                    WriteGetValueMethods(ref p, structRefs);
                    WriteConstructFromMethods(ref p, mergedStructRef, undefinedType);
                    WriteTryGetConstructionValueMethods(ref p, mergedStructRef, undefinedType);
                    WriteMergedMethods(ref p, mergedStructRef.MethodDimMap, dimCollection.Methods);
                    WriteAdditionalMethods(ref p, mergedStructRef);
                    WriteCastableMethods(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region    ENUM CASE API").PrintEndLine();
                p.Print("#endregion =============").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // Enum Case API");
                WriteConstraintLines(ref p, typeConstraints);
                p.OpenScope();
                {
                    p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                    p.PrintLine("private static partial class EnumCaseAPI");
                    p.OpenScope();
                    {
                        WriteEnumCaseApiProperties(ref p, dimCollection.Properties);
                        WriteEnumCaseApiIndexers(ref p, dimCollection.Indexers);
                        WriteEnumCaseApiMethods(ref p, dimCollection.Methods);
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#region    INTERNALS").PrintEndLine();
                p.Print("#endregion =========").PrintEndLine();
                p.PrintEndLine();

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // Internals");
                WriteConstraintLines(ref p, typeConstraints);
                p.OpenScope();
                {
                    WriteHelperConstants(ref p);
                }
                p.CloseScope();
                p.PrintEndLine();

                WriteEnumCaseExtensions(
                      ref p
                    , compilation.UnityCollections
                    , enumCaseType
                    , withEnumExtensions
                    , parentIsNamespace
                    , typeName
                    , enumExtensionsAttributeOwner
                    , typeAccessibility
                    , token
                );
            }
            p = p.DecreasedIndent();

            return p.Result;
        }

        public readonly void WriteContainerCode(
              ref Printer p
            , in PolyEnumStructCompilationSpec compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var dimCollection = new DimCollections();

            GenerateMerged(
                  out _
                , out var mergedStructRef
                , out var partialInterfaceRef
                , out var undefinedType
                , out var enumCaseType
                , token
            );

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();
            supportContainer.Declaration.WriteDeclaration(ref p, token);
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
                WriteEnumCaseEnum(ref p, enumCaseType.UnderlyingType, undefinedType);
                WriteInterface(
                      ref p
                    , interfaceDef
                    , partialInterfaceRef.Properties
                    , partialInterfaceRef.Indexers
                    , partialInterfaceRef.Methods
                );

                if (genericInterfaceDef.IsValid)
                {
                    WriteInterface(
                          ref p
                        , genericInterfaceDef
                        , partialInterfaceRef.GenericProperties
                        , partialInterfaceRef.GenericIndexers
                        , partialInterfaceRef.GenericMethods
                    );
                }

                WriteUndefinedStruct(
                      ref p
                    , interfaceDef
                    , genericInterfaceDef
                    , GetUndefinedStruct()
                    , mergedStructRef
                    , definedUndefinedStruct
                    , autoEquatable
                    , typeName
                );
                WriteCaseStructs(
                      ref p
                    , structs
                    , mergedStructRef
                    , autoEquatable
                    , typeName
                    , interfaceDef
                    , genericInterfaceDef
                    , writeContainerCases: true
                );

                p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
                p.PrintLine("internal static partial class EnumCaseAPI");
                p.OpenScope();
                {
                    FillDimCollections(mergedStructRef, dimCollection);
                    WriteEnumCaseApiProperties(ref p, dimCollection.Properties);
                    WriteEnumCaseApiIndexers(ref p, dimCollection.Indexers);
                    WriteEnumCaseApiMethods(ref p, dimCollection.Methods);
                }
                p.CloseScope();
            }
            p.CloseScope();

            if (withEnumExtensions)
            {
                p.PrintEndLine();

                WriteEnumCaseExtensions(
                      ref p
                    , compilation.UnityCollections
                    , enumCaseType
                    , withEnumExtensions
                    , parentIsNamespace
                    , supportContainer.Declaration.Name
                    , enumExtensionsAttributeOwner
                    , typeAccessibility
                    , token
                );
            }
        }

        private readonly string WriteTargetCode(
              in PolyEnumStructCompilationSpec compilation
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var dimCollection = new DimCollections();

            GenerateMerged(
                  out var structRefs
                , out var mergedStructRef
                , out _
                , out var undefinedType
                , out _
                , token
            );

            var enumCaseName = $"{supportContainer.TypeName}.EnumCase";
            var enumCaseApiName = $"{supportContainer.TypeName}.EnumCaseAPI";
            var p = new Printer(0, 1024 * 384, token);
            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();
            p = p.IncreasedIndent();

            p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // Case Structs");
            WriteConstraintLines(ref p, typeConstraints);
            p.OpenScope();
            {
                WriteCaseStructs(
                      ref p
                    , structs
                    , mergedStructRef
                    , autoEquatable
                    , typeName
                    , interfaceDef
                    , genericInterfaceDef
                    , writeContainerCases: false
                );
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("partial struct ").Print(typeSelfName).Print(" : ")
                .Print(supportContainer.TypeName).Print(".").Print(interfaceDef.name);
            PrintGenericInterface(ref p, supportContainer.TypeName, genericInterfaceDef);
            p.Print(", g__SRCS.IUnion, g__ET.IHasValue");

            if (autoEquatable)
            {
                p.Print(", g__S.IEquatable<").Print(typeSelfName).Print(">");
            }

            p.PrintEndLine(" // Enum Struct");
            WriteConstraintLines(ref p, typeConstraints);
            p.OpenScope();
            {
                if (isExplicitLayout)
                {
                    WriteExplicitFields(ref p, structRefs, mergedStructRef.EnumCaseSize, enumCaseName);
                }
                else
                {
                    WriteMergedFields(ref p, mergedStructRef.FieldRefs, enumCaseName);
                }

                WriteConstructors(ref p, structRefs, enumCaseName);
                WriteUnionPattern(
                      ref p
                    , structRefs
                    , compilation.EnableNullable
                    , enumCaseName
                    , HasCommonValueProperty(mergedStructRef)
                );
                WriteMergedProperties(
                      ref p
                    , mergedStructRef.PropertyDimMap
                    , dimCollection.Properties
                    , enumCaseName
                    , enumCaseApiName
                );
                WriteMergedIndexers(
                      ref p
                    , mergedStructRef.IndexerDimMap
                    , dimCollection.Indexers
                    , enumCaseName
                    , enumCaseApiName
                );
                WriteImplicitOperators(ref p, structRefs, mergedStructRef);
                WriteGetValueMethods(ref p, structRefs, enumCaseName);
                var undefinedCaseName = GetUndefinedStruct().name;
                WriteConstructFromMethods(ref p, mergedStructRef, undefinedCaseName);
                WriteTryGetConstructionValueMethods(ref p, mergedStructRef, undefinedCaseName, enumCaseName);
                WriteMergedMethods(
                      ref p
                    , mergedStructRef.MethodDimMap
                    , dimCollection.Methods
                    , enumCaseName
                    , enumCaseApiName
                );
                WriteAdditionalMethods(ref p, mergedStructRef, enumCaseName);
                WriteCastableMethods(ref p, enumCaseName);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("partial struct ").Print(typeSelfName).PrintEndLine(" // Internals");
            WriteConstraintLines(ref p, typeConstraints);
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
            }
            p.CloseScope();
            p = p.DecreasedIndent();
            return p.Result.TrimEnd('\n');
        }

        private readonly void WriteEnumCaseEnum(ref Printer p, string underlyingType, string undefinedType)
        {
            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine("public enum EnumCase : ").PrintEndLine(underlyingType);
            p.OpenScope();
            {
                if (separateContainer == false)
                {
                    p.PrintBeginLine("/// <inheritdoc cref=\"").Print(typeName).Print(".")
                        .Print(undefinedType).PrintEndLine("\"/>");
                    p.PrintBeginLine("/// <seealso cref=\"").Print(typeName).Print(".")
                        .Print(undefinedType).PrintEndLine("\"/>");
                }

                p.PrintLine("Undefined = 0,");
                p.PrintEndLine();

                var structs = this.structs;
                var count = structs.Count - 1;

                for (var i = 0; i < count; i++)
                {
                    var def = structs[i];
                    var value = i + 1;

                    if (separateContainer == false)
                    {
                        p.PrintBeginLine("/// <inheritdoc cref=\"").Print(typeName).Print(".")
                            .Print(def.name).PrintEndLine("\"/>");
                        p.PrintBeginLine("/// <seealso cref=\"").Print(typeName).Print(".")
                            .Print(def.name).PrintEndLine("\"/>");
                    }

                    p.PrintBeginLine(def.identifier).Print(" = ").Print(value).PrintEndLine(",");
                    p.PrintEndLine();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteInterface(
              ref Printer p
            , in PartialInterfaceRef interfaceRef
            , in MergedStructRef mergedStructRef
        )
        {
            var interfaceDef = this.interfaceDef;
            var accessor = interfaceDef.definedInterface ? "" : "public ";

            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine(accessor).Print("partial interface ").PrintEndLine(interfaceDef.name);
            p.OpenScope();
            {
                foreach (var def in interfaceRef.Properties)
                {
                    WriteProperty(ref p, def);
                }

                foreach (var def in interfaceRef.Indexers)
                {
                    WriteIndexer(ref p, def);
                }

                foreach (var def in interfaceRef.Methods)
                {
                    WriteMethod(ref p, def);
                }

                WriteTryGetConstructionValueMethods(ref p, mergedStructRef);

                p.PrintLine("EnumCase GetEnumCase();");
                p.PrintEndLine();

                p.PrintBeginLine(typeSelfName).Print(" To").Print(typeName).PrintEndLine("();");
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteProperty(ref Printer p, in PropertyDeclaration def)
            {
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name).Print(" ")
                    .Print(def.name).Print(" { ")
                    .PrintIf(def.IsWriteOnly == false, "get; ")
                    .PrintIf(def.CanHaveSetter && def.setter.IsValid, "set; ")
                    .PrintEndLine("}");
                p.PrintEndLine();
            }

            static void WriteIndexer(ref Printer p, in IndexerDeclaration def)
            {
                p.PrintBeginLine().Print(GetReturnRefKind(def.refKind)).Print(def.returnType.name).Print(" this[");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.Print("] { ")
                    .PrintIf(def.IsWriteOnly == false, "get; ")
                    .PrintIf(def.CanHaveSetter && def.setter.IsValid, "set; ")
                    .PrintEndLine("}");
                p.PrintEndLine();
            }

            static void WriteMethod(ref Printer p, in MethodDeclaration def)
            {
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name).Print(" ").Print(def.name).Print("(");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(");");
                p.PrintEndLine();
            }

            static void WriteTryGetConstructionValueMethods(ref Printer p, in MergedStructRef mergedStructRef)
            {
                var typeToStructs = mergedStructRef.TypeToStructsMap;

                foreach (var kv in typeToStructs)
                {
                    var type = kv.Key;

                    p.PrintBeginLine("bool TryGetConstructionValue(out ")
                        .Print(type.name).PrintEndLine(" value, int index = default);");
                    p.PrintEndLine();
                }
            }
        }

        private static void WriteInterface(
              ref Printer p
            , in InterfaceSpec interfaceDef
            , IReadOnlyList<PropertyDeclaration> properties
            , IReadOnlyList<IndexerDeclaration> indexers
            , IReadOnlyList<MethodDeclaration> methods
        )
        {
            var accessor = interfaceDef.definedInterface ? string.Empty : "public ";
            p.PrintLine(GENERATED_CODE);
            p.PrintBeginLine(accessor).Print("partial interface ").Print(interfaceDef.declarationName);

            if (interfaceDef.declarationName.Contains('<'))
            {
                p.Print(" : IEnumCase");
            }

            p.PrintEndLine();
            WriteConstraintLines(ref p, interfaceDef.constraints);
            p.OpenScope();
            {
                foreach (var def in properties)
                {
                    p.PrintBeginLine().Print(GetReturnRefKind(def.refKind)).Print(def.returnType.name)
                        .Print(" ").Print(def.name).Print(" { ")
                        .PrintIf(def.IsWriteOnly == false, "get; ")
                        .PrintIf(def.CanHaveSetter && def.setter.IsValid, "set; ")
                        .PrintEndLine("}");
                    p.PrintEndLine();
                }

                foreach (var def in indexers)
                {
                    p.PrintBeginLine().Print(GetReturnRefKind(def.refKind)).Print(def.returnType.name)
                        .Print(" this[");
                    WriteParameters(ref p, def.parameters);
                    p.Print("] { ")
                        .PrintIf(def.IsWriteOnly == false, "get; ")
                        .PrintIf(def.CanHaveSetter && def.setter.IsValid, "set; ")
                        .PrintEndLine("}");
                    p.PrintEndLine();
                }

                foreach (var def in methods)
                {
                    p.PrintBeginLine().Print(GetReturnRefKind(def.refKind)).Print(def.returnType.name)
                        .Print(" ").Print(def.name).Print("(");
                    WriteParameters(ref p, def.parameters);
                    p.PrintEndLine(");");
                    p.PrintEndLine();
                }

                if (interfaceDef.declarationName.Contains('<') == false)
                {
                    p.PrintLine("EnumCase GetEnumCase();");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteUndefinedStruct(
              ref Printer p
            , in InterfaceSpec interfaceDef
            , string structName
            , in MergedStructRef mergedStructRef
            , in DefinedUndefinedStruct definedUndefinedStruct
            , bool autoEquatable
            , string typeName
            , string typeSelfName
        )
        {
            if (definedUndefinedStruct != DefinedUndefinedStruct.None)
            {
                return;
            }

            var structId = new StructId {
                name = structName,
                identifier = UNDEFINED_NAME
            };

            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial ").Print("struct ").Print(structName)
                .Print(" : ").Print(interfaceDef.name);

            if (autoEquatable)
            {
                p.Print(", g__S.IEquatable<").Print(structName).Print(">");
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                foreach (var def in mergedStructRef.PropertyDimMap.Keys)
                {
                    var isRefReturn = IsReturnRefKind(def.refKind);
                    WriteProperty(ref p, def, isRefReturn);
                }

                foreach (var def in mergedStructRef.IndexerDimMap.Keys)
                {
                    var isRefReturn = IsReturnRefKind(def.refKind);
                    WriteIndexer(ref p, def, isRefReturn);
                }

                foreach (var def in mergedStructRef.MethodDimMap.Keys)
                {
                    var isRefReturn = IsReturnRefKind(def.refKind);
                    WriteMethod(ref p, def, isRefReturn);
                }

                WriteTryGetConstructionValueMethods(ref p, mergedStructRef, structId);

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly EnumCase GetEnumCase()");
                p.OpenScope();
                {
                    p.PrintLine("return EnumCase.Undefined;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(typeSelfName)
                    .Print(" To").Print(typeName).PrintEndLine("()");
                p.OpenScope();
                {
                    p.PrintLine("return this;");
                }
                p.CloseScope();

                if (autoEquatable)
                {
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override bool Equals(object obj)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return obj is ").Print(structName).PrintEndLine(" other && Equals(other);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly bool Equals(").Print(structName).PrintEndLine(" other)");
                    p.OpenScope();
                    {
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override int GetHashCode()");
                    p.OpenScope();
                    {
                        p.PrintLine("return 0;");
                    }
                    p.CloseScope();
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            return;

            static void WriteProperty(ref Printer p, in PropertyDeclaration def, bool isRefReturn)
            {
                p.PrintBeginLine("public ")
                    .PrintIf(def.IsReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ")
                    .PrintEndLine(def.name);
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("get");
                        p.OpenScope();

                        if (isRefReturn)
                        {
                            p.PrintBeginLine("throw new g__S.InvalidOperationException(\"")
                                .Print("Cannot return by reference from case 'Undefined'.")
                                .PrintEndLine("\");");
                        }
                        else
                        {
                            p.PrintLine("return default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("set { }");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteIndexer(ref Printer p, in IndexerDeclaration def, bool isRefReturn)
            {
                p.PrintBeginLine("public ")
                    .PrintIf(def.IsReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" this[");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine("]");
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("get");
                        p.OpenScope();

                        if (isRefReturn)
                        {
                            p.PrintBeginLine("throw new g__S.InvalidOperationException(\"")
                                .Print("Cannot return any reference from the default case.")
                                .PrintEndLine("\");");
                        }
                        else
                        {
                            p.PrintLine("return default;");
                        }

                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("set { }");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteMethod(ref Printer p, in MethodDeclaration def, bool isRefReturn)
            {
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ")
                    .PrintIf(def.isReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ").Print(def.name).Print("(");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.OpenScope();
                {
                    foreach (var arg in def.parameters.AsReadOnlySpan())
                    {
                        if (arg.refKind is RefKind.Ref or RefKind.Out)
                        {
                            p.PrintBeginLine(arg.name).PrintEndLine(" = default;");
                        }
                    }

                    if (def.returnsVoid)
                    {
                        p.PrintLine("return;");
                    }
                    else if (isRefReturn)
                    {
                        p.PrintBeginLine("throw new g__S.InvalidOperationException(\"")
                            .Print("Cannot return any reference from the default case.")
                            .PrintEndLine("\");");
                    }
                    else
                    {
                        p.PrintLine("return default;");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteUndefinedStruct(
              ref Printer p
            , in InterfaceSpec baseInterface
            , in InterfaceSpec genericInterface
            , in StructSpec definition
            , in MergedStructRef mergedStructRef
            , in DefinedUndefinedStruct definedUndefinedStruct
            , bool autoEquatable
            , string typeName
        )
        {
            if (definedUndefinedStruct != DefinedUndefinedStruct.None || definition.IsValid == false)
            {
                return;
            }

            var structId = new StructId {
                name = definition.name,
                identifier = UNDEFINED_NAME,
            };
            p.PrintBeginLine(GENERATED_CODE).PrintEndLine(EXCLUDE_COVERAGE);
            p.PrintBeginLine("public partial struct ").Print(definition.declarationName)
                .Print(" : ").Print(baseInterface.name);

            if (genericInterface.IsValid)
            {
                p.Print(", ").Print(genericInterface.declarationName);
            }

            if (autoEquatable)
            {
                p.Print(", g__S.IEquatable<").Print(definition.declarationName).Print(">");
            }

            p.PrintEndLine();
            WriteConstraintLines(ref p, definition.declarationConstraints);
            p.OpenScope();
            {
                foreach (var property in mergedStructRef.PropertyDimMap.Keys)
                {
                    WriteDefaultProperty(ref p, property);
                }

                foreach (var indexer in mergedStructRef.IndexerDimMap.Keys)
                {
                    WriteDefaultIndexer(ref p, indexer);
                }

                foreach (var method in mergedStructRef.MethodDimMap.Keys)
                {
                    WriteDefaultMethod(ref p, method);
                }

                WriteTryGetConstructionValueMethods(ref p, mergedStructRef, structId);
                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintLine("public readonly EnumCase GetEnumCase()");
                p.OpenScope();
                {
                    p.PrintLine("return EnumCase.Undefined;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(definition.targetTypeName).Print(" To")
                    .Print(typeName).Print(definition.toMethodTypeParameters).PrintEndLine("()");
                WriteConstraintLines(ref p, definition.toMethodConstraints);
                p.OpenScope();
                {
                    p.PrintLine("return this;");
                }
                p.CloseScope();

                if (autoEquatable)
                {
                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override bool Equals(object obj)");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return obj is ").Print(definition.declarationName)
                            .PrintEndLine(" other && Equals(other);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly bool Equals(").Print(definition.declarationName)
                        .PrintEndLine(" other)");
                    p.OpenScope();
                    {
                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly override int GetHashCode()");
                    p.OpenScope();
                    {
                        p.PrintLine("return 0;");
                    }
                    p.CloseScope();
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteMergedFields(
              ref Printer p
            , List<MergedFieldRef> fieldRefs
            , string enumCaseName = ENUM_CASE_NAME
        )
        {
            foreach (var fieldRef in fieldRefs)
            {
                var def = fieldRef.Value;

                p.PrintBeginLine("public ")
                    .PrintIf(isReadOnly || def.isReadOnly, "readonly ")
                    .Print(string.Equals(fieldRef.Name, "enumCase", StringComparison.Ordinal)
                        ? enumCaseName
                        : def.returnType.name)
                    .Print(" ")
                    .Print(fieldRef.Name)
                    .PrintEndLine(";");
            }

            p.PrintEndLine();
        }

        private readonly void WriteExplicitFields(
              ref Printer p
            , List<StructRef> structRefs
            , int enumCaseAlignment
            , string enumCaseName = ENUM_CASE_NAME
        )
        {
            foreach (var structRef in structRefs)
            {
                var def = structRef.Value;

                WriteFieldOffset(ref p, 0);
                p.Print(" public ")
                    .PrintIf(isReadOnly, "readonly ")
                    .Print(def.name)
                    .Print(" case_").Print(def.identifier).PrintEndLine(";");
            }

            WriteFieldOffset(ref p, GetEnumCaseOffset(structRefs, enumCaseAlignment));
            p.Print(" public ").PrintIf(isReadOnly, "readonly ").Print(enumCaseName).PrintEndLine(" enumCase;");

            p.PrintEndLine();

            return;

            static void WriteFieldOffset(ref Printer p, int offset)
            {
                p.PrintLine("// TODO(unsafe-evolution): mark this overlapping field safe/unsafe when the new syntax is available.");
                p.PrintBeginLine("[").Print(FIELD_OFFSET).Print("(").Print(offset).Print(")]");
            }
        }

        private static int GetEnumCaseOffset(List<StructRef> structRefs, int enumCaseAlignment)
        {
            var offset = 0;
            var count = structRefs.Count;

            for (var i = 0; i < count; i++)
            {
                offset = Math.Max(offset, structRefs[i].Value.size);
            }

            var remainder = offset % enumCaseAlignment;

            return remainder == 0 ? offset : offset + enumCaseAlignment - remainder;
        }

        private readonly void WriteConstructors(
              ref Printer p
            , List<StructRef> structRefs
            , string enumCaseName = ENUM_CASE_NAME
        )
        {
            var isExplicitLayout = this.isExplicitLayout;

            foreach (var structRef in structRefs)
            {
                var def = structRef.Value;
                var structIn = def.size > 8 ? "in " : "";

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public ").Print(typeName).Print("(")
                    .Print(structIn).Print(def.name).PrintEndLine(" @case) : this()");
                p.OpenScope();
                {
                    p.PrintBeginLine("this.enumCase = ").Print(enumCaseName).Print(".")
                        .Print(def.identifier).PrintEndLine(";");

                    if (isExplicitLayout)
                    {
                        p.PrintBeginLine("this.case_").Print(def.identifier).PrintEndLine(" = @case;");
                    }
                    else
                    {
                        foreach (var kv in structRef.FieldToMergedFieldMap)
                        {
                            p.PrintBeginLine("this.").Print(kv.Value).Print(" = @case.").Print(kv.Key).PrintEndLine(";");
                        }

                        if (structRef.HiddenFieldToMergedFieldMap.Count > 0)
                        {
                            p.PrintBeginLine("@case.CopyStorage(g__ET.GenericT.T<").Print(def.name).Print(">()");

                            foreach (var kv in structRef.HiddenFieldToMergedFieldMap)
                            {
                                p.Print(", ").Print(kv.Key).Print(": out this.").Print(kv.Value);
                            }

                            p.PrintEndLine(");");
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteUnionPattern(
              ref Printer p
            , List<StructRef> structRefs
            , bool enableNullable
            , string enumCaseName = ENUM_CASE_NAME
            , bool hasExplicitHasValue = false
        )
        {
            p.PrintBeginLine()
                .PrintIf(hasExplicitHasValue == false, "public ")
                .Print("object").PrintIf(enableNullable, "?")
                .PrintIf(hasExplicitHasValue, " g__SRCS.IUnion.Value", " Value")
                .PrintEndLine();
            p.OpenScope();
            {
                p.PrintLine("get => this.enumCase switch");
                p.OpenScope();
                {
                    foreach (var structRef in structRefs)
                    {
                        var def = structRef.Value;

                        p.PrintBeginLine(enumCaseName).Print(".").Print(def.identifier)
                            .Print(" => GetValueOrDefault(g__ET.GenericT.T<").Print(def.name).PrintEndLine(">()),");
                    }

                    p.PrintBeginLine("_ => null").PrintEndLine(",");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public bool HasValue");
            p.OpenScope();
            {
                p.PrintLine("get => this.enumCase switch");
                p.OpenScope();
                {
                    foreach (var structRef in structRefs)
                    {
                        var def = structRef.Value;
                        p.PrintBeginLine(enumCaseName).Print(".").Print(def.identifier).PrintEndLine(" => true,");
                    }

                    p.PrintLine("_ => false");
                }
                p.CloseScope("};");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteImplicitOperators(
              ref Printer p
            , List<StructRef> structRefs
            , MergedStructRef mergedStructRef
        )
        {
            var enumSize = isExplicitLayout
                ? GetEnumCaseOffset(structRefs, mergedStructRef.EnumCaseSize) + mergedStructRef.EnumCaseSize
                : mergedStructRef.Size;
            var enumIn = enumSize > 8 ? "in " : "";

            foreach (var structRef in structRefs)
            {
                var def = structRef.Value;
                var structIn = def.size > 8 ? "in " : "";

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static implicit operator ").Print(typeSelfName).Print("(")
                    .Print(structIn)
                    .Print(def.name)
                    .PrintEndLine(" @case)");
                p.OpenScope();
                {
                    p.PrintLine("return new(@case);");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public static explicit operator ").Print(def.name).Print("(")
                    .Print(enumIn)
                    .Print(typeSelfName)
                    .PrintEndLine(" @enum)");
                p.OpenScope();
                {
                    p.PrintBeginLine("return @enum.GetValueOrThrow(g__ET.GenericT.T<").Print(def.name).PrintEndLine(">());");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteGetValueMethods(
              ref Printer p
            , List<StructRef> structRefs
            , string enumCaseName = ENUM_CASE_NAME
        )
        {
            var isExplicitLayout = this.isExplicitLayout;

            foreach (var structRef in structRefs)
            {
                var def = structRef.Value;
                var structIn = def.size > 8 ? "in " : "";
                var returnsStoredCase = isExplicitLayout
                    && (structRef.FieldToMergedFieldMap.Count > 0 || def.hasUnlistedStorage);

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(def.name)
                    .Print(" GetValueOrThrow(g__ET.T<").Print(def.name).PrintEndLine("> _)");
                p.OpenScope();
                {
                    p.PrintBeginLine("ThrowIfUncastable(this.enumCase, ").Print(enumCaseName).Print(".")
                        .Print(def.identifier).PrintEndLine(");");
                    p.PrintEndLine();

                    if (returnsStoredCase)
                    {
                        p.PrintBeginLine("return this.case_").Print(def.identifier).PrintEndLine(";");
                    }
                    else if (structRef.HiddenFieldToMergedFieldMap.Count > 0)
                    {
                        WriteCaseWithHiddenFields(ref p, "return ", structRef);
                    }
                    else if (structRef.FieldToMergedFieldMap.Count < 1)
                    {
                        p.PrintLine("return new();");
                    }
                    else
                    {
                        p.PrintLine("return new(");
                        p = p.IncreasedIndent();
                        {
                            var first = true;

                            foreach (var kv in structRef.FieldToMergedFieldMap)
                            {
                                p.PrintBeginLine().PrintIf(first, "  ", ", ")
                                    .Print(kv.Key).Print(": g__ET.Option.Some(this.").Print(kv.Value).PrintEndLine(")");

                                first = false;
                            }
                        }
                        p = p.DecreasedIndent();
                        p.PrintLine(");");
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly ").Print(def.name)
                    .Print(" GetValueOrDefault(g__ET.T<").Print(def.name).Print("> _ = default, ")
                    .Print(structIn).Print(def.name).PrintEndLine(" @default = default)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (IsCastable(this.enumCase, ").Print(enumCaseName).Print(".")
                        .Print(def.identifier).PrintEndLine("))");
                    p.OpenScope();
                    {
                        if (returnsStoredCase)
                        {
                            p.PrintBeginLine("return this.case_").Print(def.identifier).PrintEndLine(";");
                        }
                        else if (structRef.HiddenFieldToMergedFieldMap.Count > 0)
                        {
                            WriteCaseWithHiddenFields(ref p, "return ", structRef);
                        }
                        else if (structRef.FieldToMergedFieldMap.Count < 1)
                        {
                            p.PrintLine("return new();");
                        }
                        else
                        {
                            p.PrintLine("return new(");
                            p = p.IncreasedIndent();
                            {
                                var first = true;

                                foreach (var kv in structRef.FieldToMergedFieldMap)
                                {
                                    p.PrintBeginLine().PrintIf(first, "  ", ", ")
                                        .Print(kv.Key).Print(": g__ET.Option.Some(this.").Print(kv.Value).PrintEndLine(")");

                                    first = false;
                                }
                            }
                            p = p.DecreasedIndent();
                            p.PrintLine(");");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("return @default;");
                }
                p.CloseScope();
                p.PrintEndLine();

                p.PrintLine(AGGRESSIVE_INLINING);
                p.PrintBeginLine("public readonly bool TryGetValue(out ").Print(def.name).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintBeginLine("if (IsCastable(this.enumCase, ").Print(enumCaseName).Print(".")
                        .Print(def.identifier).PrintEndLine("))");
                    p.OpenScope();
                    {
                        if (returnsStoredCase)
                        {
                            p.PrintBeginLine("value = this.case_").Print(def.identifier).PrintEndLine(";");
                        }
                        else if (structRef.HiddenFieldToMergedFieldMap.Count > 0)
                        {
                            WriteCaseWithHiddenFields(ref p, "value = ", structRef);
                            p.PrintEndLine();
                        }
                        else if (structRef.FieldToMergedFieldMap.Count < 1)
                        {
                            p.PrintLine("value = new();");
                        }
                        else
                        {
                            p.PrintLine("value = new(");
                            p = p.IncreasedIndent();
                            {
                                var first = true;

                                foreach (var kv in structRef.FieldToMergedFieldMap)
                                {
                                    p.PrintBeginLine().PrintIf(first, "  ", ", ")
                                        .Print(kv.Key).Print(": g__ET.Option.Some(this.").Print(kv.Value).PrintEndLine(")");

                                    first = false;
                                }
                            }
                            p = p.DecreasedIndent();
                            p.PrintLine(");");
                            p.PrintEndLine();
                        }

                        p.PrintLine("return true;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine("value = default;");
                    p.PrintLine("return false;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteCaseWithHiddenFields(ref Printer p, string assignment, StructRef structRef)
        {
            p.PrintBeginLine(assignment).PrintEndLine("new(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  g__ET.GenericT.T<").Print(structRef.Value.name).PrintEndLine(">()");

                foreach (var kv in structRef.FieldToMergedFieldMap)
                {
                    p.PrintBeginLine(", ").Print(kv.Key).Print(": g__ET.Option.Some(this.").Print(kv.Value)
                        .PrintEndLine(")");
                }

                foreach (var kv in structRef.HiddenFieldToMergedFieldMap)
                {
                    p.PrintBeginLine(", ").Print(kv.Key).Print(": this.").Print(kv.Value).PrintEndLine();
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(");");
        }

        private readonly void WriteConstructFromMethods(
              ref Printer p
            , in MergedStructRef mergedStructRef
            , string undefinedType
        )
        {
            var creationMap = mergedStructRef.TypeValueToStructMap;

            foreach (var kv1 in creationMap)
            {
                var type = kv1.Key;

                p.PrintBeginLine("public static ").Print(typeSelfName).Print(" ConstructFrom(")
                    .Print(type.name).PrintEndLine(" value)");
                p.OpenScope();
                {
                    p.PrintLine("return value switch");
                    p.OpenScope();
                    {
                        foreach (var kv2 in kv1.Value)
                        {
                            var value = kv2.Key;
                            var structName = kv2.Value;

                            p.PrintBeginLine()
                                .PrintIf(type.isEnum, type.name).PrintIf(type.isEnum, ".")
                                .Print(value.value).Print(" => new ").Print(structName.name).PrintEndLine("(),");
                        }

                        p.PrintBeginLine("_ => new ").Print(undefinedType).PrintEndLine("(),");
                    }
                    p.CloseScope("};");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteMergedProperties(
              ref Printer p
            , Dictionary<PropertyDeclaration, bool> dimMap
            , List<PropertyDeclaration> dimList
            , string enumCaseName = ENUM_CASE_NAME
            , string enumCaseApiName = "EnumCaseAPI"
        )
        {
            dimList.Clear();

            var isReadOnly = this.isReadOnly;
            var genericTypeArguments = GetGenericInterfaceTypeArguments();

            foreach (var kv in dimMap)
            {
                var def = kv.Key;
                var isDim = kv.Value;

                Write(
                      ref p
                    , isReadOnly
                    , def
                    , isDim
                    , structs
                    , enumCaseName
                    , enumCaseApiName
                    , genericTypeArguments
                );

                if (isDim)
                {
                    dimList.Add(def);
                }
            }

            return;

            static void Write(
                  ref Printer p
                , bool isReadOnly
                , in PropertyDeclaration def
                , bool isDim
                , ReadOnlySpan<StructSpec> structs
                , string enumCaseName
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                var readonlyWritten = def.IsReadOnly && def.CanHaveSetter == false;

                p.PrintBeginLine("public ")
                    .PrintIf(readonlyWritten, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ")
                    .PrintEndLine(def.name);
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintBeginLine()
                            .PrintIf(def.getter.isReadOnly && readonlyWritten == false, "readonly ")
                            .PrintEndLine("get");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case ").Print(enumCaseName).Print(".")
                                        .Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteGetterCase(
                                              ref p
                                            , isReadOnly
                                            , def
                                            , isDim
                                            , structDef
                                            , enumCaseApiName
                                            , genericTypeArguments
                                        );
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteGetterCase(
                                          ref p
                                        , isReadOnly
                                        , def
                                        , isDim
                                        , structs[last]
                                        , enumCaseApiName
                                        , genericTypeArguments
                                    );
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine("set");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case ").Print(enumCaseName).Print(".")
                                        .Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteSetterCase(
                                              ref p
                                            , isReadOnly
                                            , def
                                            , isDim
                                            , structDef
                                            , enumCaseApiName
                                            , genericTypeArguments
                                        );
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteSetterCase(
                                          ref p
                                        , isReadOnly
                                        , def
                                        , isDim
                                        , structs[last]
                                        , enumCaseApiName
                                        , genericTypeArguments
                                    );
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteGetterCase(
                  ref Printer p
                , bool isReadOnly
                , in PropertyDeclaration def
                , bool isDim
                , in StructSpec structDef
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print("var result_for_enum_case = ")
                    .Print(GetAnyRef(def.refKind));
                {
                    if (isDim)
                    {
                        p.Print(enumCaseApiName).Print(".Property_Get_").Print(def.name);
                        PrintHelperTypeArguments(
                              ref p
                            , def.genericInterface
                            , genericTypeArguments
                            , structDef.name
                        );
                        p.Print("(ref enum_case)");
                    }
                    else
                    {
                        p.Print("enum_case.").Print(def.name);
                    }
                }
                p.PrintEndLine(";");

                p.PrintLineIf(isReadOnly == false && def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintBeginLine("return ").Print(GetAnyRef(def.refKind)).PrintEndLine("result_for_enum_case;");
            }

            static void WriteSetterCase(
                  ref Printer p
                , bool isReadOnly
                , in PropertyDeclaration def
                , bool isDim
                , in StructSpec structDef
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");

                {
                    if (isDim)
                    {
                        p.PrintBeginLine(enumCaseApiName).Print(".Property_Set_").Print(def.name);
                        PrintHelperTypeArguments(
                              ref p
                            , def.genericInterface
                            , genericTypeArguments
                            , structDef.name
                        );
                        p.PrintEndLine("(ref enum_case, value);");
                    }
                    else
                    {
                        p.PrintBeginLine("enum_case.").Print(def.name).PrintEndLine(" = value;");
                    }
                }

                p.PrintLineIf(isReadOnly == false && def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintLine("return;");
            }
        }

        private readonly void WriteMergedIndexers(
              ref Printer p
            , Dictionary<IndexerDeclaration, bool> dimMap
            , List<IndexerDeclaration> dimList
            , string enumCaseName = ENUM_CASE_NAME
            , string enumCaseApiName = "EnumCaseAPI"
        )
        {
            dimList.Clear();

            var isReadOnly = this.isReadOnly;
            var genericTypeArguments = GetGenericInterfaceTypeArguments();

            foreach (var kv in dimMap)
            {
                var def = kv.Key;
                var isDim = kv.Value;

                Write(
                      ref p
                    , isReadOnly
                    , def
                    , isDim
                    , structs
                    , enumCaseName
                    , enumCaseApiName
                    , genericTypeArguments
                );

                if (isDim)
                {
                    dimList.Add(def);
                }
            }

            return;

            static void Write(
                  ref Printer p
                , bool isReadOnly
                , in IndexerDeclaration def
                , bool isDim
                , ReadOnlySpan<StructSpec> structs
                , string enumCaseName
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                var readonlyWritten = def.IsReadOnly && def.CanHaveSetter == false;

                p.PrintBeginLine("public ")
                    .PrintIf(readonlyWritten, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" this[");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine("]");
                p.OpenScope();
                {
                    if (def.IsWriteOnly == false)
                    {
                        p.PrintBeginLine()
                            .PrintIf(def.getter.isReadOnly && readonlyWritten == false, "readonly ")
                            .PrintEndLine("get");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case ").Print(enumCaseName).Print(".")
                                        .Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteGetterCase(
                                              ref p
                                            , isReadOnly
                                            , def
                                            , isDim
                                            , structDef
                                            , enumCaseApiName
                                            , genericTypeArguments
                                        );
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteGetterCase(
                                          ref p
                                        , isReadOnly
                                        , def
                                        , isDim
                                        , structs[last]
                                        , enumCaseApiName
                                        , genericTypeArguments
                                    );
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    if (def.CanHaveSetter && def.setter.IsValid)
                    {
                        p.PrintLine("set");
                        p.OpenScope();
                        {
                            p.PrintLine("switch (this.enumCase)");
                            p.OpenScope();
                            {
                                var length = structs.Length;
                                var last = length - 1;

                                for (var i = 0; i < last; i++)
                                {
                                    var structDef = structs[i];

                                    p.PrintBeginLine("case ").Print(enumCaseName).Print(".")
                                        .Print(structDef.identifier).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        WriteSetterCase(
                                              ref p
                                            , isReadOnly
                                            , def
                                            , isDim
                                            , structDef
                                            , enumCaseApiName
                                            , genericTypeArguments
                                        );
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    WriteSetterCase(
                                          ref p
                                        , isReadOnly
                                        , def
                                        , isDim
                                        , structs[last]
                                        , enumCaseApiName
                                        , genericTypeArguments
                                    );
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteGetterCase(
                  ref Printer p
                , bool isReadOnly
                , in IndexerDeclaration def
                , bool isDim
                , in StructSpec structDef
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .Print("var result_for_enum_case = ")
                    .Print(GetAnyRef(def.refKind))
                    .PrintIf(isDim, $"{enumCaseApiName}.Indexer_Get", "enum_case[");
                {
                    if (isDim)
                    {
                        PrintHelperTypeArguments(
                              ref p
                            , def.genericInterface
                            , genericTypeArguments
                            , structDef.name
                        );
                        p.Print("(ref enum_case, ");
                    }

                    WriteArguments(ref p, def.parameters);
                }
                p.PrintEndLineIf(isDim, ");", "];");
                p.PrintLineIf(isReadOnly == false && def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintBeginLine("return ").Print(GetAnyRef(def.refKind)).PrintEndLine("result_for_enum_case;");
            }

            static void WriteSetterCase(
                  ref Printer p
                , bool isReadOnly
                , in IndexerDeclaration def
                , bool isDim
                , in StructSpec structDef
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");

                if (isDim)
                {
                    p.PrintBeginLine(enumCaseApiName).Print(".Indexer_Set");
                    {
                        PrintHelperTypeArguments(
                              ref p
                            , def.genericInterface
                            , genericTypeArguments
                            , structDef.name
                        );
                        p.Print("(ref enum_case, value, ");
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine(");");
                }
                else
                {
                    p.PrintBeginLine("enum_case[");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.Print("]").PrintEndLine(" = value;");
                }

                p.PrintLineIf(isReadOnly == false && def.getter.isReadOnly == false, "this = enum_case;");
                p.PrintLine("return;");
            }
        }

        private readonly void WriteTryGetConstructionValueMethods(
              ref Printer p
            , in MergedStructRef mergedStructRef
            , string undefinedType
            , string enumCaseName = ENUM_CASE_NAME
        )
        {
            var typeToStructs = mergedStructRef.TypeToStructsMap;

            foreach (var kv in typeToStructs)
            {
                var type = kv.Key;
                var structs = kv.Value;

                p.PrintBeginLine("public readonly bool TryGetConstructionValue(out ")
                    .Print(type.name).PrintEndLine(" value, int index = default)");
                p.OpenScope();
                {
                    p.PrintLine("switch (this.enumCase)");
                    p.OpenScope();
                    {
                        foreach (var structId in structs)
                        {
                            p.PrintBeginLine("case ").Print(enumCaseName).Print(".")
                                .Print(structId.identifier).PrintEndLine(":");
                            p.OpenScope();
                            {
                                p.PrintBeginLine(structId.name).Print(" enum_case = (")
                                    .Print(structId.name).PrintEndLine(")this;");
                                p.PrintLine("return enum_case.TryGetConstructionValue(out value, index);");
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            p.PrintLine("value = default;");
                            p.PrintLine("return false;");
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteMergedMethods(
              ref Printer p
            , Dictionary<MethodDeclaration, bool> dimMap
            , List<MethodDeclaration> dimList
            , string enumCaseName = ENUM_CASE_NAME
            , string enumCaseApiName = "EnumCaseAPI"
        )
        {
            dimList.Clear();

            var isReadOnly = this.isReadOnly;
            var genericTypeArguments = GetGenericInterfaceTypeArguments();

            foreach (var kv in dimMap)
            {
                var def = kv.Key;
                var isDim = kv.Value;

                Write(
                      ref p
                    , isReadOnly
                    , def
                    , isDim
                    , structs
                    , enumCaseName
                    , enumCaseApiName
                    , genericTypeArguments
                );

                if (isDim)
                {
                    dimList.Add(def);
                }
            }

            return;

            static void Write(
                  ref Printer p
                , bool isReadOnly
                , in MethodDeclaration def
                , bool isDim
                , ReadOnlySpan<StructSpec> structs
                , string enumCaseName
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                p.PrintBeginLine("public ")
                    .PrintIf(def.isReadOnly, "readonly ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ").Print(def.name).Print("(");
                {
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                p.OpenScope();
                {
                    p.PrintLine("switch (this.enumCase)");
                    p.OpenScope();
                    {
                        var length = structs.Length;
                        var last = length - 1;

                        for (var i = 0; i < last; i++)
                        {
                            var structDef = structs[i];

                            p.PrintBeginLine("case ").Print(enumCaseName).Print(".")
                                .Print(structDef.identifier).PrintEndLine(":");
                            p.OpenScope();
                            {
                                WriteCase(
                                      ref p
                                    , isReadOnly
                                    , def
                                    , isDim
                                    , structDef
                                    , enumCaseApiName
                                    , genericTypeArguments
                                );
                            }
                            p.CloseScope();
                            p.PrintEndLine();
                        }

                        p.PrintLine("default:");
                        p.OpenScope();
                        {
                            WriteCase(
                                  ref p
                                , isReadOnly
                                , def
                                , isDim
                                , structs[last]
                                , enumCaseApiName
                                , genericTypeArguments
                            );
                        }
                        p.CloseScope();
                    }
                    p.CloseScope();
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteCase(
                  ref Printer p
                , bool isReadOnly
                , in MethodDeclaration def
                , bool isDim
                , in StructSpec structDef
                , string enumCaseApiName
                , string genericTypeArguments
            )
            {
                p.PrintBeginLine(structDef.name).Print(" enum_case = (").Print(structDef.name).PrintEndLine(")this;");
                p.PrintBeginLine()
                    .Print(GetReturnRefKind(def.refKind))
                    .PrintIf(def.returnsVoid == false, "var result_for_enum_case = ")
                    .Print(GetAnyRef(def.refKind))
                    .PrintIf(isDim, $"{enumCaseApiName}.", "enum_case.").Print(def.name);
                {
                    if (isDim)
                    {
                        PrintHelperTypeArguments(
                              ref p
                            , def.genericInterface
                            , genericTypeArguments
                            , structDef.name
                        );
                    }

                    p.Print("(").PrintIf(isDim && def.parameters.Count > 0, "ref enum_case, ")
                        .PrintIf(isDim && def.parameters.Count < 1, "ref enum_case");
                    WriteArguments(ref p, def.parameters);
                }
                p.PrintEndLine(");");
                p.PrintLineIf(isReadOnly == false && def.isReadOnly == false, "this = enum_case;");
                p.PrintBeginLine("return")
                    .Print(GetAnyRefSpacePrefix(def.refKind))
                    .PrintIf(def.returnsVoid == false, " result_for_enum_case")
                    .PrintEndLine(";");
            }
        }

        private readonly void WriteAdditionalMethods(
              ref Printer p
            , in MergedStructRef mergedStructRef
            , string enumCaseName = ENUM_CASE_NAME
        )
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public readonly ").Print(enumCaseName).PrintEndLine(" GetEnumCase()");
            p.OpenScope();
            {
                p.PrintLine("return enumCase;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public readonly ").Print(typeSelfName).Print(" To").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintLine("return this;");
            }
            p.CloseScope();
            p.PrintEndLine();

            if (autoEquatable == false)
            {
                return;
            }

            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("public readonly override bool Equals(object obj)");
            p.OpenScope();
            {
                p.PrintBeginLine("return obj is ").Print(typeSelfName).PrintEndLine(" other && Equals(other);");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("public readonly bool Equals(").Print(typeSelfName).PrintEndLine(" other)");
            p.OpenScope();
            {
                p.PrintLine("return");
                p = p.IncreasedIndent();

                var fieldRefs = mergedStructRef.FieldRefs;
                var count = fieldRefs.Count;

                if (count == 0)
                {
                    p.PrintLine("true");
                }

                for (var i = 0; i < count; i++)
                {
                    var fieldRef = fieldRefs[i];
                    var fieldName = fieldRef.Name;
                    var op = i > 0 ? "&& " : "   ";

                    if (fieldRef.Value.returnType.isEnum)
                    {
                        p.PrintBeginLine(op).Print(fieldName).Print(" == other.").Print(fieldName).PrintEndLine("");
                    }
                    else
                    {
                        p.PrintBeginLine(op)
                            .Print("global::System.Collections.Generic.EqualityComparer<")
                            .Print(fieldRef.Value.returnType.name)
                            .Print(">.Default.Equals(").Print(fieldName).Print(", other.")
                            .Print(fieldName).PrintEndLine(")");
                    }
                }

                p.PrintLine(";");
                p = p.DecreasedIndent();
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("public readonly override int GetHashCode()");
            p.OpenScope();
            {
                p.PrintLine("var hash = new g__ET.HashValue();");

                var fieldRefs = mergedStructRef.FieldRefs;
                var count = fieldRefs.Count;

                for (var i = 0; i < count; i++)
                {
                    var fieldName = fieldRefs[i].Name;
                    p.PrintBeginLine("hash.Add(").Print(fieldName).PrintEndLine(");");
                }

                p.PrintLine("return hash.ToHashCode();");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteCastableMethods(ref Printer p, string enumCaseName = ENUM_CASE_NAME)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("private static bool IsCastable(").Print(enumCaseName).Print(" a, ")
                .Print(enumCaseName).PrintEndLine(" b)");
            p.OpenScope();
            {
                p.PrintLine("return a == b;");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(VALIDATION_ATTRIBUTES);
            p.PrintBeginLine("private static void ThrowIfUncastable")
                .Print("(").Print(enumCaseName).Print(" source, ").Print(enumCaseName).PrintEndLine(" target)");
            p.OpenScope();
            {
                p.PrintLine("if (IsCastable(source, target) == false)");
                p.OpenScope();
                {
                    p.PrintLine("throw new g__S.InvalidCastException(");
                    p.WithIncreasedIndent().PrintBeginLine("$\"")
                        .Print("Cannot cast '").Print(typeName).Print("' into '{target}' ")
                        .Print("because it currently stores a '{source}'.")
                        .PrintEndLine("\"");
                    p.PrintLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteEnumCaseApiProperties(ref Printer p, List<PropertyDeclaration> defs)
        {
            var genericParameters = GetGenericInterfaceTypeParameters();
            var genericInterface = GetGenericInterfaceReference();
            var caseParameter = GetEnumCaseTypeParameterName();

            foreach (var def in defs)
            {
                WriteGetter(ref p, def, genericParameters, genericInterface, caseParameter);
                WriteSetter(ref p, def, genericParameters, genericInterface, caseParameter);
            }

            return;

            static void WriteGetter(
                  ref Printer p
                , in PropertyDeclaration def
                , string genericParameters
                , string genericInterface
                , string caseParameter
            )
            {
                if (def.getter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" Property_Get_").Print(def.name);
                PrintHelperDeclarationTypeParameters(ref p, def.genericInterface, genericParameters, caseParameter);
                p.Print("(ref ").Print(caseParameter).PrintEndLine(" enum_case)");
                WriteHelperConstraint(ref p, def.genericInterface, genericInterface, caseParameter);
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print(GetReturnRefKind(def.refKind))
                        .Print("return ")
                        .Print(GetAnyRef(def.refKind))
                        .Print("enum_case.").Print(def.name).PrintEndLine(";");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetter(
                  ref Printer p
                , in PropertyDeclaration def
                , string genericParameters
                , string genericInterface
                , string caseParameter
            )
            {
                if (def.setter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static void Property_Set_").Print(def.name);
                PrintHelperDeclarationTypeParameters(ref p, def.genericInterface, genericParameters, caseParameter);
                p.Print("(ref ").Print(caseParameter).Print(" enum_case, ")
                    .Print(def.returnType.name).Print(" setter_value")
                    .PrintEndLine(")");
                WriteHelperConstraint(ref p, def.genericInterface, genericInterface, caseParameter);
                p.OpenScope();
                {
                    p.PrintBeginLine("enum_case.").Print(def.name).PrintEndLine(" = setter_value;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteEnumCaseApiIndexers(ref Printer p, List<IndexerDeclaration> defs)
        {
            var genericParameters = GetGenericInterfaceTypeParameters();
            var genericInterface = GetGenericInterfaceReference();
            var caseParameter = GetEnumCaseTypeParameterName();

            foreach (var def in defs)
            {
                WriteGetter(ref p, def, genericParameters, genericInterface, caseParameter);
                WriteSetter(ref p, def, genericParameters, genericInterface, caseParameter);
            }

            return;

            static void WriteGetter(
                  ref Printer p
                , in IndexerDeclaration def
                , string genericParameters
                , string genericInterface
                , string caseParameter
            )
            {
                if (def.getter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" Indexer_Get");
                PrintHelperDeclarationTypeParameters(ref p, def.genericInterface, genericParameters, caseParameter);
                p.Print("(");
                {
                    p.Print("ref ").Print(caseParameter).Print(" enum_case, ");
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                WriteHelperConstraint(ref p, def.genericInterface, genericInterface, caseParameter);
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print(GetReturnRefKind(def.refKind))
                        .Print("return ")
                        .Print(GetAnyRef(def.refKind))
                        .Print("enum_case[");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine("];");
                }
                p.CloseScope();
                p.PrintEndLine();
            }

            static void WriteSetter(
                  ref Printer p
                , in IndexerDeclaration def
                , string genericParameters
                , string genericInterface
                , string caseParameter
            )
            {
                if (def.setter.IsValid == false)
                {
                    return;
                }

                p.PrintBeginLine("public static void Indexer_Set");
                PrintHelperDeclarationTypeParameters(ref p, def.genericInterface, genericParameters, caseParameter);
                p.Print("(");
                {
                    p.Print("ref ").Print(caseParameter).Print(" enum_case, ");
                    p.Print(def.returnType.name).Print(" setter_value, ");
                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                WriteHelperConstraint(ref p, def.genericInterface, genericInterface, caseParameter);
                p.OpenScope();
                {
                    p.PrintBeginLine().Print("enum_case[");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine("] = setter_value;");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteEnumCaseApiMethods(ref Printer p, List<MethodDeclaration> defs)
        {
            var genericParameters = GetGenericInterfaceTypeParameters();
            var genericInterface = GetGenericInterfaceReference();
            var caseParameter = GetEnumCaseTypeParameterName();

            foreach (var def in defs)
            {
                Write(ref p, def, genericParameters, genericInterface, caseParameter);
            }

            return;

            static void Write(
                  ref Printer p
                , in MethodDeclaration def
                , string genericParameters
                , string genericInterface
                , string caseParameter
            )
            {
                p.PrintBeginLine("public static ")
                    .Print(GetReturnRefKind(def.refKind))
                    .Print(def.returnType.name)
                    .Print(" ").Print(def.name);
                PrintHelperDeclarationTypeParameters(ref p, def.genericInterface, genericParameters, caseParameter);
                p.Print("(");
                {
                    p.Print("ref ").Print(caseParameter).Print(" enum_case");

                    if (def.parameters.Count > 0)
                    {
                        p.Print(", ");
                    }

                    WriteParameters(ref p, def.parameters);
                }
                p.PrintEndLine(")");
                WriteHelperConstraint(ref p, def.genericInterface, genericInterface, caseParameter);
                p.OpenScope();
                {
                    p.PrintBeginLine()
                        .Print(GetReturnRefKind(def.refKind))
                        .PrintIf(def.returnsVoid == false, "return ")
                        .Print(GetAnyRef(def.refKind))
                        .Print("enum_case.").Print(def.name).Print("(");
                    {
                        WriteArguments(ref p, def.parameters);
                    }
                    p.PrintEndLine(");");
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteCaseStructs(
              ref Printer p
            , ReadOnlySpan<StructSpec> defs
            , MergedStructRef mergedStructRef
            , bool autoEquatable
            , string typeName
            , string typeSelfName
        )
        {
            var defLength = defs.Length;
            var dedupFieldMap = new Dictionary<string, FieldSpec>();
            var fieldNames = new List<string>();

            for (var i = 0; i < defLength; i++)
            {
                ref readonly var def = ref defs[i];

                if (def.implicitlyDeclared)
                {
                    continue;
                }

                FillDedupFieldMap(def, dedupFieldMap, fieldNames);

                var structId = new StructId {
                    name = def.name,
                    identifier = def.identifier,
                };

                p.PrintBeginLine("partial ").PrintIf(def.isRecord, "record ").Print("struct ")
                    .Print(def.name).Print(" : IEnumCase");

                if (autoEquatable)
                {
                    p.Print(", IEquatable<").Print(def.name).Print(">");
                }

                p.PrintEndLine();
                p.OpenScope();
                {
                    WriteCaseStructConstructor(ref p, def, dedupFieldMap, fieldNames);
                    WriteCaseStructStorageMembers(ref p, def, dedupFieldMap, fieldNames);
                    WriteTryGetConstructionValueMethods(ref p, mergedStructRef, structId);

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("public readonly EnumCase GetEnumCase()");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return EnumCase.").Print(def.identifier).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly ").Print(typeSelfName)
                        .Print(" To").Print(typeName).PrintEndLine("()");
                    p.OpenScope();
                    {
                        p.PrintLine("return this;");
                    }
                    p.CloseScope();

                    if (autoEquatable)
                    {
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("public readonly override bool Equals(object obj)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return obj is ").Print(def.name).PrintEndLine(" other && Equals(other);");
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintBeginLine("public readonly bool Equals(").Print(def.name).PrintEndLine(" other)");
                        p.OpenScope();
                        {
                            p.PrintLine("return");
                            p = p.IncreasedIndent();

                            var fieldNameLength = fieldNames.Count;

                            if (fieldNameLength == 0)
                            {
                                p.PrintLine("true");
                            }

                            for (var k = 0; k < fieldNameLength; k++)
                            {
                                var fieldName = fieldNames[k];

                                if (dedupFieldMap.TryGetValue(fieldName, out var fieldDef) == false)
                                {
                                    continue;
                                }

                                var op = k > 0 ? "&& " : "   ";

                                if (fieldDef.returnType.isEnum)
                                {
                                    p.PrintBeginLine(op).Print(fieldName).Print(" == other.")
                                        .Print(fieldName).PrintEndLine("");
                                }
                                else
                                {
                                    p.PrintBeginLine(op)
                                        .Print("global::System.Collections.Generic.EqualityComparer<")
                                        .Print(fieldDef.returnType.name)
                                        .Print(">.Default.Equals(").Print(fieldName).Print(", other.")
                                        .Print(fieldName).PrintEndLine(")");
                                }
                            }

                            p.PrintLine(";");
                            p = p.DecreasedIndent();
                        }
                        p.CloseScope();
                        p.PrintEndLine();

                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("public readonly override int GetHashCode()");
                        p.OpenScope();
                        {
                            p.PrintLine("var hash = new g__ET.HashValue();");

                            var fieldNameLength = fieldNames.Count;

                            for (var k = 0; k < fieldNameLength; k++)
                            {
                                var fieldName = fieldNames[k];

                                if (dedupFieldMap.ContainsKey(fieldName) == false)
                                {
                                    continue;
                                }

                                p.PrintBeginLine("hash.Add(").Print(fieldName).PrintEndLine(");");
                            }

                            p.PrintLine("return hash.ToHashCode();");
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private readonly void WriteCaseStructs(
              ref Printer p
            , ReadOnlySpan<StructSpec> definitions
            , MergedStructRef mergedStructRef
            , bool autoEquatable
            , string typeName
            , in InterfaceSpec baseInterface
            , in InterfaceSpec genericInterface
            , bool writeContainerCases
        )
        {
            var dedupFieldMap = new Dictionary<string, FieldSpec>();
            var fieldNames = new List<string>();

            foreach (ref readonly var definition in definitions)
            {
                if (definition.implicitlyDeclared
                    || definition.ownerIsContainer != writeContainerCases
                )
                {
                    continue;
                }

                FillDedupFieldMap(definition, dedupFieldMap, fieldNames);
                var structId = new StructId {
                    name = definition.name,
                    identifier = definition.identifier,
                };
                var interfaceOwner = writeContainerCases ? string.Empty : $"{supportContainer.TypeName}.";
                p.PrintBeginLine("partial ").PrintIf(definition.isRecord, "record ").Print("struct ")
                    .Print(definition.declarationName).Print(" : ").Print(interfaceOwner).Print(baseInterface.name);

                if (genericInterface.IsValid)
                {
                    p.Print(", ").Print(interfaceOwner).Print(genericInterface.declarationName);
                }

                if (autoEquatable)
                {
                    p.Print(", g__S.IEquatable<").Print(definition.declarationName).Print(">");
                }

                p.PrintEndLine();
                p.OpenScope();
                {
                    WriteCaseStructConstructor(ref p, definition, dedupFieldMap, fieldNames);
                    WriteCaseStructStorageMembers(ref p, definition, dedupFieldMap, fieldNames);
                    WriteTryGetConstructionValueMethods(ref p, mergedStructRef, structId);
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly ").Print(interfaceOwner).PrintEndLine("EnumCase GetEnumCase()");
                    p.OpenScope();
                    {
                        p.PrintBeginLine("return ").Print(interfaceOwner).Print("EnumCase.")
                            .Print(definition.identifier).PrintEndLine(";");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintBeginLine("public readonly ").Print(definition.targetTypeName).Print(" To")
                        .Print(typeName).Print(definition.toMethodTypeParameters).PrintEndLine("()");
                    WriteConstraintLines(ref p, definition.toMethodConstraints);
                    p.OpenScope();
                    {
                        p.PrintLine("return this;");
                    }
                    p.CloseScope();

                    if (autoEquatable)
                    {
                        p.PrintEndLine();
                        p.PrintLine(AGGRESSIVE_INLINING);
                        p.PrintLine("public readonly override bool Equals(object obj)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("return obj is ").Print(definition.declarationName)
                                .PrintEndLine(" other && Equals(other);");
                        }
                        p.CloseScope();
                    }
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteEnumCaseExtensions(
              ref Printer p
            , bool referenceUnityCollections
            , in EnumCaseType enumCaseType
            , bool withEnumExtensions
            , bool parentIsNamespace
            , string structTypeName
            , string attributeOwner
            , Accessibility accessibility
            , CancellationToken token
        )
        {
            if (withEnumExtensions == false)
            {
                return;
            }

            var typeName = $"{structTypeName}_{ENUM_CASE_NAME}";
            var qualifiedName = $"{structTypeName}.{ENUM_CASE_NAME}";
            var extensionsName = EnumExtensionsDeclaration.GetNameExtensionsClass(typeName);
            var structName = EnumExtensionsDeclaration.GetNameExtendedStruct(typeName);

            p.PrintBeginLine("partial struct ").Print(structName).PrintEndLine(" // EnumCaseExtended");
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintBeginLine("static partial class ").Print(extensionsName).PrintEndLine(" // EnumCaseExtensions");
            p.OpenScope();
            {
                WriteHelperConstants(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();

            p.Print("#region ENUM CASE - ENUM EXTENSIONS").PrintEndLine();
            p.Print("#endregion ========================").PrintEndLine();
            p.PrintEndLine();

            var enumExtensions = new EnumExtensionsDeclaration(referenceUnityCollections, enumCaseType.MaxByteCount) {
                GeneratedCode = GENERATED_CODE,
                InterfaceGeneratedCode = GENERATED_CODE_LITERAL,
                ExcludeCoverage = EXCLUDE_COVERAGE,
                AggressiveInlining = AGGRESSIVE_INLINING,
                Name = typeName,
                ExtensionsName = extensionsName,
                StructName = structName,
                ParentIsNamespace = parentIsNamespace,
                FullyQualifiedName = qualifiedName,
                AttributeFullyQualifiedName = string.IsNullOrEmpty(attributeOwner)
                    ? string.Empty
                    : $"{attributeOwner}.{qualifiedName}",
                AttributeInterfaceName = string.IsNullOrEmpty(attributeOwner)
                    ? string.Empty
                    : $"{attributeOwner}.I{extensionsName}",
                AttributeExtensionsName = string.IsNullOrEmpty(attributeOwner)
                    ? string.Empty
                    : $"{attributeOwner}.{extensionsName}",
                AttributeStructName = string.IsNullOrEmpty(attributeOwner)
                    ? string.Empty
                    : $"{attributeOwner}.{structName}",
                UnderlyingTypeName = enumCaseType.UnderlyingType,
                Accessibility = accessibility,
                IsDisplayAttributeUsed = false,
                Members = enumCaseType.Members,
            };

            enumExtensions.WriteCode(ref p, token);
        }

        private static void WriteHelperConstants(ref Printer p)
        {
            p.PrintBeginLine("private const ").Print(METHOD_IMPL_OPTIONS)
                .Print(" INLINING = ").Print(INLINING).PrintEndLine(";");
            p.PrintEndLine();

            p.PrintBeginLine("private const string GENERATOR = ").Print(GENERATOR).PrintEndLine(";");
            p.PrintEndLine();
        }

        private readonly StructSpec GetUndefinedStruct()
        {
            foreach (var @struct in structs.AsReadOnlySpan())
            {
                if (@struct.isUndefined)
                {
                    return @struct;
                }
            }

            return default;
        }

        private readonly string GetGenericInterfaceTypeParameters()
        {
            var names = new List<string>();

            foreach (var index in genericInterfaceTargetIndices.AsReadOnlySpan())
            {
                names.Add(targetParameters[index].name);
            }

            return string.Join(", ", names);
        }

        private readonly string GetGenericInterfaceTypeArguments()
            => GetGenericInterfaceTypeParameters();

        private readonly string GetGenericInterfaceReference()
            => genericInterfaceDef.IsValid ? genericInterfaceDef.declarationName : interfaceDef.name;

        private readonly string GetEnumCaseTypeParameterName()
        {
            var names = new HashSet<string>(StringComparer.Ordinal);

            foreach (var parameter in targetParameters.AsReadOnlySpan())
            {
                names.Add(parameter.name);
            }
            var name = "TEnumCase";

            if (names.Contains(name) == false)
            {
                return name;
            }

            for (var index = 0; ; index++)
            {
                name = $"TEnumCase{index}";

                if (names.Contains(name) == false)
                {
                    return name;
                }
            }
        }

        private static void PrintGenericInterface(
              ref Printer p
            , string owner
            , in InterfaceSpec genericInterface
        )
        {
            if (genericInterface.IsValid)
            {
                p.Print(", ").Print(owner).Print(".").Print(genericInterface.declarationName);
            }
        }

        private static void FillDimCollections(in MergedStructRef merged, DimCollections result)
        {
            result.Properties.Clear();
            result.Indexers.Clear();
            result.Methods.Clear();

            foreach (var pair in merged.PropertyDimMap)
            {
                if (pair.Value)
                {
                    result.Properties.Add(pair.Key);
                }
            }

            foreach (var pair in merged.IndexerDimMap)
            {
                if (pair.Value)
                {
                    result.Indexers.Add(pair.Key);
                }
            }

            foreach (var pair in merged.MethodDimMap)
            {
                if (pair.Value)
                {
                    result.Methods.Add(pair.Key);
                }
            }
        }

        private static bool HasCommonValueProperty(in MergedStructRef merged)
        {
            foreach (var declaration in merged.PropertyDimMap.Keys)
            {
                if (string.Equals(declaration.name, "Value", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static void PrintHelperDeclarationTypeParameters(
              ref Printer p
            , bool genericInterface
            , string genericParameters
            , string caseParameter
        )
        {
            p.Print("<");

            if (genericInterface)
            {
                p.Print(genericParameters).Print(", ");
            }

            p.Print(caseParameter).Print(">");
        }

        private static void WriteHelperConstraint(
              ref Printer p
            , bool genericInterfaceMember
            , string genericInterface
            , string caseParameter
        )
        {
            p.WithIncreasedIndent().PrintBeginLine("where ").Print(caseParameter).Print(" : struct, ")
                .Print(genericInterfaceMember ? genericInterface : "IEnumCase")
                .PrintEndLine();
        }

        private static void WriteConstraintLines(ref Printer p, string constraints)
        {
            if (string.IsNullOrEmpty(constraints))
            {
                return;
            }

            foreach (var constraint in constraints.Split('\n'))
            {
                p.PrintLine(constraint);
            }
        }

        private static void WriteDefaultProperty(ref Printer p, in PropertyDeclaration definition)
        {
            var isRefReturn = IsReturnRefKind(definition.refKind);
            p.PrintBeginLine("public ").PrintIf(definition.IsReadOnly, "readonly ")
                .Print(GetReturnRefKind(definition.refKind)).Print(definition.returnType.name)
                .Print(" ").PrintEndLine(definition.name);
            p.OpenScope();
            {
                if (definition.IsWriteOnly == false)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        if (isRefReturn)
                        {
                            p.PrintLine("throw new g__S.InvalidOperationException(");
                            p.WithIncreasedIndent().PrintLine(
                                "\"Cannot return any reference from the default case.\""
                            );
                            p.PrintLine(");");
                        }
                        else
                        {
                            p.PrintLine("return default;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (definition.CanHaveSetter && definition.setter.IsValid)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set { }");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteDefaultIndexer(ref Printer p, in IndexerDeclaration definition)
        {
            var isRefReturn = IsReturnRefKind(definition.refKind);
            p.PrintBeginLine("public ").PrintIf(definition.IsReadOnly, "readonly ")
                .Print(GetReturnRefKind(definition.refKind)).Print(definition.returnType.name)
                .Print(" this[");
            WriteParameters(ref p, definition.parameters);
            p.PrintEndLine("]");
            p.OpenScope();
            {
                if (definition.IsWriteOnly == false)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("get");
                    p.OpenScope();
                    {
                        if (isRefReturn)
                        {
                            p.PrintLine("throw new g__S.InvalidOperationException(");
                            p.WithIncreasedIndent().PrintLine(
                                "\"Cannot return any reference from the default case.\""
                            );
                            p.PrintLine(");");
                        }
                        else
                        {
                            p.PrintLine("return default;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                if (definition.CanHaveSetter && definition.setter.IsValid)
                {
                    p.PrintLine(AGGRESSIVE_INLINING);
                    p.PrintLine("set { }");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteDefaultMethod(ref Printer p, in MethodDeclaration definition)
        {
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("public ").PrintIf(definition.isReadOnly, "readonly ")
                .Print(GetReturnRefKind(definition.refKind)).Print(definition.returnType.name)
                .Print(" ").Print(definition.name).Print("(");
            WriteParameters(ref p, definition.parameters);
            p.PrintEndLine(")");
            p.OpenScope();
            {
                foreach (var parameter in definition.parameters.AsReadOnlySpan())
                {
                    if (parameter.refKind is RefKind.Ref or RefKind.Out)
                    {
                        p.PrintBeginLine(parameter.name).PrintEndLine(" = default;");
                    }
                }

                if (definition.returnsVoid)
                {
                    p.PrintLine("return;");
                }
                else if (IsReturnRefKind(definition.refKind))
                {
                    p.PrintLine("throw new g__S.InvalidOperationException(");
                    p.WithIncreasedIndent().PrintLine(
                        "\"Cannot return any reference from the default case.\""
                    );
                    p.PrintLine(");");
                }
                else
                {
                    p.PrintLine("return default;");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void PrintHelperTypeArguments(
              ref Printer p
            , bool genericInterfaceMember
            , string genericTypeArguments
            , string caseTypeName
        )
        {
            p.Print("<");

            if (genericInterfaceMember)
            {
                p.Print(genericTypeArguments).Print(", ");
            }

            p.Print(caseTypeName).Print(">");
        }

        private static void WriteParameters(ref Printer p, ReadOnlySpan<SlimParameterSpec> defs, bool outToRef = false)
        {
            var lastIndex = defs.Length - 1;

            for (var i = 0; i < defs.Length; i++)
            {
                ref readonly var def = ref defs[i];

                p.Print(GetRefKind(def.refKind, outToRef)).Print(def.type.name).Print(" ").Print(def.name);

                if (i < lastIndex)
                {
                    p.Print(", ");
                }
            }
        }

        private static void WriteArguments(ref Printer p, ReadOnlySpan<SlimParameterSpec> defs)
        {
            var lastIndex = defs.Length - 1;

            for (var i = 0; i < defs.Length; i++)
            {
                ref readonly var def = ref defs[i];

                p.Print(GetRefKind(def.refKind)).Print(def.name);

                if (i < lastIndex)
                {
                    p.Print(", ");
                }
            }
        }

        private static void WriteCaseStructConstructor(
              ref Printer p
            , in StructSpec def
            , Dictionary<string, FieldSpec> dedupFieldMap
            , List<string> fieldNames
        )
        {
            if (dedupFieldMap.Count < 1)
            {
                return;
            }

            p.PrintLine(AGGRESSIVE_INLINING);
            var constructorName = def.declarationName;
            var genericIndex = constructorName.IndexOf('<');

            if (genericIndex >= 0)
            {
                constructorName = constructorName.Substring(0, genericIndex);
            }

            p.PrintBeginLine("public ").Print(constructorName).PrintEndLine("(");
            p = p.IncreasedIndent();
            {
                var fieldNameLength = fieldNames.Count;

                for (var i = 0; i < fieldNameLength; i++)
                {
                    var fieldName = fieldNames[i];

                    if (dedupFieldMap.TryGetValue(fieldName, out var fieldDef) == false)
                    {
                        continue;
                    }

                    p.PrintBeginLine()
                        .PrintIf(i > 0, ", ", "  ")
                        .Print("g__ET.Option<").Print(fieldDef.returnType.name).Print("> ")
                        .Print(fieldDef.name).PrintEndLine(" = default");
                }
            }
            p = p.DecreasedIndent();
            p.PrintBeginLine(")");

            if (def.parameters.Count > 0)
            {
                p.Print(" : this(");
                {
                    var paramDefs = def.parameters.AsReadOnlySpan();
                    var paramDefLength = paramDefs.Length;

                    for (var i = 0; i < paramDefLength; i++)
                    {
                        var paramType = paramDefs[i].field.returnType;
                        p.PrintIf(i > 0, ", ").Print("default(").Print(paramType.name).Print(")");
                    }
                }
                p.PrintEndLine(")");
            }
            else if (def.hasUnlistedStorage)
            {
                p.PrintEndLine(" : this()");
            }
            else
            {
                p.PrintEndLine();
            }

            p.OpenScope();
            {
                var fieldNameLength = fieldNames.Count;

                for (var i = 0; i < fieldNameLength; i++)
                {
                    var fieldName = fieldNames[i];

                    if (dedupFieldMap.ContainsKey(fieldName) == false)
                    {
                        continue;
                    }

                    p.PrintBeginLine("this.").Print(fieldName).Print(" = ")
                        .Print(fieldName).PrintEndLine(".GetValueOrDefault();");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteCaseStructStorageMembers(
              ref Printer p
            , in StructSpec def
            , Dictionary<string, FieldSpec> dedupFieldMap
            , List<string> fieldNames
        )
        {
            var hiddenFields = def.hiddenFields.AsReadOnlySpan();

            if (hiddenFields.Length < 1)
            {
                return;
            }

            var constructorName = def.declarationName;
            var genericIndex = constructorName.IndexOf('<');

            if (genericIndex >= 0)
            {
                constructorName = constructorName.Substring(0, genericIndex);
            }

            var fieldNameCount = fieldNames.Count;

            p.PrintLine(EDITOR_BROWSABLE_NEVER);
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintBeginLine("internal ").Print(constructorName).PrintEndLine("(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  g__ET.T<").Print(def.declarationName).PrintEndLine("> g__tag");

                for (var i = 0; i < fieldNameCount; i++)
                {
                    if (dedupFieldMap.TryGetValue(fieldNames[i], out var fieldDef) == false)
                    {
                        continue;
                    }

                    p.PrintBeginLine(", g__ET.Option<").Print(fieldDef.returnType.name).Print("> ")
                        .PrintEndLine(fieldDef.name);
                }

                foreach (ref readonly var hiddenField in hiddenFields)
                {
                    p.PrintBeginLine(", ").Print(hiddenField.returnType.name).Print(" ")
                        .PrintEndLine(hiddenField.name);
                }
            }
            p = p.DecreasedIndent();
            p.PrintBeginLine(") : this(");
            {
                var first = true;

                for (var i = 0; i < fieldNameCount; i++)
                {
                    var fieldName = fieldNames[i];

                    if (dedupFieldMap.ContainsKey(fieldName) == false)
                    {
                        continue;
                    }

                    p.PrintIf(first == false, ", ").Print(fieldName).Print(": ").Print(fieldName);
                    first = false;
                }
            }
            p.PrintEndLine(")");
            p.OpenScope();
            {
                foreach (ref readonly var hiddenField in hiddenFields)
                {
                    p.PrintBeginLine("this.").Print(hiddenField.name).Print(" = ")
                        .Print(hiddenField.name).PrintEndLine(";");
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine(EDITOR_BROWSABLE_NEVER);
            p.PrintLine(AGGRESSIVE_INLINING);
            p.PrintLine("internal readonly void CopyStorage(");
            p = p.IncreasedIndent();
            {
                p.PrintBeginLine("  g__ET.T<").Print(def.declarationName).PrintEndLine("> g__tag");

                foreach (ref readonly var hiddenField in hiddenFields)
                {
                    p.PrintBeginLine(", out ").Print(hiddenField.returnType.name).Print(" ")
                        .PrintEndLine(hiddenField.name);
                }
            }
            p = p.DecreasedIndent();
            p.PrintLine(")");
            p.OpenScope();
            {
                foreach (ref readonly var hiddenField in hiddenFields)
                {
                    p.PrintBeginLine(hiddenField.name).Print(" = this.").Print(hiddenField.name).PrintEndLine(";");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteTryGetConstructionValueMethods(
              ref Printer p
            , in MergedStructRef mergedStructRef
            , in StructId structId
        )
        {
            if (mergedStructRef.StructToValuesMap.TryGetValue(structId, out var typeToValues) == false)
            {
                typeToValues = new();
            }

            foreach (var kv in mergedStructRef.TypeToStructsMap)
            {
                if (typeToValues.ContainsKey(kv.Key) == false)
                {
                    typeToValues.Add(kv.Key, s_emptyValues);
                }
            }

            Write(ref p, typeToValues);

            return;

            static void Write(ref Printer p, Dictionary<TypeSpec, List<ConstructionValue>> typeToValues)
            {
                foreach (var kv in typeToValues)
                {
                    var type = kv.Key;
                    var values = kv.Value;
                    var valueCount = values.Count;

                    p.PrintBeginLine("public readonly bool TryGetConstructionValue(out ")
                        .Print(type.name).PrintEndLine(" value, int index = default)");
                    p.OpenScope();
                    {
                        if (valueCount > 1)
                        {
                            p.PrintLine("switch (index)");
                            p.OpenScope();
                            {
                                for (var orderIndex = 0; orderIndex < valueCount; orderIndex++)
                                {
                                    p.PrintBeginLine().Print(orderIndex).PrintEndLine(":");
                                    p.OpenScope();
                                    {
                                        p.PrintBeginLine("value = ")
                                            .PrintIf(type.isEnum, type.name)
                                            .PrintIf(type.isEnum, ".")
                                            .Print(values[orderIndex]).PrintEndLine(";");
                                        p.PrintLine("return true;");
                                    }
                                    p.CloseScope();
                                    p.PrintEndLine();
                                }

                                p.PrintLine("default:");
                                p.OpenScope();
                                {
                                    p.PrintLine("value = default;");
                                    p.PrintLine("return false;");
                                }
                                p.CloseScope();
                            }
                            p.CloseScope();
                        }
                        else if (valueCount == 1)
                        {
                            p.PrintBeginLine("value = ")
                                .PrintIf(type.isEnum, type.name)
                                .PrintIf(type.isEnum, ".")
                                .Print(values[0]).PrintEndLine(";");
                            p.PrintLine("return true;");
                        }
                        else
                        {
                            p.PrintLine("value = default;");
                            p.PrintLine("return false;");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private static void FillDedupFieldMap(
              in StructSpec def
            , Dictionary<string, FieldSpec> dedupFieldMap
            , List<string> fieldNames
        )
        {
            dedupFieldMap.Clear();
            fieldNames.Clear();

            var parameters = def.parameters.AsReadOnlySpan();
            var parameterLength = parameters.Length;

            for (var i = 0; i < parameterLength; i++)
            {
                ref readonly var paramDef = ref parameters[i];
                var fieldDef = paramDef.field;

                if (dedupFieldMap.ContainsKey(fieldDef.name) == false)
                {
                    fieldNames.Add(fieldDef.name);
                    dedupFieldMap.Add(fieldDef.name, fieldDef);
                }
            }

            var fieldDefs = def.fields.AsReadOnlySpan();
            var fieldDefLength = fieldDefs.Length;

            for (var i = 0; i < fieldDefLength; i++)
            {
                ref readonly var fieldDef = ref fieldDefs[i];

                if (dedupFieldMap.ContainsKey(fieldDef.name) == false)
                {
                    fieldNames.Add(fieldDef.name);
                    dedupFieldMap.Add(fieldDef.name, fieldDef);
                }
            }
        }

        private static bool IsReturnRefKind(in RefKind refKind)
        {
            return refKind is RefKind.Ref or RefKind.Out or RefKind.RefReadOnly;
        }

        private static string GetAnyRef(in RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.RefReadOnly => "ref ",
                _ => "",
            };
        }

        private static string GetAnyRefSpacePrefix(in RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => " ref",
                RefKind.RefReadOnly => " ref",
                _ => "",
            };
        }

        private static string GetReturnRefKind(in RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.RefReadOnly => "ref readonly ",
                _ => "",
            };
        }

        private static string GetRefKind(in RefKind refKind)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.Out => "out ",
                RefKind.In => "in ",
                _ => "",
            };
        }

        private static string GetRefKind(in RefKind refKind, bool outToRef)
        {
            return refKind switch {
                RefKind.Ref => "ref ",
                RefKind.Out => outToRef ? "ref " : "out ",
                RefKind.In => "in ",
                _ => "",
            };
        }
    }
}
