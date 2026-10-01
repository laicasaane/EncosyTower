using Cathei.BakingSheet;
using EncosyTower.Databases.Authoring;

namespace EncosyTower.Databases.Settings
{
    partial class DatabaseCollectionSettings
    {
        partial class LocalCsvFolderSettings
        {
            public const string PROGRESS_TITLE = "Convert CSV Files";

            public override string ProgressTitle => PROGRESS_TITLE;

            protected override ISheetImporter GetImporter(string inputFolderPath)
            {
                return new DatabaseCsvSheetConverter(
                      inputFolderPath
                    , extension
                    , fileSystem: null
                    , emptyRowAllowance
                    , includeSubFolders
                    , includeCommentedFiles
                );
            }
        }
    }
}
