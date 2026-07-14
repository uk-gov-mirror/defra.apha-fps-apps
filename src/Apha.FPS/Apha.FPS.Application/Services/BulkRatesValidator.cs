using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Validates parsed staging rows against structural, duplicate-key, business and reference rules.
    /// Spec §12. Returns StagingValidationError rows and computed BulkRatesRowCounts.
    /// Severity: 'Error' blocks release; 'Warning' does not.
    /// </summary>
    public class BulkRatesValidator
    {
        private readonly IBulkRatesRepository _repository;

        public BulkRatesValidator(IBulkRatesRepository repository)
        {
            _repository = repository;
        }

        public async Task<BulkRatesValidationResult> ValidateAsync(
            BulkRatesParseResult parseResult,
            int fpsYear,
            string jobName,
            CancellationToken ct = default)
        {
            // File-level parse errors become Error-severity validation entries on row 0
            if (parseResult.HasParseErrors)
            {
                var fileErrors = parseResult.ParseErrors.Select((msg, i) => new StagingValidationError
                {
                    JobQueueId = parseResult.JobQueueId,
                    UploadVersion = 0,
                    SourceRowNumber = 0,
                    FieldName = "file",
                    ValidationCode = "FILE_ERROR",
                    Severity = "Error",
                    ValidationMessage = msg
                }).ToList();

                return new BulkRatesValidationResult { Errors = fileErrors, RowCounts = new() };
            }

            return jobName switch
            {
                "BulkTestRatesUpdate" => await ValidateFecAsync(parseResult, fpsYear, ct),
                "BulkStaffRatesUpdate" => ValidateStaff(parseResult),
                "BulkAnimalRatesUpdate" => ValidateAnimal(parseResult),
                _ => new BulkRatesValidationResult
                {
                    Errors =
                    [
                        new() { Severity = "Error", ValidationCode = "UNKNOWN_JOB", ValidationMessage = $"Unknown job: {jobName}" }
                    ]
                }
            };
        }

        // ── FEC + AGRUP validation ───────────────────────────────────────────────

        private async Task<BulkRatesValidationResult> ValidateFecAsync(
            BulkRatesParseResult parseResult, int fpsYear, CancellationToken ct)
        {
            var errors = new List<StagingValidationError>();
            var fecRows = parseResult.FecRows;
            var agrupRows = parseResult.AgrupRows;
            var jobQueueId = parseResult.JobQueueId;

            // ── Duplicate FEC test codes within the upload
            var fecDuplicates = fecRows.GroupBy(r => r.TestCode, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

            // ── Bulk reference check for FEC test codes
            var allTestCodes = fecRows.Select(r => r.TestCode).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var existingTestCodes = await _repository.GetExistingTestCodesAsync(allTestCodes, fpsYear, ct);

            // ── Bulk reference check for AGRUP keys
            var agrupKeys = agrupRows
                .Select(r => (r.TestCode, r.Buyer))
                .ToHashSet();
            var existingAgrupKeys = await _repository.GetExistingAgrupKeysAsync(agrupKeys, fpsYear, ct);

            // ── FEC row validation (spec §12.1 + §12.2)
            int fecInsert = 0, fecUpdate = 0, fecUnchanged = 0;
            for (int rowNum = 0; rowNum < fecRows.Count; rowNum++)
            {
                var row = fecRows[rowNum];
                var sourceRow = rowNum + 2; // 1-based, row 1 is header

                if (fecDuplicates.Contains(row.TestCode))
                    AddError(errors, jobQueueId, sourceRow, "testcode", "DUPLICATE_TEST_CODE",
                        $"TestCode '{row.TestCode}' appears more than once in the FEC worksheet.");

                if (row.FecNewRate == null)
                    AddError(errors, jobQueueId, sourceRow, "fecnewrate", "MISSING_FEC_NEW_RATE",
                        "FEC New rate is mandatory for every row.");

                if (row.FecNewRate.HasValue && row.FecNewRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "fecnewrate", "NEGATIVE_RATE",
                        "Negative rates are not permitted.");

                var isNew = !existingTestCodes.Contains(row.TestCode);
                if (isNew)
                {
                    fecInsert++;
                    if (string.IsNullOrWhiteSpace(row.ItemDescription))
                        AddError(errors, jobQueueId, sourceRow, "itemdescription", "MISSING_FOR_INSERT",
                            "Item Description is required for new Test Code inserts.");
                    if (string.IsNullOrWhiteSpace(row.ShortDescription))
                        AddError(errors, jobQueueId, sourceRow, "shortdescription", "MISSING_FOR_INSERT",
                            "Short Description is required for new Test Code inserts.");
                    if (string.IsNullOrWhiteSpace(row.Owner))
                        AddError(errors, jobQueueId, sourceRow, "owner", "MISSING_FOR_INSERT",
                            "Owner is required for new Test Code inserts.");
                }
                else
                {
                    // Existing row: changes to description/owner are ignored (add warning if materially different)
                    if (!string.IsNullOrWhiteSpace(row.ItemDescription))
                        AddWarning(errors, jobQueueId, sourceRow, "itemdescription", "IGNORED_ON_UPDATE",
                            "Item Description changes are ignored for existing Test Codes (update-only modifies price columns).");

                    // Classify: unchanged if FEC New == existing DefraUnitPrice
                    // We don't have the existing price here, so we classify as Update
                    // (the worker re-classifies precisely at execution)
                    fecUpdate++;
                }
            }

            // ── AGRUP row validation (spec §12.3)
            var fecTestCodesInUpload = allTestCodes;
            int agrupInsert = 0, agrupUpdate = 0, agrupUnchanged = 0;
            var agrupDuplicates = agrupRows
                .GroupBy(r => (r.TestCode.ToUpperInvariant(), r.Buyer.ToUpperInvariant()))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            for (int rowNum = 0; rowNum < agrupRows.Count; rowNum++)
            {
                var row = agrupRows[rowNum];
                var sourceRow = rowNum + 2;

                if (agrupDuplicates.Contains((row.TestCode.ToUpperInvariant(), row.Buyer.ToUpperInvariant())))
                    AddError(errors, jobQueueId, sourceRow, "testcode+buyer", "DUPLICATE_AGRUP_KEY",
                        $"TestCode+Buyer combination '{row.TestCode}/{row.Buyer}' appears more than once.");

                // Test Code must exist in current year OR be a new insert in the same FEC upload
                if (!existingTestCodes.Contains(row.TestCode) && !fecTestCodesInUpload.Contains(row.TestCode))
                    AddError(errors, jobQueueId, sourceRow, "testcode", "TEST_CODE_NOT_FOUND",
                        $"TestCode '{row.TestCode}' does not exist in FPS year {fpsYear} and is not being inserted by this FEC upload.");

                var isNew = !existingAgrupKeys.Contains((row.TestCode, row.Buyer));
                if (isNew)
                {
                    agrupInsert++;
                    if (row.AgrupNew == null)
                        AddError(errors, jobQueueId, sourceRow, "agrupnew", "MISSING_FOR_INSERT",
                            "Agrup New is required for new TestCode+Buyer inserts.");
                }
                else
                {
                    if (row.AgrupNew == null)
                    {
                        agrupUnchanged++;
                        continue; // blank AgrupNew on existing row = no update (spec §11.2)
                    }
                    agrupUpdate++;
                }

                if (row.AgrupNew.HasValue && row.AgrupNew.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "agrupnew", "NEGATIVE_RATE",
                        "Negative rates are not permitted.");

                // "Same as FEC" comments validation
                if (string.Equals(row.Comments, "Same as FEC", StringComparison.OrdinalIgnoreCase))
                {
                    var fecMatch = fecRows.FirstOrDefault(f =>
                        string.Equals(f.TestCode, row.TestCode, StringComparison.OrdinalIgnoreCase));
                    if (fecMatch != null && row.AgrupNew.HasValue && fecMatch.FecNewRate.HasValue
                        && row.AgrupNew.Value != fecMatch.FecNewRate.Value)
                        AddError(errors, jobQueueId, sourceRow, "agrupnew", "SAME_AS_FEC_MISMATCH",
                            $"Comments is 'Same as FEC' but Agrup New ({row.AgrupNew}) does not equal FEC New ({fecMatch.FecNewRate}) for TestCode '{row.TestCode}'.");
                }
            }

            var totalFec = fecRows.Count;
            var totalAgrup = agrupRows.Count;
            var counts = new BulkRatesRowCounts
            {
                Total = totalFec + totalAgrup,
                Insert = fecInsert + agrupInsert,
                Update = fecUpdate + agrupUpdate,
                Unchanged = fecUnchanged + agrupUnchanged,
                Invalid = errors.Count(e => e.Severity == "Error"),
                Valid = totalFec + totalAgrup - errors.Count(e => e.Severity == "Error")
            };

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        // ── Staff validation ─────────────────────────────────────────────────────

        private static BulkRatesValidationResult ValidateStaff(BulkRatesParseResult parseResult)
        {
            var errors = new List<StagingValidationError>();
            var rows = parseResult.StaffRows;
            var jobQueueId = parseResult.JobQueueId;

            var duplicates = rows.GroupBy(r => r.PcGrade, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var sourceRow = i + 2;

                if (string.IsNullOrWhiteSpace(row.PcGrade))
                    AddError(errors, jobQueueId, sourceRow, "pcgrade", "MISSING_GRADE", "PcGrade is required.");
                else if (duplicates.Contains(row.PcGrade))
                    AddError(errors, jobQueueId, sourceRow, "pcgrade", "DUPLICATE_GRADE",
                        $"Grade '{row.PcGrade}' appears more than once.");

                if (row.PayRate.HasValue && row.PayRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "payrate", "NEGATIVE_RATE", "Negative rates are not permitted.");
                if (row.Npr.HasValue && row.Npr.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "npr", "NEGATIVE_RATE", "Negative rates are not permitted.");
                if (row.Ohr.HasValue && row.Ohr.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "ohr", "NEGATIVE_RATE", "Negative rates are not permitted.");
            }

            return new BulkRatesValidationResult
            {
                Errors = errors,
                RowCounts = new BulkRatesRowCounts
                {
                    Total = rows.Count,
                    Update = rows.Count,
                    Invalid = errors.Count(e => e.Severity == "Error"),
                    Valid = rows.Count - errors.Count(e => e.Severity == "Error")
                }
            };
        }

        // ── Animal validation ────────────────────────────────────────────────────

        private static BulkRatesValidationResult ValidateAnimal(BulkRatesParseResult parseResult)
        {
            var errors = new List<StagingValidationError>();
            var rows = parseResult.AnimalRows;
            var jobQueueId = parseResult.JobQueueId;

            var duplicates = rows.GroupBy(r => r.AnimalType, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1).Select(g => g.Key).ToHashSet(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var sourceRow = i + 2;

                if (string.IsNullOrWhiteSpace(row.AnimalType))
                    AddError(errors, jobQueueId, sourceRow, "animaltype", "MISSING_ANIMAL_TYPE", "AnimalType is required.");
                else if (duplicates.Contains(row.AnimalType))
                    AddError(errors, jobQueueId, sourceRow, "animaltype", "DUPLICATE_ANIMAL_TYPE",
                        $"AnimalType '{row.AnimalType}' appears more than once.");

                if (row.DailyRate.HasValue && row.DailyRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "dailyrate", "NEGATIVE_RATE", "Negative rates are not permitted.");
                if (row.DefraDailyRate.HasValue && row.DefraDailyRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "defradailyrate", "NEGATIVE_RATE", "Negative rates are not permitted.");
            }

            return new BulkRatesValidationResult
            {
                Errors = errors,
                RowCounts = new BulkRatesRowCounts
                {
                    Total = rows.Count,
                    Update = rows.Count,
                    Invalid = errors.Count(e => e.Severity == "Error"),
                    Valid = rows.Count - errors.Count(e => e.Severity == "Error")
                }
            };
        }

        // ── Error helpers ────────────────────────────────────────────────────────

        private static void AddError(
            List<StagingValidationError> list, Guid jobQueueId,
            int sourceRowNumber, string? fieldName, string code, string message)
        {
            list.Add(new StagingValidationError
            {
                JobQueueId = jobQueueId,
                UploadVersion = 0, // set to actual version by service before persisting
                SourceRowNumber = sourceRowNumber,
                FieldName = fieldName,
                ValidationCode = code,
                Severity = "Error",
                ValidationMessage = message
            });
        }

        private static void AddWarning(
            List<StagingValidationError> list, Guid jobQueueId,
            int sourceRowNumber, string? fieldName, string code, string message)
        {
            list.Add(new StagingValidationError
            {
                JobQueueId = jobQueueId,
                UploadVersion = 0,
                SourceRowNumber = sourceRowNumber,
                FieldName = fieldName,
                ValidationCode = code,
                Severity = "Warning",
                ValidationMessage = message
            });
        }
    }
}
