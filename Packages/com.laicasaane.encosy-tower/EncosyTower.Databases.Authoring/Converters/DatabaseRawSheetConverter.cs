// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Raw;
using EncosyTower.Data.Authoring;

namespace EncosyTower.Databases.Authoring
{
    /// <summary>
    /// Generic sheet converter for cell-based Spreadsheet sources.
    /// </summary>
    public abstract class DatabaseRawSheetConverter : RawSheetConverter
    {
        private readonly NamingMapCache _namingMaps = new();

        protected DatabaseRawSheetConverter(int emptyRowAllowance = 5)
            : base(
                  DataConvertingContext.Default.TimeZoneInfo
                , DataConvertingContext.Default.FormatProvider
            )
        {
            EmptyRowAllowance = emptyRowAllowance;
        }

        public override void Reset()
        {
            base.Reset();
            _namingMaps.Clear();
        }

        protected abstract override Task<bool> LoadData();

        protected abstract override IEnumerable<IRawSheetImporterPage> GetPages(string sheetName);

        protected abstract override Task<bool> SaveData();

        protected abstract override IRawSheetExporterPage CreatePage(string sheetName);

        protected override bool ShouldProcessSheet(SheetConvertingContext context, PropertyInfo sheetProperty)
            => DatabaseSheetNaming.ShouldProcessSheet(context, sheetProperty);

        protected override string GetImportSheetName(PropertyInfo sheetProperty)
            => DatabaseSheetNaming.GetExternalSheetName(sheetProperty);

        protected override string ToPropertyName(PropertyInfo sheetProperty, ISheet sheet, string externalName)
            => _namingMaps.GetOrCreate(sheetProperty, sheet).GetProperName(externalName);

        protected override string GetExportSheetName(PropertyInfo sheetProperty, ISheet sheet)
            => DatabaseSheetNaming.GetExternalSheetName(sheetProperty);

        protected override string ToExternalName(PropertyInfo sheetProperty, ISheet sheet, string propertyName)
            => _namingMaps.GetOrCreate(sheetProperty, sheet).GetSerializedName(propertyName);
    }
}
