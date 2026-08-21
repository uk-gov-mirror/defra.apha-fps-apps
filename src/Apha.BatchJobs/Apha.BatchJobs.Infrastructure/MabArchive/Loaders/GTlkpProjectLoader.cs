using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.MabArchive.Loaders;

internal sealed class GTlkpProjectLoader : MabArchiveExecutionLoaderBase
{
    public GTlkpProjectLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 2;

    public override string Name => "g_tlkpproject";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        // g_tlkpproject is global (no year column; PK is parentproject alone) and shared across all years.
        // Legacy sp_DeleteYearsFPSData/sp_AddG_tlkpProject kept it current via delete-then-reinsert every
        // cycle, refreshing descriptive fields (esp. projectstatus) to match whichever year was last processed.
        // A prior conversion of the delete side caused a critical data-loss incident (2026-06-23: bulk delete
        // of this global table purged 1,050+ historical rows) and was fixed by removing the delete entirely.
        // That fix left the insert side as insert-only/skip-existing, which stopped the data loss but also
        // stopped refreshing already-archived rows, leaving projectstatus etc. stale forever after first write.
        // Upsert (no DELETE statement touches this table, ever) fixes the staleness without reintroducing any
        // bulk-delete risk against the global table.
        return await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO mabarchive.g_tlkpproject
                (parentproject, projecttitle, costbookno, disease, contract, shorttitle, projectstatus)
            SELECT DISTINCT ON (t.parentproject)
                t.parentproject, t.projecttitle, t.costbookno, t.disease, t.contract, t.shorttitle, t.projectstatus
            FROM fps.tlkpproject t
            WHERE t.fpsyear = {year}
            ORDER BY t.parentproject
            ON CONFLICT (parentproject) DO UPDATE SET
                projecttitle  = excluded.projecttitle,
                costbookno    = excluded.costbookno,
                disease       = excluded.disease,
                contract      = excluded.contract,
                shorttitle    = excluded.shorttitle,
                projectstatus = excluded.projectstatus;
        ", cancellationToken);
    }
}
