using static EncosyTower.Databases.Authoring.Generators.Helpers;

namespace EncosyTower.Databases.Authoring.Generators
{
    partial struct DatabaseSpec
    {
        public readonly string WriteSheet(in SheetSpec sheet, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var stringComparer = StringComparer.Ordinal;
            var databaseTypeName = this.databaseTypeName;
            var databaseTypeKeyword = this.databaseTypeKeyword;
            var converterMap = new Dictionary<string, ScopedConverterSpec>(stringComparer);

            foreach (var scopedConverter in scopedConverters)
            {
                token.ThrowIfCancellationRequested();

                if (scopedConverter.scopeKey == sheet.scopeKey)
                {
                    var key = ScopedConverterSpec.MakeMemberKey(
                          scopedConverter.declaringDataTypeFullName
                        , scopedConverter.propertyName
                    );

                    converterMap[key] = scopedConverter;
                }
            }

            var dataMap = new Dictionary<string, DataSpec>(allDataModels.Count, stringComparer);

            foreach (var dm in allDataModels)
            {
                token.ThrowIfCancellationRequested();

                dataMap[dm.fullName] = ApplyScopedConverters(dm, converterMap);
            }

            var sheetName = sheet.sheetName;
            var idTypeFullName = sheet.idTypeFullName;
            var idTypeSimpleName = sheet.idTypeSimpleName;
            var dataTypeFullName = sheet.dataTypeFullName;
            var dataTypeSimpleName = sheet.dataTypeSimpleName;
            var hasIdTypeDeclaration = dataMap.ContainsKey(idTypeFullName);
            var sheetIdTypeName = hasIdTypeDeclaration ? $"{sheetName}.__{idTypeSimpleName}" : idTypeFullName;
            var sheetDataTypeName = $"{sheetName}.__{dataTypeSimpleName}";

            var p = new Printer(0, 1024 * 16, token);
            p = p.IncreasedIndent();
            p.PrintEndLine();

            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print("partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseTypeName)
                .Print(" : ").Print(PR_ICONTAINS).Print("<").Print(databaseTypeName).Print(".").Print(sheetName).Print(">")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine()
                    .Print("public abstract partial class ").Print(sheetName)
                    .Print(" : g__CBS.Sheet<").Print(sheetIdTypeName).Print(", ").Print(sheetDataTypeName).Print(">")
                    .Print(", g__ETDBA.IDataSheet, g__ETDBA.IToDataArray<").Print(dataTypeFullName).Print(">")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IDataSheet.Preprocess(g__CBS.SheetConvertingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("public void Preprocess(g__CBS.SheetConvertingContext context)");
                    p.OpenScope();
                    {
                        p.PrintLine("OnPreprocess(context);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IDataSheet.Preprocess(g__CBS.SheetConvertingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("partial void OnPreprocess(g__CBS.SheetConvertingContext context);");
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IDataSheet.Process(g__CBS.SheetConvertingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("public void Process(g__CBS.SheetConvertingContext context)");
                    p.OpenScope();
                    {
                        p.PrintLine("OnProcess(context);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IDataSheet.Process(g__CBS.SheetConvertingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("partial void OnProcess(g__CBS.SheetConvertingContext context);");
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IDataSheet.Postprocess(g__CBS.SheetConvertingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("public void Postprocess(g__CBS.SheetConvertingContext context)");
                    p.OpenScope();
                    {
                        p.PrintLine("OnPostprocess(context);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IDataSheet.Postprocess(g__CBS.SheetConvertingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("partial void OnPostprocess(g__CBS.SheetConvertingContext context);");
                    p.PrintEndLine();

                    p.PrintBeginLine("public ").Print(dataTypeFullName).PrintEndLine("[] ToDataArray()");
                    p.OpenScope();
                    {
                        p.PrintLine("if (this.Items == null || this.Count == 0)");
                        p.WithIncreasedIndent()
                            .PrintBeginLine("return new ").Print(dataTypeFullName).PrintEndLine("[0];");
                        p.PrintEndLine();

                        p.PrintLine("var rows = this.Items;");
                        p.PrintLine("var count = this.Count;");
                        p.PrintBeginLine("var result = new ").Print(dataTypeFullName).PrintEndLine("[count];");
                        p.PrintEndLine();

                        p.PrintLine("for (var i = 0; i < count; i++)");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("result[i] = (rows[i] ?? __")
                                .Print(dataTypeSimpleName).Print(".Default).To")
                                .Print(dataTypeSimpleName).PrintEndLine("();");
                        }
                        p.CloseScope();

                        p.PrintEndLine();
                        p.PrintLine("return result;");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    if (dataMap.TryGetValue(dataTypeFullName, out var dataModel))
                    {
                        dataModel.WriteCode(
                              ref p
                            , dataMap
                            , sheet.horizontalCollections
                            , sheet.generatedKeyTypeFullNames
                            , sheet.generatedKeyEquality
                            , idTypeFullName
                            , token
                        );
                    }

                    if (hasIdTypeDeclaration && dataMap.TryGetValue(idTypeFullName, out var idDataModel))
                    {
                        idDataModel.WriteCode(
                              ref p
                            , dataMap
                            , sheet.horizontalCollections
                            , sheet.generatedKeyTypeFullNames
                            , sheet.generatedKeyEquality
                            , null
                            , token
                        );
                    }

                    foreach (var nestedFullName in sheet.nestedDataTypeFullNames)
                    {
                        if (dataMap.TryGetValue(nestedFullName, out var nestedModel))
                        {
                            nestedModel.WriteCode(
                                  ref p
                                , dataMap
                                , sheet.horizontalCollections
                                , sheet.generatedKeyTypeFullNames
                                , sheet.generatedKeyEquality
                                , null
                                , token
                            );
                        }
                    }
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#else").PrintEndLine().PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("public abstract partial class ").Print(sheetName).PrintEndLine(" { }");
                p.PrintEndLine();

                p.Print("#endif").PrintEndLine().PrintEndLine();
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private static DataSpec ApplyScopedConverters(
              in DataSpec shape
            , Dictionary<string, ScopedConverterSpec> converterLookup
        )
        {
            if (converterLookup.Count < 1)
            {
                return shape;
            }

            var copy = shape;
            copy.propRefs = ApplyScopedConverters(shape.fullName, shape.propRefs, converterLookup);
            copy.fieldRefs = ApplyScopedConverters(shape.fullName, shape.fieldRefs, converterLookup);

            if (shape.baseTypeRefs.Count > 0)
            {
                using var baseBuilder = ImmutableArrayBuilder<BaseDataSpec>.Rent();

                foreach (var layer in shape.baseTypeRefs)
                {
                    var layerCopy = layer;
                    layerCopy.propRefs = ApplyScopedConverters(layer.fullName, layer.propRefs, converterLookup);
                    layerCopy.fieldRefs = ApplyScopedConverters(layer.fullName, layer.fieldRefs, converterLookup);
                    baseBuilder.Add(layerCopy);
                }

                copy.baseTypeRefs = baseBuilder.ToImmutable().AsEquatableArray();
            }

            return copy;
        }

        private static EquatableArray<MemberSpec> ApplyScopedConverters(
              string declaringTypeFullName
            , EquatableArray<MemberSpec> members
            , Dictionary<string, ScopedConverterSpec> converterLookup
        )
        {
            if (members.Count < 1)
            {
                return members;
            }

            using var builder = ImmutableArrayBuilder<MemberSpec>.Rent();

            foreach (var member in members)
            {
                var copy = member;
                var key = ScopedConverterSpec.MakeMemberKey(declaringTypeFullName, member.propertyName);

                if (converterLookup.TryGetValue(key, out var resolution))
                {
                    copy.converter = resolution.converter;
                    copy.sheetConverter = resolution.sheetConverter;
                }

                builder.Add(copy);
            }

            return builder.ToImmutable().AsEquatableArray();
        }
    }
}
