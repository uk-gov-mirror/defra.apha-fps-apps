using Apha.Common.BulkRates.Validation;
using Apha.Common.BulkRates.Validation.StaffAnimal;
using Apha.Common.Constants;
using Apha.FPS.Core.Entities.BulkRates;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Orchestrates Bulk Rates upload/release validation. FEC/AGRUP business rules live in
    /// Apha.Common.BulkRates.Validation.IBulkRatesValidationService —
    /// this class's job is to build that service's ValidationContext from bulk
    /// repository reads plus the parsed/staged rows, call it once, and map the returned
    /// ValidationFinding list onto StagingValidationError/BulkRatesRowCounts. It must not
    /// reimplement any FEC/AGRUP rule itself ("every item... calls the shared validator
    /// rather than implementing its own copy"). Staff/Animal rules live the same way in
    /// Apha.Common.BulkRates.Validation.StaffAnimal.IStaffAnimalValidationService
    /// — a parallel, differently-shaped service, not the
    /// ValidationContext.
    /// </summary>
    public class BulkRatesValidator
    {
        private readonly IBulkRatesRepository _repository;
        private readonly IBulkRatesValidationService _validationService;
        private readonly IStaffAnimalValidationService _staffAnimalValidationService;

        public BulkRatesValidator(
            IBulkRatesRepository repository,
            IBulkRatesValidationService validationService,
            IStaffAnimalValidationService staffAnimalValidationService)
        {
            _repository = repository;
            _validationService = validationService;
            _staffAnimalValidationService = staffAnimalValidationService;
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
                BulkRatesJobNames.Staff => await ValidateStaffAsync(
                    parseResult.JobQueueId, fpsYear, parseResult.StaffRows, uploadVersion, ct),
                BulkRatesJobNames.Animal => await ValidateAnimalAsync(
                    parseResult.JobQueueId, fpsYear, parseResult.AnimalRows, uploadVersion, ct),
                _ => new BulkRatesValidationResult
                {
                    Errors =
                    [
                        new() { Severity = "Error", ValidationCode = "UNKNOWN_JOB", ValidationMessage = $"Unknown job: {jobName}" }
                    ]
                }
            };
        }

        // ── Release-time re-validation + freeze ───────────────────────────────────

        /// <summary>
        /// Re-runs the same rules against the currently staged rows (read back from
        /// the DB — release time has no fresh parseResult in hand) and the current live/
        /// reference data, and packages the per-row classification for
        /// BulkRatesRequestService.ReleaseForApprovalAsync to freeze onto staging.
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
                AgrupFreezes = agrupFreezes
            };
        }

        /// <summary>
        /// Staff equivalent of BuildFreezeAsync — re-runs its rules
        /// against the currently staged rows (DB read-back) and current live data, for
        /// BulkRatesRequestService.ReleaseForApprovalAsync to freeze onto staging once
        /// clean. validation_version is StaffAnimalValidationVersion.Current — a rule-set
        /// version, not UploadVersion (§3, unlike FEC/AGRUP's validation_version column).
        /// </summary>
        public async Task<BulkRatesStaffFreezeResult> BuildStaffFreezeAsync(
            Guid jobQueueId, int fpsYear, CancellationToken ct = default)
        {
            var stagedRows = await _repository.GetStaffStagingRowsAsync(jobQueueId, ct);
            var context = await BuildStaffAnimalContextAsync(jobQueueId, fpsYear, stagedRows, [], ct);
            var result = _staffAnimalValidationService.Validate(context);

            var blockingErrors = result.StaffResults
                .SelectMany(r => r.Errors)
                .Where(f => f.Severity == ValidationSeverity.Error)
                .ToList();

            var freezes = result.StaffResults.Select(r => new StaffFreezeEntry(
                r.PcGrade, r.Action,
                r.Source?.PayRate, r.Source?.Npr, r.Source?.Ohr,
                r.Effective?.PayRate, r.Effective?.Npr, r.Effective?.Ohr)).ToList();

            return new BulkRatesStaffFreezeResult { BlockingErrors = blockingErrors, Freezes = freezes };
        }

        /// <summary>As BuildStaffFreezeAsync, for Animal.</summary>
        public async Task<BulkRatesAnimalFreezeResult> BuildAnimalFreezeAsync(
            Guid jobQueueId, int fpsYear, CancellationToken ct = default)
        {
            var stagedRows = await _repository.GetAnimalStagingRowsAsync(jobQueueId, ct);
            var context = await BuildStaffAnimalContextAsync(jobQueueId, fpsYear, [], stagedRows, ct);
            var result = _staffAnimalValidationService.Validate(context);

            var blockingErrors = result.AnimalResults
                .SelectMany(r => r.Errors)
                .Where(f => f.Severity == ValidationSeverity.Error)
                .ToList();

            var freezes = result.AnimalResults.Select(r => new AnimalFreezeEntry(
                r.AnimalType, r.Action,
                r.Source?.DailyRate, r.Source?.DefraDailyRate, r.Source?.PlanByWeek, r.Source?.Species, r.Source?.SecurityLevel,
                r.Effective?.DailyRate, r.Effective?.DefraDailyRate, r.Effective?.PlanByWeek, r.Effective?.Species, r.Effective?.SecurityLevel)).ToList();

            return new BulkRatesAnimalFreezeResult { BlockingErrors = blockingErrors, Freezes = freezes };
        }

        // ── Calculated action for display, when nothing is frozen yet ────────────────

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

        // ── FEC + AGRUP validation ─────────────────────────────────────────────────

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
            // fps.staging_validation_error (reads them from the frozen staging columns
            // instead, once release-time re-validation has run).
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
                    // the immutability check below. New rows have no live value to echo,
                    // so they correctly stay null and fall through to the
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

        // ── Staff/Animal validation ────────────────────────────────────────────────
        //
        // Findings carry PcGrade/AnimalType as ValidationFinding.BusinessKey, which MapFinding
        // (shared with FEC/AGRUP — it only special-cases the "AGRUP" sheet) turns into
        // StagingValidationError.TestCode, so the Web UI can attach them inline to the matching
        // staging grid row the same way it does for FEC/AGRUP's own TestCode. MISSING_GRADE/
        // MISSING_ANIMAL_TYPE necessarily leave BusinessKey null (there's no key to attach to),
        // so those findings stay in the unmatched/top-of-page list by design, not by omission.

        private async Task<BulkRatesValidationResult> ValidateStaffAsync(
            Guid jobQueueId, int fpsYear, IReadOnlyList<StaffStagingRow> rows, int uploadVersion, CancellationToken ct)
        {
            var context = await BuildStaffAnimalContextAsync(jobQueueId, fpsYear, rows, [], ct);
            var result = _staffAnimalValidationService.Validate(context);

            var errors = result.StaffResults
                .SelectMany(r => r.Errors)
                .Select(f => MapFinding(f, jobQueueId, uploadVersion))
                .ToList();

            var counts = ComputeStaffAnimalRowCounts(rows.Count, result.StaffResults.Select(r => r.Action), errors);

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        private async Task<BulkRatesValidationResult> ValidateAnimalAsync(
            Guid jobQueueId, int fpsYear, IReadOnlyList<AnimalStagingRow> rows, int uploadVersion, CancellationToken ct)
        {
            var context = await BuildStaffAnimalContextAsync(jobQueueId, fpsYear, [], rows, ct);
            var result = _staffAnimalValidationService.Validate(context);

            var errors = result.AnimalResults
                .SelectMany(r => r.Errors)
                .Select(f => MapFinding(f, jobQueueId, uploadVersion))
                .ToList();

            var counts = ComputeStaffAnimalRowCounts(rows.Count, result.AnimalResults.Select(r => r.Action), errors);

            return new BulkRatesValidationResult { Errors = errors, RowCounts = counts };
        }

        /// <summary>
        /// Builds IStaffAnimalValidationService's context from bulk repository reads (mirroring
        /// FEC/AGRUP's BuildContextAsync) plus the given staged rows, which the two callers
        /// above source differently: a fresh parse at upload time, or a DB read-back at release
        /// time (BuildStaffFreezeAsync/BuildAnimalFreezeAsync). SourceRow is synthesized from
        /// list position in both cases — the same simplification BuildContextAsync already makes
        /// for FEC/AGRUP's release-time read-back, since the original worksheet row number isn't
        /// persisted to staging.
        /// </summary>
        private async Task<StaffAnimalValidationContext> BuildStaffAnimalContextAsync(
            Guid jobQueueId, int fpsYear,
            IReadOnlyList<StaffStagingRow> stagedStaffRows,
            IReadOnlyList<AnimalStagingRow> stagedAnimalRows,
            CancellationToken ct)
        {
            var liveStaffRows = await _repository.GetStaffRowsForExportAsync(fpsYear, ct);
            var liveAnimalRows = await _repository.GetAnimalRowsForExportAsync(fpsYear, ct);

            var liveStaffLookup = liveStaffRows.ToDictionary(
                r => StaffAnimalValidationKeys.PcGrade(r.PcGrade),
                r => new LiveStaffRow { PcGrade = r.PcGrade, PayRate = r.PayRate, Npr = r.Npr, Ohr = r.Ohr });

            var liveAnimalLookup = liveAnimalRows.ToDictionary(
                r => StaffAnimalValidationKeys.AnimalType(r.AnimalType),
                r => new LiveAnimalRow
                {
                    AnimalType = r.AnimalType,
                    DailyRate = r.DailyRate,
                    DefraDailyRate = r.DefraDailyRate,
                    PlanByWeek = r.PlanByWeek ?? false,
                    Species = r.Species,
                    SecurityLevel = r.SecurityLevel
                });

            var stagedStaff = stagedStaffRows.Select((r, i) => new ValidationStaffRow
            {
                PcGrade = r.PcGrade,
                PayRate = r.PayRate,
                Npr = r.Npr,
                Ohr = r.Ohr,
                SourceRow = i + 2
            }).ToList();

            var stagedAnimal = stagedAnimalRows.Select((r, i) => new ValidationAnimalRow
            {
                AnimalType = r.AnimalType,
                DailyRate = r.DailyRate,
                DefraDailyRate = r.DefraDailyRate,
                PlanByWeek = r.PlanByWeek,
                Species = r.Species,
                SecurityLevel = r.SecurityLevel,
                SourceRow = i + 2
            }).ToList();

            return new StaffAnimalValidationContext
            {
                JobQueueId = jobQueueId,
                FpsYear = fpsYear,
                LiveStaffLookup = liveStaffLookup,
                LiveAnimalLookup = liveAnimalLookup,
                StagedStaffRows = stagedStaff,
                StagedAnimalRows = stagedAnimal
            };
        }

        /// <summary>
        /// Staff/Animal equivalent of ComputeRowCounts — no Insert bucket (update-only, gating
        /// decision #3): NotFound/Invalid rows both carry an Error-severity finding, so they're
        /// counted the same way FEC/AGRUP's Invalid bucket already is, from the mapped errors
        /// rather than from StaffAnimalCalculatedAction directly.
        /// </summary>
        private static BulkRatesRowCounts ComputeStaffAnimalRowCounts(
            int total, IEnumerable<string> actions, IReadOnlyList<StagingValidationError> errors)
        {
            int update = 0, unchanged = 0;
            foreach (var action in actions)
            {
                if (action == StaffAnimalCalculatedAction.Update) update++;
                else if (action == StaffAnimalCalculatedAction.NoChange) unchanged++;
            }

            var invalid = errors.Count(e => e.Severity == ValidationSeverity.Error);
            return new BulkRatesRowCounts
            {
                Total = total,
                Update = update,
                Unchanged = unchanged,
                Invalid = invalid,
                Valid = total - invalid
            };
        }
    }
}
