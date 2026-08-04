namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// Implementation. See IStaffAnimalValidationService for the contract this fulfils. The
    /// rules are described below. Invalid-data checks (missing/duplicate key, negative rate)
    /// take priority over NotFound — they're a property of the uploaded row itself, independent
    /// of whether a live counterpart exists — so a row failing both is reported as Invalid, not
    /// NotFound.
    /// </summary>
    public sealed class StaffAnimalValidationService : IStaffAnimalValidationService
    {
        public StaffAnimalValidationResult Validate(StaffAnimalValidationContext context)
            => new()
            {
                StaffResults = ValidateStaff(context),
                AnimalResults = ValidateAnimal(context),
            };

        // ── Staff ────────────────────────────────────────────────────────────────

        private static IReadOnlyList<StaffValidationResult> ValidateStaff(StaffAnimalValidationContext ctx)
        {
            var results = new List<StaffValidationResult>();

            var duplicates = ctx.StagedStaffRows
                .Where(r => !string.IsNullOrWhiteSpace(r.PcGrade))
                .GroupBy(r => StaffAnimalValidationKeys.PcGrade(r.PcGrade))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedStaffRows)
            {
                if (string.IsNullOrWhiteSpace(row.PcGrade))
                {
                    results.Add(new StaffValidationResult
                    {
                        PcGrade = row.PcGrade,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("MISSING_GRADE", "Staff", null, row.SourceRow, "PcGrade is required.")],
                    });
                    continue;
                }

                var key = StaffAnimalValidationKeys.PcGrade(row.PcGrade);
                var errors = new List<ValidationFinding>();

                if (duplicates.Contains(key))
                    errors.Add(Error("DUPLICATE_GRADE", "Staff", row.PcGrade, row.SourceRow,
                        $"Grade '{row.PcGrade}' appears more than once."));
                if (row.PayRate is < 0)
                    errors.Add(Error("NEGATIVE_RATE", "Staff", row.PcGrade, row.SourceRow,
                        "Negative rates are not permitted.", "payrate"));
                if (row.Npr is < 0)
                    errors.Add(Error("NEGATIVE_RATE", "Staff", row.PcGrade, row.SourceRow,
                        "Negative rates are not permitted.", "npr"));
                if (row.Ohr is < 0)
                    errors.Add(Error("NEGATIVE_RATE", "Staff", row.PcGrade, row.SourceRow,
                        "Negative rates are not permitted.", "ohr"));

                var hasLive = ctx.LiveStaffLookup.TryGetValue(key, out var live);
                var effective = new StaffFieldState
                {
                    PayRate = StaffAnimalFieldComparer.NormalizeAmount(row.PayRate),
                    Npr = StaffAnimalFieldComparer.NormalizeAmount(row.Npr),
                    Ohr = StaffAnimalFieldComparer.NormalizeAmount(row.Ohr),
                };

                if (errors.Count > 0)
                {
                    results.Add(new StaffValidationResult
                    {
                        PcGrade = row.PcGrade,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        Source = hasLive ? ToState(live!) : null,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = errors,
                    });
                    continue;
                }

                if (!hasLive)
                {
                    results.Add(new StaffValidationResult
                    {
                        PcGrade = row.PcGrade,
                        Action = StaffAnimalCalculatedAction.NotFound,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("GRADE_NOT_FOUND", "Staff", row.PcGrade, row.SourceRow,
                            $"PcGrade '{row.PcGrade}' does not exist.")],
                    });
                    continue;
                }

                var unchanged =
                    StaffAnimalFieldComparer.AmountEquals(row.PayRate, live!.PayRate) &&
                    StaffAnimalFieldComparer.AmountEquals(row.Npr, live.Npr) &&
                    StaffAnimalFieldComparer.AmountEquals(row.Ohr, live.Ohr);

                results.Add(new StaffValidationResult
                {
                    PcGrade = row.PcGrade,
                    Action = unchanged ? StaffAnimalCalculatedAction.NoChange : StaffAnimalCalculatedAction.Update,
                    Source = ToState(live),
                    Effective = effective,
                    ValidationVersion = StaffAnimalValidationVersion.Current,
                });
            }

            return results;
        }

        private static StaffFieldState ToState(LiveStaffRow live) => new()
        {
            PayRate = StaffAnimalFieldComparer.NormalizeAmount(live.PayRate),
            Npr = StaffAnimalFieldComparer.NormalizeAmount(live.Npr),
            Ohr = StaffAnimalFieldComparer.NormalizeAmount(live.Ohr),
        };

        // ── Animal ───────────────────────────────────────────────────────────────

        private static IReadOnlyList<AnimalValidationResult> ValidateAnimal(StaffAnimalValidationContext ctx)
        {
            var results = new List<AnimalValidationResult>();

            var duplicates = ctx.StagedAnimalRows
                .Where(r => !string.IsNullOrWhiteSpace(r.AnimalType))
                .GroupBy(r => StaffAnimalValidationKeys.AnimalType(r.AnimalType))
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToHashSet();

            foreach (var row in ctx.StagedAnimalRows)
            {
                if (string.IsNullOrWhiteSpace(row.AnimalType))
                {
                    results.Add(new AnimalValidationResult
                    {
                        AnimalType = row.AnimalType,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("MISSING_ANIMAL_TYPE", "Animal", null, row.SourceRow, "AnimalType is required.")],
                    });
                    continue;
                }

                var key = StaffAnimalValidationKeys.AnimalType(row.AnimalType);
                var errors = new List<ValidationFinding>();

                if (duplicates.Contains(key))
                    errors.Add(Error("DUPLICATE_ANIMAL_TYPE", "Animal", row.AnimalType, row.SourceRow,
                        $"AnimalType '{row.AnimalType}' appears more than once."));
                if (row.DailyRate is < 0)
                    errors.Add(Error("NEGATIVE_RATE", "Animal", row.AnimalType, row.SourceRow,
                        "Negative rates are not permitted.", "dailyrate"));
                if (row.DefraDailyRate is < 0)
                    errors.Add(Error("NEGATIVE_RATE", "Animal", row.AnimalType, row.SourceRow,
                        "Negative rates are not permitted.", "defradailyrate"));

                var hasLive = ctx.LiveAnimalLookup.TryGetValue(key, out var live);
                var effective = new AnimalFieldState
                {
                    DailyRate = StaffAnimalFieldComparer.NormalizeAmount(row.DailyRate),
                    DefraDailyRate = StaffAnimalFieldComparer.NormalizeAmount(row.DefraDailyRate),
                    PlanByWeek = StaffAnimalFieldComparer.NormalizeFlag(row.PlanByWeek),
                    Species = StaffAnimalFieldComparer.NormalizeText(row.Species),
                    SecurityLevel = StaffAnimalFieldComparer.NormalizeText(row.SecurityLevel),
                };

                if (errors.Count > 0)
                {
                    results.Add(new AnimalValidationResult
                    {
                        AnimalType = row.AnimalType,
                        Action = StaffAnimalCalculatedAction.Invalid,
                        Source = hasLive ? ToState(live!) : null,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = errors,
                    });
                    continue;
                }

                if (!hasLive)
                {
                    results.Add(new AnimalValidationResult
                    {
                        AnimalType = row.AnimalType,
                        Action = StaffAnimalCalculatedAction.NotFound,
                        Effective = effective,
                        ValidationVersion = StaffAnimalValidationVersion.Current,
                        Errors = [Error("ANIMAL_TYPE_NOT_FOUND", "Animal", row.AnimalType, row.SourceRow,
                            $"AnimalType '{row.AnimalType}' does not exist.")],
                    });
                    continue;
                }

                var unchanged =
                    StaffAnimalFieldComparer.AmountEquals(row.DailyRate, live!.DailyRate) &&
                    StaffAnimalFieldComparer.AmountEquals(row.DefraDailyRate, live.DefraDailyRate) &&
                    StaffAnimalFieldComparer.FlagEquals(row.PlanByWeek, live.PlanByWeek) &&
                    StaffAnimalFieldComparer.TextEquals(row.Species, live.Species) &&
                    StaffAnimalFieldComparer.TextEquals(row.SecurityLevel, live.SecurityLevel);

                results.Add(new AnimalValidationResult
                {
                    AnimalType = row.AnimalType,
                    Action = unchanged ? StaffAnimalCalculatedAction.NoChange : StaffAnimalCalculatedAction.Update,
                    Source = ToState(live),
                    Effective = effective,
                    ValidationVersion = StaffAnimalValidationVersion.Current,
                });
            }

            return results;
        }

        private static AnimalFieldState ToState(LiveAnimalRow live) => new()
        {
            DailyRate = StaffAnimalFieldComparer.NormalizeAmount(live.DailyRate),
            DefraDailyRate = StaffAnimalFieldComparer.NormalizeAmount(live.DefraDailyRate),
            PlanByWeek = live.PlanByWeek,
            Species = StaffAnimalFieldComparer.NormalizeText(live.Species),
            SecurityLevel = StaffAnimalFieldComparer.NormalizeText(live.SecurityLevel),
        };

        // ── Finding factory ──────────────────────────────────────────────────────

        private static ValidationFinding Error(
            string code, string sheet, string? businessKey, int sourceRow, string message, string? field = null)
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
    }
}
