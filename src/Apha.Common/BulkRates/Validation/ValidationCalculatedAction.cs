namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// The classification the shared validator assigns to a staged row.
    /// Persisted verbatim into the frozen fps.tblstaging*.calculated_action columns
    /// at release time and re-derived by the worker
    /// to detect drift between what was approved and what would
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
