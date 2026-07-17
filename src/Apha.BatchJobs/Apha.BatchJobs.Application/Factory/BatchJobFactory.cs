using Apha.BatchJobs.Application.Interfaces;
using Apha.BatchJobs.Domain.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Apha.BatchJobs.Application.Factory;

/// <summary>
/// Batch job factory implementation that resolves job handlers from the DI container.
/// </summary>
public sealed class BatchJobFactory : IBatchJobFactory
{
    private readonly IServiceProvider _serviceProvider;
    private static readonly IReadOnlyDictionary<string, Type> ConventionalJobMap = BuildConventionalJobMap();

    /// <summary>
    /// Initializes a new instance of the BatchJobFactory.
    /// </summary>
    /// <param name="serviceProvider">Service provider for resolving job instances.</param>
    public BatchJobFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc />
    public IBatchJob Create(string jobName)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name cannot be null or empty.", nameof(jobName));

        // Fast path: resolve only the requested job type (avoids constructing all handlers).
        if (ConventionalJobMap.TryGetValue(jobName, out var jobType))
        {
            if (_serviceProvider.GetService(jobType) is IBatchJob job)
                return job;
        }

        var jobs = _serviceProvider.GetServices<IBatchJob>().ToList();
        var matches = jobs
            .Where(j => string.Equals(j.Name, jobName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
            throw new InvalidOperationException($"Job '{jobName}' is not registered. Available jobs: {string.Join(", ", jobs.Select(j => j.Name).Distinct(StringComparer.OrdinalIgnoreCase))}");

        if (matches.Count > 1)
            throw new InvalidOperationException($"Multiple job handlers are registered with Name='{jobName}'. Job names must be unique.");

        return matches[0];
    }

    /// <inheritdoc />
    public IEnumerable<string> GetAvailableJobs() =>
        _serviceProvider.GetServices<IBatchJob>()
            .Select(j => j.Name)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static IReadOnlyDictionary<string, Type> BuildConventionalJobMap()
    {
        var batchJobType = typeof(IBatchJob);
        var assembly = batchJobType.Assembly;

        return assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                batchJobType.IsAssignableFrom(t))
            .Select(t => new
            {
                Type = t,
                Name = GetConventionalName(t)
            })
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Where(g => g.Count() == 1)
            .ToDictionary(g => g.Key, g => g.Single().Type, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetConventionalName(Type type)
    {
        var name = type.Name;

        if (name.EndsWith("JobHandler", StringComparison.Ordinal))
            name = name[..^"JobHandler".Length];
        else if (name.EndsWith("Handler", StringComparison.Ordinal))
            name = name[..^"Handler".Length];
        else if (name.EndsWith("Job", StringComparison.Ordinal))
            name = name[..^"Job".Length];

        // Keep historical capitalization used by callers.
        if (string.Equals(name, nameof(BatchJobNames.MabArchive), StringComparison.Ordinal))
            return BatchJobNames.MabArchive;

        return name;
    }
}
