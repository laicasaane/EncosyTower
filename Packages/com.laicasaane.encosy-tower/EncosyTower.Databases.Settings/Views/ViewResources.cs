using EncosyTower.Logging;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    internal readonly record struct ViewResources(VisualElement Root)
    {
        public static implicit operator ViewResources(VisualElement root)
            => new(root);

        public ViewResources_GoogleSheet GoogleSheet => new(this);

        public ViewResources_LocalFolder LocalFolder => new(this);

        public string DatabaseCollectionLabel => GetText("database-collection-label");

        public string DatabaseTypeMissing => GetText("database-invalid");

        public string RelativePath => GetText("relative-path");

        private string GetText(string name)
        {
            if (Root.Q<Label>(name: name)?.text is string value)
            {
                return value;
            }

            StaticDevLogger.LogError($"Cannot find Label by name `{name}`");
            return string.Empty;
        }

        public readonly record struct ViewResources_GoogleSheet(ViewResources Resources)
        {
            private const string BASE = "google-sheet";

            public string Credential => Resources.GetText($"{BASE}__credential");

            public string CredentialMissing => Resources.GetText($"{BASE}__credential-missing");

            public string ApiKeyMissing => Resources.GetText($"{BASE}__api-key-missing");

            public string SpreadSheetId => Resources.GetText($"{BASE}__spreadsheet");

            public string SpreadSheetIdInvalid => Resources.GetText($"{BASE}__spreadsheet-invalid");

            public string TokenFolderInvalid => Resources.GetText("token-folder-invalid");

            public string TokenFolderMissing => Resources.GetText("token-folder-missing");

            public string OutputFolderInvalid => Resources.GetText("output-folder-invalid");

            public string OutputFolderMissing => Resources.GetText("output-folder-missing");
        }

        public readonly record struct ViewResources_LocalFolder(ViewResources Resources)
        {
            public string InputFolderInvalid => Resources.GetText("input-folder-invalid");

            public string InputFolderMissing => Resources.GetText("input-folder-missing");

            public string OutputFolderInvalid => Resources.GetText("output-folder-invalid");

            public string OutputFolderMissing => Resources.GetText("output-folder-missing");
        }
    }
}
