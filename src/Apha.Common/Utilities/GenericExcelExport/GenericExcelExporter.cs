using Apha.Common.Utilities.GenericExcelExport.Attributes;
using ClosedXML.Excel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Apha.Common.Utilities.GenericExcelExport
{
    /// <inheritdoc cref="IGenericExcelExporter"/>
    public sealed class GenericExcelExporter : IGenericExcelExporter
    {
        public byte[] Export<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            var rows = data ?? Enumerable.Empty<T>();
            var columns = GetColumns(typeof(T));

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(SanitiseSheetName(sheetName));

            WriteHeader(worksheet, columns);
            var lastDataRow = WriteRows(worksheet, columns, rows);
            ApplyStyling(worksheet, columns, lastDataRow);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void WriteHeader(IXLWorksheet worksheet, IReadOnlyList<ExportColumn> columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                worksheet.Cell(1, i + 1).Value = columns[i].Header;
            }
        }

        private static int WriteRows<T>(IXLWorksheet worksheet, IReadOnlyList<ExportColumn> columns, IEnumerable<T> rows)
        {
            int row = 2;
            foreach (var item in rows)
            {
                for (int col = 0; col < columns.Count; col++)
                {
                    var rawValue = ConvertExcelValue(columns[col].Property.GetValue(item));
                    worksheet.Cell(row, col + 1).Value = XLCellValue.FromObject(rawValue);
                }
                row++;
            }
            return Math.Max(1, row - 1);
        }

        private static void ApplyStyling(IXLWorksheet worksheet, IReadOnlyList<ExportColumn> columns, int lastDataRow)
        {
            int lastColumn = Math.Max(1, columns.Count);

            var headerRange = worksheet.Range(1, 1, 1, lastColumn);
            headerRange.Style.Font.Bold = true;

            var allCellsRange = worksheet.Range(1, 1, lastDataRow, lastColumn);
            allCellsRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            allCellsRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            for (int i = 0; i < columns.Count; i++)
            {
                var column = worksheet.Column(i + 1);

                if (!string.IsNullOrWhiteSpace(columns[i].Format))
                {
                    column.Style.NumberFormat.Format = columns[i].Format;
                }

                if (columns[i].Width > 0)
                {
                    column.Width = columns[i].Width;
                }
                else
                {
                    column.AdjustToContents();
                }
            }
        }

        private static IReadOnlyList<ExportColumn> GetColumns(Type type)
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && p.GetIndexParameters().Length == 0)
                .Where(p => p.GetCustomAttribute<ExcelIgnoreAttribute>() is null)
                .Select((p, index) => new ExportColumn(p, index))
                .OrderBy(c => c.Order)
                .ThenBy(c => c.DeclarationIndex)
                .ToList();
        }

        private static string SanitiseSheetName(string sheetName)
        {
            if (string.IsNullOrWhiteSpace(sheetName))
            {
                return "Sheet1";
            }

            // Excel sheet names are limited to 31 chars and cannot contain: \ / ? * [ ] :
            var cleaned = new string(sheetName.Where(c => !"\\/?*[]:".Contains(c)).ToArray());
            return cleaned.Length > 31 ? cleaned[..31] : cleaned;
        }

        private static object? ConvertExcelValue(object? value)
        {
            return value switch
            {
                null => null,
                DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                TimeOnly t => t.ToTimeSpan(),
                _ => value
            };
        }

        private sealed class ExportColumn
        {
            public ExportColumn(PropertyInfo property, int declarationIndex)
            {
                Property = property;
                DeclarationIndex = declarationIndex;

                var excelColumn = property.GetCustomAttribute<ExcelColumnAttribute>();
                var display = property.GetCustomAttribute<DisplayAttribute>();
                var displayFormat = property.GetCustomAttribute<DisplayFormatAttribute>();

                Header = FirstNonEmpty(excelColumn?.Name, display?.GetName(), property.Name);
                Order = excelColumn?.Order ?? int.MaxValue;
                Width = excelColumn?.Width ?? 0;
                Format = !string.IsNullOrWhiteSpace(excelColumn?.Format)
                    ? excelColumn!.Format
                    : ExtractNumberFormat(displayFormat?.DataFormatString);
            }

            public PropertyInfo Property { get; }
            public int DeclarationIndex { get; }
            public string Header { get; }
            public int Order { get; }
            public double Width { get; }
            public string? Format { get; }

            private static string FirstNonEmpty(params string?[] candidates)
            {
                foreach (var candidate in candidates)
                {
                    if (!string.IsNullOrWhiteSpace(candidate))
                    {
                        return candidate;
                    }
                }
                return string.Empty;
            }

            // Converts a .NET composite format string such as "{0:C}" or "{0:dd/MM/yyyy}"
            // into an Excel number format string that ClosedXML understands.
            private static string? ExtractNumberFormat(string? dataFormatString)
            {
                if (string.IsNullOrWhiteSpace(dataFormatString))
                {
                    return null;
                }

                string token = dataFormatString;
                int start = dataFormatString.IndexOf(':');
                int end = dataFormatString.IndexOf('}');
                if (start >= 0 && end > start)
                {
                    token = dataFormatString.Substring(start + 1, end - start - 1);
                }

                // Map common .NET standard numeric specifiers to Excel format codes.
                return token switch
                {
                    "C" or "c" => "£#,##0.00",
                    "C0" or "c0" => "£#,##0",
                    "N" or "n" or "N2" or "n2" => "#,##0.00",
                    "N0" or "n0" => "#,##0",
                    "P" or "p" or "P2" or "p2" => "0.00%",
                    "P0" or "p0" => "0%",
                    "D" or "d" => "dd/MM/yyyy",
                    _ => token
                };
            }
        }
    }
}
