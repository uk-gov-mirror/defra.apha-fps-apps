using Apha.BatchJobs.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.BatchJobs.Infrastructure.Repositories.MabArchive.Loaders;

internal sealed class MyTlkpProjectLoader : MabArchiveExecutionLoaderBase
{
    internal MyTlkpProjectLoader(BatchJobsDbContext context) : base(context) { }

    public override int Sequence => 3;

    public override string Name => "my_tlkpproject";

    protected override async Task<int> LoadCoreAsync(BatchJobsDbContext context, int year, CancellationToken cancellationToken)
    {
        var sourceRows = await context.MaSrcTlkpProject
            .AsNoTracking()
            .Where(t => t.FpsYear == year)
            .Select(t => new
            {
                ParentProject = t.ParentProject,
                Program = t.Program,
                Customer = t.Customer,
                Manager = t.Manager,
                TransferIncome = t.TransferIncome,
                CustIncome = t.CustIncome,
                WipEoy = t.WipEoy,
                WipLimit = t.WipLimit,
                WipCurrent = t.WipCurrent,
                ProjectStatus = t.ProjectStatus,
                DateCreated = t.DateCreated,
                FecCost = t.FecCost,
                Profit = t.Profit,
                BudgetCvl = t.BudgetCvl,
                CaseworkSub = t.CaseworkSub,
                PvsIncome = t.PvsIncome,
                PlanCaseworkDebit = t.PlanCaseworkDebit,
                Disease = t.Disease,
                Contract = t.Contract,
                Finished = t.Finished,
                Comments = t.Comments,
                CarryOver = t.CarryOver,
                IsDefraProject = t.IsDefraProject,
                CostCentre = t.CostCentre,
                OracleProjectCode = t.OracleProjectCode,
                SubAccountCode = t.SubAccountCode,
                ProjectGroup = t.ProjectGroup,
                IncomeAccountCode = t.IncomeAccountCode
            })
            .ToListAsync(cancellationToken);

        var rows = sourceRows
            .Select(t => new MaDstMyTlkpProject
            {
                Year = year,
                ParentProject = t.ParentProject,
                Program = t.Program,
                Customer = t.Customer,
                Manager = t.Manager,
                TransferIncome = t.TransferIncome,
                CustIncome = t.CustIncome,
                WipEoy = t.WipEoy,
                WipLimit = t.WipLimit,
                WipCurrent = t.WipCurrent,
                ProjectStatus = t.ProjectStatus,
                DateCreated = t.DateCreated,
                FecCost = t.FecCost,
                Profit = t.Profit,
                BudgetCvl = t.BudgetCvl,
                CaseworkSub = t.CaseworkSub,
                PvsIncome = t.PvsIncome,
                PlanCaseworkDebit = t.PlanCaseworkDebit,
                Disease = t.Disease,
                Contract = t.Contract,
                Finished = t.Finished,
                Comments = t.Comments,
                CarryOver = t.CarryOver,
                IsDefraProject = t.IsDefraProject,
                CostCentre = t.CostCentre,
                OracleProjectCode = t.OracleProjectCode,
                SubAccountCode = t.SubAccountCode,
                ProjectGroup = t.ProjectGroup,
                IncomeAccountCode = t.IncomeAccountCode
            })
            .ToList();

        if (rows.Count == 0)
        {
            return 0;
        }

        await context.MaDstMyTlkpProject.AddRangeAsync(rows, cancellationToken);
        return await context.SaveChangesAsync(cancellationToken);
    }
}

