using Apha.BatchJobs.Application.Jobs.ScheduledJobs.MABArchive.Services;
using Apha.BatchJobs.Infrastructure.Data;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal abstract class MabArchiveLoaderBase : IMabArchiveLoader
{
    private readonly BatchJobsDbContext _context;

    protected MabArchiveLoaderBase(BatchJobsDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public abstract int Sequence { get; }

    public abstract string Name { get; }

    public Task<int> LoadAsync(int year, CancellationToken cancellationToken)
    {
        return ExecuteAsync(_context, year, cancellationToken);
    }

    protected abstract Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}

internal abstract class MabArchiveExecutionLoaderBase : MabArchiveLoaderBase
{
    protected MabArchiveExecutionLoaderBase(BatchJobsDbContext context) : base(context) { }

    protected override Task<int> ExecuteAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        return LoadCoreAsync(context, year, cancellationToken);
    }

    protected abstract Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken);
}

