namespace Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive;

/// <summary>
/// Execution context for MABArchive load operations, resolved from fps.tblyearmaster.
/// </summary>
/// <param name="OpenYear">The single FPS year with status 'Open'. Receives full totals, archive, and project processing.</param>
/// <param name="PlannedYear">The FPS year with status 'Planned', if any. Receives project-only refresh.</param>
public sealed record MabArchiveExecutionContext(
    int OpenYear,
    int? PlannedYear);
