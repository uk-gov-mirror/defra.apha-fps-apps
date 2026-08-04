namespace Apha.Common.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// Identifies the deterministic Staff/Animal rule set and normalization semantics in effect
    /// when a frozen decision (source/effective state + calculated_action) was produced. A
    /// plain integer constant, per the plan's explicit "implementation choice, not
    /// decided there" — bump it whenever <see cref="StaffAnimalFieldComparer"/> or
    /// <see cref="StaffAnimalValidationService"/>'s classification rules change in a way that
    /// could alter a previously-frozen outcome.
    ///
    /// Default policy: a frozen validation_version that no longer matches
    /// <see cref="Current"/> fails safely — the request must be revalidated and released again.
    /// Enforcing that comparison is the caller's job (this service does not see frozen state);
    /// this constant only supplies the value to compare against.
    /// </summary>
    public static class StaffAnimalValidationVersion
    {
        public const int Current = 1;
    }
}
