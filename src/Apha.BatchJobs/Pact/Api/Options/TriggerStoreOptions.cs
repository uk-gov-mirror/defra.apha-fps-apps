namespace Apha.BatchJobs.Pact.Api.Options;

/// <summary>
/// Configuration options for the trigger attempt store (memory or Redis).
/// Bind from <c>BatchJobs:TriggerStore</c> in appsettings or environment.
/// </summary>
public sealed class TriggerStoreOptions
{
    /// <summary>How long a cached trigger record should be retained (minutes).</summary>
    public int EntryTtlMinutes { get; set; } = 60;
}
