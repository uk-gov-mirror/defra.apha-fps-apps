using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Npgsql;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries;

internal sealed class RecreateSummariesExecutionContext : IRecreateSummariesExecutionContext
{
    public RecreateSummariesExecutionContext(BatchJobsDbContext dbContext, NpgsqlConnection connection, int fpsYear)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        FpsYear = fpsYear;
    }

    public BatchJobsDbContext DbContext { get; }
    public NpgsqlConnection Connection { get; }
    public int FpsYear { get; }
}
