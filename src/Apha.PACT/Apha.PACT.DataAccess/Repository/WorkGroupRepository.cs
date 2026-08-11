using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.PACT.DataAccess.Repository
{
    public class WorkGroupRepository : BaseRepository, IWorkGroupRepository
    {
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupRepository(FpsDbContext context, IFpsRequestContext requestContext) : base(context)
        {
            _requestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        private const string WorkGroupColumn     = "WorkGroup";
        private const string ParentProjectColumn = "ParentProject";
        private const string TimeCodeColumn      = "TimeCode";
        private const string PactStaffIdColumn   = "PACTStaffID";
        private const string NameColumn          = "Name";
        private const string MonthColumn         = "Month";
        private const string HoursColumn         = "Hours";
        private const string ManagerColumn       = "Manager";

        public async Task<IEnumerable<WorkGroup>> GetAllWorkGroupsAsync()
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<List<string>> GetAllWorkGroupNamesAsync()
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .Select(w => w.WorkGroupName)
                .OrderBy(x => x)
                .ToListAsync();
        }        

        public async Task<List<WorkGroupStaffItem>> GetStaffByWorkGroupAsync()
        {
            return await (
                from grade in _context.PactWorkGroupGradeViews.AsNoTracking()
                join staff in _context.WorkGroupStaffViews.AsNoTracking()
                    on grade.WgGrade equals staff.WorkGroupGrade
                join wg in _context.WorkGroups.AsNoTracking()
                    on grade.WorkGroup equals wg.WorkGroupName
                where staff.PersonStatus == null || staff.PersonStatus == "A"
                select new WorkGroupStaffItem
                {
                    WorkGroup = wg.WorkGroupName,
                    PactId = staff.PactId,
                    SpNumber = staff.SpNumber,
                    Name = staff.Name
                })
                .ToListAsync();
        }


        // ─── WorkGroup Maintenance CRUD + lookups (migrated from FPS) ───────────────

        public async Task<PagedData<WorkGroup>> GetPagedAsync(PaginationParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var baseQuery = _context.WorkGroups
                .AsNoTracking()
                .AsQueryable();

            baseQuery = ApplyWorkGroupMaintenanceFilter(baseQuery, query.Filter);
            baseQuery = ApplyFpsWorkGroupSorting(baseQuery, query.SortBy, query.Descending);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<WorkGroup?> GetByKeyAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return null;

            return await _context.WorkGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkGroupName == workGroupName);
        }

        public async Task<WorkGroup> CreateAsync(WorkGroup workGroup)
        {
            ArgumentNullException.ThrowIfNull(workGroup);

            workGroup.FpsYear = _requestContext.FpsYear;

            _context.WorkGroups.Add(workGroup);
            await _context.SaveChangesAsync();
            return workGroup;
        }

        public async Task<WorkGroup> UpdateAsync(string originalWorkGroupName, WorkGroup workGroup)
        {
            ArgumentNullException.ThrowIfNull(workGroup);
            if (string.IsNullOrWhiteSpace(originalWorkGroupName))
                throw new ArgumentException("Original WorkGroupName must be supplied.", nameof(originalWorkGroupName));

            var existing = await _context.WorkGroups
                .FirstOrDefaultAsync(w => w.WorkGroupName == originalWorkGroupName);

            if (existing is null)
                throw new KeyNotFoundException($"Workgroup '{originalWorkGroupName}' not found for the active FPS year.");

            existing.WorkGroupName   = workGroup.WorkGroupName;
            existing.ProfitCentre    = workGroup.ProfitCentre;
            existing.CostCentre      = workGroup.CostCentre;
            existing.CostCentreOld   = workGroup.CostCentreOld;
            existing.Owner           = workGroup.Owner;
            existing.Description     = workGroup.Description;
            existing.CentralOverhead = workGroup.CentralOverhead;
            existing.SendEmail       = workGroup.SendEmail;
            existing.Cos90           = workGroup.Cos90;
            existing.EmailRecipient  = workGroup.EmailRecipient;
            existing.FpsYear         = _requestContext.FpsYear;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return false;

            var deleted = await _context.WorkGroups
                .Where(w => w.WorkGroupName == workGroupName)
                .ExecuteDeleteAsync();

            return deleted > 0;
        }

        public async Task<bool> ExistsAsync(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return false;

            return await _context.WorkGroups
                .AsNoTracking()
                .AnyAsync(w => w.WorkGroupName == workGroupName);
        }

        public async Task<IEnumerable<string>> GetAllProfitCentresAsync()
        {
            return await _context.ProfitCentres
                .AsNoTracking()
                .Select(pc => pc.ProfitCentreId)
                .Distinct()
                .OrderBy(pc => pc)
                .ToListAsync();
        }

        public async Task<IEnumerable<Owner>> GetOwnersAsync()
        {
            var result = await _context.WorkGroupStaffViews
                .AsNoTracking()
                .Join(
                    _context.PactWorkGroupGradeViews.AsNoTracking(),
                    staff => staff.WorkGroupGrade,
                    wggg  => wggg.WgGrade,
                    (staff, wggg) => new
                    {
                        staff.Name,
                        wggg.WorkGroup,
                        wggg.GradeCode
                    })
                .Where(x => x.Name != null
                         && !x.Name.ToLower().Contains("general")
                         && !x.Name.ToLower().Contains("vacancy"))
                .Where(x => x.GradeCode != null && !x.GradeCode.StartsWith("G"))
                .Distinct()
                .OrderBy(x => x.Name)
                .Select(x => new Owner
                {
                    Name      = x.Name!,
                    WorkGroup = x.WorkGroup,
                    GradeCode = x.GradeCode,
                    Expr1     = x.GradeCode != null
                                    ? x.GradeCode.Substring(0, 1)
                                    : null
                })
                .ToListAsync();

            return result;
        }

        public async Task<IEnumerable<double?>> GetCostCentresByProfitCentreAsync(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return Enumerable.Empty<double?>();

            return await _context.WorkGroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.CostCentre != null)
                .Select(w => w.CostCentre)
                .Distinct()
                .OrderBy(cc => cc)
                .ToListAsync();
        }

        private static IQueryable<WorkGroup> ApplyWorkGroupMaintenanceFilter(IQueryable<WorkGroup> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson) || filterJson.Trim() == "{}")
                return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null)
                return query;

            if (filters.TryGetValue("WorkGroupName", out var wgName) && !string.IsNullOrWhiteSpace(wgName))
                query = query.Where(w => EF.Functions.ILike(w.WorkGroupName, $"%{wgName}%"));

            if (filters.TryGetValue("ProfitCentre", out var pc) && !string.IsNullOrWhiteSpace(pc))
                query = query.Where(w => EF.Functions.ILike(w.ProfitCentre, $"%{pc}%"));

            if (filters.TryGetValue("Description", out var desc) && !string.IsNullOrWhiteSpace(desc))
                query = query.Where(w => w.Description != null && EF.Functions.ILike(w.Description, $"%{desc}%"));

            return query;
        }

        public async Task<IEnumerable<SummarisedWgTimeView>> GetSummarisedWorkgroupTimeAsync(
            string workGroup)
        {
            return await _context.SummarisedWgTimeViews
                .AsNoTracking()
                .Where(e => e.WorkGroup == workGroup)
                .ToListAsync();
        }

        public async Task<PagedData<WorkGroupTimeCode>> GetWorkGroupTimeCodeAsync(
            PaginationParameters<string> query, string? workGroup, int? monthNumber)
        {
            var baseQuery = _context.PactWorkGroupGradeViews
                .Join(_context.WorkGroupStaffViews,
                    gradeView => gradeView.WgGrade,
                    staff => staff.WorkGroupGrade,
                    (gradeView, staff) => new { gradeView, staff })
                .Join(_context.MonthlyTimes,
                    gradeStaff => gradeStaff.staff.PactId,
                    timeRecord => timeRecord.PactStaffId,
                    (gradeStaff, timeRecord) => new WorkGroupTimeCode
                    {
                        PACTStaffID   = timeRecord.PactStaffId,
                        ParentProject = timeRecord.ParentProject,
                        WorkGroup     = gradeStaff.gradeView.WorkGroup,
                        Name          = gradeStaff.staff.Name,
                        TimeCode      = timeRecord.TimeCode,
                        Month         = timeRecord.Month,
                        Hours         = timeRecord.Hours ?? 0
                    })
                .Distinct()
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(e => e.WorkGroup == workGroup);

            if (monthNumber.HasValue)
                baseQuery = baseQuery.Where(e => (int)e.Month == monthNumber.Value);

            baseQuery = ApplyWorkGroupTimeCodeFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = (query.SortBy, query.Descending) switch
                {
                    (PactStaffIdColumn, true)  => baseQuery.OrderByDescending(e => e.PACTStaffID),
                    (PactStaffIdColumn, false) => baseQuery.OrderBy(e => e.PACTStaffID),
                    (WorkGroupColumn,      true)  => baseQuery.OrderByDescending(e => e.WorkGroup),
                    (WorkGroupColumn,      false) => baseQuery.OrderBy(e => e.WorkGroup),
                    (ParentProjectColumn,  true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    (ParentProjectColumn,  false) => baseQuery.OrderBy(e => e.ParentProject),
                    (TimeCodeColumn,       true)  => baseQuery.OrderByDescending(e => e.TimeCode),
                    (TimeCodeColumn,       false) => baseQuery.OrderBy(e => e.TimeCode),
                    (MonthColumn,     true)  => baseQuery.OrderByDescending(e => e.Month),
                    (MonthColumn,     false) => baseQuery.OrderBy(e => e.Month),
                    (HoursColumn,     true)  => baseQuery.OrderByDescending(e => e.Hours),
                    (HoursColumn,     false) => baseQuery.OrderBy(e => e.Hours),
                    (_,               true)  => baseQuery.OrderByDescending(e => e.Name),
                    _                        => baseQuery.OrderBy(e => e.Name),
                };
            else
                baseQuery = baseQuery.OrderBy(e => e.Name);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<PagedData<WorkGroupValidTimeCode>> GetWorkGroupValidTimeCodeAsync(
            PaginationParameters<string> query, string workGroup)
        {
            var baseQuery = _context.TimeCodeValids
                .AsNoTracking()
                .Join(_context.Projects.AsNoTracking(),
                    timeCodeValid => timeCodeValid.ParentProject,
                    project       => project.ParentProject,
                    (timeCodeValid, project) => new WorkGroupValidTimeCode
                    {
                        WorkGroup     = timeCodeValid.WorkGroup,
                        TimeCode      = timeCodeValid.TimeCode,
                        ParentProject = timeCodeValid.ParentProject,
                        Manager       = project.Manager,
                        Active        = timeCodeValid.Active
                    });

            if (!string.IsNullOrWhiteSpace(workGroup))
                baseQuery = baseQuery.Where(e => e.WorkGroup == workGroup);

            baseQuery = ApplyWorkGroupValidTimeCodeFilter(baseQuery, query.Filter);

            if (!string.IsNullOrWhiteSpace(query.SortBy))
                baseQuery = (query.SortBy, query.Descending) switch
                {
                    (WorkGroupColumn,      true)  => baseQuery.OrderByDescending(e => e.WorkGroup),
                    (WorkGroupColumn,      false) => baseQuery.OrderBy(e => e.WorkGroup),
                    (TimeCodeColumn,       true)  => baseQuery.OrderByDescending(e => e.TimeCode),
                    (TimeCodeColumn,       false) => baseQuery.OrderBy(e => e.TimeCode),
                    (ParentProjectColumn,  true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    (ParentProjectColumn,  false) => baseQuery.OrderBy(e => e.ParentProject),
                    (ManagerColumn,        true)  => baseQuery.OrderByDescending(e => e.Manager),
                    (ManagerColumn,        false) => baseQuery.OrderBy(e => e.Manager),
                    (_,                    true)  => baseQuery.OrderByDescending(e => e.ParentProject),
                    _                             => baseQuery.OrderBy(e => e.ParentProject),
                };
            else
                baseQuery = baseQuery.OrderBy(e => e.ParentProject);

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<WgSummarisedStaffTimeUsageView>> GetWgSummarisedStaffTimeUsageAsync(
            string staffName)
        {
            return await _context.WgSummarisedStaffTimeUsageViews
                .AsNoTracking()
                .Where(e => e.Name == staffName)
                .ToListAsync();
        }

        public async Task<PactProfitCentreView?> GetProfitCentreAsync(string profitCentre)
        {
            return await _context.PactProfitCentreViews
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProfitCentre == profitCentre);
        }

        public async Task<IEnumerable<WorkGroup>> GetWorkGroupsForEmailAsync(string profitCentre)
        {
            return await _context.WorkGroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.SendEmail == 1)
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<List<WorkGroupView>> GetWorkGroupsByProfitCentreForBudgetAsync(string profitCentre)
        {
            return await _context.WorkGroupViews
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre
                         && w.UserEmail != null && w.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Distinct()
                .OrderBy(w => w.WorkGroupName)
                .ToListAsync();
        }

        public async Task<PagedData<WorkGroupView>> GetWorkGroupsByProfitCentreForBudgetPagedAsync(
            PaginationParameters<string> query, string profitCentre)
        {
            var q = _context.WorkGroupViews
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre
                         && w.UserEmail != null && w.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Distinct()
                .AsQueryable();

            q = ApplyWorkGroupViewFilter(q, query.Filter);
            q = ApplyWorkGroupViewSort(q, query.SortBy, query.Descending);

            var pageNumber = query.Page     > 0 ? query.Page     : 1;
            var pageSize   = query.PageSize > 0 ? query.PageSize : 5;
            return await ApplyPaging<WorkGroupView>(q, pageNumber, pageSize);
        }

        public async Task<IEnumerable<TimeSheetTemplateRow>> GetTimeSheetTemplateAsync(
            string workGroup, short month, short layout)
        {
            if (layout == 2)
            {
                var flat = await (
                    from t in _context.TimeCodeValids
                    join wg in _context.PactWorkGroupGradeViews on t.WorkGroup equals wg.WorkGroup
                    join s in _context.PactStaffViews on wg.WgGrade equals s.WorkGroupGrade
                    join jc in _context.JobCodes on t.JobCode equals jc.JobCodeId into jcGroup
                    from jc in jcGroup.DefaultIfEmpty()
                    join tp in _context.TestorProducts on t.TestCode equals tp.ItemCode into tpGroup
                    from tp in tpGroup.DefaultIfEmpty()
                    where t.WorkGroup == workGroup
                          && t.Active
                          && s.PersonStatus != "I"
                    orderby t.TimeCode, t.ParentProject, s.Name
                    select new
                    {
                        t.TimeCode,
                        t.ParentProject,
                        StaffName = s.Name,
                        Description = jc != null ? jc.JobCodeName : tp.ItemDescription
                    })
                    .AsNoTracking()
                    .ToListAsync();

                var rows = flat
                    .GroupBy(x => new { x.TimeCode, x.ParentProject })
                    .OrderBy(g => g.Key.TimeCode).ThenBy(g => g.Key.ParentProject)
                    .Select(g => new TimeSheetTemplateRow
                    {
                        StaffName = string.Join(", ", g.Select(x => x.StaffName).Distinct().OrderBy(n => n)),
                        TimeCode = g.Key.TimeCode,
                        Description = g.Select(x => x.Description).FirstOrDefault(d => d != null),
                        ParentProject = g.Key.ParentProject,
                        Month = month,
                        Hours = null
                    })
                    .ToList();

                return rows;
            }
            else
            {
                var rows = await (
                    from t in _context.TimeCodeValids
                    join wg in _context.PactWorkGroupGradeViews on t.WorkGroup equals wg.WorkGroup
                    join s in _context.PactStaffViews on wg.WgGrade equals s.WorkGroupGrade
                    where t.WorkGroup == workGroup && t.Active
                    orderby t.WorkGroup, s.Name, t.TimeCode, t.ParentProject
                    select new TimeSheetTemplateRow
                    {
                        StaffName = s.Name ?? string.Empty,
                        TimeCode = t.TimeCode,
                        Description = null,
                        ParentProject = t.ParentProject,
                        Month = month,
                        Hours = null
                    })
                    .AsNoTracking()
                    .ToListAsync();

                return rows;
            }
        }

        public async Task<IEnumerable<OutputSheetTemplateRow>> GetOutputSheetTemplateAsync(
            string workGroup, short month)
        {
            var rows = await (
                from tc in _context.TestCapabilities
                join tr in _context.TestRequirements on tc.TestCode equals tr.TestCode
                join tp in _context.TestorProducts on tc.TestCode equals tp.ItemCode
                where tc.WorkGroup == workGroup && tr.Active != 0
                orderby tc.TestCode, tr.Buyer
                select new OutputSheetTemplateRow
                {
                    TestCode = tc.TestCode,
                    ItemDescription = tp.ItemDescription,
                    Buyer = tr.Buyer,
                    Month = month,
                    Volume = null
                })
                .AsNoTracking()
                .ToListAsync();

            return rows;
        }

        public async Task<PagedData<WorkGroup>> GetWorkGroupsByProfitCentreAsync(
            PaginationParameters<string> query, string profitCentre)
        {
            var baseQuery = _context.WorkGroups
                .AsNoTracking()
                .Where(w => w.ProfitCentre == profitCentre && w.FpsYear == _context.FilterFpsYear);

            baseQuery = ApplyWorkGroupFilter(baseQuery, query.Filter);

            // SendEmailYes / SendEmailNo are view-model-only computed properties that have no
            // corresponding column on the WorkGroup entity; fall back to WorkGroupName for those.
            var sortBy = query.SortBy is nameof(WorkGroup.WorkGroupName) or nameof(WorkGroup.EmailRecipient)
                ? query.SortBy
                : nameof(WorkGroup.WorkGroupName);

            baseQuery = query.Descending
                ? baseQuery.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : baseQuery.OrderBy(e => EF.Property<object>(e, sortBy));

            return await ApplyPaging(baseQuery, query.Page, query.PageSize);
        }

        public async Task<bool> SetSendEmailForProfitCentreWorkGroupsAsync(string profitCentre, short flag)
        {
            var fpsYear = _context.FilterFpsYear;
            var affectedRows = await _context.WorkGroups
                .Where(wg => wg.FpsYear == fpsYear
                          && _context.ProfitCentres
                                .Any(pc => pc.ProfitCentreId == profitCentre
                                        && pc.ProfitCentreId == wg.ProfitCentre))
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.SendEmail, flag));
            return affectedRows >= 0;
        }

        public async Task<bool> SetSendEmailForAllWorkGroupsAsync(short flag)
        {
            var fpsYear = _context.FilterFpsYear;
            var affectedRows = await _context.WorkGroups
                .Where(w => w.FpsYear == fpsYear)
                .ExecuteUpdateAsync(s => s.SetProperty(w => w.SendEmail, flag));
            return affectedRows >= 0;
        }

        public async Task<bool> UpdateWorkGroupEmailAsync(string workGroupName, short sendEmail, string? emailRecipient)
        {
            var fpsYear = _context.FilterFpsYear;
            var affectedRows = await _context.WorkGroups
                .Where(w => w.WorkGroupName == workGroupName && w.FpsYear == fpsYear)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(w => w.SendEmail, sendEmail)
                    .SetProperty(w => w.EmailRecipient, emailRecipient));
            return affectedRows >= 0;
        }

        private static IQueryable<WorkGroup> ApplyWorkGroupFilter(
            IQueryable<WorkGroup> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("WorkGroupName", out var workGroupName) && !string.IsNullOrWhiteSpace(workGroupName))
                query = query.Where(w => EF.Functions.ILike(w.WorkGroupName, $"%{workGroupName}%"));

            if (filters.TryGetValue("EmailRecipient", out var emailRecipient) && !string.IsNullOrWhiteSpace(emailRecipient))
                query = query.Where(w => w.EmailRecipient != null &&
                                         EF.Functions.ILike(w.EmailRecipient, $"%{emailRecipient}%"));

            return query;
        }

        private static IQueryable<WorkGroupValidTimeCode> ApplyWorkGroupValidTimeCodeFilter(
            IQueryable<WorkGroupValidTimeCode> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(WorkGroupColumn, out var workGroup) && !string.IsNullOrWhiteSpace(workGroup))
                query = query.Where(e => EF.Functions.ILike(e.WorkGroup, $"%{workGroup}%"));

            if (filters.TryGetValue(TimeCodeColumn, out var timeCode) && !string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(e => EF.Functions.ILike(e.TimeCode, $"%{timeCode}%"));

            if (filters.TryGetValue(ParentProjectColumn, out var parentProject) && !string.IsNullOrWhiteSpace(parentProject))
                query = query.Where(e => EF.Functions.ILike(e.ParentProject, $"%{parentProject}%"));

            if (filters.TryGetValue(ManagerColumn, out var manager) && !string.IsNullOrWhiteSpace(manager))
                query = query.Where(e => EF.Functions.ILike(e.Manager!, $"%{manager}%"));

            return query;
        }

        private static IQueryable<WorkGroupTimeCode> ApplyWorkGroupTimeCodeFilter(
            IQueryable<WorkGroupTimeCode> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue(PactStaffIdColumn, out var pactStaffId) && !string.IsNullOrWhiteSpace(pactStaffId))
                query = query.Where(e => EF.Functions.ILike(e.PACTStaffID, $"%{pactStaffId}%"));

            if (filters.TryGetValue(NameColumn, out var name) && !string.IsNullOrWhiteSpace(name))
                query = query.Where(e => EF.Functions.ILike(e.Name!, $"%{name}%"));

            if (filters.TryGetValue(WorkGroupColumn, out var workGroupFilter) && !string.IsNullOrWhiteSpace(workGroupFilter))
                query = query.Where(e => EF.Functions.ILike(e.WorkGroup!, $"%{workGroupFilter}%"));

            if (filters.TryGetValue(ParentProjectColumn, out var parentProject) && !string.IsNullOrWhiteSpace(parentProject))
                query = query.Where(e => EF.Functions.ILike(e.ParentProject, $"%{parentProject}%"));

            if (filters.TryGetValue(TimeCodeColumn, out var timeCode) && !string.IsNullOrWhiteSpace(timeCode))
                query = query.Where(e => EF.Functions.ILike(e.TimeCode, $"%{timeCode}%"));

            return query;
        }

        private static IQueryable<WorkGroupView> ApplyWorkGroupViewFilter(
            IQueryable<WorkGroupView> query, string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson)) return query;

            var filters = JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson);
            if (filters is null) return query;

            if (filters.TryGetValue("WorkGroupName", out var wgName) && !string.IsNullOrWhiteSpace(wgName))
                query = query.Where(w => EF.Functions.ILike(w.WorkGroupName, $"%{wgName}%"));

            if (filters.TryGetValue("WorkGroup", out var wgName2) && !string.IsNullOrWhiteSpace(wgName2))
                query = query.Where(w => EF.Functions.ILike(w.WorkGroupName, $"%{wgName2}%"));

            if (filters.TryGetValue("ProfitCentre", out var pc) && !string.IsNullOrWhiteSpace(pc))
                query = query.Where(w => EF.Functions.ILike(w.ProfitCentre, $"%{pc}%"));

            if (filters.TryGetValue("Owner", out var owner) && !string.IsNullOrWhiteSpace(owner))
                query = query.Where(w => EF.Functions.ILike(w.Owner!, $"%{owner}%"));

            if (filters.TryGetValue("Description", out var desc) && !string.IsNullOrWhiteSpace(desc))
                query = query.Where(w => EF.Functions.ILike(w.Description!, $"%{desc}%"));

            return query;
        }

        private static IQueryable<WorkGroupView> ApplyWorkGroupViewSort(
            IQueryable<WorkGroupView> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "workgroupname" => descending ? query.OrderByDescending(w => w.WorkGroupName) : query.OrderBy(w => w.WorkGroupName),
                "profitcentre"  => descending ? query.OrderByDescending(w => w.ProfitCentre)  : query.OrderBy(w => w.ProfitCentre),
                "owner"         => descending ? query.OrderByDescending(w => w.Owner)         : query.OrderBy(w => w.Owner),
                "description"   => descending ? query.OrderByDescending(w => w.Description)   : query.OrderBy(w => w.Description),
                _               => query.OrderBy(w => w.WorkGroupName)
            };
        }

        // FPS-specific WorkGroup Maintenance sorting (frmMaintWorkGroup2 grid).
        // Kept separate from ApplyWorkGroupSorting so PACT sorting behaviour is unaffected.
        // Adds the CostCentre column, which the FPS maintenance grid exposes as sortable.
        private static IQueryable<WorkGroup> ApplyFpsWorkGroupSorting(IQueryable<WorkGroup> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "workgroupname"   => descending ? query.OrderByDescending(w => w.WorkGroupName)   : query.OrderBy(w => w.WorkGroupName),
                "profitcentre"    => descending ? query.OrderByDescending(w => w.ProfitCentre)    : query.OrderBy(w => w.ProfitCentre),
                "costcentre"      => descending ? query.OrderByDescending(w => w.CostCentre)      : query.OrderBy(w => w.CostCentre),
                "description"     => descending ? query.OrderByDescending(w => w.Description)     : query.OrderBy(w => w.Description),
                "owner"           => descending ? query.OrderByDescending(w => w.Owner)           : query.OrderBy(w => w.Owner),
                "centraloverhead" => descending ? query.OrderByDescending(w => w.CentralOverhead) : query.OrderBy(w => w.CentralOverhead),
                _                 => query.OrderBy(w => w.WorkGroupName)
            };
        }
    }
}
