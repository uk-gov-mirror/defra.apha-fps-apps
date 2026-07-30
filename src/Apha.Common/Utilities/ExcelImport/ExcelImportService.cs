using ClosedXML.Excel;

namespace Apha.Common.Utilities.ExcelImport
{
    public class ExcelImportService : IExcelImportService
    {
        public ExcelImportResult<T> ReadExcel<T>(
            IXLWorkbook workbook,
            Func<IXLRangeRow, Dictionary<string, int>, T> rowMapper,
            IEnumerable<string>? requiredHeaders = null,
            int worksheetIndex = 1,
            string? invalidTemplateErrorMessage = null)
        {
            var result = new ExcelImportResult<T>();

            try
            {
                var worksheet = workbook.Worksheet(worksheetIndex);
                var usedRows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? new List<IXLRangeRow>();

                if (usedRows.Count <= 1)
                {
                    result.IsSuccess = false;
                    result.ErrorMessage = "No data rows found in the uploaded Excel file.";
                    return result;
                }

                // Build header map from first row
                var headerMap = BuildHeaderMap(usedRows[0]);

                // Validate required headers if provided
                if (requiredHeaders != null)
                {
                    var missingHeaders = GetMissingRequiredHeaders(headerMap, requiredHeaders).ToList();
                    if (missingHeaders.Count > 0)
                    {
                        result.IsSuccess = false;
                        result.MissingHeaders = missingHeaders;
                        result.ErrorMessage = invalidTemplateErrorMessage 
                            ?? "The uploaded Excel file format is not correct. Please use the correct template.";
                        return result;
                    }
                }

                // Map data rows using the provided mapper function
                result.Rows = new List<T>(usedRows.Count - 1);
                foreach (var row in usedRows.Skip(1))
                {
                    var mappedRow = rowMapper(row, headerMap);
                    result.Rows.Add(mappedRow);
                }

                result.TotalRows = result.Rows.Count;
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.ErrorMessage = $"Error reading Excel file: {ex.Message}";
            }

            return result;
        }

        public Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var header = NormalizeHeader(cell.GetString());
                if (!string.IsNullOrWhiteSpace(header) && !map.ContainsKey(header))
                    map[header] = cell.Address.ColumnNumber;
            }
            return map;
        }

        public IEnumerable<string> GetMissingRequiredHeaders(
            Dictionary<string, int> headerMap,
            IEnumerable<string> requiredHeaders)
        {
            foreach (var header in requiredHeaders)
            {
                if (!headerMap.ContainsKey(NormalizeHeader(header)))
                    yield return header;
            }
        }

        public string NormalizeHeader(string headerText)
        {
            return new string((headerText ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
        }

        public string? GetText(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;
            var text = cell.GetString()?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
    }
}
