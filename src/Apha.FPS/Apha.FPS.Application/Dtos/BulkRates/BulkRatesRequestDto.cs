using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// Internal application DTO combining the queue entry with parsed upload metadata.
    /// Returned by IBulkRatesRequestService.GetRequestAsync.
    /// </summary>
    public class BulkRatesRequestDto
    {
        public BulkRatesQueueEntry Entry { get; set; } = null!;
        public BulkRatesUploadMetadata? UploadMetadata { get; set; }
        public IReadOnlyList<BulkRatesQueueLog> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }
}
