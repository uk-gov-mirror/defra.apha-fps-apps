using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.PACT.DataAccess.Repository
{
    public class BosworthInterfaceRepository : BaseRepository, IBosworthInterfaceRepository
    {
        public BosworthInterfaceRepository(FpsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TimePurchaseProject>> GetTimePurchaseProjectAsync(string project)
        {
            var result = await _context.TimeCostCalcs.AsNoTracking()
                .Where(timeCost => timeCost.Project == project)
                .Join(_context.WorkGroupStaffViews.AsNoTracking(),
                    timeCost => timeCost.StaffId,
                    staff => staff.PactId,
                    (timeCost, staff) => new { timeCost, staff })
                .Join(_context.Projects.AsNoTracking(),
                    outer => outer.timeCost.Project,
                    project => project.ParentProject,
                    (outer, project) => new { outer.timeCost, outer.staff, project })
                .Join(_context.PactWorkGroupGradeViews.AsNoTracking(),
                    outer => outer.staff.WorkGroupGrade,
                    workGroupGrade => workGroupGrade.WgGrade,
                    (outer, workGroupGrade) => new TimePurchaseProject
                    {
                        Project = outer.timeCost.Project,
                        SellingWg = workGroupGrade.WorkGroup,
                        GradeCode = workGroupGrade.GradeCode,
                        Name = outer.staff.Name,
                        Time = outer.timeCost.Time,
                        Cost = outer.timeCost.Cost,
                        Month = outer.timeCost.Month,
                        JobCode = outer.timeCost.JobCode
                    })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<TimeSaleProfitCentre>> GetTimeSaleProfitCentreAsync(string profitCentre)
        {
            var result = await _context.WorkGroups.AsNoTracking()
                .Where(workGroup => workGroup.ProfitCentre == profitCentre)
                .Join(_context.TimeCostCalcs.AsNoTracking(),
                    workGroup => workGroup.WorkGroupName,
                    timeCost => timeCost.WorkGroup,
                    (workGroup, timeCost) => new { workGroup, timeCost })
                .Join(_context.Projects.AsNoTracking(),
                    outer => outer.timeCost.Project,
                    project => project.ParentProject,
                    (outer, project) => new { outer.workGroup, outer.timeCost, project })
                .GroupBy(outer => new
                {
                    outer.workGroup.ProfitCentre,
                    WorkGroup = outer.workGroup.WorkGroupName,
                    outer.timeCost.GradeCode,
                    outer.timeCost.Name,
                    outer.project.ParentProject,
                    outer.timeCost.JobCode
                })
                .Select(group => new TimeSaleProfitCentre
                {
                    ProfitCentre = group.Key.ProfitCentre,
                    WorkGroup = group.Key.WorkGroup,
                    GradeCode = group.Key.GradeCode,
                    Name = group.Key.Name,
                    ParentProject = group.Key.ParentProject,
                    JobCode = group.Key.JobCode,
                    SumOfTime = group.Sum(item => item.timeCost.Time),
                    SumOfCost = group.Sum(item => item.timeCost.Cost)
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<TimeSaleWorkGroup>> GetTimeSaleWorkGroupAsync(string workGroup)
        {
            var result = await _context.PactWorkGroupGradeViews.AsNoTracking()
                .Where(workGroupGrade => workGroupGrade.WorkGroup == workGroup)
                .Join(_context.WorkGroupStaffViews.AsNoTracking(),
                    workGroupGrade => workGroupGrade.WgGrade,
                    staff => staff.WorkGroupGrade,
                    (workGroupGrade, staff) => new { workGroupGrade, staff })
                .Join(_context.TimeCostCalcs.AsNoTracking(),
                    outer => outer.staff.PactId,
                    timeCost => timeCost.StaffId,
                    (outer, timeCost) => new { outer.workGroupGrade, outer.staff, timeCost })
                .Join(_context.Projects.AsNoTracking(),
                    outer => outer.timeCost.Project,
                    project => project.ParentProject,
                    (outer, project) => new TimeSaleWorkGroup
                    {
                        SellingWg = outer.workGroupGrade.WorkGroup,
                        Name = outer.staff.Name,
                        Time = outer.timeCost.Time,
                        Cost = outer.timeCost.Cost,
                        Month = outer.timeCost.Month,
                        PlanCategory = "",  // Since PlanCategory is not available in the Project entity, keep it empty for this report output to maintain the consistency.
                        Program = project.Program,
                        Project = outer.timeCost.Project,
                        JobCode = outer.timeCost.JobCode,
                        Manager = project.Manager
                    })
                .Distinct()
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<TestSaleSellingWorkgroup>> GetTestSaleSellingWorkgroupAsync(string workGroup)
        {
            var data = await _context.MonthlyOutputs.AsNoTracking()
                .Where(monthlyOutput => monthlyOutput.WorkGroup == workGroup)
                .Join(_context.WorkGroups.AsNoTracking(),
                    monthlyOutput => monthlyOutput.WorkGroup,
                    wrkGroup => wrkGroup.WorkGroupName,
                    (monthlyOutput, wrkGroup) => new { monthlyOutput, wrkGroup })
                .Join(_context.ProfitCentres.AsNoTracking(),
                    outer => outer.wrkGroup.ProfitCentre,
                    profitCentre => profitCentre.ProfitCentreId,
                    (outer, profitCentre) => new { outer.monthlyOutput, outer.wrkGroup, profitCentre })
                .Join(_context.Projects.AsNoTracking(),
                    outer => outer.monthlyOutput.Buyer,
                    project => project.ParentProject,
                    (outer, project) => new { outer.monthlyOutput, outer.wrkGroup, outer.profitCentre, project })
                .Join(_context.TestCapabilities.AsNoTracking(),
                    outer => new { outer.monthlyOutput.WorkGroup, outer.monthlyOutput.TestCode },
                    testCapability => new { WorkGroup = testCapability.WorkGroup, TestCode = testCapability.TestCode },
                    (outer, testCapability) => new { outer.monthlyOutput, outer.wrkGroup, outer.profitCentre, outer.project, testCapability })
                .Join(_context.TestRequirements.AsNoTracking(),
                    outer => new { Buyer = outer.monthlyOutput.Buyer, outer.monthlyOutput.TestCode },
                    testRequirement => new { testRequirement.Buyer, testRequirement.TestCode },
                    (outer, testRequirement) => new { outer.monthlyOutput, outer.wrkGroup, outer.profitCentre, outer.project, outer.testCapability, testRequirement })
                .Join(_context.Programs.AsNoTracking(),
                    outer => outer.project.Program,
                    program => program.ProgramNo,
                    (outer, program) => new { outer.monthlyOutput, outer.wrkGroup, outer.profitCentre, outer.project, outer.testCapability, outer.testRequirement, program })
                .Select(outer => new
                {
                    ProgramNo = outer.program.ProgramNo,
                    BuyerType = (outer.monthlyOutput.Buyer.StartsWith("FT") || outer.monthlyOutput.Buyer.StartsWith("UT"))
                        ? "Commercial"
                        : outer.monthlyOutput.Buyer,
                    Buyer = outer.project.ParentProject,
                    SellerWG = outer.monthlyOutput.WorkGroup,
                    Portfolio = outer.testCapability.PlanPortfolio,
                    TestCode = outer.monthlyOutput.TestCode,
                    Month = outer.monthlyOutput.Month,
                    Volume = outer.monthlyOutput.Volume,
                    UnitPrice = outer.testRequirement.UnitPrice
                })
                .Distinct()
                .ToListAsync();

            var result = data.Select(x => new TestSaleSellingWorkgroup
            {
                ProgramNo = x.ProgramNo,
                BuyerType = x.BuyerType,
                Buyer = x.Buyer,
                SellerWG = x.SellerWG,
                Portfolio = x.Portfolio,
                TestCode = x.TestCode,
                Month = x.Month,
                Volume = x.Volume,
                Fee = x.Volume.HasValue && x.UnitPrice.HasValue
                    ? (decimal)x.Volume.Value * x.UnitPrice.Value
                    : null
            }).ToList();

            return result;
        }

        public async Task<IEnumerable<TestSaleBuyingProject>> GetTestSaleBuyingProjectAsync(string parentProject)
        {
            var data = await _context.Programs.AsNoTracking()
                .Join(_context.Projects.AsNoTracking(),
                    program => program.ProgramNo,
                    project => project.Program,
                    (program, project) => new { program, project })
                .Where(outer => outer.project.ParentProject == parentProject)
                .Join(_context.MonthlyOutputs.AsNoTracking(),
                    outer => outer.project.ParentProject,
                    monthlyOutput => monthlyOutput.Buyer,
                    (outer, monthlyOutput) => new { outer.program, outer.project, monthlyOutput })
                .Join(_context.WorkGroups.AsNoTracking(),
                    outer => outer.monthlyOutput.WorkGroup,
                    wrkGroup => wrkGroup.WorkGroupName,
                    (outer, wrkGroup) => new { outer.program, outer.project, outer.monthlyOutput, wrkGroup })
                .Join(_context.ProfitCentres.AsNoTracking(),
                    outer => outer.wrkGroup.ProfitCentre,
                    profitCentre => profitCentre.ProfitCentreId,
                    (outer, profitCentre) => new { outer.program, outer.project, outer.monthlyOutput, outer.wrkGroup, profitCentre })
                .Join(_context.TestRequirements.AsNoTracking(),
                    outer => new { Buyer = outer.monthlyOutput.Buyer, outer.monthlyOutput.TestCode },
                    testRequirement => new { testRequirement.Buyer, testRequirement.TestCode },
                    (outer, testRequirement) => new { outer.program, outer.project, outer.monthlyOutput, outer.wrkGroup, outer.profitCentre, testRequirement })
                .Select(outer => new
                {
                    ProgramNo = outer.program.ProgramNo,
                    Buyer = outer.project.ParentProject,
                    SellerPC = outer.profitCentre.ProfitCentreId,
                    SellerWG = outer.monthlyOutput.WorkGroup,
                    TestCode = outer.monthlyOutput.TestCode,
                    Month = outer.monthlyOutput.Month,
                    Volume = outer.monthlyOutput.Volume,
                    UnitPrice = outer.testRequirement.UnitPrice
                })
                .Distinct()
                .ToListAsync();

            var result = data.Select(x => new TestSaleBuyingProject
            {
                ProgramNo = x.ProgramNo,
                Buyer = x.Buyer,
                SellerPC = x.SellerPC,
                SellerWG = x.SellerWG,
                TestCode = x.TestCode,
                Month = x.Month,
                Volume = x.Volume,
                Charge = x.Volume.HasValue && x.UnitPrice.HasValue
                    ? x.UnitPrice.Value * (decimal)x.Volume.Value
                    : null
            }).ToList();

            return result;
        }
    }
}