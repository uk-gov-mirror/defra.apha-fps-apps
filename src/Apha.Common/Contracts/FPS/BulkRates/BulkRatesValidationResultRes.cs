namespace Apha.Common.Contracts.FPS.BulkRates
{
    public class BulkRatesValidationResultRes
    {
        public Guid JobQueueId { get; set; }
        public int UploadVersion { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public bool CanRelease { get; set; }
        public IReadOnlyList<BulkRatesValidationErrorRes> Errors { get; set; } = [];
    }

    public class BulkRatesValidationErrorRes
    {
        public int SourceRowNumber { get; set; }
        public string? FieldName { get; set; }
        public string? ValidationCode { get; set; }
        public string Severity { get; set; } = "Error";
        public string ValidationMessage { get; set; } = string.Empty;
    }
}
