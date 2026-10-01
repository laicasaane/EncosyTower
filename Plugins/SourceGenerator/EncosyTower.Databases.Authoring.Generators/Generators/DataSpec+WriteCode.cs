using EncosyTower.SourceGen.Data.Helpers;
using static EncosyTower.Databases.Authoring.Generators.Helpers;

namespace EncosyTower.Databases.Authoring.Generators
{
    partial struct DataSpec
    {
        public readonly void WriteCode(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , EquatableArray<string> generatedKeyTypeFullNames
            , EquatableArray<GeneratedKeyEqualitySpec> generatedKeyEquality
            , string idTypeFullName
            , CancellationToken token
        )
        {
            token.ThrowIfCancellationRequested();
            var typeName = simpleName;
            var typeFullName = fullName;
            var isRowData = string.IsNullOrEmpty(idTypeFullName) == false;
            var isGeneratedKey = Contains(generatedKeyTypeFullNames, typeFullName);
            var equalityMode = GetEqualityMode(generatedKeyEquality, typeFullName);

            p.PrintLine(PR_SERIALIZABLE);

            if (isRowData)
            {
                p.PrintBeginLine("[g__ETDBASG.GeneratedSheetRow(typeof(").Print(idTypeFullName)
                    .Print("), typeof(").Print(typeFullName).PrintEndLine("))]");
            }
            else
            {
                p.PrintBeginLine("[g__ETDBASG.GeneratedDataRow(typeof(").Print(typeFullName).PrintEndLine("))]");
            }

            p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
            p.PrintBeginLine("public partial class __").Print(typeName);

            if (isRowData)
            {
                p.Print(" : g__CBS.SheetRow");

                if (dataMap.ContainsKey(idTypeFullName))
                {
                    p.Print("<__").Print(GetSimpleName(idTypeFullName)).Print(">");
                }
                else
                {
                    p.Print("<").Print(idTypeFullName).Print(">");
                }
            }

            if (isGeneratedKey && equalityMode != GeneratedKeyEqualityMode.Invalid)
            {
                p.Print(isRowData ? ", g__S.IEquatable<__" : " : g__S.IEquatable<__")
                    .Print(typeName).Print(">");
            }

            p.PrintEndLine();
            p.OpenScope();
            {
                p.PrintBeginLine("public static readonly __").Print(typeName)
                    .Print(" Default = new __").Print(typeName).PrintEndLine("();");
                p.PrintEndLine();

                WriteConstructor(ref p, dataMap, horizontalCollections, typeName);

                foreach (var baseType in baseTypeRefs)
                {
                    WriteProperties(
                          ref p
                        , dataMap
                        , horizontalCollections
                        , baseType.fullName
                        , isRowData
                        , baseType.propRefs
                    );

                    WriteProperties(
                          ref p
                        , dataMap
                        , horizontalCollections
                        , baseType.fullName
                        , isRowData
                        , baseType.fieldRefs
                    );
                }

                WriteProperties(ref p, dataMap, horizontalCollections, typeFullName, isRowData, propRefs);

                WriteProperties(ref p, dataMap, horizontalCollections, typeFullName, isRowData, fieldRefs);

                if (isGeneratedKey && equalityMode == GeneratedKeyEqualityMode.Generate)
                {
                    WriteEqualityMembers(ref p, dataMap, horizontalCollections, typeName);
                }

                WriteConvertMethod(ref p, dataMap);

                foreach (var baseType in baseTypeRefs)
                {
                    WriteToCollectionMethod(
                          ref p
                        , dataMap
                        , horizontalCollections
                        , baseType.fullName
                        , baseType.propRefs
                    );
                    WriteToCollectionMethod(
                          ref p
                        , dataMap
                        , horizontalCollections
                        , baseType.fullName
                        , baseType.fieldRefs
                    );
                }

                WriteToCollectionMethod(ref p, dataMap, horizontalCollections, typeFullName, propRefs);
                WriteToCollectionMethod(ref p, dataMap, horizontalCollections, typeFullName, fieldRefs);

                WriteSheetValueConverters(ref p);
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private readonly void WriteConstructor(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string typeName
        )
        {
            p.PrintBeginLine("public __").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                foreach (var baseType in baseTypeRefs)
                {
                    WriteCtorMembers(
                          ref p
                        , dataMap
                        , horizontalCollections
                        , baseType.propRefs
                        , baseType.fullName
                    );

                    WriteCtorMembers(
                          ref p
                        , dataMap
                        , horizontalCollections
                        , baseType.fieldRefs
                        , baseType.fullName
                    );
                }

                WriteCtorMembers(ref p, dataMap, horizontalCollections, propRefs, fullName);

                WriteCtorMembers(ref p, dataMap, horizontalCollections, fieldRefs, fullName);

                p.PrintEndLine();

                p.PrintLine("OnConstructor();");
            }
            p.CloseScope();
            p.PrintEndLine();

            p.PrintLine("partial void OnConstructor();");
            p.PrintEndLine();
        }

        private static void WriteCtorMembers(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , EquatableArray<MemberSpec> memberModels
            , string declaringTypeFullName
        )
        {
            foreach (var member in memberModels)
            {
                var manualAuthoring = member.manualAuthoring;

                if (manualAuthoring.defined)
                {
                    var type = manualAuthoring.type;
                    var collection = manualAuthoring.collection;

                    if (type.IsValid)
                    {
                        var newExpression = GetNewExpression(
                              collection
                            , type
                            , dataMap
                            , horizontalCollections
                            , declaringTypeFullName
                            , member.propertyName
                        );

                        p.PrintBeginLine("this.").Print(member.propertyName)
                            .Print(" = ").Print(newExpression).PrintEndLine(";");
                    }
                }

                {
                    var newExpression = GetNewExpression(
                          member.SelectCollection()
                        , member.SelectType()
                        , dataMap
                        , horizontalCollections
                        , declaringTypeFullName
                        , member.propertyName
                    );

                    p.PrintBeginLine("this.").Print(member.propertyName)
                        .PrintIf(member.manualAuthoring.defined, MemberManualAuthoring.SUFFIX)
                        .Print(" = ").Print(newExpression).PrintEndLine(";");
                }
            }
        }

        private static void WriteProperties(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string declaringTypeFullName
            , bool isRowData
            , EquatableArray<MemberSpec> memberModels
        )
        {
            foreach (var member in memberModels)
            {
                var isId = isRowData && member.propertyName == "Id";
                var isNotId = !isId;
                var manualAuthoring = member.manualAuthoring;

                if (isNotId && manualAuthoring.defined)
                {
                    var type = manualAuthoring.type;
                    var collection = manualAuthoring.collection;

                    if (type.IsValid)
                    {
                        var propertyTypeName = GetPropertyTypeName(
                                  collection
                                , type
                                , dataMap
                                , horizontalCollections
                                , declaringTypeFullName
                                , member.propertyName
                            );

                        p.PrintBeginLine("public ").Print(propertyTypeName).Print(" ").Print(member.propertyName)
                            .PrintEndLine(" { get; set; }");
                        p.PrintEndLine();
                    }
                }

                if (isId && manualAuthoring.defined == false)
                {
                    continue;
                }

                if (manualAuthoring.defined)
                {
                    p.PrintLine("[g__CBS.NonSerialized]");
                }
                else if (member.sheetConverter.kind != ConverterKind.None)
                {
                    var converter = member.sheetConverter;
                    var hash = HashValue64.FNV1a(converter.converterTypeFullName);

                    p.PrintBeginLine("[g__CBS.SheetValueConverter(typeof(__SheetValueConverter_")
                        .Print(converter.destType.simpleName).Print('_').Print(hash)
                        .PrintEndLine("))]");
                }

                {
                    var propertyTypeName = GetPropertyTypeName(
                          member.SelectCollection()
                        , member.SelectType()
                        , dataMap
                        , horizontalCollections
                        , declaringTypeFullName
                        , member.propertyName
                    );

                    p.PrintBeginLine("public ").Print(propertyTypeName).Print(" ").Print(member.propertyName)
                        .PrintIf(member.manualAuthoring.defined, MemberManualAuthoring.SUFFIX)
                        .PrintEndLine(" { get; set; }");
                    p.PrintEndLine();
                }
            }
        }

        private static string GetNewExpression(
              CollectionSpec collection
            , TypeSpec type
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string declaringTypeFullName
            , string propertyName
        )
        {
            if (collection.kind != CollectionKind.NotCollection)
            {
                var horizontal = IsHorizontal(horizontalCollections, declaringTypeFullName, propertyName);
                return $"new {GetAuthoringTypeName(collection, type, dataMap, horizontal)}()";
            }

            if (dataMap.ContainsKey(type.fullName))
            {
                return $"new __{type.simpleName}()";
            }

            if (type.isValueType)
            {
                return "default";
            }

            if (type.fullName == "string")
            {
                return "string.Empty";
            }

            return type.hasParameterlessConstructor ? $"new {type.fullName}()" : "default";
        }

        private static string GetPropertyTypeName(
              CollectionSpec collection
            , TypeSpec type
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string declaringTypeFullName
            , string propertyName
        )
        {
            var horizontal = IsHorizontal(horizontalCollections, declaringTypeFullName, propertyName);
            return GetAuthoringTypeName(collection, type, dataMap, horizontal);
        }

        private static string GetAuthoringTypeName(
              CollectionSpec collection
            , TypeSpec type
            , Dictionary<string, DataSpec> dataMap
            , bool horizontal
        )
        {
            if (collection.kind == CollectionKind.Dictionary)
            {
                var collectionTypeName = horizontal ? PR_DICTIONARY_T : PR_VERTICAL_DICTIONARY_T;
                var keyTypeName = GetAuthoringTypeName(
                      GetArgumentCollection(collection, 0)
                    , collection.keyType
                    , dataMap
                    , horizontal: false
                );
                var elementTypeName = GetAuthoringTypeName(
                      GetArgumentCollection(collection, 1)
                    , collection.elementType
                    , dataMap
                    , horizontal: false
                );
                return $"{collectionTypeName}<{keyTypeName}, {elementTypeName}>";
            }

            if (collection.kind != CollectionKind.NotCollection)
            {
                var collectionTypeName = horizontal ? PR_LIST_T : PR_VERTICAL_LIST_T;
                var elementTypeName = GetAuthoringTypeName(
                      GetArgumentCollection(collection, 0)
                    , collection.elementType
                    , dataMap
                    , horizontal: false
                );
                return $"{collectionTypeName}<{elementTypeName}>";
            }

            return dataMap.ContainsKey(type.fullName) ? $"__{type.simpleName}" : type.fullName;
        }

        private static bool IsHorizontal(
              EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string declaringTypeFullName
            , string propertyName
        )
        {
            foreach (var entry in horizontalCollections)
            {
                if (string.Equals(entry.targetTypeFullName, declaringTypeFullName, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                foreach (var candidate in entry.propertyNames)
                {
                    if (string.Equals(candidate, propertyName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool Contains(EquatableArray<string> values, string value)
        {
            foreach (var candidate in values)
            {
                if (string.Equals(candidate, value, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private static GeneratedKeyEqualityMode GetEqualityMode(
              EquatableArray<GeneratedKeyEqualitySpec> customizations
            , string typeFullName
        )
        {
            foreach (var customization in customizations)
            {
                if (string.Equals(customization.typeFullName, typeFullName, StringComparison.Ordinal))
                {
                    return customization.mode;
                }
            }

            return GeneratedKeyEqualityMode.Generate;
        }

        private static CollectionSpec GetArgumentCollection(CollectionSpec collection, int index)
            => index < collection.argumentCollections.Count ? collection.argumentCollections[index] : default;

        private readonly void WriteEqualityMembers(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string typeName
        )
        {
            var members = new List<EqualityMember>();
            var names = new HashSet<string>(StringComparer.Ordinal);

            foreach (var baseType in baseTypeRefs)
            {
                AddMembers(baseType.fullName, baseType.propRefs);
                AddMembers(baseType.fullName, baseType.fieldRefs);
            }

            AddMembers(fullName, propRefs);
            AddMembers(fullName, fieldRefs);

            p.PrintBeginLine("public bool Equals(__").Print(typeName).PrintEndLine(" other)");
            p.WithIncreasedIndent().PrintLine("=> other != null");

            for (var i = 0; i < members.Count; i++)
            {
                var equalityMember = members[i];
                var member = equalityMember.member;
                var propertyName = member.propertyName;
                var collection = member.manualAuthoring.defined
                    ? member.manualAuthoring.collection
                    : member.SelectCollection();
                var type = member.manualAuthoring.defined
                    ? member.manualAuthoring.type
                    : member.SelectType();

                if (collection.kind == CollectionKind.NotCollection
                    && string.Equals(type.fullName, "string", StringComparison.Ordinal)
                )
                {
                    p.WithIncreasedIndent().PrintBeginLine("&& g__S.String.Equals(this.").Print(propertyName)
                        .Print(", other.").Print(propertyName).PrintEndLine(", g__S.StringComparison.Ordinal)");
                }
                else
                {
                    var propertyTypeName = GetPropertyTypeName(
                          collection
                        , type
                        , dataMap
                        , horizontalCollections
                        , equalityMember.declaringTypeFullName
                        , propertyName
                    );
                    p.WithIncreasedIndent().PrintBeginLine("&& g__SCG.EqualityComparer<")
                        .Print(propertyTypeName).Print(">.Default.Equals(this.").Print(propertyName)
                        .Print(", other.").Print(propertyName).PrintEndLine(")");
                }
            }

            p.WithIncreasedIndent().PrintLine(";");
            p.PrintEndLine();

            p.PrintLine("public override bool Equals(object obj)");
            p.WithIncreasedIndent().PrintBeginLine("=> obj is __").Print(typeName)
                .PrintEndLine(" other && Equals(other);");
            p.PrintEndLine();

            p.PrintLine("public override int GetHashCode()");

            if (members.Count < 1)
            {
                p.WithIncreasedIndent().PrintLine("=> 0;");
            }
            else if (members.Count <= 8)
            {
                p.WithIncreasedIndent().PrintBeginLine("=> g__S.HashCode.Combine(");
                PrintHashArguments(ref p, members, 0, members.Count, prefixComma: false, closeExpression: true);
            }
            else
            {
                p.OpenScope();
                {
                    p.PrintBeginLine("var hash = g__S.HashCode.Combine(");
                    PrintHashArguments(ref p, members, 0, 8, prefixComma: false, closeExpression: true);

                    var index = 8;

                    while (index < members.Count)
                    {
                        var count = Math.Min(7, members.Count - index);
                        p.PrintBeginLine("hash = g__S.HashCode.Combine(hash");
                        PrintHashArguments(ref p, members, index, count, prefixComma: true, closeExpression: false);
                        p.PrintEndLine(");");
                        index += count;
                    }

                    p.PrintEndLine();
                    p.PrintLine("return hash;");
                }
                p.CloseScope();
            }

            p.PrintEndLine();

            void AddMembers(string declaringTypeFullName, EquatableArray<MemberSpec> source)
            {
                foreach (var member in source)
                {
                    var propertyName = member.propertyName;

                    if (names.Add(propertyName))
                    {
                        members.Add(new EqualityMember {
                            declaringTypeFullName = declaringTypeFullName,
                            member = member,
                        });
                    }
                }
            }

                static void PrintHashArguments(
                  ref Printer p
                , List<EqualityMember> source
                , int start
                , int count
                , bool prefixComma
                , bool closeExpression
            )
            {
                for (var i = 0; i < count; i++)
                {
                    p.Print(i == 0 && prefixComma == false ? string.Empty : ", ").Print("this.")
                        .Print(source[start + i].member.propertyName);
                }

                if (closeExpression)
                {
                    p.PrintEndLine(");");
                }
            }
        }

        private readonly void WriteSheetValueConverters(ref Printer p)
        {
            var hashes = new HashSet<ulong>();

            foreach (var baseType in baseTypeRefs)
            {
                WriteSheetValueConverter(ref p, baseType.propRefs, hashes);
                WriteSheetValueConverter(ref p, baseType.fieldRefs, hashes);
            }

            WriteSheetValueConverter(ref p, propRefs, hashes);
            WriteSheetValueConverter(ref p, fieldRefs, hashes);

            static void WriteSheetValueConverter(
                  ref Printer p
                , EquatableArray<MemberSpec> members
                , HashSet<ulong> hashes
            )
            {
                foreach (var member in members)
                {
                    if (member.manualAuthoring.defined)
                    {
                        continue;
                    }

                    var converter = member.sheetConverter;

                    if (converter.kind == ConverterKind.None)
                    {
                        continue;
                    }

                    var hash = HashValue64.FNV1a(converter.converterTypeFullName);

                    if (hashes.Add(hash) == false)
                    {
                        continue;
                    }

                    var targetType = member.SelectType().fullName;

                    p.PrintBeginLine(PR_EXCLUDE_COVERAGE).PrintEndLine(PR_GENERATED_CODE);
                    p.PrintBeginLine("public sealed class ").Print("__SheetValueConverter_")
                        .Print(converter.destType.simpleName).Print('_').Print(hash)
                        .Print(" : g__CBS.SheetValueConverter<").Print(targetType).PrintEndLine(">");
                    p.OpenScope();
                    {
                        p.PrintLine(PR_AGGRESSIVE_INLINING);
                        p.PrintBeginLine("protected override ").Print(targetType)
                            .PrintEndLine(" StringToValue(g__S.Type type, string value, g__CBS.SheetValueConvertingContext context)");
                        p.WithIncreasedIndent()
                            .PrintBeginLine("=> ").Print(converter.Convert("value")).PrintEndLine(";");
                        p.PrintEndLine();

                        p.PrintLine(PR_AGGRESSIVE_INLINING);
                        p.PrintBeginLine("protected override string ValueToString(g__S.Type type, ")
                            .Print(targetType).PrintEndLine(" value, g__CBS.SheetValueConvertingContext context)");
                        p.WithIncreasedIndent()
                            .PrintBeginLine("=> g__S.Convert.ToString(value, context.FormatProvider) ?? string.Empty;").PrintEndLine();
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }
            }
        }

        private readonly void WriteConvertMethod(ref Printer p, Dictionary<string, DataSpec> dataMap)
        {
            var typeName = simpleName;
            var typeFullName = fullName;
            var isValueType = this.isValueType;

            p.PrintBeginLine("public ").Print(typeFullName).Print(" To").Print(typeName).PrintEndLine("()");
            p.OpenScope();
            {
                p.PrintBeginLine("var result = new ").Print(typeFullName).PrintEndLine("();");
                p.PrintEndLine();

                foreach (var baseType in baseTypeRefs)
                {
                    p.OpenScope();
                    {
                        p.PrintBeginLine("var resultBase = result as ").Print(baseType.fullName).PrintEndLine(";");

                        WriteConvertType(
                              ref p
                            , baseType.fullName
                            , baseType.isValueType
                            , "resultBase"
                            , dataMap
                            , baseType.validIdentifier
                            , baseType.propRefs
                            , baseType.fieldRefs
                        );
                    }
                    p.CloseScope();
                    p.PrintEndLine();
                }

                WriteConvertType(
                      ref p
                    , typeFullName
                    , isValueType
                    , "result"
                    , dataMap
                    , validIdentifier
                    , propRefs
                    , fieldRefs
                );

                p.PrintEndLine();

                p.PrintLine("return result;");
            }
            p.CloseScope();
            p.PrintEndLine();
        }

        private static void WriteConvertType(
              ref Printer p
            , string typeFullName
            , bool isValueType
            , string resultName
            , Dictionary<string, DataSpec> dataMap
            , string typeValidIdentifier
            , EquatableArray<MemberSpec> props
            , EquatableArray<MemberSpec> fields
        )
        {
            foreach (var member in fields)
            {
                WriteConvertMember(ref p, typeFullName, isValueType, resultName, dataMap, typeValidIdentifier, member);
            }

            foreach (var member in props)
            {
                WriteConvertMember(ref p, typeFullName, isValueType, resultName, dataMap, typeValidIdentifier, member);
            }
        }

        private static void WriteConvertMember(
              ref Printer p
            , string typeFullName
            , bool isValueType
            , string resultName
            , Dictionary<string, DataSpec> dataMap
            , string typeValidIdentifier
            , MemberSpec member
        )
        {
            var type = member.SelectType();
            var coll = member.SelectCollection();
            var memberPropName = GetMemberPropName(member);
            string expression;

            if (coll.kind != CollectionKind.NotCollection)
            {
                if (RequiresCollectionConversion(coll, dataMap))
                {
                    var methodName = GetToCollectionMethodName(
                          memberPropName
                        , coll.elementType.simpleName
                        , coll.kind.ToString()
                    );
                    expression = member.converter.Convert($"this.{methodName}()");
                }
                else
                {
                    expression = member.converter.Convert(
                        GetSimpleCollectionConversionExpression(memberPropName, coll)
                    );
                }
            }
            else
            {
                if (dataMap.ContainsKey(type.fullName))
                {
                    expression = member.converter.Convert(
                        $"(this.{memberPropName} ?? __{type.simpleName}.Default).To{type.simpleName}()"
                    );
                }
                else if (type.isValueType)
                {
                    expression = member.converter.Convert($"this.{memberPropName}");
                }
                else if (type.fullName == "string")
                {
                    expression = member.converter.Convert($"this.{memberPropName} ?? string.Empty");
                }
                else
                {
                    var newSuffix = type.hasParameterlessConstructor ? $" ?? new {type.fullName}()" : " ?? default";

                    expression = member.converter.Convert($"this.{memberPropName}{newSuffix}");
                }
            }

            p.PrintBeginLine(typeFullName).Print(".").Print(typeValidIdentifier).Print("_ValueSetter.Set_")
                .Print(member.propertyName).Print("(").PrintIf(isValueType, "ref ").Print(resultName)
                .Print(", ").Print(expression).PrintEndLine(");");
        }

        private static void WriteToCollectionMethod(
              ref Printer p
            , Dictionary<string, DataSpec> dataMap
            , EquatableArray<HorizontalCollectionSpec> horizontalCollections
            , string declaringTypeFullName
            , EquatableArray<MemberSpec> memberModels
        )
        {
            foreach (var member in memberModels)
            {
                var coll = member.SelectCollection();

                if (coll.kind == CollectionKind.NotCollection)
                {
                    continue;
                }

                if (RequiresCollectionConversion(coll, dataMap) == false)
                {
                    continue;
                }

                var memberPropName = GetMemberPropName(member);
                var methodName = GetToCollectionMethodName(
                      memberPropName
                    , coll.elementType.simpleName
                    , coll.kind.ToString()
                );
                var horizontal = IsHorizontal(horizontalCollections, declaringTypeFullName, member.propertyName);

                p.PrintBeginLine("private ").Print(GetRuntimeCollectionType(coll)).Print(" ")
                    .Print(methodName).PrintEndLine("()");
                p.OpenScope();
                {
                    var converterName = $"Convert{member.propertyName}_Root";
                    p.PrintBeginLine("return ").Print(converterName).Print("(this.")
                        .Print(memberPropName).PrintEndLine(");");
                    p.PrintEndLine();

                    WriteCollectionLocalFunction(
                          ref p
                        , converterName
                        , member.SelectType()
                        , coll
                        , dataMap
                        , horizontal
                    );
                }
                p.CloseScope();
                p.PrintEndLine();
            }
        }

        private static void WriteCollectionLocalFunction(
              ref Printer p
            , string functionName
            , TypeSpec runtimeType
            , CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
            , bool horizontal
        )
        {
            var authoringTypeName = GetAuthoringTypeName(collection, runtimeType, dataMap, horizontal);

            p.PrintBeginLine("static ").Print(GetRuntimeCollectionType(collection)).Print(" ").Print(functionName)
                .Print("(").Print(authoringTypeName).PrintEndLine(" source)");
            p.OpenScope();
            {
                WriteEmptyCollectionReturn(ref p, collection);
                p.PrintEndLine();

                if (collection.kind == CollectionKind.Dictionary)
                {
                    WriteDictionaryConversion(ref p, functionName, collection, dataMap);
                }
                else
                {
                    WriteSequenceConversion(ref p, functionName, collection, dataMap);
                }
            }
            p.CloseScope();
            p.PrintEndLine();

            if (collection.kind == CollectionKind.Dictionary)
            {
                WriteChildCollectionFunction(
                      ref p
                    , $"{functionName}_Key"
                    , collection.keyType
                    , GetArgumentCollection(collection, 0)
                    , dataMap
                );
                WriteChildCollectionFunction(
                      ref p
                    , $"{functionName}_Value"
                    , collection.elementType
                    , GetArgumentCollection(collection, 1)
                    , dataMap
                );
            }
            else
            {
                WriteChildCollectionFunction(
                      ref p
                    , $"{functionName}_Item"
                    , collection.elementType
                    , GetArgumentCollection(collection, 0)
                    , dataMap
                );
            }
        }

        private static void WriteChildCollectionFunction(
              ref Printer p
            , string functionName
            , TypeSpec runtimeType
            , CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (collection.kind != CollectionKind.NotCollection)
            {
                WriteCollectionLocalFunction(ref p, functionName, runtimeType, collection, dataMap, horizontal: false);
            }
        }

        private static void WriteEmptyCollectionReturn(ref Printer p, CollectionSpec collection)
        {
            p.PrintLine("if (source == null || source.Count == 0)");
            p.OpenScope();
            {
                p.PrintBeginLine("return ").Print(GetEmptyRuntimeCollectionExpression(collection)).PrintEndLine(";");
            }
            p.CloseScope();
        }

        private static void WriteDictionaryConversion(
              ref Printer p
            , string functionName
            , CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
        )
        {
            var keyTypeName = collection.keyType.fullName;
            var elementTypeName = collection.elementType.fullName;

            p.PrintBeginLine("var result = new ").Print(PR_DICTIONARY_T).Print("<").Print(keyTypeName)
                .Print(", ").Print(elementTypeName).PrintEndLine(">(source.Count);");
            p.PrintEndLine();
            p.PrintLine("foreach (var pair in source)");
            p.OpenScope();
            {
                p.PrintBeginLine("var key = ").Print(GetConvertedValueExpression(
                      "pair.Key"
                    , $"{functionName}_Key"
                    , collection.keyType
                    , GetArgumentCollection(collection, 0)
                    , dataMap
                )).PrintEndLine(";");
                p.PrintBeginLine("var value = ").Print(GetConvertedValueExpression(
                      "pair.Value"
                    , $"{functionName}_Value"
                    , collection.elementType
                    , GetArgumentCollection(collection, 1)
                    , dataMap
                )).PrintEndLine(";");
                p.PrintEndLine();
                p.PrintLine("result[key] = value;");
            }
            p.CloseScope();
            p.PrintEndLine();
            p.PrintLine("return result;");
        }

        private static void WriteSequenceConversion(
              ref Printer p
            , string functionName
            , CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
        )
        {
            var elementTypeName = collection.elementType.fullName;
            var childCollection = GetArgumentCollection(collection, 0);
            var convertedItem = GetConvertedValueExpression(
                  "source[i]"
                , $"{functionName}_Item"
                , collection.elementType
                , childCollection
                , dataMap
            );

            if (collection.kind == CollectionKind.Array)
            {
                p.PrintBeginLine("var result = new ").Print(elementTypeName).PrintEndLine("[source.Count];");
            }
            else
            {
                p.PrintBeginLine("var result = new ").Print(GetConcreteRuntimeCollectionType(collection))
                    .PrintEndLine("(source.Count);");
            }

            p.PrintEndLine();
            p.PrintLine("for (var i = 0; i < source.Count; i++)");
            p.OpenScope();
            {
                if (collection.kind == CollectionKind.Array)
                {
                    p.PrintBeginLine("result[i] = ").Print(convertedItem).PrintEndLine(";");
                }
                else
                {
                    var methodName = collection.kind switch {
                        CollectionKind.Queue => "Enqueue",
                        CollectionKind.Stack => "Push",
                        _ => "Add",
                    };

                    p.PrintBeginLine("result.").Print(methodName).Print("(").Print(convertedItem).PrintEndLine(");");
                }
            }
            p.CloseScope();
            p.PrintEndLine();
            p.PrintLine("return result;");
        }

        private static string GetConvertedValueExpression(
              string expression
            , string childFunctionName
            , TypeSpec runtimeType
            , CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (collection.kind != CollectionKind.NotCollection)
            {
                return $"{childFunctionName}({expression})";
            }

            if (dataMap.ContainsKey(runtimeType.fullName))
            {
                return $"({expression} ?? __{runtimeType.simpleName}.Default).To{runtimeType.simpleName}()";
            }

            if (runtimeType.fullName == "string")
            {
                return $"{expression} ?? string.Empty";
            }

            if (runtimeType.isValueType)
            {
                return expression;
            }

            return runtimeType.hasParameterlessConstructor
                ? $"{expression} ?? new {runtimeType.fullName}()"
                : $"{expression} ?? default";
        }

        private static string GetEmptyRuntimeCollectionExpression(CollectionSpec collection)
        {
            if (collection.kind == CollectionKind.Array)
            {
                return $"new {collection.elementType.fullName}[0]";
            }

            return $"new {GetConcreteRuntimeCollectionType(collection)}()";
        }

        private static string GetConcreteRuntimeCollectionType(CollectionSpec collection)
            => collection.kind switch {
                CollectionKind.Dictionary =>
                    $"{PR_DICTIONARY_T}<{collection.keyType.fullName}, {collection.elementType.fullName}>",
                CollectionKind.HashSet => $"{PR_HASH_SET_T}<{collection.elementType.fullName}>",
                CollectionKind.Queue => $"{PR_QUEUE_T}<{collection.elementType.fullName}>",
                CollectionKind.Stack => $"{PR_STACK_T}<{collection.elementType.fullName}>",
                _ => $"{PR_LIST_T}<{collection.elementType.fullName}>",
            };

        private static string GetRuntimeCollectionType(CollectionSpec collection)
            => collection.kind == CollectionKind.Array
                ? $"{collection.elementType.fullName}[]"
                : GetConcreteRuntimeCollectionType(collection);

        private static bool RequiresCollectionConversion(
              CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
        )
        {
            if (collection.kind == CollectionKind.NotCollection)
            {
                return false;
            }

            if (collection.kind == CollectionKind.Dictionary)
            {
                return RequiresArgumentConversion(
                      collection.keyType
                    , GetArgumentCollection(collection, 0)
                    , dataMap
                ) || RequiresArgumentConversion(
                      collection.elementType
                    , GetArgumentCollection(collection, 1)
                    , dataMap
                );
            }

            return RequiresArgumentConversion(
                  collection.elementType
                , GetArgumentCollection(collection, 0)
                , dataMap
            );
        }

        private static bool RequiresArgumentConversion(
              TypeSpec type
            , CollectionSpec collection
            , Dictionary<string, DataSpec> dataMap
        )
            => collection.kind != CollectionKind.NotCollection || dataMap.ContainsKey(type.fullName);

        private static string GetSimpleCollectionConversionExpression(
              string memberPropName
            , CollectionSpec collection
        )
            => collection.kind switch {
                CollectionKind.Array => $"this.{memberPropName}?.ToArray() ?? new {collection.elementType.fullName}[0]",
                CollectionKind.Dictionary =>
                    $"this.{memberPropName} ?? new {PR_DICTIONARY_T}<" +
                    $"{collection.keyType.fullName}, {collection.elementType.fullName}>()",
                CollectionKind.HashSet =>
                    $"this.{memberPropName} == null ? new {PR_HASH_SET_T}<{collection.elementType.fullName}>() : " +
                    $"new {PR_HASH_SET_T}<{collection.elementType.fullName}>(this.{memberPropName})",
                CollectionKind.Queue =>
                    $"this.{memberPropName} == null ? new {PR_QUEUE_T}<{collection.elementType.fullName}>() : " +
                    $"new {PR_QUEUE_T}<{collection.elementType.fullName}>(this.{memberPropName})",
                CollectionKind.Stack =>
                    $"this.{memberPropName} == null ? new {PR_STACK_T}<{collection.elementType.fullName}>() : " +
                    $"new {PR_STACK_T}<{collection.elementType.fullName}>(this.{memberPropName})",
                _ => $"this.{memberPropName} ?? new {PR_LIST_T}<{collection.elementType.fullName}>()",
            };

        private static string GetToCollectionMethodName(string propertyName, string elemSimpleName, string collectionName)
            => $"To{elemSimpleName}{collectionName}For{propertyName}";

        private static string GetSimpleName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return fullName;
            }

            var dot = fullName.LastIndexOf('.');
            return dot >= 0 ? fullName.Substring(dot + 1) : fullName;
        }

        private static string GetMemberPropName(MemberSpec member)
        {
            var result = member.propertyName;

            if (member.manualAuthoring.defined)
            {
                result = $"{result}{MemberManualAuthoring.SUFFIX}";
            }

            return result;
        }

        private struct EqualityMember
        {
            public string declaringTypeFullName;
            public MemberSpec member;
        }
    }
}
