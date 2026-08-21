namespace Apha.BatchJobs.Application.Configuration;

/// <summary>
/// AWS logging settings, named to match the "AwsLogging:LogGroupName" convention already used
/// by the sibling Apha.PIMS/Apha.PACT/Apha.FPS APIs — a top-level, job-agnostic section rather
/// than nested under any one feature's settings.
/// </summary>
public sealed class AwsLoggingSettings
{
    public string LogGroupName { get; set; } = string.Empty;
}
