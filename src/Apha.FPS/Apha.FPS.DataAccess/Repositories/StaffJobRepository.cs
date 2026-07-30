using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class StaffJobRepository : BaseRepository, IStaffJobRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public StaffJobRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<PagedData<StaffJobView>> GetJobStaffCostAsync(PaginationParameters<string> query, string jobCode)
        {
            var queryStaffJob = await BuildJobStaffCostQueryAsync(jobCode);

            var result = (await queryStaffJob.ToListAsync())
                .Select(ComputeStaffCost)
                .ToList();

            var lookupStaffList = await GetStaffWorkgroupLookup();
            var staffNameMap = lookupStaffList
                .GroupBy(s => s.StaffID)
                .ToDictionary(g => g.Key, g => g.First().Name);

            foreach (var item in result)
            {
                if (item.StaffID != null && staffNameMap.TryGetValue(item.StaffID, out var name))
                    item.Name = name;
            }

            result = ApplySorting(result, query.SortBy, query.Descending);
            result = ApplyStaffJobFilterInMemory(result, query.Filter);

            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<decimal> GetTotalStaffCostAsync(string jobCode)
        {
            var query = await BuildJobStaffCostQueryAsync(jobCode);
            var result = (await query.ToListAsync()).Select(ComputeStaffCost).ToList();
            return result.Sum(x => x.StaffCost) ?? 0m;
        }

        public async Task<List<StaffWorkgroupLookup>> GetStaffWorkgroupLookup()
        {
            var query = (from s in _dbContext.StaffViews
                         join sp in _dbContext.StaffPickViews on s.StaffId equals sp.StaffId
                         where s.UserEmail != null && s.UserEmail.ToLower() == _requestContext.UserEmailId
                         select new StaffWorkgroupLookup
                         {
                             StaffID = s.StaffId ?? "",
                             Name = s.Name ?? "",
                             WorkGroupGrade = s.WorkgroupGrade ?? "",
                             HrsAvail = s.HrsAvail ?? 0,
                             HrsPaid = s.HrsPaid ?? 0,
                             Leave = s.Leave ?? 0,
                             SickSpecial = s.SickSpecial ?? 0
                         }).Distinct().OrderBy(e => e.Name);

            return await query.ToListAsync();
        }

        public async Task<StaffWorkgroupLookup?> GetStaffSummaryByIdAsync(string staffId)
        {
            return await _dbContext.StaffViews
                .AsNoTracking()
                .Where(s => s.StaffId == staffId)
                .Select(s => new StaffWorkgroupLookup
                {
                    StaffID = s.StaffId ?? "",
                    Name = s.Name ?? "",
                    WorkGroupGrade = s.WorkgroupGrade ?? "",
                    HrsAvail = s.HrsAvail ?? 0,
                    HrsPaid = s.HrsPaid ?? 0,
                    Leave = s.Leave ?? 0,
                    SickSpecial = s.SickSpecial ?? 0
                })
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> GetStaffChargeRate(string staffId, string jobcode)
        {
            var result =
                    from wg in _dbContext.WorkGroupEmployees
                    join e in _dbContext.Employees
                        on wg.SpNumber equals e.SPNumber
                    join w in _dbContext.WorkgroupGrades
                        on wg.WorkGroupGrade equals w.WgGrade
                    join p in _dbContext.ProfitCentreGrades
                        on w.ProfitCentreGrade equals p.PcGrade
                    join s in _dbContext.StaffJobs
                        on wg.PactId equals s.StaffId
                    join t in _dbContext.Projects
                        on s.JobCode equals t.ParentProject
                    where s.StaffId == staffId
                    select new
                    {
                        ParentProject = t.ParentProject,
                        ChargeRate = t.IsDefraProject == -1
                            ? p.DefraChargeRate
                            : p.ChargeRate
                    };

            decimal? changeRate = await result.Where(e => e.ParentProject == jobcode).Select(e => e.ChargeRate).FirstOrDefaultAsync();
            changeRate ??= await result.Select(e => e.ChargeRate).FirstOrDefaultAsync();
            return changeRate;
        }

        public async Task<StaffJob?> GetByIdAsync(string staffId, string jobCode)
        {
            var query = await _dbContext.StaffJobs
                    .FirstOrDefaultAsync(sj => sj.StaffId == staffId && sj.JobCode == jobCode);
            return query;
        }

        public async Task<StaffJobView?> GetViewByStaffIdAsync(string staffId, string jobCode)
        {
            var queryStaffJob = await BuildJobStaffCostQueryAsync(jobCode);
            var record = await queryStaffJob.Where(e => e.StaffID == staffId).FirstOrDefaultAsync();

            var lookupStaffList = await GetStaffWorkgroupLookup();
            var staffName = lookupStaffList
                .Where(p => p.StaffID == staffId).Select(s => new { s.StaffID, s.Name }).FirstOrDefault();

            record?.Name = staffName?.Name;

            return record != null ? ComputeStaffCost(record) : null;
        }

        public async Task<StaffJob> AddAsync(StaffJob staffJob)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingStaffJob = await _dbContext.StaffJobs
                        .FirstOrDefaultAsync(sj => sj.StaffId == staffJob.StaffId
                                                && sj.JobCode == staffJob.JobCode);

                    if (existingStaffJob is not null)
                        throw new InvalidOperationException(
                            $"Staff job with StaffId {staffJob.StaffId} and JobCode {staffJob.JobCode} already exists");

                    var newStaffJob = new StaffJob
                    {
                        StaffId = staffJob.StaffId,
                        JobCode = staffJob.JobCode,
                        PlannedHours = staffJob.PlannedHours,
                        FpsYear = _requestContext.FpsYear
                    };

                    var logEntry = CreateStaffJobLogEntry(newStaffJob.StaffId, newStaffJob.JobCode, newStaffJob.PlannedHours, "I");

                    _dbContext.StaffJobs.Add(newStaffJob);
                    _dbContext.StaffJobLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return newStaffJob;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<StaffJob> UpdateAsync(StaffJob staffJob)
        {
            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var existingStaffJob = await _dbContext.StaffJobs
                        .FirstOrDefaultAsync(sj => sj.StaffId == staffJob.StaffId
                                                && sj.JobCode == staffJob.JobCode);

                    if (existingStaffJob is null)
                        throw new InvalidOperationException(
                            $"Staff job with StaffId {staffJob.StaffId} and JobCode {staffJob.JobCode} not found");

                    existingStaffJob.PlannedHours = staffJob.PlannedHours;
                    existingStaffJob.FpsYear = _requestContext.FpsYear;

                    var logEntry = CreateStaffJobLogEntry(existingStaffJob.StaffId, existingStaffJob.JobCode, existingStaffJob.PlannedHours, "I");

                    _dbContext.StaffJobLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return existingStaffJob;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        public async Task<bool> DeleteAsync(string staffId, string jobCode)
        {
            var staffJob = await _dbContext.StaffJobs
                   .FirstOrDefaultAsync(sj => sj.StaffId == staffId && sj.JobCode == jobCode);

            if (staffJob is null)
                return false;

            var strategy = _dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var logEntry = CreateStaffJobLogEntry(staffJob.StaffId, staffJob.JobCode, staffJob.PlannedHours, "D");

                    _dbContext.StaffJobs.Remove(staffJob);
                    _dbContext.StaffJobLogs.Add(logEntry);
                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }

        private static StaffJobView ComputeStaffCost(StaffJobView view)
        {
            view.StaffCost = (decimal)view.PlannedHours *
                             (view.ChargeRate ?? 0m) *
                             ((view.SectorName ?? "").Trim().ToLower() == "charge" ? 1m : 0m);
            return view;
        }

        private StaffJobLog CreateStaffJobLogEntry(string staffId, string jobCode, double plannedHours, string insertDelete)
        {
            return new StaffJobLog
            {
                StaffId = staffId,
                JobCode = jobCode,
                PlannedHours = plannedHours,
                DateTime = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                UserId = _requestContext.UserEmailId,
                InsertDelete = insertDelete,
                FpsYear = _requestContext.FpsYear
            };
        }

        private async Task<IQueryable<StaffJobView>> BuildJobStaffCostQueryAsync(string jobCode)
        {
            var dutyHours = await _dbContext.TblSettings
                .Where(e => e.Id == "HoursInDay")
                .Select(e => e.Setting)
                .FirstOrDefaultAsync();


            var projProgram = (from p in _dbContext.ProjectViews
                               join prg in _dbContext.ProgramViews on
                                   new { p.Program, p.UserId } equals new { Program = prg.ProgramNo, prg.UserId }
                               where p.ParentProject == jobCode
                                     && p.UserEmail != null
                                     && p.UserEmail.ToLower() == _requestContext.UserEmailId
                               select new
                               {
                                   p.ParentProject,
                                   prg.SectorName,
                                   p.IsDefraProject,
                                   prg.UserId,
                                   prg.UserEmail
                               }).Distinct();

            return (from sj in _dbContext.StaffJobTblViews
                    join s in _dbContext.StaffGeneralViews on sj.StaffId equals s.StaffId
                    join wg in _dbContext.WorkgroupGrades on s.WorkGroupGrade equals wg.WgGrade
                    join pc in _dbContext.ProfitCentreGrades on wg.ProfitCentreGrade equals pc.PcGrade
                    join pp in projProgram on
                        new { sj.JobCode, sj.UserId } equals new { JobCode = pp.ParentProject, pp.UserId }
                    let dailyRate = (pp.IsDefraProject == -1 ? pc.DefraChargeRate : pc.ChargeRate)
                    where sj.JobCode == jobCode
                    select new StaffJobView
                    {
                        StaffID = sj.StaffId,
                        JobCode = sj.JobCode,
                        PlannedHours = sj.PlannedHours ?? 0,
                        Name = "",
                        WorkGroupGrade = s.WorkGroupGrade,
                        ChargeRate = dailyRate,
                        StaffCost = 0m,
                        GradeCode = wg.GradeCode,
                        WorkGroup = wg.Workgroup,
                        SectorName = pp.SectorName,
                        Days = dutyHours != null ? (sj.PlannedHours ?? 0) / Convert.ToDouble(dutyHours) : 0
                    }).Distinct().OrderBy(e => e.Name).AsQueryable();
        }

        private static List<StaffJobView> ApplySorting(List<StaffJobView> list, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return list;

            IEnumerable<StaffJobView> sorted = sortBy.ToLower() switch
            {
                "name"         => descending ? list.OrderByDescending(i => i.Name)         : list.OrderBy(i => i.Name),
                "chargerate"   => descending ? list.OrderByDescending(i => i.ChargeRate)   : list.OrderBy(i => i.ChargeRate),
                "plannedhours" => descending ? list.OrderByDescending(i => i.PlannedHours) : list.OrderBy(i => i.PlannedHours),
                "days"         => descending ? list.OrderByDescending(i => i.Days)         : list.OrderBy(i => i.Days),
                "staffcost"    => descending ? list.OrderByDescending(i => i.StaffCost)    : list.OrderBy(i => i.StaffCost),
                _              => list
            };

            return sorted.ToList();
        }

        private static List<StaffJobView> ApplyStaffJobFilterInMemory(List<StaffJobView> list, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return list;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return list;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Name", out var name) && name != null)
            {
                var nameStr = name.ToString()!;
                list = list.Where(x => x.Name != null && x.Name.Contains(nameStr, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return list;
        }        

        public async Task<PagedData<StaffResourceUtilisationView>> GetStaffResourceUtilisationAsync(
            PaginationParameters<string> query, string workgroup)
        {
            // Stage 1: fetch flat distinct rows from the DB, mirroring the SQL SELECT list.
            // FpsYear is driven from _requestContext so it is never hardcoded.
            var rawData = await (
                from wg in _context.WorkgroupGrades

                join s in _context.StaffViews
                    on new { WorkgroupGrade = wg.WgGrade, FpsYear = (int?)wg.FpsYear }
                    equals new { s.WorkgroupGrade, s.FpsYear }

                join pc in _context.ProfitCentreGrades
                    on new { ProfitCentreGrade = wg.ProfitCentreGrade, FpsYear = wg.FpsYear }
                    equals new { ProfitCentreGrade = pc.PcGrade, FpsYear = (int?)pc.FpsYear }

                join sj in _context.StaffJobRmViews
                    on new { StaffId = s.StaffId, FpsYear = s.FpsYear }
                    equals new { sj.StaffId, sj.FpsYear } into staffJobs
                from sj in staffJobs.DefaultIfEmpty()

                join p in _context.Projects
                    on sj.JobCode equals p.ParentProject into projects
                from p in projects.DefaultIfEmpty()

                where EF.Functions.ILike(s.Name!, "%General")
                      && wg.FpsYear == _requestContext.FpsYear
                      && EF.Functions.ILike(s.UserEmail!, _requestContext.UserEmailId)
                      && wg.Workgroup == workgroup

                select new RawUtilisationRow
                {
                    ProfitCentre  = pc.ProfitCentre,
                    Workgroup     = wg.Workgroup,
                    WgGrade       = wg.WgGrade,
                    StaffId       = s.StaffId,
                    Name          = s.Name,
                    HrsAvail      = s.HrsAvail,
                    Program       = p  != null ? p.Program       : null,
                    ProjectStatus = p  != null ? p.ProjectStatus : null,
                    PlannedHours  = sj != null ? sj.PlannedHours : (double?)null
                }
            ).Distinct().ToListAsync();

            // Stage 2: aggregate in-memory — mirrors SQL GROUP BY + SUM(CASE WHEN …).
            var result = rawData
                .GroupBy(x => new
                {
                    x.ProfitCentre,
                    x.Workgroup,
                    x.WgGrade,
                    x.StaffId,
                    x.Name,
                    x.HrsAvail
                })
                .Select(g => BuildUtilisationView(g))
                .AsQueryable();

            result = result.Where(e => e.WorkGroup == workgroup).AsQueryable();
            result = ApplyStaffResourceUtilisationFilter(result, query.Filter);
            result = ApplyStaffResourceUtilisationSorting(result, query.SortBy, query.Descending);
            return base.ApplyPaging(result.AsEnumerable().ToList(), query.Page, query.PageSize);
        }

        private static StaffResourceUtilisationView BuildUtilisationView(
            IEnumerable<RawUtilisationRow> group)
        {
            var first = group.First();

            double hrsAvail     = first.HrsAvail ?? 0d;
            double plannedZt    = group.Sum(x => x.Program       == "zt_prog"      ? (x.PlannedHours ?? 0d) : 0d);
            double nApproved    = group.Sum(x => x.ProjectStatus == "Not Approved" ? (x.PlannedHours ?? 0d) : 0d);
            double approvedRaw  = group.Sum(x => x.ProjectStatus == "Approved"     ? (x.PlannedHours ?? 0d) : 0d);

            double approvedSoct = Math.Round(approvedRaw - plannedZt,               2);
            double availSoct    = Math.Round(hrsAvail    - plannedZt,               2);
            double left         = Math.Round(availSoct   - approvedSoct - nApproved, 2);

            bool hasHrs = hrsAvail != 0d;

            return new StaffResourceUtilisationView
            {
                ProfitCentre       = first.ProfitCentre,
                WorkGroup          = first.Workgroup,
                WgGrade            = first.WgGrade,
                StaffId            = first.StaffId,
                Name               = first.Name,
                HrsAvail           = hrsAvail,
                PlannedZt          = plannedZt,
                AvailSoct          = availSoct,
                NotApprovedSoct    = nApproved,
                ApprovedSoct       = approvedSoct,
                Left               = left,
                ApprovedUtilPct    = hasHrs ? Math.Round(approvedSoct              * 100d / hrsAvail, 2) : null,
                NotApprovedUtilPct = hasHrs ? Math.Round(nApproved                 * 100d / hrsAvail, 2) : null,
                TotalUtilPct       = hasHrs ? Math.Round((approvedSoct + nApproved) * 100d / hrsAvail, 2) : null
            };
        }

        private sealed class RawUtilisationRow
        {
            public string? ProfitCentre { get; set; }
            public string? Workgroup { get; set; }
            public string? WgGrade { get; set; }
            public string? StaffId { get; set; }
            public string? Name { get; set; }
            public double? HrsAvail { get; set; }
            public string? Program { get; set; }
            public string? ProjectStatus { get; set; }
            public double? PlannedHours { get; set; }
        }

        private static IQueryable<StaffResourceUtilisationView> ApplyStaffResourceUtilisationFilter(
            IQueryable<StaffResourceUtilisationView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("WgGrade", out var wgGrade) && wgGrade != null)
                query = query.Where(x => x.WgGrade != null &&
                    x.WgGrade.Contains(wgGrade.ToString()!, StringComparison.OrdinalIgnoreCase));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => x.Name != null &&
                    x.Name.Contains(name.ToString()!, StringComparison.OrdinalIgnoreCase));

            return query;
        }

        private static IQueryable<StaffResourceUtilisationView> ApplyStaffResourceUtilisationSorting(
            IQueryable<StaffResourceUtilisationView> query, string? sortBy, bool descending)
        {
            Expression<Func<StaffResourceUtilisationView, object?>> keySelector = (sortBy?.ToLower()) switch
            {
                "name" => x => x.Name,
                "hrsavail" => x => x.HrsAvail,
                "plannedzt" => x => x.PlannedZt,
                "availsoct" => x => x.AvailSoct,
                "notapprovedsoct" => x => x.NotApprovedSoct,
                "approvedsoct" => x => x.ApprovedSoct,
                "left" => x => x.Left,
                "approvedutilpct" => x => x.ApprovedUtilPct,
                "notapprovedutilpct" => x => x.NotApprovedUtilPct,
                "totalutilpct" => x => x.TotalUtilPct,
                _ => x => x.WgGrade
            };

            bool applyDescending = descending && !string.IsNullOrEmpty(sortBy);
            return applyDescending
                ? query.OrderByDescending(keySelector)
                : query.OrderBy(keySelector);
        }

        public async Task<double> GetZtTotalHoursByStaffIdAsync(string staffId)
        {
            return await (from sj in _dbContext.StaffJobTblViews
                          join jc in _dbContext.JobCodes on sj.JobCode equals jc.JobCodeId
                          where sj.StaffId == staffId
                                && jc.Type != null && jc.Type.ToUpper() == "ZT"
                          select (double?)sj.PlannedHours)
                .SumAsync(h => h ?? 0);
        }

        public async Task<PagedData<StaffJobZtView>> GetZtStaffJobsByStaffIdPagedAsync(PaginationParameters<string> query, string staffId)
        {
            var baseQuery = (from sj in _dbContext.StaffJobTblViews
                             join jc in _dbContext.ProjectViews on sj.JobCode equals jc.ParentProject
                             where sj.StaffId == staffId
                             && (EF.Functions.ILike(jc.UserEmail!, _requestContext.UserEmailId))
                             select new StaffJobZtView
                             {
                                 StaffID = sj.StaffId,
                                 JobCode = sj.JobCode,
                                 PlannedHours = (double?)sj.PlannedHours ?? 0,
                                 Name = jc.ProjectTitle
                             }).Distinct().AsQueryable();

            baseQuery = ApplyZtStaffJobFilter(baseQuery, query.Filter);
            baseQuery = ApplyZtSorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.AsNoTracking().ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<StaffJobZtView?> GetZtStaffJobDetailsByIdAsync(string staffId, string jobCode)
        {
            var baseQuery = (from vsjt in _dbContext.StaffJobTblViews
                             join vjc in _dbContext.ProjectViews on vsjt.JobCode equals vjc.ParentProject
                             where vsjt.StaffId == staffId
                             && (EF.Functions.ILike(vjc.UserEmail!, _requestContext.UserEmailId))
                             && vsjt.JobCode == jobCode
                             select new StaffJobZtView
                             {
                                 StaffID = vsjt.StaffId,
                                 JobCode = vsjt.JobCode,
                                 PlannedHours = (double?)vsjt.PlannedHours ?? 0,
                                 Name = vjc.ProjectTitle
                             }).Distinct().AsQueryable();

            var result = await baseQuery.AsNoTracking().FirstOrDefaultAsync();
            return result;
        }

        private static IQueryable<StaffJobZtView> ApplyZtSorting(IQueryable<StaffJobZtView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query;

            return sortBy.ToLower() switch
            {
                "jobcode" => descending ? query.OrderByDescending(x => x.JobCode) : query.OrderBy(x => x.JobCode),
                "plannedhours" => descending ? query.OrderByDescending(x => x.PlannedHours) : query.OrderBy(x => x.PlannedHours),
                "name" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                _ => query
            };
        }

        public async Task<PagedData<StaffJobView>> GetStaffJobsAllocationByJobCodeWgGradePagedAsync(PaginationParameters<string> query, string jobcode, string wgGrade)
        {
            var baseQuery = (from sj in _dbContext.StaffJobs
                             join wge in _dbContext.WorkGroupEmployees on sj.StaffId equals wge.PactId
                             join wgg in _dbContext.WorkgroupGrades on wge.WorkGroupGrade equals wgg.WgGrade
                             join wg in _dbContext.Workgroups on wgg.Workgroup equals wg.WorkGroupName
                             join pc in _dbContext.ProfitCentres on wg.ProfitCentre equals pc.ProfitCentreId
                             join upc in _dbContext.UserProfitcentres on pc.ProfitCentreId equals upc.ProfitCentre
                             join emp in _dbContext.Employees on wge.SpNumber equals emp.SPNumber
                             join u in _dbContext.Users on upc.UserId equals u.UserId
                             where wge.WorkGroupGrade == wgGrade &&  sj.JobCode == jobcode
                                && EF.Functions.ILike(u.UserEmail!, _requestContext.UserEmailId)
                             select new StaffJobView
                             {
                                 StaffID = sj.StaffId,
                                 JobCode = sj.JobCode,
                                 PlannedHours = sj.PlannedHours,
                                 WorkGroupGrade = wge.WorkGroupGrade,
                                 Name = (emp.LastName ?? string.Empty) + ", " +
                                                 (emp.FirstName ?? string.Empty),
                             }).Distinct().AsQueryable();

            baseQuery = ApplyStaffJobByStaffIdFilter(baseQuery, query.Filter);
            baseQuery = ApplyStaffJobByStaffIdSorting(baseQuery, query.SortBy, query.Descending);

            var result = await baseQuery.AsNoTracking().ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        private static IQueryable<StaffJobView> ApplyStaffJobByStaffIdFilter(IQueryable<StaffJobView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("StaffID", out var staffId) && staffId != null)
                query = query.Where(x => EF.Functions.ILike(x.StaffID!, $"%{staffId}%"));

            if (dict.TryGetValue("JobCode", out var jobCode) && jobCode != null)
                query = query.Where(x => EF.Functions.ILike(x.JobCode!, $"%{jobCode}%"));

            if (dict.TryGetValue("WorkGroupGrade", out var workGroupGrade) && workGroupGrade != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroupGrade!, $"%{workGroupGrade}%"));

            return query;
        }

        private static IQueryable<StaffJobView> ApplyStaffJobByStaffIdSorting(IQueryable<StaffJobView> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(x => x.JobCode);

            return sortBy.ToLower() switch
            {
                "staffid" => descending ? query.OrderByDescending(x => x.StaffID) : query.OrderBy(x => x.StaffID),
                "jobcode" => descending ? query.OrderByDescending(x => x.JobCode) : query.OrderBy(x => x.JobCode),
                "plannedhours" => descending ? query.OrderByDescending(x => x.PlannedHours) : query.OrderBy(x => x.PlannedHours),
                "workgroupgrade" => descending ? query.OrderByDescending(x => x.WorkGroupGrade) : query.OrderBy(x => x.WorkGroupGrade),
                _ => query.OrderBy(x => x.JobCode)
            };
        }

        private static IQueryable<StaffJobZtView> ApplyZtStaffJobFilter(IQueryable<StaffJobZtView> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!, $"%{name}%"));

            if (dict.TryGetValue("PlannedHours", out var plannedHours) && plannedHours != null)
                query = query.Where(x => EF.Functions.ILike(x.PlannedHours.ToString(), $"%{plannedHours}%"));

            return query;
        }
    }
}
