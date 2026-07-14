using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Options;
using Apha.BatchJobs.Pact.Api.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.UnitTests.Unit;

public sealed class TriggerAttemptStoreTests
{
    [Fact]
    public async Task MemoryStore_SaveAndGetByJobExecutionId_RoundTripsRecord()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new MemoryTriggerAttemptStore(
            memoryCache,
            Options.Create(new TriggerStoreOptions { EntryTtlMinutes = 60 }));

        var record = CreateRecord("11111111-1111-1111-1111-111111111111", "RecreateSummary");

        await store.SaveAsync(record);
        var loaded = await store.GetByJobExecutionIdAsync(record.JobExecutionId);

        Assert.NotNull(loaded);
        Assert.Equal(record.JobExecutionId, loaded!.JobExecutionId);
        Assert.Equal(record.JobName, loaded.JobName);
        Assert.Equal("PactInMemoryCache", store.StoreName);
    }

    [Fact]
    public async Task MemoryStore_GetLatestByJobName_ReturnsLatestSavedRecord()
    {
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var store = new MemoryTriggerAttemptStore(
            memoryCache,
            Options.Create(new TriggerStoreOptions { EntryTtlMinutes = 60 }));

        var first = CreateRecord("11111111-1111-1111-1111-111111111111", "RecreateSummary");
        var second = CreateRecord("22222222-2222-2222-2222-222222222222", "RecreateSummary");

        await store.SaveAsync(first);
        await store.SaveAsync(second);

        var latest = await store.GetLatestByJobNameAsync("RecreateSummary");

        Assert.NotNull(latest);
        Assert.Equal(second.JobExecutionId, latest!.JobExecutionId);
    }

    [Fact]
    public async Task RedisStore_SaveAndGetByJobExecutionId_RoundTripsRecord()
    {
        var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var store = new RedisTriggerAttemptStore(
            distributedCache,
            Options.Create(new TriggerStoreOptions { EntryTtlMinutes = 60 }),
            NullLogger<RedisTriggerAttemptStore>.Instance);

        var record = CreateRecord("33333333-3333-3333-3333-333333333333", "RecreateSummary");

        await store.SaveAsync(record);
        var loaded = await store.GetByJobExecutionIdAsync(record.JobExecutionId);

        Assert.NotNull(loaded);
        Assert.Equal(record.JobExecutionId, loaded!.JobExecutionId);
        Assert.Equal(record.EventId, loaded.EventId);
        Assert.Equal("PactRedisCache", store.StoreName);
    }

    [Fact]
    public async Task RedisStore_GetLatestByJobName_UsesLatestExecutionIdPointer()
    {
        var distributedCache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
        var store = new RedisTriggerAttemptStore(
            distributedCache,
            Options.Create(new TriggerStoreOptions { EntryTtlMinutes = 60 }),
            NullLogger<RedisTriggerAttemptStore>.Instance);

        var first = CreateRecord("44444444-4444-4444-4444-444444444444", "RecreateSummary");
        var second = CreateRecord("55555555-5555-5555-5555-555555555555", "RecreateSummary");

        await store.SaveAsync(first);
        await store.SaveAsync(second);

        var latest = await store.GetLatestByJobNameAsync("RecreateSummary");

        Assert.NotNull(latest);
        Assert.Equal(second.JobExecutionId, latest!.JobExecutionId);
    }

    private static TriggerAttemptRecord CreateRecord(string jobExecutionId, string jobName)
        => new()
        {
            JobExecutionId = jobExecutionId,
            JobName = jobName,
            AcceptedAtUtc = DateTime.UtcNow,
            EventId = "localproc-1234",
            WorkerProcessLaunched = true,
            Status = "WorkerProcessStarted",
            WorkerExitCode = null,
            StoredAtUtc = DateTime.UtcNow
        };
}
