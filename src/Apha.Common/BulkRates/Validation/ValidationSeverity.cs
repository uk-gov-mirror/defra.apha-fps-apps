namespace Apha.Common.BulkRates.Validation
{
    /// <summary>
    /// Matches fps.staging_validation_error's chk_staging_validation_error_severity
    /// CHECK constraint values exactly, so findings map onto that table without translation.
    /// </summary>
    public static class ValidationSeverity
    {
        public const string Error = "Error";
        public const string Warning = "Warning";
        public const string Info = "Info";
    }
}
