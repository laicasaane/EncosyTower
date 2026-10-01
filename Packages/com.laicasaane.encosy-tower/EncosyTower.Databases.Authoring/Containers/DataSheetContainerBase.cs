// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Cathei.BakingSheet;
using EncosyTower.Collections;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace EncosyTower.Databases.Authoring
{
    public abstract class DataSheetContainerBase : SheetContainerBase
    {
        private readonly ILogger _logger;
        private readonly HashSet<string> _ignoredSheetProperties;

        protected DataSheetContainerBase(ILogger logger) : base(logger)
        {
            _logger = logger;
            _ignoredSheetProperties = new();
        }

        public ILogger Logger => _logger;

        public HashSetReadOnly<string> IgnoredSheetProperties => _ignoredSheetProperties;

        public void IgnoreSheetProperties(IEnumerable<string> properties)
        {
            if (properties != null)
            {
                foreach (var prop in properties)
                {
                    _ignoredSheetProperties.Add(prop);
                }
            }
        }

        public bool CheckSheetPropertyIsIgnored(string property)
        {
            if (string.IsNullOrWhiteSpace(property)) return true;
            return _ignoredSheetProperties.Contains(property);
        }

        public override void PostLoad()
        {
            var context = new SheetConvertingContext
            {
                Container = this,
                Logger = _logger,
            };

            var properties = GetSheetProperties();
            var rowTypeToSheet = new Dictionary<Type, ISheet>(properties.Count);
            var rowTypeToProperty = new Dictionary<Type, string>(properties.Count);
            var dataSheets = new List<IDataSheet>(properties.Count);

            foreach (var pair in properties)
            {
                if (CheckSheetPropertyIsIgnored(pair.Key))
                {
                    continue;
                }

                if (pair.Value.GetValue(this) is not ISheet sheet)
                {
                    LogError_MissingSheet(context.Logger, GetType(), pair.Key, pair.Value.PropertyType);
                    continue;
                }

                sheet.Name = pair.Key;

                if (rowTypeToSheet.TryAdd(sheet.RowType, sheet))
                {
                    rowTypeToProperty.Add(sheet.RowType, pair.Key);

                    if (sheet is IDataSheet dataSheet)
                    {
                        dataSheets.Add(dataSheet);
                    }
                }
                else
                {
                    // row type must be unique in a sheet container
                    LogError_DuplicateRowType(
                          context.Logger
                        , GetType()
                        , rowTypeToProperty[sheet.RowType]
                        , pair.Key
                        , sheet.RowType
                    );
                }
            }

            OnBeforeMapReferences(context);

            // making sure all references are mapped before calling PostLoad
            foreach (var sheet in rowTypeToSheet.Values)
            {
                sheet.MapReferences(context, rowTypeToSheet);
            }

            OnAfterMapReferences(context);
            OnBeforePostLoad(context);

            foreach (var sheet in rowTypeToSheet.Values)
            {
                sheet.PostLoad(context);
            }

            OnAfterPostLoad(context);
            OnBeforePreprocess(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Preprocess(context);
            }

            OnAfterPreprocess(context);
            OnBeforeProcess(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Process(context);
            }

            OnAfterProcess(context);
            OnBeforePostprocess(context);

            foreach (var sheet in dataSheets)
            {
                sheet.Postprocess(context);
            }

            OnAfterPostprocess(context);
        }

        /// <summary>
        /// Callback invoked before all <c>sheet.MapReferences</c>.
        /// </summary>
        protected virtual void OnBeforeMapReferences(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.MapReferences</c>.
        /// </summary>
        protected virtual void OnAfterMapReferences(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.PostLoad</c>.
        /// </summary>
        protected virtual void OnBeforePostLoad(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.PostLoad</c>.
        /// </summary>
        protected virtual void OnAfterPostLoad(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.Preprocess</c>.
        /// </summary>
        protected virtual void OnBeforePreprocess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.Preprocess</c>.
        /// </summary>
        protected virtual void OnAfterPreprocess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.Process</c>.
        /// </summary>
        protected virtual void OnBeforeProcess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.Process</c>.
        /// </summary>
        protected virtual void OnAfterProcess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked before all <c>sheet.Postprocess</c>.
        /// </summary>
        protected virtual void OnBeforePostprocess(SheetConvertingContext context) { }

        /// <summary>
        /// Callback invoked after all <c>sheet.Postprocess</c>.
        /// </summary>
        protected virtual void OnAfterPostprocess(SheetConvertingContext context) { }

        [MethodImpl(MethodImplOptions.NoInlining)]
        [UnityEngine.HideInCallstack, StackTraceHidden]
        private static void LogError_MissingSheet(
              ILogger logger
            , Type containerType
            , string sheetProperty
            , Type sheetType
        )
            => logger.LogError(
                "Sheet container {ContainerType} property {SheetProperty} expected {SheetType}, but no sheet " +
                "instance was assigned. Assign a generated sheet before loading."
                , containerType
                , sheetProperty
                , sheetType
            );

        [MethodImpl(MethodImplOptions.NoInlining)]
        [UnityEngine.HideInCallstack, StackTraceHidden]
        private static void LogError_DuplicateRowType(
              ILogger logger
            , Type containerType
            , string acceptedSheetProperty
            , string duplicateSheetProperty
            , Type rowType
        )
            => logger.LogError(
                "Sheet container {ContainerType} properties {AcceptedSheetProperty} and " +
                "{DuplicateSheetProperty} use the same row type {RowType}. The first sheet remains active; assign " +
                "a unique row type."
                , containerType
                , acceptedSheetProperty
                , duplicateSheetProperty
                , rowType
            );
    }
}
