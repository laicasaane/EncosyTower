using System;
using System.Collections.Generic;
using System.IO;
using EncosyTower.Collections;
using EncosyTower.Collections.Extensions;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    using DatabaseSettings = DatabaseCollectionSettings.DatabaseSettings;
    using LocalFolderSettings = DatabaseCollectionSettings.LocalFolderSettings;

    internal class LocalFolderAutoConverter : AssetPostprocessor
    {
        private static readonly string s_csvExt = ".csv";
        private static readonly string s_excelExt = ".xlsx";

        static void OnPostprocessAllAssets(
              string[] importedAssets
            , string[] deletedAssets
            , string[] movedAssets
            , string[] movedFromAssetPaths
            , bool didDomainReload
        )
        {
            if (importedAssets.Length < 1)
            {
                return;
            }

            var collectionSettings = DatabaseCollectionSettings.Instance;
            var databases = collectionSettings._databases;
            var assetPaths = new List<string>(importedAssets);
            var paths = new HashSet<string>(importedAssets.Length);
            var csvExt = s_csvExt;
            var excelExt = s_excelExt;

            foreach (var database in databases)
            {
                TryConvert(
                      database
                    , DataSourceFlags.Csv
                    , collectionSettings
                    , database.csvSettings
                    , assetPaths.AsListFast()
                    , csvExt
                    , paths
                );

                TryConvert(
                      database
                    , DataSourceFlags.Excel
                    , collectionSettings
                    , database.excelSettings
                    , assetPaths.AsListFast()
                    , excelExt
                    , paths
                );
            }
        }

        private static void TryConvert(
              DatabaseSettings databaseSettings
            , DataSourceFlags source
            , UnityEngine.Object owner
            , LocalFolderSettings localFolderSettings
            , ListFast<string> assetPaths
            , string extension
            , HashSet<string> paths
        )
        {
            if (localFolderSettings.enabled == false
                || localFolderSettings.liveConversion == false
            )
            {
                return;
            }

            paths.Clear();
            Filter(localFolderSettings, assetPaths, extension, paths);

            if (paths.Count < 1)
            {
                return;
            }

            ConversionTask.Run(
                  databaseSettings
                , source
                , owner
                , DatabaseCollectionSettings.LocalFolderSettings.GetProgressTitle(source)
            );
        }

        private static void Filter(
              LocalFolderSettings settings
            , ListFast<string> assetPaths
            , string extension
            , HashSet<string> paths
        )
        {
            var folderPath = settings.inputRelativeFolderPath;

            if (string.IsNullOrWhiteSpace(folderPath)
                || folderPath.StartsWith("Assets", StringComparison.Ordinal) == false
            )
            {
                return;
            }

            for (var i = assetPaths.Count - 1; i >= 0; i--)
            {
                var path = assetPaths[i];

                if (path.EndsWith(extension, StringComparison.Ordinal) == false)
                {
                    continue;
                }

                var relative = Path.GetRelativePath(folderPath, path);

                if (relative.StartsWith("..", StringComparison.Ordinal))
                {
                    continue;
                }

                paths.Add(path);
                assetPaths.RemoveAtSwapBack(i);
            }
        }
    }
}
