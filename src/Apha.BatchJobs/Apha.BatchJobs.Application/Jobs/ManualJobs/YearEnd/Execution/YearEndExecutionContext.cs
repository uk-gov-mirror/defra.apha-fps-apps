using System.Text.Json;

namespace Apha.BatchJobs.Application.Jobs.ManualJobs.YearEnd.Execution;

/// <summary>
/// Immutable execution context for Year End worker services.
/// </summary>
public sealed record YearEndExecutionContext(
    string CorrelationId,
    string? ParametersJson,
    int? CurrentFpsYear,
    int? TargetFpsYear)
{
    /// <summary>
    /// Builds execution context from worker environment variables.
    /// </summary>
    public static YearEndExecutionContext FromEnvironment(string? correlationId)
    {
        var parametersJson = Environment.GetEnvironmentVariable("BATCH_JOB_PARAMETERS_JSON");
        var currentFpsYear = TryReadInt(parametersJson, "currentFpsYear");
        var targetFpsYear = TryReadInt(parametersJson, "targetFpsYear");

        return new YearEndExecutionContext(
            string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("D") : correlationId,
            parametersJson,
            currentFpsYear,
            targetFpsYear);
    }

    private static int? TryReadInt(string? json, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty(propertyName, out var value))
            {
                return null;
            }

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var parsedInt))
            {
                return parsedInt;
            }

            if (value.ValueKind == JsonValueKind.String
                && int.TryParse(value.GetString(), out parsedInt))
            {
                return parsedInt;
            }
        }
        catch (JsonException)
        {
            // Parameters are validated in worker contract pre-check.
        }

        return null;
    }
}
