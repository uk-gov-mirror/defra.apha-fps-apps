using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    /// <summary>
    /// Repository implementation for the Stage 2 Check Resource Allocation
    /// (frmResourceAllocation) read-only grids.
    /// </summary>
    public class ResourceAllocationRepository : BaseRepository, IResourceAllocationRepository
    {
        private readonly FpsDbContext _dbContext;

        public ResourceAllocationRepository(FpsDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<PagedData<ResourceStaffGeneralSummaryRow>> GetPagedStaffAllocationsByWorkGroupGradeAsync(
            string workGroupGrade, PaginationParameters<string> query)
        {
            var all = await GetStaffAllocationsByWorkGroupGradeAsync(workGroupGrade);
            all = ApplyStaffAllocationFilterAndSort(all, query);
            return ApplyPaging(all, query.Page, query.PageSize);
        }

        public async Task<PagedData<ResourceStaffJobDetailRow>> GetPagedStaffJobDetailsByStaffIdAsync(
            string staffId, PaginationParameters<string> query)
        {
            var all = await GetStaffJobDetailsByStaffIdAsync(staffId);
            all = ApplyStaffJobDetailFilterAndSort(all, query);
            return ApplyPaging(all, query.Page, query.PageSize);
        }

        private static List<ResourceStaffGeneralSummaryRow> ApplyStaffAllocationFilterAndSort(
            List<ResourceStaffGeneralSummaryRow> rows, PaginationParameters<string> query)
        {
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter) ?? [];

                if (filters.TryGetValue("Name", out var nameFilter) && !string.IsNullOrWhiteSpace(nameFilter))
                    rows = rows.Where(r => ContainsIgnoreCase(r.Name, nameFilter)).ToList();
            }

            IOrderedEnumerable<ResourceStaffGeneralSummaryRow> ordered = query.SortBy switch
            {
                "StaffId" => rows.OrderBy(r => r.StaffId),
                "HrsAvail" => rows.OrderBy(r => r.HrsAvail),
                "PlannedHours" => rows.OrderBy(r => r.PlannedHours),
                "ChargeHours" => rows.OrderBy(r => r.ChargeHours),
                "AppPlannedHours" => rows.OrderBy(r => r.AppPlannedHours),
                "AppChargeHours" => rows.OrderBy(r => r.AppChargeHours),
                "Allocation" => rows.OrderBy(r => r.Allocation),
                "Utilization" => rows.OrderBy(r => r.Utilization),
                "AppUtilization" => rows.OrderBy(r => r.AppUtilization),
                _ => rows.OrderBy(r => r.Name),
            };

            return query.Descending ? ordered.Reverse().ToList() : ordered.ToList();
        }

        private static List<ResourceStaffJobDetailRow> ApplyStaffJobDetailFilterAndSort(
            List<ResourceStaffJobDetailRow> rows, PaginationParameters<string> query)
        {
            if (!string.IsNullOrWhiteSpace(query.Filter))
            {
                var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(query.Filter) ?? [];

                if (filters.TryGetValue("Project", out var projectFilter) && !string.IsNullOrWhiteSpace(projectFilter))
                    rows = rows.Where(r => ContainsIgnoreCase(r.JobCode, projectFilter)).ToList();

                if (filters.TryGetValue("Description", out var descFilter) && !string.IsNullOrWhiteSpace(descFilter))
                    rows = rows.Where(r => ContainsIgnoreCase(r.JobDescription, descFilter)).ToList();

                if (filters.TryGetValue("Status", out var statusFilter) && !string.IsNullOrWhiteSpace(statusFilter))
                    rows = rows.Where(r => ContainsIgnoreCase(r.ProjectStatus, statusFilter)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
                rows = rows.Where(r => ContainsIgnoreCase(r.JobCode, query.Search)
                                    || ContainsIgnoreCase(r.JobDescription, query.Search)).ToList();

            IOrderedEnumerable<ResourceStaffJobDetailRow> ordered = query.SortBy switch
            {
                "Project" => rows.OrderBy(r => r.JobCode),
                "Description" => rows.OrderBy(r => r.JobDescription),
                "Hour" => rows.OrderBy(r => r.PlannedHours),
                "Status" => rows.OrderBy(r => r.ProjectStatus),
                _ => rows.OrderBy(r => r.JobCode),
            };

            return query.Descending ? ordered.Reverse().ToList() : ordered.ToList();
        }

        /// <summary>Null-safe case-insensitive contains check.</summary>
        private static bool ContainsIgnoreCase(string? value, string filter) =>
            value != null && value.Contains(filter, StringComparison.OrdinalIgnoreCase);

        private async Task<List<ResourceStaffJobDetailRow>> GetStaffJobDetailsByStaffIdAsync(string staffId)
        {
            return await (
                from job in _dbContext.StaffJobTblViews.AsNoTracking()
                where job.StaffId == staffId
                join proj in _dbContext.ProjectViews.AsNoTracking()
                    on job.JobCode equals proj.ParentProject into jobProjects
                from proj in jobProjects.DefaultIfEmpty()
                select new ResourceStaffJobDetailRow
                {
                    StaffId = job.StaffId,
                    PlannedHours = job.PlannedHours,
                    JobCode = job.JobCode,
                    JobDescription = proj.ParentProject,
                    Programme = proj.Program,
                    ProjectStatus = proj.ProjectStatus
                }
            ).Distinct()
             .ToListAsync();
        }

        private async Task<List<ResourceStaffGeneralSummaryRow>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade)
        {
            // ── Step 1: qryZTonly_CTE ────────────────────────────────────────────────
            // SUM(plannedhours) per StaffID where project Program = 'ZT_Prog'.
            // Mirrors the CTE and is resolved in-memory to avoid a grouped LEFT JOIN
            // that EF Core cannot translate in a single expression tree.
            var ztHoursById = await (
                from proj in _dbContext.Projects.AsNoTracking()
                where proj.Program == "ZT_Prog"
                join sj in _dbContext.StaffJobs.AsNoTracking()
                    on proj.ParentProject equals sj.JobCode
                group sj by sj.StaffId into g
                select new
                {
                    StaffId = g.Key,
                    SumPlannedHours = g.Sum(j => j.PlannedHours)
                }
            ).ToDictionaryAsync(x => x.StaffId, x => x.SumPlannedHours);

            // ── Step 2: Main grouped query ───────────────────────────────────────────
            // vtblStaff  → tblWGEmployee INNER JOIN tblEmployee (on SPNumber)
            //                            INNER JOIN WorkGroupGrade (on WorkGroupGrade = WGGrade)
            // sj         → tblStaffJob LEFT JOIN (filtered to known WGEmployee PACTids implicitly
            //              because we join on wg.PactId = sj.StaffId from the outer side)
            // p          → tlkpProject LEFT JOIN on sj.Jobcode = p.ParentProject
            // WHERE name LIKE '%General' AND WorkGroupGrade = @workGroupGrade
            var grouped = await (
                from wg in _dbContext.WorkGroupEmployees.AsNoTracking()
                    // WorkGroupGrade IN (SELECT WGGrade FROM WorkGroupGrade)
                join wgg in _dbContext.WorkgroupGrades.AsNoTracking()
                    on wg.WorkGroupGrade equals wgg.WgGrade
                // tblEmployee join (CROSS JOIN + WHERE SPNumber matches = INNER JOIN)
                join e in _dbContext.Employees.AsNoTracking()
                    on wg.SpNumber equals e.SPNumber
                where wg.WorkGroupGrade == workGroupGrade &&
                    ((e.LastName ?? "") + ", " + (e.FirstName ?? "")).EndsWith("General")
                // LEFT JOIN tblStaffJob on s.StaffID = sj.StaffID
                join sj in _dbContext.StaffJobs.AsNoTracking()
                    on wg.PactId equals sj.StaffId into staffJobs
                from sj in staffJobs.DefaultIfEmpty()
                    // LEFT JOIN tlkpProject on sj.Jobcode = p.ParentProject
                join proj in _dbContext.Projects.AsNoTracking()
                    on sj.JobCode equals proj.ParentProject into jobProjects
                from proj in jobProjects.DefaultIfEmpty()
                group new { sj, proj } by new
                {
                    wg.WorkGroupGrade,
                    StaffId = wg.PactId,
                    Name = (e.LastName ?? "") + ", " + (e.FirstName ?? ""),
                    wg.HrsAvail
                } into g
                orderby g.Key.WorkGroupGrade, g.Key.Name
                select new
                {
                    g.Key.WorkGroupGrade,
                    g.Key.StaffId,
                    g.Key.Name,
                    g.Key.HrsAvail,
                    AppPlannedHours = g.Sum(x =>
                        x.proj.ParentProject != null && x.sj.JobCode != null
                            ? x.sj.PlannedHours
                            : 0.0),
                    PlannedHours = g.Sum(x =>
                        x.sj.JobCode != null ? x.sj.PlannedHours : 0.0)
                }
            ).ToListAsync();

            // ── Step 3: Merge ZtHours and compute derived columns in-memory ──────────
            return grouped.Select(row =>
            {
                var ztHrs = ztHoursById.TryGetValue(row.StaffId, out var zt) ? zt : 0.0;
                return new ResourceStaffGeneralSummaryRow
                {
                    WorkGroupGrade = row.WorkGroupGrade,
                    StaffId = row.StaffId,
                    Name = row.Name,
                    HrsAvail = Math.Round(row.HrsAvail, 2),
                    ZtHours = Math.Round(ztHrs, 2),
                    AppPlannedHours = Math.Round(row.AppPlannedHours, 2),
                    PlannedHours = Math.Round(row.PlannedHours, 2),
                    ChargeHours = Math.Round(row.PlannedHours - ztHrs, 2),
                    AppChargeHours = Math.Round(row.AppPlannedHours - ztHrs, 2),
                    Allocation = (row.HrsAvail == 0) ? (double?)null : Math.Round((row.PlannedHours / row.HrsAvail), 4),
                    Utilization = (row.HrsAvail == 0) ? (double?)null : Math.Round(((row.PlannedHours - ztHrs) / row.HrsAvail), 4),
                    AppUtilization = (row.HrsAvail == 0) ? (double?)null : Math.Round(((row.AppPlannedHours - ztHrs) / row.HrsAvail), 4)
                };
            }).ToList();
        }
    }
}