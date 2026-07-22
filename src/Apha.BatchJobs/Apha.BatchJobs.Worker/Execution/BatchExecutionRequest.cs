using Apha.BatchJobs.Domain.Enums;

namespace Apha.BatchJobs.Worker.Execution;

/// <summary>
/// Validated inputs for one batch job execution, resolved by <see cref="BatchExecutionRequestResolver"/>.
/// HealthCheck never reaches this type — <c>Program.cs</c> short-circuits before the resolver
/// is ever invoked (see the startup refactoring spec's finding on HealthCheck).
/// </summary>
public sealed record BatchExecutionRequest(
    string JobName,
    RunMode RunMode,
    Guid JobExecutionId,
    string RequestedBy,
    DateTime? RequestedAtUtc,
    string? ParametersJson);
