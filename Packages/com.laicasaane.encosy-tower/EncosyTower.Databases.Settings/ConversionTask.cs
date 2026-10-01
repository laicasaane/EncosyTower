using System;
using System.Threading.Tasks;
using EncosyTower.Logging;
using UnityEditor;

namespace EncosyTower.Databases.Settings
{
    using DatabaseSettings = DatabaseCollectionSettings.DatabaseSettings;

    internal delegate void ReportAction(string title, string message);

    internal sealed class ConversionTask
    {
        private readonly int _id;
        private readonly Task<bool> _task;
        private readonly Action _unsubscribe;
        private readonly Action _removeProgress;
        private readonly Action<Exception> _logException;
        private readonly Action _refresh;

        private bool _isCompleted;

        private ConversionTask(int id, Task<bool> task)
        {
            _id = id;
            _task = task;
            _unsubscribe = Unsubscribe;
            _removeProgress = RemoveProgress;
            _logException = static exception => StaticDevLogger.LogException(exception);
            _refresh = Refresh;
        }

        internal ConversionTask(
              Task<bool> task
            , Action unsubscribe
            , Action removeProgress
            , Action<Exception> logException
            , Action refresh
        )
        {
            _task = task;
            _unsubscribe = unsubscribe;
            _removeProgress = removeProgress;
            _logException = logException;
            _refresh = refresh;
        }

        public static void Run(
              DatabaseSettings settings
            , DataSourceFlags sources
            , UnityEngine.Object owner
            , string title
        )
        {
            if (settings == null)
            {
                return;
            }

            var id = Progress.Start(title);
            var task = new ConversionTask(id, settings.ConvertEditorAsync(
                  sources
                , owner
                , (title, msg) => Progress.Report(id, 0f, msg)
                , default
                , true
            ));

            EditorApplication.update += task.Update;
        }

        internal void Update()
        {
            if (_isCompleted || _task.IsCompleted == false)
            {
                return;
            }

            _isCompleted = true;
            _unsubscribe();
            _removeProgress();

            if (_task.IsCanceled)
            {
                return;
            }

            if (_task.IsFaulted)
            {
                _logException(_task.Exception);
                return;
            }

            if (_task.Result)
            {
                _refresh();
            }
        }

        private void Unsubscribe()
            => EditorApplication.update -= Update;

        private void RemoveProgress()
            => Progress.Remove(_id);

        private static void Refresh()
            => AssetDatabase.Refresh();
    }
}
