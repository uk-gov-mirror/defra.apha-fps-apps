namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// DR-VAL-01 implementation. See IBulkRatesValidationService for the contract this
    /// fulfils and plan §3/§3.1/§3.2 for the rules below.
    /// </summary>
    public sealed class BulkRatesValidationService : IBulkRatesValidationService
    {
        public IReadOnlyList<ValidationFinding> Validate(ValidationContext context)
        {
            var findings = new List<ValidationFinding>();

            var withdrawnFecTestCodes = ValidateFec(context, findings);
            ValidateAgrup(context, findings, withdrawnFecTestCodes);
            if (context.IncludeWorkerOnlyChecks)
                ValidateLiveWithdrawalConflicts(context, findings, withdrawnFecTestCodes);
            ValidateSnapshotPreservation(context, findings);

            return findings;
        }

        // ── FEC ──────────────────────────────────────────────────────────────────

        /// <summary>Validates all staged FEC rows and returns the set of TestCodes classified as ZeroRateWithdrawal (needed by the AGRUP-side withdrawal-conflict checks).</summary>
        private static HashSet<string> ValidateFec(ValidationContext ctx, ICollection<ValidationFinding> findings)
        {
            var withdrawn = new HashSet<string>();

            var duplicates = ctx.StagedFecRows
                .GroupBy(r => BulkRatesValidationKeys.TestCode(r.TestCode))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedFecRows)
            {
                var key = BulkRatesValidationKeys.TestCode(row.TestCode);

                if (duplicates.Contains(key))
                    findings.Add(Error("DUPLICATE_TEST_CODE", "FEC", row.TestCode, row.SourceRow,
                        $"TestCode '{row.TestCode}' appears more than once in the FEC worksheet."));

                if (row.FecNewRate is < 0)
                    findings.Add(Error("NEGATIVE_RATE", "FEC", row.TestCode, row.SourceRow,
                        "Negative rates are not permitted.", "fecnewrate"));

                if (!ctx.LiveFecLookup.TryGetValue(key, out var live))
                {
                    // New Test Code.
                    if (row.FecNewRate is null)
                        findings.Add(Error("MISSING_FEC_NEW_RATE", "FEC", row.TestCode, row.SourceRow,
                            "FEC New rate is mandatory for a new Test Code.", "fecnewrate"));
                    if (string.IsNullOrWhiteSpace(row.ItemDescription))
                        findings.Add(Error("MISSING_FOR_INSERT", "FEC", row.TestCode, row.SourceRow,
                            "Item Description is required for new Test Code inserts.", "itemdescription"));
                    if (string.IsNullOrWhiteSpace(row.ShortDescription))
                        findings.Add(Error("MISSING_FOR_INSERT", "FEC", row.TestCode, row.SourceRow,
                            "Short Description is required for new Test Code inserts.", "shortdescription"));
                    if (string.IsNullOrWhiteSpace(row.Owner))
                        findings.Add(Error("MISSING_FOR_INSERT", "FEC", row.TestCode, row.SourceRow,
                            "Owner is required for new Test Code inserts.", "owner"));

                    if (row.FecNewRate is >= 0)
                        findings.Add(Classification("FEC", row.TestCode, row.SourceRow,
                            ValidationCalculatedAction.Insert, row.FecNewRate));
                }
                else
                {
                    // Existing Test Code: description/owner changes are ignored (spec, unchanged).
                    if (!string.IsNullOrWhiteSpace(row.ItemDescription))
                        findings.Add(Warning("IGNORED_ON_UPDATE", "FEC", row.TestCode, row.SourceRow,
                            "Item Description changes are ignored for existing Test Codes (update-only modifies price columns).", "itemdescription"));

                    // Existing + blank/zero rate = Zero-Rate Withdrawal, not an error (reconciliation §2.1) —
                    // supersedes the old "blank is always a mandatory-field error" rule. Still counts
                    // toward withdrawnFecTestCodes even when already zero (a positive AGRUP row under an
                    // already-zero FEC Test Code is still a conflict, regardless of whether this upload
                    // is what caused the transition) — but the classification itself is NoChange, not a
                    // fresh withdrawal, when the live rate was already 0 (nothing is actually changing,
                    // so nothing should be written/audited as an update).
                    if (row.FecNewRate is null or 0)
                    {
                        withdrawn.Add(key);
                        var alreadyZero = (live.UnitPriceVla ?? 0) == 0 && (live.DefraUnitPrice ?? 0) == 0;
                        findings.Add(Classification("FEC", row.TestCode, row.SourceRow,
                            alreadyZero ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.ZeroRateWithdrawal,
                            0m));
                    }
                    else if (row.FecNewRate is > 0)
                    {
                        var unchanged = row.FecNewRate == live.UnitPriceVla && row.FecNewRate == live.DefraUnitPrice;
                        findings.Add(Classification("FEC", row.TestCode, row.SourceRow,
                            unchanged ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.Update,
                            row.FecNewRate));
                    }
                    // Negative already reported above; no classification for an invalid value.
                }
            }

            return withdrawn;
        }

        // ── AGRUP ────────────────────────────────────────────────────────────────

        private static void ValidateAgrup(
            ValidationContext ctx, ICollection<ValidationFinding> findings, IReadOnlySet<string> withdrawnFecTestCodes)
        {
            var fecTestCodesInUpload = ctx.StagedFecRows
                .Select(r => BulkRatesValidationKeys.TestCode(r.TestCode))
                .ToHashSet();

            var duplicates = ctx.StagedAgrupRows
                .GroupBy(r => BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedAgrupRows)
            {
                var businessKey = $"{row.TestCode}/{row.Buyer}";
                var testCodeKey = BulkRatesValidationKeys.TestCode(row.TestCode);
                var agrupKey = BulkRatesValidationKeys.AgrupKey(row.TestCode, row.Buyer);

                if (duplicates.Contains(agrupKey))
                    findings.Add(Error("DUPLICATE_AGRUP_KEY", "AGRUP", businessKey, row.SourceRow,
                        $"TestCode+Buyer combination '{businessKey}' appears more than once."));

                var testCodeExists = ctx.LiveFecLookup.ContainsKey(testCodeKey) || fecTestCodesInUpload.Contains(testCodeKey);
                if (!testCodeExists)
                    findings.Add(Error("TEST_CODE_NOT_FOUND", "AGRUP", businessKey, row.SourceRow,
                        $"TestCode '{row.TestCode}' does not exist in FPS year {ctx.FpsYear} and is not being inserted by this FEC upload.", "testcode"));

                if (row.AgrupNew is < 0)
                    findings.Add(Error("NEGATIVE_RATE", "AGRUP", businessKey, row.SourceRow,
                        "Negative rates are not permitted.", "agrupnew"));

                if (!ctx.LiveAgrupLookup.TryGetValue(agrupKey, out var live))
                    ValidateNewAgrupRow(ctx, row, businessKey, findings);
                else
                    ValidateExistingAgrupRow(row, live, businessKey, findings);

                // "Same as FEC" comment cross-check (unchanged from the existing validator).
                if (string.Equals(row.Comments, "Same as FEC", StringComparison.OrdinalIgnoreCase))
                {
                    var fecMatch = ctx.StagedFecRows.FirstOrDefault(
                        f => BulkRatesValidationKeys.TestCode(f.TestCode) == testCodeKey);
                    if (fecMatch is not null && row.AgrupNew.HasValue && fecMatch.FecNewRate.HasValue
                        && row.AgrupNew.Value != fecMatch.FecNewRate.Value)
                    {
                        findings.Add(Error("SAME_AS_FEC_MISMATCH", "AGRUP", businessKey, row.SourceRow,
                            $"Comments is 'Same as FEC' but Agrup New ({row.AgrupNew}) does not equal FEC New ({fecMatch.FecNewRate}) for TestCode '{row.TestCode}'.", "agrupnew"));
                    }
                }

                // DR-API-09 interim BC-05 safety net, staged-vs-withdrawal (release-time,
                // snapshot-independent — this is about what THIS upload contains, not what
                // was previously downloaded). The snapshot-scoped/live-data counterpart is
                // ValidateLiveWithdrawalConflicts below (plan §5.2).
                if (withdrawnFecTestCodes.Contains(testCodeKey) && row.AgrupNew is > 0)
                {
                    findings.Add(Error("AGRUP_POSITIVE_FOR_WITHDRAWN_FEC", "AGRUP", businessKey, row.SourceRow,
                        $"FEC TestCode '{row.TestCode}' is being withdrawn (zeroed) in this upload, but AGRUP row '{businessKey}' still has a positive rate ({row.AgrupNew}).", "agrupnew"));
                }
            }
        }

        private static void ValidateNewAgrupRow(
            ValidationContext ctx, ValidationAgrupRow row, string businessKey, ICollection<ValidationFinding> findings)
        {
            if (row.AgrupNew is null)
            {
                findings.Add(Error("MISSING_FOR_INSERT", "AGRUP", businessKey, row.SourceRow,
                    "Agrup New is required for new TestCode+Buyer inserts.", "agrupnew"));
            }
            else if (row.AgrupNew.Value == 0)
            {
                // BC-01 temporary rule (DR-API-03): block a new AGRUP row at zero until
                // business confirms permanent behaviour.
                findings.Add(Error("NEW_AGRUP_ZERO_RATE_BLOCKED", "AGRUP", businessKey, row.SourceRow,
                    "New AGRUP rows with a zero rate are not currently permitted, pending business confirmation (BC-01).", "agrupnew"));
            }

            var hasProjectBuyerCode = !string.IsNullOrWhiteSpace(row.ProjectBuyerCode);
            var hasWorkGroup = !string.IsNullOrWhiteSpace(row.TestBuyerWorkGroup);

            if (!hasProjectBuyerCode && !hasWorkGroup)
            {
                findings.Add(Error("MISSING_ROUTING_FIELD", "AGRUP", businessKey, row.SourceRow,
                    "At least one of ProjectBuyerCode or TestBuyerWorkGroup must be supplied for a new AGRUP row (BC-02)."));
            }
            else
            {
                if (hasProjectBuyerCode && !ctx.ProjectLookup.Contains(BulkRatesValidationKeys.TestCode(row.ProjectBuyerCode!)))
                    findings.Add(Error("INVALID_PROJECT_BUYER_CODE", "AGRUP", businessKey, row.SourceRow,
                        $"ProjectBuyerCode '{row.ProjectBuyerCode}' does not exist for FPS year {ctx.FpsYear}.", "projectbuyercode"));

                if (hasWorkGroup && !ctx.CapabilityLookup.Contains(BulkRatesValidationKeys.CapabilityKey(row.TestCode, row.TestBuyerWorkGroup!)))
                    findings.Add(Error("INVALID_TEST_BUYER_WORKGROUP", "AGRUP", businessKey, row.SourceRow,
                        $"TestCode '{row.TestCode}' / WorkGroup '{row.TestBuyerWorkGroup}' is not a recognised capability for FPS year {ctx.FpsYear}.", "testbuyerworkgroup"));
            }

            if (row.AgrupNew is > 0)
                findings.Add(Classification("AGRUP", businessKey, row.SourceRow, ValidationCalculatedAction.Insert, row.AgrupNew));
        }

        private static void ValidateExistingAgrupRow(
            ValidationAgrupRow row, LiveAgrupRow live, string businessKey, ICollection<ValidationFinding> findings)
        {
            // Existing-key routing-field immutability, Bulk-Rates-scoped (DR-API-05,
            // reconciliation §2.5) — not a system-wide tlkptestreqmt rule; other writers
            // (e.g. the PACT maintenance path) may still permit controlled changes.
            // Comparison matches the citext columns' own case-insensitivity and adds no
            // extra trimming (DR-API-05 comparison note) — Excel-introduced whitespace on an
            // otherwise-unedited protected cell is a known, accepted risk, not silently
            // "fixed" here without business confirmation.
            if (RoutingFieldChanged(row.ProjectBuyerCode, live.ProjectBuyerCode))
                findings.Add(Error("ROUTING_FIELD_CHANGED", "AGRUP", businessKey, row.SourceRow,
                    $"ProjectBuyerCode cannot be changed for an existing AGRUP row (was '{live.ProjectBuyerCode}').", "projectbuyercode"));

            if (RoutingFieldChanged(row.TestBuyerCode, live.TestBuyerCode))
                findings.Add(Error("ROUTING_FIELD_CHANGED", "AGRUP", businessKey, row.SourceRow,
                    $"TestBuyerCode cannot be changed for an existing AGRUP row (was '{live.TestBuyerCode}').", "testbuyercode"));

            // Existing + blank/zero rate = Zero-Rate Withdrawal, not a silent no-op
            // (reconciliation §2.2) — supersedes the old "blank on existing row = unchanged" rule.
            // As with FEC above: if the live rate is already 0, nothing is actually changing —
            // classify NoChange, not a fresh withdrawal.
            if (row.AgrupNew is null or 0)
            {
                var alreadyZero = (live.UnitPrice ?? 0) == 0;
                findings.Add(Classification("AGRUP", businessKey, row.SourceRow,
                    alreadyZero ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.ZeroRateWithdrawal,
                    0m));
            }
            else if (row.AgrupNew is > 0)
            {
                var unchanged = row.AgrupNew == live.UnitPrice;
                findings.Add(Classification("AGRUP", businessKey, row.SourceRow,
                    unchanged ? ValidationCalculatedAction.NoChange : ValidationCalculatedAction.Update,
                    row.AgrupNew));
            }
        }

        private static bool RoutingFieldChanged(string? staged, string? live)
            => !string.Equals(staged ?? string.Empty, live ?? string.Empty, StringComparison.OrdinalIgnoreCase);

        // ── Worker-side BC-05 interim rule (plan §5.2) ──────────────────────────

        /// <summary>
        /// Only called when ValidationContext.IncludeWorkerOnlyChecks is true — see that
        /// property for why this must not run at API release time. A live positive AGRUP row
        /// related to a withdrawn FEC TestCode that was NOT present in the frozen download
        /// snapshot cannot have been caught by the staged-row check above (DR-API-09) — it
        /// didn't exist, or wasn't yet linked to that TestCode, at download time. The worker
        /// re-builds ValidationContext.LiveAgrupLookup under the row lock immediately before
        /// calling here (§5.1), which is what makes this catch the race the rule exists to close.
        /// </summary>
        private static void ValidateLiveWithdrawalConflicts(
            ValidationContext ctx, ICollection<ValidationFinding> findings, IReadOnlySet<string> withdrawnFecTestCodes)
        {
            if (withdrawnFecTestCodes.Count == 0)
                return;

            var snapshotAgrupKeys = ctx.FrozenSnapshot
                .Where(s => string.Equals(s.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase) && s.Buyer is not null)
                .Select(s => BulkRatesValidationKeys.AgrupKey(s.TestCode, s.Buyer!))
                .ToHashSet();

            foreach (var live in ctx.LiveAgrupLookup.Values)
            {
                var testCodeKey = BulkRatesValidationKeys.TestCode(live.TestCode);
                if (!withdrawnFecTestCodes.Contains(testCodeKey))
                    continue;
                if (live.UnitPrice is not > 0)
                    continue;

                var agrupKey = BulkRatesValidationKeys.AgrupKey(live.TestCode, live.Buyer);
                if (snapshotAgrupKeys.Contains(agrupKey))
                    continue; // already covered by the staged-row check in ValidateAgrup

                var businessKey = $"{live.TestCode}/{live.Buyer}";
                findings.Add(RequestLevel("LIVE_AGRUP_POSITIVE_FOR_WITHDRAWN_FEC", "AGRUP", businessKey,
                    $"FEC TestCode '{live.TestCode}' is being withdrawn, but live AGRUP row '{businessKey}' " +
                    "(not present at download time) still has a positive rate — BC-05 interim rule."));
            }
        }

        // ── Downloaded-snapshot preservation ──

        private static void ValidateSnapshotPreservation(ValidationContext ctx, ICollection<ValidationFinding> findings)
        {
            if (ctx.DownloadVersion is null)
                return; // nothing was ever downloaded for this request to compare against

            var stagedFecKeys = ctx.StagedFecRows
                .Select(r => BulkRatesValidationKeys.TestCode(r.TestCode))
                .ToHashSet();
            var stagedAgrupKeys = ctx.StagedAgrupRows
                .Select(r => BulkRatesValidationKeys.AgrupKey(r.TestCode, r.Buyer))
                .ToHashSet();

            foreach (var snap in ctx.FrozenSnapshot)
            {
                if (string.Equals(snap.Sheet, "FEC", StringComparison.OrdinalIgnoreCase))
                {
                    if (!stagedFecKeys.Contains(BulkRatesValidationKeys.TestCode(snap.TestCode)))
                        findings.Add(RequestLevel("MISSING_DOWNLOADED_KEY", "FEC", snap.TestCode,
                            $"FEC — Downloaded Test Code {snap.TestCode} is missing from the uploaded workbook."));
                }
                else if (string.Equals(snap.Sheet, "AGRUP", StringComparison.OrdinalIgnoreCase) && snap.Buyer is not null)
                {
                    var key = BulkRatesValidationKeys.AgrupKey(snap.TestCode, snap.Buyer);
                    if (!stagedAgrupKeys.Contains(key))
                        findings.Add(RequestLevel("MISSING_DOWNLOADED_KEY", "AGRUP", $"{snap.TestCode}/{snap.Buyer}",
                            $"AGRUP — Downloaded Test Code {snap.TestCode} / Buyer {snap.Buyer} is missing from the uploaded workbook."));
                }
            }
        }

        // ── Finding factories ────────────────────────────────────────────────────

        private static ValidationFinding Error(
            string code, string sheet, string businessKey, int? sourceRow, string message, string? field = null)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Error,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Field = field,
                Message = message,
            };

        private static ValidationFinding Warning(
            string code, string sheet, string businessKey, int? sourceRow, string message, string? field = null)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Warning,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Field = field,
                Message = message,
            };

        private static ValidationFinding RequestLevel(string code, string sheet, string businessKey, string message)
            => new()
            {
                ValidationCode = code,
                Severity = ValidationSeverity.Error,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = null,
                IsRequestLevel = true,
                Message = message,
            };

        private static ValidationFinding Classification(
            string sheet, string businessKey, int sourceRow, string action, decimal? effectiveNewRate)
            => new()
            {
                ValidationCode = "ROW_CLASSIFIED",
                Severity = ValidationSeverity.Info,
                Sheet = sheet,
                BusinessKey = businessKey,
                SourceRow = sourceRow,
                Message = $"Calculated action: {action}.",
                CalculatedAction = action,
                EffectiveNewRate = effectiveNewRate,
            };
    }
}
