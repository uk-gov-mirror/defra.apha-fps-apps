namespace Apha.FPSApps.Application.Dtos.FPS.BulkRates
{
    /// <summary>
    /// Web-side DTO matching the JSON shape returned by the FPS API Bulk Rates endpoints
    /// (create, release, approve, reject, cancel, get). Mirrors <c>BulkRatesRequestDto</c>
    /// as serialised by the FPS API action filter into <c>ApiResponse&lt;T&gt;.Data</c>.
    /// </summary>
    public class BulkRatesRequestDetailDto
    {
        public BulkRatesQueueEntryDto Entry { get; set; } = new();
        public BulkRatesUploadMetadataDto? UploadMetadata { get; set; }
        public List<BulkRatesQueueLogDto> Log { get; set; } = [];
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }

    /// <summary>
    /// Mirrors <c>BulkRatesQueueEntry</c> (Apha.FPS.Core) as serialised over the wire.
    /// </summary>
    public class BulkRatesQueueEntryDto
    {
        public Guid JobQueueId { get; set; }
        public int JobId { get; set; }
        public string JobName { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid JobExecutionId { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public DateTime RequestedAtUtc { get; set; }
        public int FpsYear { get; set; }
        public string? UploadFilename { get; set; }
        public string? UploadChecksumSha256 { get; set; }
        public int? UploadVersion { get; set; }
        public DateTime? UploadValidatedAtUtc { get; set; }
        public string? UploadRowCountsJson { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }
        public string? RejectedBy { get; set; }
        public DateTime? RejectedAtUtc { get; set; }
        public string? RejectionReason { get; set; }
        public string? CancelledBy { get; set; }
        public DateTime? CancelledAtUtc { get; set; }
        public string? CancellationReason { get; set; }
        public string? TriggeredBy { get; set; }
        public DateTime? TriggeredAtUtc { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FailureReason { get; set; }
    }

    /// <summary>
    /// Mirrors <c>BulkRatesUploadMetadata</c> as serialised over the wire.
    /// </summary>
    public class BulkRatesUploadMetadataDto
    {
        public string? Filename { get; set; }
        public string? ChecksumSha256 { get; set; }
        public int UploadVersion { get; set; }
        public DateTime? ValidationCompletedAtUtc { get; set; }
        public BulkRatesRowCountsDto RowCounts { get; set; } = new();
    }

    public class BulkRatesRowCountsDto
    {
        public int Total { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int Insert { get; set; }
        public int Update { get; set; }
        public int Unchanged { get; set; }
    }

    /// <summary>
    /// Mirrors <c>BulkRatesQueueLog</c> as serialised over the wire.
    /// </summary>
    public class BulkRatesQueueLogDto
    {
        public long LogId { get; set; }
        public Guid JobQueueId { get; set; }
        public string Note { get; set; } = string.Empty;
        public string? Actor { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }

    /// <summary>Mirrors <c>BulkRatesStagingDataDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStagingDataDto
    {
        public List<BulkRatesStagingFecRowDto> FecRows { get; set; } = [];
        public List<BulkRatesStagingAgrupRowDto> AgrupRows { get; set; } = [];
        public List<BulkRatesStagingStaffRowDto> StaffRows { get; set; } = [];
        public List<BulkRatesStagingAnimalRowDto> AnimalRows { get; set; } = [];
    }

    /// <summary>Mirrors <c>BulkRatesStagingFecRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStagingFecRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public decimal? UnitPriceVla { get; set; }
        public decimal? DefraUnitPrice { get; set; }
        public decimal? FecNewRate { get; set; }
        public string? ItemDescription { get; set; }
        public string? ShortDescription { get; set; }
        public string? Owner { get; set; }
        public string? Comments { get; set; }
    }

    /// <summary>Mirrors <c>BulkRatesStagingAgrupRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStagingAgrupRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string TestCode { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public decimal? Agrup { get; set; }
        public decimal? AgrupNew { get; set; }
        public double? NoRequired { get; set; }
        public DateTime? DateCreated { get; set; }
        public short? Active { get; set; }
        public string? Comments { get; set; }
    }

    /// <summary>Mirrors <c>BulkRatesStagingStaffRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStagingStaffRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string PcGrade { get; set; } = string.Empty;
        public decimal? PayRate { get; set; }
        public decimal? PayRateNew { get; set; }
        public decimal? Npr { get; set; }
        public decimal? NprNew { get; set; }
        public decimal? Ohr { get; set; }
        public decimal? OhrNew { get; set; }
    }

    /// <summary>Mirrors <c>BulkRatesStagingAnimalRowDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStagingAnimalRowDto
    {
        public string Status { get; set; } = string.Empty;
        public string AnimalType { get; set; } = string.Empty;
        public string? Species { get; set; }
        public string? SecurityLevel { get; set; }
        public decimal? DailyRate { get; set; }
        public decimal? DailyRateNew { get; set; }
        public decimal? DefraDailyRate { get; set; }
        public decimal? DefraDailyRateNew { get; set; }
        public bool? PlanByWeek { get; set; }
    }
}
