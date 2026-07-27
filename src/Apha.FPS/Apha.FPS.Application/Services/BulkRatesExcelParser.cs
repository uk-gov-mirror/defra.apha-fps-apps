using ClosedXML.Excel;
using Apha.Common.Utilities.ExcelExport;
using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Parses Bulk Rates Excel uploads using ClosedXML.
    /// Validates worksheet presence and column headers; extracts rows into staging row models.
    /// Money values are read as decimals (not strings) per CR-13 / spec §12.
    /// The Excel Change column is ignored; Change is recalculated server-side.
    /// </summary>
    public class BulkRatesExcelParser
    {
        // ── Worksheet and column name constants (spec §11) ───────────────────────

        // Test / FEC worksheet
        private const string FecSheet = "FEC";
        private const string AgrupSheet = "AGRUP";

        private static readonly string[] FecRequiredColumns =
        [
            "TestCode", "Unit Price VLA", "Defra Unit Price", "FEC New",
            "Change", "Item Description", "Short Description", "Owner", "Comments"
        ];

        private static readonly string[] AgrupRequiredColumns =
        [
            "Test Code", "Buyer", "Agrup", "Agrup New",
            "Change", "No Required", "Date Created", "Active", "Comments",
            // DR-UI-02: routing columns. Cell values are optional per row (existing rows are
            // protected/reference-only; a new row supplies at least one) — only the column
            // headers themselves are required to exist.
            "Project Buyer Code", "Test Buyer Code", "Test Buyer Work Group"
        ];

        // Staff worksheet
        private const string StaffSheet = "Staff";
        private static readonly string[] StaffRequiredColumns = ["PcGrade", "Pay Rate", "NPR", "OHR"];

        // Animal worksheet
        private const string AnimalSheet = "Animals";
        private static readonly string[] AnimalRequiredColumns =
            ["AnimalType", "Species", "Security Level", "Daily Rate", "Defra Daily Rate", "Plan By Week"];

        // ── Public entry point ───────────────────────────────────────────────────

        public BulkRatesParseResult Parse(
            byte[] fileBytes, string filename, string jobName, Guid jobQueueId)
        {
            using var stream = new MemoryStream(fileBytes);

            if (!filename.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return new BulkRatesParseResult
                {
                    JobName = jobName,
                    JobQueueId = jobQueueId,
                    ParseErrors = ["Only .xlsx files are accepted."]
                };
            }

            using var workbook = new XLWorkbook(stream);

            var metadata = ReadProtectedMetadata(workbook);

            var result = jobName switch
            {
                "BulkTestRatesUpdate" => ParseTestRates(workbook, jobQueueId, jobName),
                "BulkStaffRatesUpdate" => ParseStaffRates(workbook, jobQueueId, jobName),
                "BulkAnimalRatesUpdate" => ParseAnimalRates(workbook, jobQueueId, jobName),
                _ => new BulkRatesParseResult
                {
                    JobName = jobName,
                    JobQueueId = jobQueueId,
                    ParseErrors = [$"Unrecognised job name: {jobName}"]
                }
            };

            // DR-VAL-03: attach protected metadata regardless of job type so the upload
            // flow can enforce the active-download-version contract without re-opening
            // the workbook a second time.
            return result with { WorkbookMetadata = metadata };
        }

        // ── FEC + AGRUP ──────────────────────────────────────────────────────────

        private static BulkRatesParseResult ParseTestRates(IXLWorkbook wb, Guid jobQueueId, string jobName)
        {
            var parseErrors = new List<string>();

            if (!TryGetSheet(wb, FecSheet, out var fecSheet))
                parseErrors.Add($"Worksheet '{FecSheet}' not found.");
            if (!TryGetSheet(wb, AgrupSheet, out var agrupSheet))
                parseErrors.Add($"Worksheet '{AgrupSheet}' not found.");

            if (parseErrors.Count > 0 || fecSheet == null || agrupSheet == null)
                return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, ParseErrors = parseErrors };

            var (fecHeaders, fecHeaderErrors) = ReadHeaders(fecSheet, FecRequiredColumns);
            var (agrupHeaders, agrupHeaderErrors) = ReadHeaders(agrupSheet, AgrupRequiredColumns);
            parseErrors.AddRange(fecHeaderErrors.Select(e => $"FEC: {e}"));
            parseErrors.AddRange(agrupHeaderErrors.Select(e => $"AGRUP: {e}"));

            if (parseErrors.Count > 0)
                return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, ParseErrors = parseErrors };

            var fecRows = new List<FecStagingRow>();
            foreach (var row in fecSheet.RowsUsed().Skip(1))
            {
                var testCode = CellString(row, fecHeaders, "TestCode");
                if (string.IsNullOrWhiteSpace(testCode)) continue;

                var unitPriceVla = CellDecimalOrNull(row, fecHeaders, "Unit Price VLA");
                var defraUnitPrice = CellDecimalOrNull(row, fecHeaders, "Defra Unit Price");
                var fecNew = CellDecimalOrNull(row, fecHeaders, "FEC New");

                fecRows.Add(new FecStagingRow
                {
                    JobQueueId = jobQueueId,
                    TestCode = testCode.Trim(),
                    UnitPriceVla = unitPriceVla,
                    DefraUnitPrice = defraUnitPrice,
                    FecNewRate = fecNew,
                    // Change calculated server-side; ignore Excel value
                    Change = fecNew.HasValue && defraUnitPrice.HasValue
                        ? fecNew.Value - defraUnitPrice.Value : null,
                    ItemDescription = CellString(row, fecHeaders, "Item Description"),
                    ShortDescription = CellString(row, fecHeaders, "Short Description"),
                    Owner = CellString(row, fecHeaders, "Owner"),
                    Comments = CellString(row, fecHeaders, "Comments")
                });
            }

            var agrupRows = new List<AgrupStagingRow>();
            foreach (var row in agrupSheet.RowsUsed().Skip(1))
            {
                var testCode = CellString(row, agrupHeaders, "Test Code");
                if (string.IsNullOrWhiteSpace(testCode)) continue;

                var agrup = CellDecimalOrNull(row, agrupHeaders, "Agrup");
                var agrupNew = CellDecimalOrNull(row, agrupHeaders, "Agrup New");

                agrupRows.Add(new AgrupStagingRow
                {
                    JobQueueId = jobQueueId,
                    TestCode = testCode.Trim(),
                    Buyer = CellString(row, agrupHeaders, "Buyer")?.Trim() ?? string.Empty,
                    Agrup = agrup,
                    AgrupNew = agrupNew,
                    Change = agrupNew.HasValue && agrup.HasValue
                        ? agrupNew.Value - agrup.Value : null,
                    NoRequired = CellDoubleOrNull(row, agrupHeaders, "No Required"),
                    DateCreated = CellDateOrNull(row, agrupHeaders, "Date Created"),
                    Active = CellShortOrNull(row, agrupHeaders, "Active"),
                    Comments = CellString(row, agrupHeaders, "Comments"),
                    ProjectBuyerCode = CellString(row, agrupHeaders, "Project Buyer Code"),
                    TestBuyerCode = CellString(row, agrupHeaders, "Test Buyer Code"),
                    TestBuyerWorkGroup = CellString(row, agrupHeaders, "Test Buyer Work Group")
                });
            }

            return new BulkRatesParseResult
            {
                JobName = jobName,
                JobQueueId = jobQueueId,
                FecRows = fecRows,
                AgrupRows = agrupRows
            };
        }

        // ── Staff ────────────────────────────────────────────────────────────────

        private static BulkRatesParseResult ParseStaffRates(IXLWorkbook wb, Guid jobQueueId, string jobName)
        {
            var parseErrors = new List<string>();

            if (!TryGetSheet(wb, StaffSheet, out var sheet))
                parseErrors.Add($"Worksheet '{StaffSheet}' not found.");

            if (parseErrors.Count > 0 || sheet == null)
                return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, ParseErrors = parseErrors };

            var (headers, headerErrors) = ReadHeaders(sheet, StaffRequiredColumns);
            if (headerErrors.Count > 0)
                return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, ParseErrors = headerErrors };

            var rows = new List<StaffStagingRow>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var grade = CellString(row, headers, "PcGrade");
                if (string.IsNullOrWhiteSpace(grade)) continue;

                rows.Add(new StaffStagingRow
                {
                    JobQueueId = jobQueueId,
                    PcGrade = grade.Trim(),
                    PayRate = CellDecimalOrNull(row, headers, "Pay Rate"),
                    Npr = CellDecimalOrNull(row, headers, "NPR"),
                    Ohr = CellDecimalOrNull(row, headers, "OHR")
                });
            }

            return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, StaffRows = rows };
        }

        // ── Animal ───────────────────────────────────────────────────────────────

        private static BulkRatesParseResult ParseAnimalRates(IXLWorkbook wb, Guid jobQueueId, string jobName)
        {
            var parseErrors = new List<string>();

            if (!TryGetSheet(wb, AnimalSheet, out var sheet))
                parseErrors.Add($"Worksheet '{AnimalSheet}' not found.");

            if (parseErrors.Count > 0 || sheet == null)
                return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, ParseErrors = parseErrors };

            var (headers, headerErrors) = ReadHeaders(sheet, AnimalRequiredColumns);
            if (headerErrors.Count > 0)
                return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, ParseErrors = headerErrors };

            var rows = new List<AnimalStagingRow>();
            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                var animalType = CellString(row, headers, "AnimalType");
                if (string.IsNullOrWhiteSpace(animalType)) continue;

                rows.Add(new AnimalStagingRow
                {
                    JobQueueId = jobQueueId,
                    AnimalType = animalType.Trim(),
                    Species = CellString(row, headers, "Species"),
                    SecurityLevel = CellString(row, headers, "Security Level"),
                    DailyRate = CellDecimalOrNull(row, headers, "Daily Rate"),
                    DefraDailyRate = CellDecimalOrNull(row, headers, "Defra Daily Rate"),
                    PlanByWeek = CellBoolOrNull(row, headers, "Plan By Week")
                });
            }

            return new BulkRatesParseResult { JobName = jobName, JobQueueId = jobQueueId, AnimalRows = rows };
        }

        // ── Cell reading helpers ─────────────────────────────────────────────────

        private static bool TryGetSheet(IXLWorkbook wb, string name, out IXLWorksheet? sheet)
        {
            sheet = wb.Worksheets.FirstOrDefault(w =>
                string.Equals(w.Name, name, StringComparison.OrdinalIgnoreCase));
            return sheet != null;
        }

        private static (Dictionary<string, int> headers, List<string> errors) ReadHeaders(
            IXLWorksheet sheet, string[] required)
        {
            var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var errors = new List<string>();
            var headerRow = sheet.Row(1);

            foreach (var cell in headerRow.CellsUsed())
                headers[cell.GetValue<string>().Trim()] = cell.Address.ColumnNumber;

            foreach (var col in required)
                if (!headers.ContainsKey(col))
                    errors.Add($"Required column '{col}' not found.");

            return (headers, errors);
        }

        private static string? CellString(IXLRow row, Dictionary<string, int> headers, string column)
        {
            if (!headers.TryGetValue(column, out var colNum)) return null;
            var val = row.Cell(colNum).GetValue<string>().Trim();
            return string.IsNullOrWhiteSpace(val) ? null : val;
        }

        private static decimal? CellDecimalOrNull(IXLRow row, Dictionary<string, int> headers, string column)
        {
            if (!headers.TryGetValue(column, out var colNum)) return null;
            var cell = row.Cell(colNum);
            if (cell.IsEmpty()) return null;
            return cell.TryGetValue<decimal>(out var val) ? val : null;
        }

        private static double? CellDoubleOrNull(IXLRow row, Dictionary<string, int> headers, string column)
        {
            if (!headers.TryGetValue(column, out var colNum)) return null;
            var cell = row.Cell(colNum);
            if (cell.IsEmpty()) return null;
            return cell.TryGetValue<double>(out var val) ? val : null;
        }

        private static DateTime? CellDateOrNull(IXLRow row, Dictionary<string, int> headers, string column)
        {
            if (!headers.TryGetValue(column, out var colNum)) return null;
            var cell = row.Cell(colNum);
            if (cell.IsEmpty()) return null;
            return cell.TryGetValue<DateTime>(out var val) ? val : null;
        }

        private static short? CellShortOrNull(IXLRow row, Dictionary<string, int> headers, string column)
        {
            if (!headers.TryGetValue(column, out var colNum)) return null;
            var cell = row.Cell(colNum);
            if (cell.IsEmpty()) return null;
            return cell.TryGetValue<short>(out var val) ? val : null;
        }

        private static bool? CellBoolOrNull(IXLRow row, Dictionary<string, int> headers, string column)
        {
            if (!headers.TryGetValue(column, out var colNum)) return null;
            var cell = row.Cell(colNum);
            if (cell.IsEmpty()) return null;
            if (cell.TryGetValue<bool>(out var b)) return b;
            if (cell.TryGetValue<int>(out var i)) return i != 0;
            return null;
        }

        /// <summary>
        /// DR-VAL-03: reads the VeryHidden protected-metadata sheet written by
        /// <c>ExcelExportService.ExportToExcelMultiSheet</c> (overload with
        /// <c>protectedMetadata</c>). Returns an empty dictionary if the sheet is absent —
        /// absence is not itself an error here; the upload flow decides what to do when
        /// the metadata is missing.
        /// </summary>
        private static IReadOnlyDictionary<string, string> ReadProtectedMetadata(IXLWorkbook wb)
        {
            var metaSheet = wb.Worksheets.FirstOrDefault(w =>
                string.Equals(w.Name, ExcelExportService.ProtectedMetadataSheetName,
                    StringComparison.OrdinalIgnoreCase));

            if (metaSheet == null)
                return new Dictionary<string, string>();

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in metaSheet.RowsUsed())
            {
                var key = row.Cell(1).GetValue<string>().Trim();
                var value = row.Cell(2).GetValue<string>().Trim();
                if (!string.IsNullOrEmpty(key))
                    result[key] = value;
            }
            return result;
        }
    }
}
