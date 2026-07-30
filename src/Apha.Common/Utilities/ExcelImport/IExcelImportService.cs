using ClosedXML.Excel;

namespace Apha.Common.Utilities.ExcelImport
{
    public interface IExcelImportService
    {
        ExcelImportResult<T> ReadExcel<T>(
            IXLWorkbook workbook,
            Func<IXLRangeRow, Dictionary<string, int>, T> rowMapper,
            IEnumerable<string>? requiredHeaders = null,
            int worksheetIndex = 1,
            string? invalidTemplateErrorMessage = null);

        IEnumerable<string> GetMissingRequiredHeaders(
            Dictionary<string, int> headerMap,
            IEnumerable<string> requiredHeaders);

        Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow);

        string NormalizeHeader(string headerText);

        string? GetText(IXLCell cell);
    }
}
