using Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.RecreateSummaries;
using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.RecreateSummaries.Steps;

internal sealed class InsertMissingProjectsStep : RecreateSummariesExecutionStepBase
{
    public override string StepName => "InsertMissingProjects";

    protected override async Task<int> ExecuteCoreAsync(RecreateSummariesExecutionContext context, CancellationToken cancellationToken)
    {
        var db = context.DbContext;
        var rowsAffected = 0;

        for (var month = 1; month <= 12; month++)
        {
            // Year-scoped missing detection: in shared multi-year schema we only insert rows for the current
            // execution year, and avoid duplicates within (project, month, fpsyear).
            var missingProjects = await (
                from p in db.RsTlkpProject.AsNoTracking()
                where p.FpsYear == context.FpsYear
                where !db.RsProjectMonth.AsNoTracking().Any(pm =>
                    pm.Project == p.ParentProject &&
                    pm.MonthNo == month &&
                    pm.FpsYear == context.FpsYear)
                select p.ParentProject)
                .Distinct()
                .ToListAsync(cancellationToken);

            if (missingProjects.Count == 0)
            {
                continue;
            }

            var inserts = missingProjects.Select(parentProject => new RsProjectMonthTable
            {
                Project = parentProject,
                MonthNo = month,
                FpsYear = context.FpsYear
            });

            await db.RsProjectMonth.AddRangeAsync(inserts, cancellationToken);
            rowsAffected += await db.SaveChangesAsync(cancellationToken);
            db.ChangeTracker.Clear();
        }

        return rowsAffected;
    }
}
