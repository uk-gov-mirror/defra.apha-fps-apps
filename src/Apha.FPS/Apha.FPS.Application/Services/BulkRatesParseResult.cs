using Apha.FPS.Core.Entities.BulkRates;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Intermediate result returned by BulkRatesExcelParser.
    /// Contains parsed (but not yet validated) staging rows for all four streams.
    /// </summary>
    public class BulkRatesParseResult
    {
        public string JobName { get; init; } = string.Empty;
        public Guid JobQueueId { get; init; }

        // FEC / Test stream
        public IReadOnlyList<FecStagingRow> FecRows { get; init; } = [];
        public IReadOnlyList<AgrupStagingRow> AgrupRows { get; init; } = [];

        // Staff stream
        public IReadOnlyList<StaffStagingRow> StaffRows { get; init; } = [];

        // Animal stream
        public IReadOnlyList<AnimalStagingRow> AnimalRows { get; init; } = [];

        /// <summary>File-level parse errors (missing worksheet, wrong columns, etc.).</summary>
        public IReadOnlyList<string> ParseErrors { get; init; } = [];

        public bool HasParseErrors => ParseErrors.Count > 0;
    }

    /// <summary>Result returned by BulkRatesValidator after running all checks.</summary>
    public class BulkRatesValidationResult
    {
        public IReadOnlyList<StagingValidationError> Errors { get; init; } = [];
        public BulkRatesRowCounts RowCounts { get; init; } = new();
    }
}
