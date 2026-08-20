using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Domain.Configuration;
using Apha.BatchJobs.Domain.Interfaces.MabArchive;
using Apha.BatchJobs.Infrastructure.Data;
using Apha.BatchJobs.Infrastructure.Repositories.MabArchive;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Apha.BatchJobs.UnitTests;

public class MabArchiveLoaderMetadataTests
{
    [Fact]
    public void LoaderMetadata_MatchesExpectedLegacyCoverage()
    {
        var loaderTypes = typeof(MabArchiveYearRepository).Assembly
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsAbstract: false } &&
                typeof(IMabArchiveLoader).IsAssignableFrom(t) &&
                t.Namespace == "Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders")
            .ToList();

        static bool InheritsFromExecutionLoaderBase(Type type)
        {
            var current = type.BaseType;
            while (current is not null)
            {
                if (string.Equals(current.Name, "MabArchiveExecutionLoaderBase", StringComparison.Ordinal))
                {
                    return true;
                }

                current = current.BaseType;
            }

            return false;
        }

        var executionLoaderTypes = loaderTypes
            .Where(InheritsFromExecutionLoaderBase)
            .ToList();

        Assert.Equal(24, executionLoaderTypes.Count);

        // Some loaders (e.g. MyStaffLoader, CR-028) take constructor dependencies (IOptions<MabArchiveSettings>,
        // ILogger<T>) instead of being parameterless. ActivatorUtilities resolves those from a minimal DI
        // container while still constructing the parameterless loaders directly.
        using var serviceProvider = new ServiceCollection()
            .AddLogging()
            .Configure<MabArchiveSettings>(_ => { })
            .AddDbContext<BatchJobsDbContext>(opts => opts.UseInMemoryDatabase("loader-metadata-test"))
            .BuildServiceProvider();

        var loaders = executionLoaderTypes
            .Select(t => (IMabArchiveLoader)ActivatorUtilities.CreateInstance(serviceProvider, t))
            .OrderBy(l => l.Sequence)
            .ToList();

        Assert.Equal(Enumerable.Range(1, 24), loaders.Select(l => l.Sequence));
        Assert.Equal(24, loaders.Select(l => l.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.DoesNotContain(loaders, l => string.IsNullOrWhiteSpace(l.Name));
    }
}

