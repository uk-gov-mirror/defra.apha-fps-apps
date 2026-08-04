namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// The centralized comparison normalization for Staff/Animal, covering at minimum
    /// PayRate/NPR/OHR/DailyRate/DefraDailyRate (amounts), Species/SecurityLevel (text), and
    /// PlanByWeek (flag). This is the *only* place null/blank/zero equivalence, string
    /// trimming/case-sensitivity, and decimal comparison are decided — StaffAnimalValidationService
    /// calls these, and so must any future worker-side drift check, so the two can
    /// never silently disagree.
    ///
    /// Rules:
    /// - Amounts: null and 0 are equivalent — a blank uploaded cell targets zero, the same as
    ///   an explicit 0 (zero is an ordinary value here, not a withdrawal signal — no ZeroRateWithdrawal
    ///   action exists in this domain).
    /// - Flags (PlanByWeek): null is equivalent to false — an unchecked/blank cell means "No".
    /// - Text (Species/SecurityLevel): null, empty, and whitespace-only are equivalent; compared
    ///   trimmed and case-insensitively (matches this codebase's citext-style key comparisons).
    /// </summary>
    public static class StaffAnimalFieldComparer
    {
        public static decimal NormalizeAmount(decimal? value) => value ?? 0m;

        public static bool AmountEquals(decimal? staged, decimal? live)
            => NormalizeAmount(staged) == NormalizeAmount(live);

        public static bool NormalizeFlag(bool? value) => value ?? false;

        public static bool FlagEquals(bool? staged, bool live) => NormalizeFlag(staged) == live;

        public static string? NormalizeText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        public static bool TextEquals(string? staged, string? live)
            => string.Equals(NormalizeText(staged), NormalizeText(live), StringComparison.OrdinalIgnoreCase);
    }
}
