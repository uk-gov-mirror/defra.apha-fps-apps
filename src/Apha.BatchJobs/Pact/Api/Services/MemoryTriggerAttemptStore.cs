using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Pact.Api.Services;

/// <summary>
/// In-process memory-backed <see cref="ITriggerAttemptStore"/>.
/// Suitable for single-instance deployments or local development.
/// Entries expire after <see cref="TriggerStoreOptions.EntryTtlMinutes"/>.
/// </summary>
public sealed class MemoryTriggerAttemptStore : ITriggerAttemptStore
{
    private readonly IMemoryCache _cache;
    private readonly TriggerStoreOptions _options;

    private static string ByIdKey(string jobExecutionId)     => $"pact:trigger:id:{jobExecutionId}";
    private static string LatestKey(string jobName)          => $"pact:trigger:latest:{jobName}";

    public string StoreName => "PactInMemoryCache";

    public MemoryTriggerAttemptStore(IMemoryCache cache, IOptions<TriggerStoreOptions> options)
    {
        _cache   = cache   ?? throw new ArgumentNullException(nameof(cache));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
    }

    public Task SaveAsync(TriggerAttemptRecord record, CancellationToken cancellationToken = default)
    {
        var ttl = TimeSpan.FromMinutes(_options.EntryTtlMinutes);
        _cache.Set(ByIdKey(record.JobExecutionId), record, ttl);
        _cache.Set(LatestKey(record.JobName),      record, ttl);
        return Task.CompletedTask;
    }

    public Task<TriggerAttemptRecord?> GetByJobExecutionIdAsync(string jobExecutionId, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(ByIdKey(jobExecutionId), out TriggerAttemptRecord? record);
        return Task.FromResult(record);
    }

    public Task<TriggerAttemptRecord?> GetLatestByJobNameAsync(string jobName, CancellationToken cancellationToken = default)
    {
        _cache.TryGetValue(LatestKey(jobName), out TriggerAttemptRecord? record);
        return Task.FromResult(record);
    }
}