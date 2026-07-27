namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// The classification DR-VAL-01 assigns to a staged row (plan §3.1/§3.2).
    /// Persisted verbatim into the frozen fps.tblstaging*.calculated_action columns
    /// (CR056/DR-DB-01) at release time (DR-API-07) and re-derived by the worker
    /// (DR-WK-04 §5.2) to detect drift between what was approved and what would
    /// execute now.
    /// </summary>
    public static class ValidationCalculatedAction
    {
        public const string NoChange = "NoChange";
        public const string Insert = "Insert";
        public const string Update = "Update";
        public const string ZeroRateWithdrawal = "ZeroRateWithdrawal";
    }
}
