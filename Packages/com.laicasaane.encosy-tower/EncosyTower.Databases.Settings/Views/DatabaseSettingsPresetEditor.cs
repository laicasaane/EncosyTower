using UnityEditor;
using UnityEngine.UIElements;

namespace EncosyTower.Databases.Settings.Views
{
    [UnityEditor.CustomEditor(typeof(DatabaseSettingsPreset), true)]
    public class DatabaseSettingsPresetEditor : UnityEditor.Editor
    {
        private DatabaseSettingsView _dbView;

        public override VisualElement CreateInspectorGUI()
        {
            DatabaseTypeVault.Initialize();

            var resources = ViewAPI.GetResources();
            var target = this.target as DatabaseSettingsPreset;
            var database = target._database;
            var databaseProperty = serializedObject.FindProperty(nameof(DatabaseSettingsPreset._database));
            var context = new DatabaseSettingsContext();
            context.Initialize(database, databaseProperty);

            var root = new VisualElement();
            ViewAPI.ApplyStyleSheetsTo(root, false);

            var dbView = _dbView = new DatabaseSettingsView(true, resources) { userData = context };
            dbView.DatabaseTypeSelected += DbView_OnDatabaseTypeSelected;
            dbView.DataSourceSettingsChanged += DbView_OnDataSourceSettingsChanged;
            dbView.Bind(context);

            root.Add(dbView);
            return root;
        }

        public override void OnInspectorGUI()
        {
            _dbView?.Update();
        }

        private void DbView_OnDatabaseTypeSelected(DatabaseSettingsView dbView, DatabaseRecord record)
        {
            var context = dbView.userData as DatabaseSettingsContext;
            var serializedObject = context.SerializedObject;

            var nameProperty = context.Database.GetNameProperty();
            var authorProperty = context.Database.GetAuthorTypeProperty();
            var databaseProperty = context.Database.GetDatabaseTypeProperty();

            if (record?.AuthorType == null)
            {
                nameProperty.stringValue = Constants.UNDEFINED;
                authorProperty.stringValue = Constants.UNDEFINED;
                databaseProperty.stringValue = Constants.UNDEFINED;
                dbView.SetDatabaseTypeChosen(false);
            }
            else
            {
                nameProperty.stringValue = record.Name;
                authorProperty.stringValue = record.AuthorType.AssemblyQualifiedName;
                databaseProperty.stringValue = record.DatabaseType.AssemblyQualifiedName;
                dbView.SetDatabaseTypeChosen(true);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }

        private void DbView_OnDataSourceSettingsChanged(DatabaseSettingsView dbView)
        {
            var context = dbView.userData as DatabaseSettingsContext;
            var serializedObject = context.SerializedObject;

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();

            EditorUtility.SetDirty(serializedObject.targetObject);
            AssetDatabase.SaveAssetIfDirty(serializedObject.targetObject);
        }
    }
}
