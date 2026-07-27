using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Apha.Common.Utilities.ExcelExport
{
    public partial class ExcelExportService : IExcelExportService
    {
        public byte[] ExportToExcel<T>(
        IEnumerable<T> data,
        string sheetName = "Sheet1")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < properties.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = properties[i].Name;
            }

            int row = 2;

            foreach (var item in data)
            {
                for (int col = 0; col < properties.Length; col++)
                {
                    var rawValue = ConvertExcelValue(properties[col].GetValue(item));
                    worksheet.Cell(row, col + 1).Value = XLCellValue.FromObject(rawValue);                     
                }

                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            return stream.ToArray();
        }        

        public byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets)
            => BuildWorkbook(sheets, protectedMetadata: null);

        public byte[] ExportToExcelMultiSheet(IEnumerable<ExcelSheetDefinition> sheets, IReadOnlyDictionary<string, string> protectedMetadata)
            => BuildWorkbook(sheets, protectedMetadata);

        private byte[] BuildWorkbook(IEnumerable<ExcelSheetDefinition> sheets, IReadOnlyDictionary<string, string>? protectedMetadata)
        {
            using var workbook = new XLWorkbook();

            foreach (var sheet in sheets)
            {
                var worksheet = workbook.Worksheets.Add(sheet.SheetName);
                var allProperties = sheet.DataType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                var properties = sheet.IncludedProperties != null
                    ? allProperties.Where(p => sheet.IncludedProperties.Contains(p.Name)).ToArray()
                    : allProperties;

                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cell(1, i + 1).Value = GetColumnHeader(properties[i]);
                }

                var columnByProperty = properties
                    .Select((p, i) => (p.Name, Column: i + 1))
                    .ToDictionary(x => x.Name, x => x.Column);

                int row = 2;
                foreach (var item in sheet.Data)
                {
                    for (int col = 0; col < properties.Length; col++)
                    {
                        if (sheet.FormulaColumns != null &&
                            sheet.FormulaColumns.TryGetValue(properties[col].Name, out var formulaTemplate))
                        {
                            worksheet.Cell(row, col + 1).FormulaA1 = ResolveFormula(formulaTemplate, row, columnByProperty);
                        }
                        else
                        {
                            var rawValue = ConvertExcelValue(properties[col].GetValue(item));
                            worksheet.Cell(row, col + 1).Value = XLCellValue.FromObject(rawValue);
                        }
                    }
                    row++;
                }

                if (sheet.ProtectedColumnNames is { Count: > 0 } && row > 2)
                    ProtectColumns(worksheet, properties, sheet.ProtectedColumnNames, lastDataRow: row - 1);
            }

            if (protectedMetadata is { Count: > 0 })
            {
                var metaSheet = workbook.Worksheets.Add(ProtectedMetadataSheetName);
                int row = 1;
                foreach (var (key, value) in protectedMetadata)
                {
                    metaSheet.Cell(row, 1).Value = key;
                    metaSheet.Cell(row, 2).Value = value;
                    row++;
                }
                metaSheet.Visibility = XLWorksheetVisibility.VeryHidden;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        /// <summary>Not a display name — never expected to be user-visible (VeryHidden).</summary>
        public const string ProtectedMetadataSheetName = "_BulkRatesMetadata";

        /// <summary>
        /// Locks the named columns' data cells (rows 2..lastDataRow) and protects the sheet,
        /// leaving every other cell — including any row a user adds below lastDataRow —
        /// editable. Row/column insertion, sorting and autofiltering stay allowed; this is a
        /// UX signal that a cell is reference-only, not the security boundary (the API
        /// re-validates and rejects any actual change regardless — DR-API-05).
        /// </summary>
        private static void ProtectColumns(
            IXLWorksheet worksheet, PropertyInfo[] properties, IReadOnlyCollection<string> protectedColumnNames, int lastDataRow)
        {
            worksheet.Range(2, 1, lastDataRow, properties.Length).Style.Protection.Locked = false;

            for (int col = 0; col < properties.Length; col++)
            {
                if (protectedColumnNames.Contains(properties[col].Name))
                    worksheet.Range(2, col + 1, lastDataRow, col + 1).Style.Protection.Locked = true;
            }

            worksheet.Protect(
                XLSheetProtectionElements.SelectEverything
                | XLSheetProtectionElements.FormatCells
                | XLSheetProtectionElements.InsertRows
                | XLSheetProtectionElements.Sort
                | XLSheetProtectionElements.AutoFilter);
        }

        private static string GetColumnHeader(PropertyInfo property)
        {
            var display = property.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? property.Name;
        }

        private static readonly Regex FormulaPlaceholder = new(@"\{(\w+)\}");

        /// <summary>
        /// Replaces each <c>{PropertyName}</c> placeholder in a <see cref="ExcelSheetDefinition.FormulaColumns"/>
        /// template with that property's own column letter plus <paramref name="row"/> (e.g. "D2").
        /// </summary>
        private static string ResolveFormula(string template, int row, IReadOnlyDictionary<string, int> columnByProperty)
        {
            return FormulaPlaceholder.Replace(template, m =>
            {
                var propertyName = m.Groups[1].Value;
                if (!columnByProperty.TryGetValue(propertyName, out var column))
                    throw new InvalidOperationException(
                        $"Formula template references unknown or excluded property '{propertyName}'.");
                return $"{GetColumnLetter(column)}{row}";
            });
        }

        private static string GetColumnLetter(int columnNumber)
        {
            var letters = new Stack<char>();
            while (columnNumber > 0)
            {
                var remainder = (columnNumber - 1) % 26;
                letters.Push((char)('A' + remainder));
                columnNumber = (columnNumber - 1) / 26;
            }
            return new string(letters.ToArray());
        }

        private object? ConvertExcelValue(object? value)
        {
            return value switch
            {
                null => null,
                DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                TimeOnly t => t.ToTimeSpan(),
                _ => value
            };
        }
    }
}
