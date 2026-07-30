using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using System.Web;

namespace Apha.Costbook.DataAccess.Repositories;

public class StaffRequirementRepository : RepositoryBase, IStaffRequirementRepository
{
    private readonly IFPSYearContext _fpsYearContext;
    private readonly IProjectRepository _projectRepo;
    private readonly ISettingsRepository _settingsRepo;
    public StaffRequirementRepository(CostbookDbContext context, IFPSYearContext fpsYearContext, ISettingsRepository settingsRepo, IProjectRepository projectRepo)
        : base(context)
    {
        _fpsYearContext = fpsYearContext;
        _settingsRepo = settingsRepo;
        _projectRepo = projectRepo;
    }

    /// <summary>
    /// LINQ equivalent of MS Access qryStaffReqGrade — with server-side sorting and paging.
    /// </summary>
    public async Task<PagedData<StaffRequirementDetailView>> GetStaffRequirementsByProjectYearAsync(
        string project, int year, PaginationParameters<string> query)
    {
        var decodedProject = HttpUtility.UrlDecode(project);
        var fpsYear = _fpsYearContext.FPSYear;

        var baseQuery =
            from sr in _context.StaffRequirements.AsNoTracking()

            join wgg in _context.WorkGroupGrades.AsNoTracking().IgnoreQueryFilters()
              on new { sr.WgGrade, FpsYear = (int?)fpsYear }
              equals new { wgg.WgGrade, FpsYear = wgg.FpsYear } into wggJoin
            from wgg in wggJoin.DefaultIfEmpty()

            join proj in _context.Projects.AsNoTracking()
                on sr.Project equals proj.ProjectId into projJoin
            from proj in projJoin.DefaultIfEmpty()

            join eu in _context.EuGradeConversions.AsNoTracking()
                on wgg.GradeCode equals eu.VlaGrade into euJoin
            from eu in euJoin.DefaultIfEmpty()

            where sr.Project == decodedProject && sr.Year == year
            select new StaffRequirementDetailView
            {
                SrIdentity   = sr.SrIdentity,
                Project      = sr.Project,
                Year         = sr.Year,
                WgGrade      = sr.WgGrade,
                Name         = sr.Name,
                Nohours      = sr.Nohours,
                Nodays       = sr.Nodays,
                Chargerate   = sr.Chargerate,
                Payrate      = sr.Payrate,
                Npr          = sr.Npr,
                Ohr          = sr.Ohr,
                WorkGroup    = wgg != null ? wgg.WorkGroup  : null,
                GradeCode    = wgg != null ? wgg.GradeCode  : null,
                Programme    = proj != null ? proj.Programme : null,
                EuroConvRate = proj != null ? proj.Euroconvrate : null,
                EuGrade      = eu  != null ? eu.EuGrade     : null
            };

        baseQuery = ApplySorting(baseQuery, query.SortBy, query.Descending);

      
        return await ApplyPaging(baseQuery, query.Page, query.PageSize);
    }

    public async Task<StaffRequirement> AddStaffRequirementAsync(StaffRequirement staffRequirement)
    {
        staffRequirement.Project = HttpUtility.UrlDecode(staffRequirement.Project);

        // Assign Payrate, Ohr, Npr from pay rates lookup based on WgGrade
        await AssignPayRateValuesAsync(staffRequirement);

        _context.StaffRequirements.Add(staffRequirement);
        await _context.SaveChangesAsync();
        return staffRequirement;
    }

    public async Task<StaffRequirement> UpdateStaffRequirementAsync(StaffRequirement staffRequirement)
    {
        staffRequirement.Project = HttpUtility.UrlDecode(staffRequirement.Project);

        // Assign Payrate, Ohr, Npr from pay rates lookup based on WgGrade
        await AssignPayRateValuesAsync(staffRequirement);

        _context.StaffRequirements.Update(staffRequirement);
        await _context.SaveChangesAsync();
        return staffRequirement;
    }

