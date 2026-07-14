using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// Result returned by the Upload operation to the controller.
    /// Carries the new validation state so the controller can build the upload response.
    /// </summary>
    public class BulkRatesUploadResultDto
    {
        public Guid JobQueueId { get; set; }
        public string Status { get; set; } = string.Empty;
        public int UploadVersion { get; set; }
        public string? Filename { get; set; }
        public BulkRatesRowCounts RowCounts { get; set; } = new();
        public IReadOnlyList<StagingValidationError> ValidationErrors { get; set; } = [];
    }
}
