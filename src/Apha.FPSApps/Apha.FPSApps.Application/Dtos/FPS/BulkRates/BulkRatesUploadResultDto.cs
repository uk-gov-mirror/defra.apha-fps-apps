namespace Apha.FPSApps.Application.Dtos.FPS.BulkRates
{
    /// <summary>
    /// Web-side DTO matching the JSON shape returned by the FPS API upload and
    /// validation endpoints (<c>POST /upload</c>, <c>GET /validation</c>).
    /// Mirrors <c>BulkRatesUploadResultDto</c> as serialised by the FPS API.
    /// </summary>
    public class BulkRatesUploadResultDto
    {
        public Guid JobQueueId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UploadVersion { get; set; }
        public string? Filename { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
        public List<BulkRatesValidationErrorDto> ValidationErrors { get; set; } = [];
    }

    /// <summary>
    /// A single row-level validation error returned by the upload / validation endpoints.
    /// Mirrors <c>StagingValidationError</c> as serialised over the wire.
    /// </summary>
    public class BulkRatesValidationErrorDto
    {
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
