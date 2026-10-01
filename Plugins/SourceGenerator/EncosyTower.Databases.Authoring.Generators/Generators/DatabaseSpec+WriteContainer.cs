using static EncosyTower.Databases.Authoring.Generators.Helpers;

namespace EncosyTower.Databases.Authoring.Generators
{
    partial struct DatabaseSpec
    {
        public readonly string WriteContainer(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var p = new Printer(0, 1024 * 16, token);
            p = p.IncreasedIndent();

            p.PrintEndLine();
            p.Print("#pragma warning disable").PrintEndLine();
            p.PrintEndLine();

            p.PrintBeginLine()
                .Print("partial ").Print(databaseTypeKeyword).Print(" ").Print(databaseTypeName)
                .Print(" : ").Print(PR_ICONTAINS).Print("<").Print(databaseTypeName).Print(".SheetContainer>")
                .PrintEndLine();
            p.OpenScope();
            {
                p.Print(DIRECTIVE).PrintEndLine();
                p.PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE).PrintLine(PR_GENERATED_SHEET_CONTAINER);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class SheetContainer")
                    .Print(" : ").Print(PR_DATA_SHEET_CONTAINER_BASE)
                    .Print(", g__ETDBA.IPostExportDatabase")
                    .PrintEndLine();
                p.OpenScope();
                {
                    p.PrintBeginLine("public SheetContainer()").PrintEndLine(" : this(g__CBSU.UnityLogger.Default)");
                    p.PrintLine("{ }");
                    p.PrintEndLine();

                    p.PrintBeginLine("public SheetContainer(g__MEL.ILogger logger)").PrintEndLine(" : base(logger)");
                    p.OpenScope();
                    {
                        foreach (var typeName in typeNames)
                        {
                            p.PrintBeginLine("this.").Print(typeName).Print(" = new ").Print(typeName).PrintEndLine("();");
                        }
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    foreach (var table in tables)
                    {
                        var typeName = table.uniqueSheetName;

                        if (table.transpose)
                        {
                            p.PrintLine("[g__CBS.Transpose]");
                        }

                        p.PrintBeginLine("public ").Print(typeName).Print(" ")
                            .Print(typeName).PrintEndLine(" { get; set; }");
                        p.PrintEndLine();
                    }

                    foreach (var sheetGroup in sheetGroups)
                    {
                        var baseSheetName = sheetGroup.baseSheetName;
                        var sheets = sheetGroup.sheets;
                        var count = sheets.Count;

                        p.PrintBeginLine("public g__SCG.List<").Print(baseSheetName).Print("> ")
                            .Print(baseSheetName).PrintEndLine("s");
                        p.OpenScope();
                        {
                            p.PrintBeginLine("get => new g__SCG.List<").Print(baseSheetName)
                                .Print(">(").Print(count).PrintEndLine(")");
                            p.OpenScope();
                            {
                                var lastIndex = count - 1;

                                for (var i = 0; i < count; i++)
                                {
                                    var asset = sheets[i];

                                    p.PrintBeginLine("this.").Print(asset.tableName).Print("_")
                                        .Print(baseSheetName).Print("_").Print(asset.propertyName);

                                    if (i < lastIndex)
                                    {
                                        p.PrintEndLine(",");
                                    }
                                    else
                                    {
                                        p.PrintEndLine();
                                    }
                                }
                            }
                            p.CloseScope("};");
                        }
                        p.CloseScope();
                        p.PrintEndLine();
                    }

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IPostExportDatabase.PostExport(g__ETDBA.DatabaseExportingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("public void PostExport(g__ETDBA.DatabaseExportingContext context)");
                    p.OpenScope();
                    {
                        p.PrintLine("OnPostExport(context);");
                    }
                    p.CloseScope();
                    p.PrintEndLine();

                    p.PrintBeginLine("/// <inheritdoc cref=\"")
                        .Print("g__ETDBA.IPostExportDatabase.PostExport(g__ETDBA.DatabaseExportingContext)")
                        .PrintEndLine("\"/>");
                    p.PrintLine("partial void OnPostExport(g__ETDBA.DatabaseExportingContext context);");
                    p.PrintEndLine();
                }
                p.CloseScope();
                p.PrintEndLine();

                p.Print("#else").PrintEndLine().PrintEndLine();

                p.PrintLine(PR_SERIALIZABLE).PrintLine(PR_GENERATED_SHEET_CONTAINER);
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintLine("public partial class SheetContainer { }");
                p.PrintEndLine();

                p.Print("#endif").PrintEndLine().PrintEndLine();

                WriteDerivedSheetClasses(ref p);
            }
            p.CloseScope();

            p = p.DecreasedIndent();
            return p.Result;
        }

        private readonly void WriteDerivedSheetClasses(ref Printer p)
        {
            foreach (var table in tables)
            {
                var tableTypeName = table.typeSimpleName;
                var propertyName = table.propertyName;
                var nameCasing = table.nameCasing;
                var idTypeFullName = table.idTypeFullName;
                var dataTypeFullName = table.dataTypeFullName;
                var tableTypeFullName = table.typeFullName;
                var assetName = table.deduplicateAssetName
                    ? nameCasing.ConvertName($"{tableTypeName}_{propertyName}")
                    : nameCasing.ConvertName(tableTypeName);

                p.PrintLine(PR_SERIALIZABLE);
                p.PrintBeginLine("[g__ETDBASG.TableNaming(\"").Print(table.propertyName)
                    .Print("\", g__ETN.NameCasing.").Print(table.nameCasing.ToString()).PrintEndLine(")]");
                p.PrintBeginLine("[g__ETDBASG.GeneratedSheet(typeof(").Print(idTypeFullName)
                    .Print("), typeof(").Print(dataTypeFullName)
                    .Print("), typeof(").Print(tableTypeFullName)
                    .Print("), \"").Print(assetName).PrintEndLine("\")]");
                p.PrintLine(PR_GENERATED_CODE).PrintLine(PR_EXCLUDE_COVERAGE);
                p.PrintBeginLine("public partial class ").Print(table.uniqueSheetName)
                    .Print(" : ").Print(table.baseSheetName).PrintEndLine(" { }");
                p.PrintEndLine();
            }
        }
    }
}
