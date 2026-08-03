using Apha.Common.BulkRates.Validation;
using Apha.Common.Constants;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Orchestrates Bulk Rates upload/release validation. FEC/AGRUP business rules live in
    /// Apha.Common.BulkRates.Validation.IBulkRatesValidationService (Phase D2) —
    /// this class's job (Phase D3) is to build that service's ValidationContext from bulk
    /// repository reads plus the parsed/staged rows, call it once, and map the returned
    /// ValidationFinding list onto StagingValidationError/BulkRatesRowCounts. It must not
    /// reimplement any FEC/AGRUP rule itself. Staff/Animal validation predates this service
    /// and is outside its scope (its ValidationContext has no Staff/Animal shape), so those two
    /// stay exactly as before.
    /// </summary>
    public class BulkRatesValidator
    {
        // Must stay in sync with StaffAnimalValidationVersion.Current in Apha.BatchJobs.
        private const int StaffValidationVersion = 1;

        private readonly IBulkRatesRepository _repository;
        private readonly IBulkRatesValidationService _validationService;

        public BulkRatesValidator(IBulkRatesRepository repository, IBulkRatesValidationService validationService)
        {
            _repository = repository;
            _validationService = validationService;
        }

        public async Task<BulkRatesValidationResult> ValidateAsync(
            BulkRatesParseResult parseResult,
            int fpsYear,
            string jobName,
            int uploadVersion,
            int? downloadVersion,
            CancellationToken ct = default)
        {
            // File-level parse errors become Error-severity validation entries on row 0
            if (parseResult.HasParseErrors)
            {
                var fileErrors = parseResult.ParseErrors.Select((msg, i) => new StagingValidationError
                {
                    JobQueueId = parseResult.JobQueueId,
                    UploadVersion = uploadVersion,
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
                BulkRatesJobNames.Fec => await ValidateFecAsync(
                    parseResult.JobQueueId, fpsYear, uploadVersion, downloadVersion,
                    parseResult.FecRows, parseResult.AgrupRows, ct),
                BulkRatesJobNames.Staff => await ValidateStaffAsync(parseResult, fpsYear, ct),
                BulkRatesJobNames.Animal => await ValidateAnimalAsync(parseResult, fpsYear, ct),
                _ => new BulkRatesValidationResult
                {
                    Errors =
                    [
                        new() { Severity = "Error", ValidationCode = "UNKNOWN_JOB", ValidationMessage = $"Unknown job: {jobName}" }
                    ]
                }
            };
        }

        // ── Release-time re-validation + freeze ──────────────────────────────────

        /// <summary>
        /// Re-runs validation against the currently staged rows (read back from
        /// the DB — release time has no fresh parseResult in hand) and the current live/
        /// reference data, and packages the per-row classification for
        /// BulkRatesRequestService.ReleaseForApprovalAsync to freeze onto staging (CR056).
        /// This is a safety net: live reference data can drift between upload and release, so
        /// release must not blindly trust the validation errors recorded at upload time.
        /// </summary>
        public async Task<BulkRatesFreezeResult> BuildFreezeAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion, CancellationToken ct = default)
        {
            var fecRows = await _repository.GetFecStagingRowsAsync(jobQueueId, ct);
            var agrupRows = await _repository.GetAgrupStagingRowsAsync(jobQueueId, ct);

            var context = await BuildContextAsync(
                jobQueueId, fpsYear, uploadVersion, downloadVersion, fecRows, agrupRows,
                includeWorkerOnlyChecks: false, ct);
            var findings = _validationService.Validate(context);

            var blockingErrors = findings.Where(f => f.Severity == ValidationSeverity.Error).ToList();

            var fecFreezes = findings
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED" && string.Equals(f.Sheet, "FEC", StringComparison.OrdinalIgnoreCase))
                .Select(f =>
                {
                    context.LiveFecLookup.TryGetValue(BulkRatesValidationKeys.TestCode(f.BusinessKey!), out var live);
                    return new BulkRatesFreezeEntry(f.BusinessKey!, null, f.CalculatedAction!, f.EffectiveNewRate, live?.DefraUnitPrice);
                })
                .ToList();

            var agrupFreezes = findings
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED" && string.Equals(f.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase))
                .Select(f =>
                {
                    var (testCode, buyer) = SplitBusinessKey(f.Sheet, f.BusinessKey);
                    context.LiveAgrupLookup.TryGetValue(BulkRatesValidationKeys.AgrupKey(testCode!, buyer!), out var live);
                    return new BulkRatesFreezeEntry(testCode!, buyer, f.CalculatedAction!, f.EffectiveNewRate, live?.UnitPrice);
                })
                .ToList();

            return new BulkRatesFreezeResult
            {
                BlockingErrors = blockingErrors,
                FecFreezes = fecFreezes,
                AgrupFreezes = agrupFreezes,
                StaffFreezes = await BuildStaffFreezesAsync(jobQueueId, fpsYear, ct)
            };
        }

        // Computes staff freeze entries by comparing staging rows to live profitcentregrade rates.
        private async Task<IReadOnlyList<StaffFreezeEntry>> BuildStaffFreezesAsync(
            Guid jobQueueId, int fpsYear, CancellationToken ct)
        {
            var staffRows = await _repository.GetStaffStagingRowsAsync(jobQueueId, ct);
            if (staffRows.Count == 0)
                return [];

            var liveRows = await _repository.GetStaffRowsForExportAsync(fpsYear, ct);
            var liveLookup = liveRows.ToDictionary(r => r.PcGrade, StringComparer.OrdinalIgnoreCase);

            return staffRows.Select(row =>
            {
                if (!liveLookup.TryGetValue(row.PcGrade, out var live))
                    return new StaffFreezeEntry(row.PcGrade, "Update", StaffValidationVersion,
                        null, null, null, row.PayRate, row.Npr, row.Ohr);

                var changed = (row.PayRate.HasValue && row.PayRate.Value != live.PayRate)
                           || (row.Npr.HasValue     && row.Npr.Value     != live.Npr)
                           || (row.Ohr.HasValue     && row.Ohr.Value     != live.Ohr);

                return new StaffFreezeEntry(row.PcGrade, changed ? "Update" : "Unchanged", StaffValidationVersion,
                    live.PayRate, live.Npr, live.Ohr, row.PayRate, row.Npr, row.Ohr);
            }).ToList();
        }

        // ── Calculated action for display, when nothing is frozen yet ──────────────

        /// <summary>
        /// Computes the same classification <see cref="BuildFreezeAsync"/> would freeze,
        /// for display purposes before a request has ever been released (calculated_action is
        /// still null on staging). Returns raw ROW_CLASSIFIED findings rather than a bespoke
        /// shape — callers (the Detail-page staging grid) read <see cref="ValidationFinding.BusinessKey"/>/
        /// <see cref="ValidationFinding.CalculatedAction"/>/<see cref="ValidationFinding.EffectiveNewRate"/>
        /// directly, so this never becomes a second, UI-owned re-implementation of the classification rule.
        /// </summary>
        public async Task<IReadOnlyList<ValidationFinding>> GetCalculatedActionsAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion, CancellationToken ct = default)
        {
            var fecRows = await _repository.GetFecStagingRowsAsync(jobQueueId, ct);
            var agrupRows = await _repository.GetAgrupStagingRowsAsync(jobQueueId, ct);

            var context = await BuildContextAsync(
                jobQueueId, fpsYear, uploadVersion, downloadVersion, fecRows, agrupRows,
                includeWorkerOnlyChecks: false, ct);

            return _validationService.Validate(context)
                .Where(f => f.ValidationCode == "ROW_CLASSIFIED")
                .ToList();
        }

        // ── FEC + AGRUP validation ────────────────────────────────────────────────

        private async Task<BulkRatesValidationResult> ValidateFecAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion,
            IReadOnlyList<FecStagingRow> fecRows, IReadOnlyList<AgrupStagingRow> agrupRows,
            CancellationToken ct)
        {
            var context = await BuildContextAsync(
                jobQueueId, fpsYear, uploadVersion, downloadVersion, fecRows, agrupRows,
                includeWorkerOnlyChecks: false, ct);
            var findings = _validationService.Validate(context);

            // ROW_CLASSIFIED findings (Info severity) are the per-row calculated-action
            // output, not user-facing validation errors — they drive RowCounts below, not
            // fps.staging_validation_error (the Detail page reads them from the frozen staging
            // columns instead, once release-time re-validation has run).
            var errors = findings
                .Where(f => f.ValidationCode != "ROW_CLASSIFIED")
                .Select(f => MapFinding(f, jobQueueId, uploadVersion))
                .ToList();

            var counts = ComputeRowCounts(fecRows.Count, agrupRows.Count, findings, errors);

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        /// <summary>
        /// Builds the ValidationContext from bulk repository reads (§3.2 — batched,
        /// never per-row) plus the given FEC/AGRUP rows, which the two callers above source
        /// differently: a fresh parse at upload time, or a DB read-back at release time.
        /// </summary>
        private async Task<ValidationContext> BuildContextAsync(
            Guid jobQueueId, int fpsYear, int uploadVersion, int? downloadVersion,
            IReadOnlyList<FecStagingRow> fecRows, IReadOnlyList<AgrupStagingRow> agrupRows,
            bool includeWorkerOnlyChecks, CancellationToken ct)
        {
            var liveFecRows = await _repository.GetFecRowsForExportAsync(fpsYear, ct);
            var liveAgrupRows = await _repository.GetAgrupRowsForExportAsync(fpsYear, ct);

            var liveFecLookup = liveFecRows.ToDictionary(
                r => BulkRatesValidationKeys.TestCode(r.TestCode),
                r => new LiveFecRow { TestCode = r.TestCode, UnitPriceVla = r.UnitPriceVla, DefraUnitPrice = r.DefraUnitPrice });

            var liveAgrupLookup = liveAgrupRows.ToDictionary(
                r => BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer),
                r => new LiveAgrupRow
                {
                    TestCode = r.TestCode,
                    Buyer = r.Buyer,
                    UnitPrice = r.Agrup,
                    ProjectBuyerCode = r.ProjectBuyerCode,
                    TestBuyerCode = r.TestBuyerCode
                });

            // Project/capability lookups are bulk (§3.2) but scoped to only the routing values
            // this upload actually supplies — not the entire reference table.
            var projectCodes = agrupRows
                .Where(r => !string.IsNullOrWhiteSpace(r.ProjectBuyerCode))
                .Select(r => r.ProjectBuyerCode!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var projectLookup = await _repository.GetExistingProjectCodesAsync(projectCodes, fpsYear, ct);

            var capabilityPairs = agrupRows
                .Where(r => !string.IsNullOrWhiteSpace(r.TestBuyerWorkGroup))
                .Select(r => (r.TestCode, r.TestBuyerWorkGroup!))
                .ToHashSet();
            var capabilityLookup = await _repository.GetExistingCapabilityPairsAsync(capabilityPairs, fpsYear, ct);

            IReadOnlyList<DownloadedSnapshotKey> frozenSnapshot = [];
            if (downloadVersion.HasValue)
            {
                var snapshotFec = await _repository.GetFecSnapshotRowsAsync(jobQueueId, downloadVersion.Value, ct);
                var snapshotAgrup = await _repository.GetAgrupSnapshotRowsAsync(jobQueueId, downloadVersion.Value, ct);
                frozenSnapshot = snapshotFec
                    .Select(r => new DownloadedSnapshotKey { Sheet = "FEC", TestCode = r.TestCode, SourceRate = r.DefraUnitPrice })
                    .Concat(snapshotAgrup.Select(r => new DownloadedSnapshotKey { Sheet = "AGRUP", TestCode = r.TestCode, Buyer = r.Buyer, SourceRate = r.Agrup }))
                    .ToList();
            }

            var stagedFec = fecRows.Select((r, i) => new ValidationFecRow
            {
                TestCode = r.TestCode,
                FecNewRate = r.FecNewRate,
                ItemDescription = r.ItemDescription,
                ShortDescription = r.ShortDescription,
                Owner = r.Owner,
                Comments = r.Comments,
                SourceRow = i + 2
            }).ToList();

            var stagedAgrup = agrupRows.Select((r, i) =>
            {
                liveAgrupLookup.TryGetValue(BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer), out var live);
                return new ValidationAgrupRow
                {
                    TestCode = r.TestCode,
                    Buyer = r.Buyer,
                    AgrupNew = r.AgrupNew,
                    // Existing rows: the workbook has no column yet to assert a routing value
                    // (Phase D5 adds that). Until then, an absent staged value must echo
                    // the live one rather than read as "blanked out" — otherwise every ordinary
                    // rate-only update on a row that already has routing data would falsely trip
                    // DR-API-05's immutability check below. New rows have no live value to echo,
                    // so they correctly stay null and fall through to DR-API-04's
                    // MISSING_ROUTING_FIELD until a workbook can actually supply one.
                    ProjectBuyerCode = r.ProjectBuyerCode ?? live?.ProjectBuyerCode,
                    TestBuyerCode = r.TestBuyerCode ?? live?.TestBuyerCode,
                    TestBuyerWorkGroup = r.TestBuyerWorkGroup,
                    Comments = r.Comments,
                    SourceRow = i + 2
                };
            }).ToList();

            return new ValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                DownloadVersion = downloadVersion,
                UploadVersion = uploadVersion,
                LiveFecLookup = liveFecLookup,
                LiveAgrupLookup = liveAgrupLookup,
                ProjectLookup = projectLookup,
                CapabilityLookup = capabilityLookup,
                StagedFecRows = stagedFec,
                StagedAgrupRows = stagedAgrup,
                FrozenSnapshot = frozenSnapshot,
                IncludeWorkerOnlyChecks = includeWorkerOnlyChecks
            };
        }

        private static StagingValidationError MapFinding(ValidationFinding finding, Guid jobQueueId, int uploadVersion)
        {
            var (testCode, buyer) = SplitBusinessKey(finding.Sheet, finding.BusinessKey);
            return new StagingValidationError
            {
                JobQueueId = jobQueueId,
                UploadVersion = uploadVersion,
                SourceRowNumber = finding.SourceRow ?? 0,
                FieldName = finding.Field,
                ValidationCode = finding.ValidationCode,
                Severity = finding.Severity,
                ValidationMessage = finding.Message,
                SheetName = finding.Sheet,
                TestCode = testCode,
                Buyer = buyer,
                IsRequestLevel = finding.IsRequestLevel
            };
        }

        /// <summary>AGRUP findings carry "TestCode/Buyer" as a single BusinessKey string; everything else is a plain TestCode.</summary>
        private static (string? TestCode, string? Buyer) SplitBusinessKey(string sheet, string? businessKey)
        {
            if (businessKey is null) return (null, null);
            if (!string.Equals(sheet, "AGRUP", StringComparison.OrdinalIgnoreCase)) return (businessKey, null);
            var parts = businessKey.Split('/', 2);
            return parts.Length == 2 ? (parts[0], parts[1]) : (businessKey, null);
        }

        private static BulkRatesRowCounts ComputeRowCounts(
            int totalFec, int totalAgrup, IReadOnlyList<ValidationFinding> findings, IReadOnlyList<StagingValidationError> errors)
        {
            int insert = 0, update = 0, unchanged = 0;
            foreach (var f in findings)
            {
                if (f.ValidationCode != "ROW_CLASSIFIED") continue;
                switch (f.CalculatedAction)
                {
                    case ValidationCalculatedAction.Insert: insert++; break;
                    case ValidationCalculatedAction.Update:
                    case ValidationCalculatedAction.ZeroRateWithdrawal: update++; break;
                    case ValidationCalculatedAction.NoChange: unchanged++; break;
                }
            }

            var total = totalFec + totalAgrup;
            var invalid = errors.Count(e => e.Severity == ValidationSeverity.Error);
            return new BulkRatesRowCounts
            {
                Total = total,
                Insert = insert,
                Update = update,
                Unchanged = unchanged,
                Invalid = invalid,
                Valid = total - invalid
            };
        }

        // ── Staff validation ──────────────────────────────────────────────────────

        private async Task<BulkRatesValidationResult> ValidateStaffAsync(
            BulkRatesParseResult parseResult, int fpsYear, CancellationToken ct)
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
                    AddError(errors, jobQueueId, sourceRow, "pcgrade", "MISSING_GRADE", "PcGrade is required.",
                        sheetName: "Staff");
                else if (duplicates.Contains(row.PcGrade))
                    AddError(errors, jobQueueId, sourceRow, "pcgrade", "DUPLICATE_GRADE",
                        $"Grade '{row.PcGrade}' appears more than once.", sheetName: "Staff");

                if (row.PayRate.HasValue && row.PayRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "payrate", "NEGATIVE_RATE", "Negative rates are not permitted.",
                        sheetName: "Staff");
                if (row.Npr.HasValue && row.Npr.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "npr", "NEGATIVE_RATE", "Negative rates are not permitted.",
                        sheetName: "Staff");
                if (row.Ohr.HasValue && row.Ohr.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "ohr", "NEGATIVE_RATE", "Negative rates are not permitted.",
                        sheetName: "Staff");
            }

            // Mirror worker comparison to compute accurate Update/Unchanged counts
            var liveRows = await _repository.GetStaffRowsForExportAsync(fpsYear, ct);
            var liveLookup = liveRows.ToDictionary(r => r.PcGrade, StringComparer.OrdinalIgnoreCase);
            int update = 0, unchanged = 0;
            foreach (var row in rows)
            {
                if (!liveLookup.TryGetValue(row.PcGrade ?? string.Empty, out var live))
                { update++; continue; }
                var changed = (row.PayRate.HasValue && row.PayRate.Value != live.PayRate)
                           || (row.Npr.HasValue     && row.Npr.Value     != live.Npr)
                           || (row.Ohr.HasValue     && row.Ohr.Value     != live.Ohr);
                if (changed) update++; else unchanged++;
            }

            var invalid = errors.Count(e => e.Severity == "Error");
            return new BulkRatesValidationResult
            {
                Errors = errors,
                RowCounts = new BulkRatesRowCounts
                {
                    Total = rows.Count,
                    Update = update,
                    Unchanged = unchanged,
                    Invalid = invalid,
                    Valid = rows.Count - invalid
                }
            };
        }

        // ── Animal validation ─────────────────────────────────────────────────────

        private async Task<BulkRatesValidationResult> ValidateAnimalAsync(
            BulkRatesParseResult parseResult, int fpsYear, CancellationToken ct)
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
                    AddError(errors, jobQueueId, sourceRow, "animaltype", "MISSING_ANIMAL_TYPE", "AnimalType is required.",
                        sheetName: "Animal");
                else if (duplicates.Contains(row.AnimalType))
                    AddError(errors, jobQueueId, sourceRow, "animaltype", "DUPLICATE_ANIMAL_TYPE",
                        $"AnimalType '{row.AnimalType}' appears more than once.", sheetName: "Animal");

                if (row.DailyRate.HasValue && row.DailyRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "dailyrate", "NEGATIVE_RATE", "Negative rates are not permitted.",
                        sheetName: "Animal");
                if (row.DefraDailyRate.HasValue && row.DefraDailyRate.Value < 0)
                    AddError(errors, jobQueueId, sourceRow, "defradailyrate", "NEGATIVE_RATE", "Negative rates are not permitted.",
                        sheetName: "Animal");
            }

            // Mirror worker comparison to compute accurate Update/Unchanged counts
            var liveRows = await _repository.GetAnimalRowsForExportAsync(fpsYear, ct);
            var liveLookup = liveRows.ToDictionary(r => r.AnimalType, StringComparer.OrdinalIgnoreCase);
            int update = 0, unchanged = 0;
            foreach (var row in rows)
            {
                if (!liveLookup.TryGetValue(row.AnimalType ?? string.Empty, out var live))
                { update++; continue; }
                var changed = (row.DailyRate.HasValue      && row.DailyRate.Value      != live.DailyRate)
                           || (row.DefraDailyRate.HasValue && row.DefraDailyRate.Value != live.DefraDailyRate)
                           || (row.PlanByWeek.HasValue     && row.PlanByWeek.Value     != live.PlanByWeek)
                           || (row.Species      is not null && row.Species      != live.Species)
                           || (row.SecurityLevel is not null && row.SecurityLevel != live.SecurityLevel);
                if (changed) update++; else unchanged++;
            }

            var invalid = errors.Count(e => e.Severity == "Error");
            return new BulkRatesValidationResult
            {
                Errors = errors,
                RowCounts = new BulkRatesRowCounts
                {
                    Total = rows.Count,
                    Update = update,
                    Unchanged = unchanged,
                    Invalid = invalid,
                    Valid = rows.Count - invalid
                }
            };
        }

        // ── Error helpers (Staff/Animal only — FEC/AGRUP findings go through MapFinding) ──

        private static void AddError(
            List<StagingValidationError> list, Guid jobQueueId,
            int sourceRowNumber, string? fieldName, string code, string message,
            string? sheetName = null, string? testCode = null, string? buyer = null)
        {
            list.Add(new StagingValidationError
            {
                JobQueueId = jobQueueId,
                UploadVersion = 0, // set to actual version by service before persisting
                SourceRowNumber = sourceRowNumber,
                FieldName = fieldName,
                ValidationCode = code,
                Severity = "Error",
                ValidationMessage = message,
                SheetName = sheetName,
                TestCode = testCode,
                Buyer = buyer
            });
        }
    }
}
