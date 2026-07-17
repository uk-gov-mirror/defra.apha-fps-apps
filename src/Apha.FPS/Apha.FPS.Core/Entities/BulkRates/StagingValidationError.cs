namespace Apha.FPS.Core.Entities.BulkRates
{
    /// <summary>
    /// Read/write model for fps.staging_validation_error.
    /// One row per error or warning per field per source row, scoped to a request and upload version.
    /// Severity CHECK: 'Error' | 'Warning' | 'Info'.
    /// </summary>
    public class StagingValidationError
    {
        public long Id { get; set; }
        public Guid JobQueueId { get; set; }
        public int UploadVersion { get; set; }
        public int SourceRowNumber { get; set; }
        public string? FieldName { get; set; }
        public string? ValidationCode { get; set; }
        public string Severity { get; set; } = "Error";
        public string ValidationMessage { get; set; } = string.Empty;
        public string? SheetName { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        public string? CurrentValue { get; set; }
        public string? ExpectedValue { get; set; }
    }
}
