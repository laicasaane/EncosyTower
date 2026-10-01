#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Raw;
using EncosyTower.Data;
using EncosyTower.Databases;
using EncosyTower.Databases.Authoring;
using EncosyTower.Databases.Authoring.SourceGen;
using EncosyTower.Naming;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using BakingSheetTranspose = Cathei.BakingSheet.TransposeAttribute;
using DataAttribute = EncosyTower.Data.DataAttribute;
using DatabaseTranspose = EncosyTower.Databases.Authoring.TransposeAttribute;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace EncosyTower.Tests.Databases.Authoring
{
    public sealed class DatabaseRawSheetIntegrationTests
    {
        [Test]
        public async Task Import_UsesSelectionNamingPageOrderAndEmptyRowAllowance()
        {
            var container = new OrderedContainer();
            container.IgnoreSheetProperties(new[] { nameof(OrderedContainer.Ignored) });

            var converter = new MemoryConverter(emptyRowAllowance: 1);
            converter.AddImportPage("item_records", "b", new[] {
                new[] { "id", "display_name" },
                new[] { "b", "B" },
            });
            converter.AddImportPage("item_records", "a", new[] {
                new[] { "id", "display_name" },
                new[] { "a1", "A1" },
                Array.Empty<string>(),
                new[] { "a2", "A2" },
            });

            var success = await converter.Import(CreateContext(container));

            Assert.That(success, Is.True);
            Assert.That(container.Ignored, Is.Null);
            Assert.That(container.Items.Select(static row => row.Id), Is.EqualTo(new[] { "a1", "a2", "b" }));
            Assert.That(container.Items["a2"].DisplayName, Is.EqualTo("A2"));
        }

        [Test]
        public async Task Export_AppliesEveryNameCasingToMembersAndDictionaryHeaders()
        {
            var container = new CasingContainer {
                Pascal = CreateCasingSheet<PascalSheet>(),
                Camel = CreateCasingSheet<CamelSheet>(),
                SnakeLower = CreateCasingSheet<SnakeLowerSheet>(),
                SnakeUpper = CreateCasingSheet<SnakeUpperSheet>(),
                KebabLower = CreateCasingSheet<KebabLowerSheet>(),
                KebabUpper = CreateCasingSheet<KebabUpperSheet>(),
            };
            var converter = new MemoryConverter { HeaderMode = HeaderMode.Flat };

            var success = await converter.Export(CreateContext(container));

            Assert.That(success, Is.True);
            AssertFlatHeaders(converter, "PascalNames", "DisplayName", "Rewards:Key", "Rewards:Value");
            AssertFlatHeaders(converter, "camelNames", "displayName", "rewards:key", "rewards:value");
            AssertFlatHeaders(converter, "snake_lower_names", "display_name", "rewards:key", "rewards:value");
            AssertFlatHeaders(converter, "SNAKE_UPPER_NAMES", "DISPLAY_NAME", "REWARDS:KEY", "REWARDS:VALUE");
            AssertFlatHeaders(converter, "kebab-lower-names", "display-name", "rewards:key", "rewards:value");
            AssertFlatHeaders(converter, "KEBAB-UPPER-NAMES", "DISPLAY-NAME", "REWARDS:KEY", "REWARDS:VALUE");
        }

        [Test]
        public async Task Import_AcceptsCanonicalDictionaryHeaderAliases()
        {
            var container = new CanonicalAliasContainer();
            var converter = new MemoryConverter();
            converter.AddImportPage("alias_records", null, new[] {
                new[] { "id", "name", "rewards:Key", "rewards:Value" },
                new[] { "quest", "Quest", "gold", "100" },
            });

            var success = await converter.Import(CreateContext(container));

            Assert.That(success, Is.True);
            Assert.That(container.Records["quest"].Rewards, Contains.Key("gold"));
            Assert.That(container.Records["quest"].Rewards["gold"], Is.EqualTo(100));
            Assert.That(container.Records["quest"].Name, Is.EqualTo("Quest"));
        }

        [Test]
        public async Task Import_HandlesNestedCollectionsCommentsAndInvalidMarkerRecovery()
        {
            var container = new NestedContainer();
            var converter = new MemoryConverter();
            converter.AddImportPage("Items", null, new[] {
                new[] {
                    "Id",
                    "Stages:[1]:Name",
                    "Stages:[1]:Rewards:{}:Key",
                    "Stages:[1]:Rewards:{}:Value",
                    "$ Notes",
                },
                new[] { "$ comment" },
                new[] { "bad" },
                new[] { null, "<#Stages:[2]#>" },
                new[] { "good" },
                new[] { null, "<#Stages:[1]#>" },
                new[] { null, "Act", null, null, "$$ ignored" },
                new[] { null, null, "<#Stages:[1]:Rewards:{}#>" },
                new[] { null, null, "gold", "5" },
            });

            var success = await converter.Import(CreateContext(container));

            Assert.That(success, Is.True);
            Assert.That(container.Items.Contains("bad"), Is.False);
            Assert.That(container.Items.Contains("good"), Is.True);
            Assert.That(container.Items["good"].Stages, Has.Count.EqualTo(1));
            Assert.That(container.Items["good"].Stages[0], Has.Count.EqualTo(1));
            Assert.That(container.Items["good"].Stages[0][0].Name, Is.EqualTo("Act"));
            Assert.That(container.Items["good"].Stages[0][0].Rewards, Has.Count.EqualTo(1));
            Assert.That(container.Items["good"].Stages[0][0].Rewards[0]["gold"], Is.EqualTo(5));
        }

        [Test]
        public async Task Import_HandlesTransposedPage()
        {
            var container = new TransposedContainer();
            var converter = new MemoryConverter();
            converter.AddImportPage("Items", null, new[] {
                new[] { "Id", "row" },
                new[] { "DisplayName", "Value" },
            });

            var success = await converter.Import(CreateContext(container));

            Assert.That(success, Is.True);
            Assert.That(container.Items["row"].DisplayName, Is.EqualTo("Value"));
        }

        [Test]
        public async Task CsvAdapter_ImportsColumnsPastId()
        {
            var fileSystem = new MemoryFileSystem();
            fileSystem.AddFile("Items.csv", "Id,DisplayName\nrow,Csv\n");
            var converter = new DatabaseCsvSheetConverter(".", fileSystem: fileSystem);

            await AssertAdapterImportsDisplayName(converter, "Csv");
        }

        [Test]
        public async Task ExcelAdapter_ImportsColumnsPastId()
        {
            var table = new DataTable("Items");
            table.Columns.Add();
            table.Columns.Add();
            table.Rows.Add("Id", "DisplayName");
            table.Rows.Add("row", "Excel");

            var page = CreatePrivatePage(
                typeof(DatabaseExcelSheetConverter),
                table, null, CultureInfo.InvariantCulture);
            var converter = new TestExcelConverter(page);

            await AssertAdapterImportsDisplayName(converter, "Excel");
        }

        [Test]
        public async Task GoogleAdapter_ImportsColumnsPastId()
        {
            var page = CreateGooglePage(new[] {
                new[] { "Id", "DisplayName" },
                new[] { "row", "Google" },
            });
            var converter = new TestGoogleConverter(page);

            await AssertAdapterImportsDisplayName(converter, "Google");
        }

        [Test]
        public void PostLoad_LogsMissingAndDuplicateSheetContext()
        {
            var logger = new CapturingLogger();
            var container = new ContextContainer(logger) {
                First = new AdapterSheet(),
                Duplicate = new AdapterSheet(),
            };

            container.PostLoad();

            Assert.That(logger.Messages, Has.Some.Contains(nameof(ContextContainer.Missing)));
            Assert.That(logger.Messages, Has.Some.Contains(nameof(AdapterSheet)));
            Assert.That(logger.Messages, Has.Some.Contains(nameof(ContextContainer.First)));
            Assert.That(logger.Messages, Has.Some.Contains(nameof(ContextContainer.Duplicate)));
            Assert.That(logger.Messages, Has.Some.Contains(nameof(AdapterRow)));
        }

        [Test]
        public void GeneratedSheets_ConvertRecursiveCollectionsAndUseValueEqualIds()
        {
            var container = new GeneratedFeatureAuthoring.SheetContainer();
            var vertical = container
                .GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_0_Vertical;
            var horizontal = container
                .GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_1_Horizontal;
            var firstId = new GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_0.__GeneratedFeatureId {
                Kind = "enemy",
                SubId = 7,
            };
            var secondId = new GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_0.__GeneratedFeatureId {
                Kind = "enemy",
                SubId = 7,
            };
            var verticalRewards = new VerticalDictionary<
                string,
                VerticalList<GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_0.__GeneratedReward>
            > {
                ["gold"] = new() {
                    new() { Amount = 100 },
                },
            };
            vertical.Add(new GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_0.__GeneratedFeatureRow {
                Id = firstId,
                Rewards = verticalRewards,
            });
            var horizontalRewards = new Dictionary<
                string,
                VerticalList<GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_1.__GeneratedReward>
            > {
                ["gold"] = new() {
                    new() { Amount = 100 },
                },
            };
            horizontal.Add(new GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_1.__GeneratedFeatureRow {
                Id = new() { Kind = "enemy", SubId = 7 },
                Rewards = horizontalRewards,
            });

            var verticalData = vertical.ToDataArray();
            var horizontalData = horizontal.ToDataArray();

            Assert.That(firstId, Is.EqualTo(secondId));
            Assert.That(firstId.GetHashCode(), Is.EqualTo(secondId.GetHashCode()));
            Assert.That(vertical.Contains(secondId), Is.True);
            Assert.That(verticalData, Has.Length.EqualTo(1));
            Assert.That(horizontalData, Has.Length.EqualTo(1));
            Assert.That(verticalData[0].Rewards["gold"][0].Amount, Is.EqualTo(100));
            Assert.That(horizontalData[0].Rewards["gold"][0].Amount, Is.EqualTo(100));

            var containerType = typeof(GeneratedFeatureAuthoring.SheetContainer);
            var verticalProperty = containerType.GetProperty(
                nameof(container.GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_0_Vertical)
            );
            var horizontalProperty = containerType.GetProperty(
                nameof(container.GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_1_Horizontal)
            );

            Assert.That(verticalProperty.GetCustomAttribute<BakingSheetTranspose>(), Is.Null);
            Assert.That(horizontalProperty.GetCustomAttribute<BakingSheetTranspose>(), Is.Not.Null);
        }

        [Test]
        public async Task GeneratedSheet_ImportsNestedValuesAndComplexId()
        {
            var container = new GeneratedFeatureAuthoring.SheetContainer();
            container.IgnoreSheetProperties(new[] {
                nameof(container.GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_1_Horizontal),
            });
            var converter = new MemoryConverter { HeaderMode = HeaderMode.Flat };
            converter.AddImportPage("Vertical", null, new[] {
                new[] {
                    "Id:Kind",
                    "Id:SubId",
                    "Rewards:Key",
                    "Rewards:Value:Amount",
                },
                new[] { "enemy", "7", "gold", "100" },
            });

            var success = await converter.Import(CreateContext(container));
            var sheet = container.GeneratedFeatureTableAsset_GeneratedFeatureRowSheet_0_Vertical;
            var lookupId = new GeneratedFeatureAuthoring.GeneratedFeatureRowSheet_0.__GeneratedFeatureId {
                Kind = "enemy",
                SubId = 7,
            };
            var data = sheet.ToDataArray();

            Assert.That(success, Is.True);
            Assert.That(sheet.Contains(lookupId), Is.True);
            Assert.That(data, Has.Length.EqualTo(1));
            Assert.That(data[0].Rewards["gold"][0].Amount, Is.EqualTo(100));
        }

        private static SheetConvertingContext CreateContext(SheetContainerBase container)
            => new() {
                Container = container,
                Logger = NullLogger.Instance,
            };

        private static async Task AssertAdapterImportsDisplayName(ISheetImporter importer, string expected)
        {
            var container = new AdapterContainer();

            var success = await importer.Import(CreateContext(container));

            Assert.That(success, Is.True);
            Assert.That(container.Items["row"].DisplayName, Is.EqualTo(expected));
        }

        private static TSheet CreateCasingSheet<TSheet>()
            where TSheet : Sheet<CasingRow>, new()
        {
            var sheet = new TSheet();
            var row = new CasingRow {
                Id = "row",
                DisplayName = "Name",
            };
            row.Rewards.Add("gold", 1);
            sheet.Add(row);
            return sheet;
        }

        private static void AssertFlatHeaders(
              MemoryConverter converter
            , string sheetName
            , params string[] expectedHeaders
        )
        {
            var page = converter.GetExportPage(sheetName);
            var headers = page.GetRow(0);

            Assert.That(headers, Is.SupersetOf(expectedHeaders));
        }

        private static IRawSheetImporterPage CreatePrivatePage(Type converterType, params object[] arguments)
        {
            var pageType = converterType.GetNestedType("Page", BindingFlags.NonPublic);
            Assert.That(pageType, Is.Not.Null);

            return (IRawSheetImporterPage)Activator.CreateInstance(
                pageType,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                binder: null,
                args: arguments,
                culture: CultureInfo.InvariantCulture);
        }

        private static IRawSheetImporterPage CreateGooglePage(IReadOnlyList<string[]> rows)
        {
            var converterType = typeof(DatabaseGoogleSheetConverter);
            var pageType = converterType.GetNestedType("Page", BindingFlags.NonPublic);
            Assert.That(pageType, Is.Not.Null);

            var constructor = pageType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)[0];
            var sheetType = constructor.GetParameters()[0].ParameterType;
            var sheet = Activator.CreateInstance(sheetType);
            var dataProperty = sheetType.GetProperty("Data");
            var gridType = dataProperty.PropertyType.GenericTypeArguments[0];
            var grid = Activator.CreateInstance(gridType);
            var rowDataProperty = gridType.GetProperty("RowData");
            var rowDataType = rowDataProperty.PropertyType.GenericTypeArguments[0];
            var rowData = CreateList(rowDataType);

            for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                var row = Activator.CreateInstance(rowDataType);
                var valuesProperty = rowDataType.GetProperty("Values");
                var cellType = valuesProperty.PropertyType.GenericTypeArguments[0];
                var cells = CreateList(cellType);

                for (var column = 0; column < rows[rowIndex].Length; column++)
                {
                    var cell = Activator.CreateInstance(cellType);
                    cellType.GetProperty("FormattedValue").SetValue(cell, rows[rowIndex][column]);
                    cells.Add(cell);
                }

                valuesProperty.SetValue(row, cells);
                rowData.Add(row);
            }

            rowDataProperty.SetValue(grid, rowData);
            var data = CreateList(gridType);
            data.Add(grid);
            dataProperty.SetValue(sheet, data);

            return (IRawSheetImporterPage)constructor.Invoke(new[] { sheet, null });
        }

        private static System.Collections.IList CreateList(Type elementType)
            => (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));

        private sealed class MemoryConverter : DatabaseRawSheetConverter
        {
            private readonly Dictionary<string, List<MemoryPage>> _importPages = new();
            private readonly Dictionary<string, MemoryPage> _exportPages = new();

            public MemoryConverter(int emptyRowAllowance = 5)
                : base(emptyRowAllowance)
            {
            }

            public void AddImportPage(string sheetName, string subName, IReadOnlyList<string[]> rows)
            {
                if (_importPages.TryGetValue(sheetName, out var pages) == false)
                {
                    pages = new List<MemoryPage>();
                    _importPages.Add(sheetName, pages);
                }

                pages.Add(new MemoryPage(subName, rows));
            }

            public MemoryPage GetExportPage(string sheetName)
                => _exportPages[sheetName];

            protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
                => _importPages.TryGetValue(sheetName, out var pages)
                    ? pages
                    : Enumerable.Empty<IRawSheetImporterPage>();

            protected override int GetColumnCount(
                  IRawSheetImporterPage page
                , int row
                , int headerColumnCount
            )
                => page is MemoryPage memoryPage
                    ? memoryPage.GetColumnCount(row)
                    : headerColumnCount;

            protected override IRawSheetExporterPage CreatePage(string sheetName)
            {
                var page = new MemoryPage(null, Array.Empty<string[]>());
                _exportPages[sheetName] = page;
                return page;
            }

            protected override Task<bool> LoadData()
                => Task.FromResult(true);

            protected override Task<bool> SaveData()
                => Task.FromResult(true);
        }

        private sealed class MemoryPage : IRawSheetImporterPage, IRawSheetExporterPage
        {
            private readonly List<List<string>> _rows;

            public MemoryPage(string subName, IReadOnlyList<string[]> rows)
            {
                SubName = subName;
                _rows = rows.Select(static row => row.ToList()).ToList();
            }

            public string SubName { get; }

            public int FindColumn(string value, int row)
                => row < _rows.Count ? _rows[row].IndexOf(value) : -1;

            public int GetColumnCount(int row)
                => row >= 0 && row < _rows.Count ? _rows[row].Count : 0;

            public IReadOnlyList<string> GetRow(int row)
                => _rows[row];

            public string GetCell(int col, int row)
                => row >= 0 && row < _rows.Count && col >= 0 && col < _rows[row].Count
                    ? _rows[row][col]
                    : null;

            public void SetCell(int col, int row, string data)
            {
                while (_rows.Count <= row)
                {
                    _rows.Add(new List<string>());
                }

                while (_rows[row].Count <= col)
                {
                    _rows[row].Add(null);
                }

                _rows[row][col] = data;
            }
        }

        private sealed class MemoryFileSystem : IExtendedFileSystem
        {
            private readonly Dictionary<string, byte[]> _files = new();

            public void AddFile(string path, string contents)
                => _files[path] = Encoding.UTF8.GetBytes(contents);

            public IEnumerable<string> GetFiles(string path, string extension)
                => GetFiles(path, extension, includeSubFolders: false);

            public IEnumerable<string> GetFiles(string path, string extension, bool includeSubFolders)
                => _files.Keys.Where(
                    file => string.Equals(Path.GetExtension(file), $".{extension}", StringComparison.Ordinal));

            public bool Exists(string path)
                => _files.ContainsKey(path);

            public Stream OpenRead(string path)
                => new MemoryStream(_files[path], writable: false);

            public void CreateDirectory(string path)
            {
            }

            public Stream OpenWrite(string path)
                => new MemoryStream();

            public void DeleteDirectory(string path, bool recursive = true)
            {
            }

            public bool DirectoryExists(string path)
                => true;
        }

        private sealed class TestExcelConverter : DatabaseExcelSheetConverter
        {
            private readonly IRawSheetImporterPage _page;

            public TestExcelConverter(IRawSheetImporterPage page)
                : base(".", fileSystem: new MemoryFileSystem())
            {
                _page = page;
            }

            protected override Task<bool> LoadData()
                => Task.FromResult(true);

            protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
                => new[] { _page };
        }

        private sealed class TestGoogleConverter : DatabaseGoogleSheetConverter
        {
            private readonly IRawSheetImporterPage _page;

            public TestGoogleConverter(IRawSheetImporterPage page)
                : base("id", new BaseClientService.Initializer())
            {
                _page = page;
            }

            protected override Task<bool> LoadData()
                => Task.FromResult(true);

            protected override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName)
                => new[] { _page };
        }

        private sealed class CapturingLogger : ILogger
        {
            public List<string> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state)
                => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel)
                => true;

            public void Log<TState>(
                  LogLevel logLevel
                , EventId eventId
                , TState state
                , Exception exception
                , Func<TState, Exception, string> formatter
            )
                => Messages.Add(formatter(state, exception));

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }

        private sealed class OrderedContainer : DataSheetContainerBase
        {
            public OrderedContainer() : base(NullLogger.Instance)
            {
            }

            public OrderedSheet Items { get; set; }

            public IgnoredSheet Ignored { get; set; }
        }

        [TableNaming("ItemRecords", NameCasing.SnakeLower)]
        private sealed class OrderedSheet : Sheet<OrderedRow>
        {
        }

        private sealed class OrderedRow : SheetRow
        {
            public string DisplayName { get; set; }
        }

        private sealed class IgnoredSheet : Sheet<IgnoredRow>
        {
        }

        private sealed class IgnoredRow : SheetRow
        {
        }

        private sealed class CasingContainer : DataSheetContainerBase
        {
            public CasingContainer() : base(NullLogger.Instance)
            {
            }

            public PascalSheet Pascal { get; set; }

            public CamelSheet Camel { get; set; }

            public SnakeLowerSheet SnakeLower { get; set; }

            public SnakeUpperSheet SnakeUpper { get; set; }

            public KebabLowerSheet KebabLower { get; set; }

            public KebabUpperSheet KebabUpper { get; set; }
        }

        private sealed class CasingRow : SheetRow
        {
            public string Name { get; set; }

            public string DisplayName { get; set; }

            public VerticalDictionary<string, int> Rewards { get; set; } = new();
        }

        [TableNaming("PascalNames", NameCasing.Pascal)]
        private sealed class PascalSheet : Sheet<CasingRow>
        {
        }

        [TableNaming("CamelNames", NameCasing.Camel)]
        private sealed class CamelSheet : Sheet<CasingRow>
        {
        }

        [TableNaming("SnakeLowerNames", NameCasing.SnakeLower)]
        private sealed class SnakeLowerSheet : Sheet<CasingRow>
        {
        }

        [TableNaming("SnakeUpperNames", NameCasing.SnakeUpper)]
        private sealed class SnakeUpperSheet : Sheet<CasingRow>
        {
        }

        [TableNaming("KebabLowerNames", NameCasing.KebabLower)]
        private sealed class KebabLowerSheet : Sheet<CasingRow>
        {
        }

        [TableNaming("KebabUpperNames", NameCasing.KebabUpper)]
        private sealed class KebabUpperSheet : Sheet<CasingRow>
        {
        }

        private sealed class CanonicalAliasContainer : DataSheetContainerBase
        {
            public CanonicalAliasContainer() : base(NullLogger.Instance)
            {
            }

            public CanonicalAliasSheet Records { get; set; }
        }

        [TableNaming("AliasRecords", NameCasing.SnakeLower)]
        private sealed class CanonicalAliasSheet : Sheet<CasingRow>
        {
        }

        private sealed class NestedContainer : DataSheetContainerBase
        {
            public NestedContainer() : base(NullLogger.Instance)
            {
            }

            public NestedSheet Items { get; set; }
        }

        private sealed class NestedSheet : Sheet<NestedRow>
        {
        }

        private sealed class NestedRow : SheetRow
        {
            public VerticalList<VerticalList<NestedStage>> Stages { get; set; } = new();
        }

        private sealed class NestedStage
        {
            public string Name { get; set; }

            public VerticalList<VerticalDictionary<string, int>> Rewards { get; set; } = new();
        }

        private sealed class TransposedContainer : DataSheetContainerBase
        {
            public TransposedContainer() : base(NullLogger.Instance)
            {
            }

            [BakingSheetTranspose]
            public AdapterSheet Items { get; set; }
        }

        private sealed class AdapterContainer : DataSheetContainerBase
        {
            public AdapterContainer() : base(NullLogger.Instance)
            {
            }

            public AdapterSheet Items { get; set; }
        }

        private sealed class ContextContainer : DataSheetContainerBase
        {
            public ContextContainer(ILogger logger) : base(logger)
            {
            }

            public AdapterSheet First { get; set; }

            public AdapterSheet Duplicate { get; set; }

            public AdapterSheet Missing { get; set; }
        }

        private sealed class AdapterSheet : Sheet<AdapterRow>
        {
        }

        private sealed class AdapterRow : SheetRow
        {
            public string DisplayName { get; set; }
        }
    }

    [DataAttribute]
    public partial struct GeneratedFeatureId
    {
        [DataProperty] public readonly string Kind => Get_Kind();

        [DataProperty] public readonly int SubId => Get_SubId();
    }

    [DataAttribute]
    public partial struct GeneratedReward
    {
        [DataProperty] public readonly int Amount => Get_Amount();
    }

    [DataAttribute]
    public partial struct GeneratedFeatureRow
    {
        [DataProperty] public readonly GeneratedFeatureId Id => Get_Id();

        [DataProperty]
        public readonly Dictionary<string, List<GeneratedReward>> Rewards => Get_Rewards();
    }

    [DataTableAsset]
    public sealed partial class GeneratedFeatureTableAsset
        : DataTableAssetBase<GeneratedFeatureId, GeneratedFeatureRow>
    {
    }

    [Database]
    public readonly partial struct GeneratedFeatureDatabase
    {
        [Table]
        public readonly GeneratedFeatureTableAsset Vertical => Get_Vertical();

        [Table]
        [Horizontal(typeof(GeneratedFeatureRow), nameof(GeneratedFeatureRow.Rewards))]
        [DatabaseTranspose]
        public readonly GeneratedFeatureTableAsset Horizontal => Get_Horizontal();
    }

    [AuthorDatabase(typeof(GeneratedFeatureDatabase))]
    public readonly partial struct GeneratedFeatureAuthoring
    {
    }
}

#endif