    public async Task<bool> DeleteStaffRequirementAsync(int srIdentity)
    {
        var deleted = await _context.StaffRequirements
            .Where(s => s.SrIdentity == srIdentity)
            .ExecuteDeleteAsync();
        return deleted > 0;
    }
    public async Task<IEnumerable<PayRateLookup>> GetPayRatesAsync(string projectId, int year, bool isDefra)
    {
        var decodedId = HttpUtility.UrlDecode(projectId);

        var currentYearSetting = await _settingsRepo.GetSettingValueByIdAsync("CurrentYear");

        if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
        {
            throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
        }

        var rows = await _context.WorkGroupGrades
            .AsNoTracking()
            .Join(
                _context.ProfitCentreGrades.AsNoTracking(),
                wg => new { ProfitCentreGrade = wg.ProfitCentreGrade, FpsYear = wg.FpsYear },
                pc => new { ProfitCentreGrade = pc.PcGrade, FpsYear = (int?)pc.FpsYear },
                (wg, pc) => new { wg.WgGrade, pc.ChargeRate, pc.DefraChargeRate, pc.PayRate, pc.Npr, pc.Ohr })
            .Where(x => isDefra ? x.DefraChargeRate != 0 : x.ChargeRate != 0)
            .ToListAsync();

        double inflationFactor = await _projectRepo.GetInflationFactorAsync("InflationStaff", decodedId, year, fyear);

        return rows.Select(x =>
        {
            var baseRate = isDefra ? (decimal?)x.DefraChargeRate : (decimal?)x.ChargeRate;
            return new PayRateLookup
            {
                WgGrade = x.WgGrade,
                ChargeRate = (decimal?)baseRate,
                PayRate = (decimal?)x.PayRate,
                Npr = (decimal?)x.Npr,
                Ohr = (decimal?)x.Ohr,
                ChargeRateWithInflamation = baseRate.HasValue ? (decimal?)(baseRate.Value * (decimal)inflationFactor) : null
            };
        });
    }

    private async Task AssignPayRateValuesAsync(StaffRequirement staffRequirement)
    {
        if (string.IsNullOrEmpty(staffRequirement.Project))
        {
            throw new InvalidOperationException("Staff requirement project cannot be null or empty.");
        }

        var decodedId = staffRequirement.Project;
        var currentYearSetting = await _settingsRepo.GetSettingValueByIdAsync("CurrentYear");

        if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
        {
            throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
        }

        // Retrieve pay rate data for the specific WgGrade
        var payRateRow = await _context.WorkGroupGrades
            .AsNoTracking()
            .Join(
                _context.ProfitCentreGrades.AsNoTracking(),
                wg => new { ProfitCentreGrade = wg.ProfitCentreGrade, FpsYear = wg.FpsYear },
                pc => new { ProfitCentreGrade = pc.PcGrade, FpsYear = (int?)pc.FpsYear },
                (wg, pc) => new { wg.WgGrade, pc.PayRate, pc.Npr, pc.Ohr })
            .Where(x => x.WgGrade == staffRequirement.WgGrade)
            .FirstOrDefaultAsync();
        double inflationFactor = await _projectRepo.GetInflationFactorAsync("InflationStaff", decodedId, staffRequirement.Year ?? 0, fyear);

        // Assign retrieved values to the staff requirement with inflation factor applied
        if (payRateRow != null)
        {
            if (payRateRow.PayRate.HasValue)
            {
                decimal payRateDecimal = payRateRow.PayRate.Value * (decimal)inflationFactor;
                staffRequirement.Payrate = (double?)payRateDecimal;
            }
            if (payRateRow.Npr.HasValue)
            {
                decimal nprDecimal = payRateRow.Npr.Value * (decimal)inflationFactor;
                staffRequirement.Npr = (double?)nprDecimal;
            }
            if (payRateRow.Ohr.HasValue)
            {
                decimal ohrDecimal = payRateRow.Ohr.Value * (decimal)inflationFactor;
                staffRequirement.Ohr = (double?)ohrDecimal;
            }
        }
    }
    // ── Private helpers ───────────────────────────────────────────────────────

    private static IQueryable<StaffRequirementDetailView> ApplySorting(
        IQueryable<StaffRequirementDetailView> query, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
            return query.OrderBy(c => c.WorkGroup).ThenBy(c => c.WgGrade);

        return sortBy.ToLower() switch
        {
            "sridentity" => ApplyOrder(query, c => c.SrIdentity, descending),
            "project"    => ApplyOrder(query, c => c.Project, descending),
            "year"       => ApplyOrder(query, c => c.Year, descending),
            "wggrade"    => ApplyOrder(query, c => c.WgGrade, descending),
            "name"       => ApplyOrder(query, c => c.Name, descending),
            "nodays"     => ApplyOrder(query, c => c.Nodays, descending),
            "nohours"    => ApplyOrder(query, c => c.Nohours, descending),
            "chargerate" => ApplyOrder(query, c => c.Chargerate, descending),
            _            => query.OrderBy(c => c.WgGrade)
        };
    }

    private static IQueryable<StaffRequirementDetailView> ApplyOrder<TKey>(
        IQueryable<StaffRequirementDetailView> query,
        System.Linq.Expressions.Expression<Func<StaffRequirementDetailView, TKey>> keySelector,
        bool descending)
    {
        return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
    }
}
