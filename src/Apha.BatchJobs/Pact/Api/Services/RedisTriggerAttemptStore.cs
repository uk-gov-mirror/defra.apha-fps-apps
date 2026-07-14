using System.Text.Json;
using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Pact.Api.Services;

/// <summary>
/// Redis-backed <see cref="ITriggerAttemptStore"/> using <see cref="IDistributedCache"/>.
/// Stores one entry per jobexecutionid and a latest-pointer per jobname.
/// Entries expire after <see cref="TriggerStoreOptions.EntryTtlMinutes"/>.
/// </summary>
public sealed class RedisTriggerAttemptStore : ITriggerAttemptStore
{
    private readonly IDistributedCache _cache;
    private readonly TriggerStoreOptions _options;
    private readonly ILogger<RedisTriggerAttemptStore> _logger;

    private static string ByIdKey(string jobExecutionId)  => $"pact:trigger:id:{jobExecutionId}";
    private static string PointerKey(string jobName)      => $"pact:trigger:latest:{jobName}";

    public string StoreName => "PactRedisCache";

    public RedisTriggerAttemptStore(
        IDistributedCache cache,
        IOptions<TriggerStoreOptions> options,
        ILogger<RedisTriggerAttemptStore> logger)
    {
        _cache   = cache   ?? throw new ArgumentNullException(nameof(cache));
        _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        _logger  = logger  ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveAsync(TriggerAttemptRecord record, CancellationToken cancellationToken = default)
    {
        var ttl = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(_options.EntryTtlMinutes)
        };

        var json = JsonSerializer.Serialize(record);
        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        await _cache.SetAsync(ByIdKey(record.JobExecutionId), bytes, ttl, cancellationToken);
        await _cache.SetAsync(PointerKey(record.JobName),
            System.Text.Encoding.UTF8.GetBytes(record.JobExecutionId), ttl, cancellationToken);

        _logger.LogDebug("Trigger attempt saved | JobExecutionId={JobExecutionId} | JobName={JobName}", record.JobExecutionId, record.JobName);
    }

    public async Task<TriggerAttemptRecord?> GetByJobExecutionIdAsync(string jobExecutionId, CancellationToken cancellationToken = default)
    {
        var bytes = await _cache.GetAsync(ByIdKey(jobExecutionId), cancellationToken);
        if (bytes is null) return null;
        return JsonSerializer.Deserialize<TriggerAttemptRecord>(System.Text.Encoding.UTF8.GetString(bytes));
    }

    public async Task<TriggerAttemptRecord?> GetLatestByJobNameAsync(string jobName, CancellationToken cancellationToken = default)
    {
        var pointerBytes = await _cache.GetAsync(PointerKey(jobName), cancellationToken);
        if (pointerBytes is null) return null;
        var latestId = System.Text.Encoding.UTF8.GetString(pointerBytes);
        return await GetByJobExecutionIdAsync(latestId, cancellationToken);
    }
}