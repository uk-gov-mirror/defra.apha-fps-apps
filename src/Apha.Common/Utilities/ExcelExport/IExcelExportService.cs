namespace Apha.Common.Utilities.ExcelExport
{
    public interface IExcelExportService
    {
        byte[] ExportToExcel<T>(
            IEnumerable<T> data,
            string sheetName = "Sheet1");

        byte[] BuildTimeSheetExcel(
            string WorkGroupName,
            short monthNumber,
            IEnumerable<WorkGroupTimeSheetRow> rows,
            short layout);

        byte[] BuildOutputSheetExcel(
            string WorkGroupName,
            short monthNumber,
            IEnumerable<WorkGroupOutputSheetRow> rows);        

        byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets);

        /// <summary>
        /// Same as <see cref="ExportToExcelMultiSheet(IEnumerable{ExcelSheetDefinition})"/>, plus a
        /// key/value block written to a VeryHidden worksheet — not a normal data cell a user could
        /// edit or discover via Excel's own "Unhide Sheet" dialog (the
        /// bulk-rates download_version protected-metadata carrier).
        /// </summary>
        byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets, IReadOnlyDictionary<string, string> protectedMetadata);

        byte[] BuildBudgetBidsCrosstabExcel(
            IEnumerable<string> accounts,
            IEnumerable<string> workgroups,
            Dictionary<string, Dictionary<string, decimal>> bidLookup);
    }
}
