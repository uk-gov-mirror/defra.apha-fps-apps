using System.Linq.Expressions;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;

namespace Apha.FPS.DataAccess.Repositories
{
    public class ProjectAuditTrailRepository : BaseRepository, IProjectAuditTrailRepository
    {
        private readonly FpsDbContext _dbContext;

        public ProjectAuditTrailRepository(FpsDbContext dbContext)
            : base(dbContext)
        {
            _dbContext = dbContext;
        }

        // fps.project_log has a direct parentproject column; no join required
        public async Task<PagedData<ProjectLog>> GetProjectLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = _dbContext.ProjectLogs
                .AsNoTracking()
                .Where(p => p.ParentProject == parentProject);

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    (p.InsertDelete != null && p.InsertDelete.ToLower().Contains(search)) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyProjectLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // join to fps.tlkpjob (JobCodes) on jobcode to resolve the parentproject association
        public async Task<PagedData<StaffJobLog>> GetStaffJobLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.StaffJobLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    p.JobCode.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyStaffJobLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();

            await PopulateStaffNamesAsync(result);

            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // fps.staffjob_log has no name column; resolve staff display names via a lookup
        // against vtblstaff_general (StaffGeneralViews), mirroring StaffJobRepository's
        // established staff-name enrichment pattern.
        private async Task PopulateStaffNamesAsync(List<StaffJobLog> logs)
        {
            var staffIds = logs.Select(l => l.StaffId).Distinct().ToList();
            if (staffIds.Count == 0)
                return;

            var staffNames = await _dbContext.StaffGeneralViews
                .AsNoTracking()
                .Where(s => s.StaffId != null && staffIds.Contains(s.StaffId))
                .ToListAsync();

            var staffNameMap = staffNames
                .GroupBy(s => s.StaffId!)
                .ToDictionary(g => g.Key, g => g.First().Name);

            foreach (var log in logs)
            {
                if (staffNameMap.TryGetValue(log.StaffId, out var name))
                    log.Name = name;
            }
        }

        // jobcode column in testreq_log is derived from projectbuyercode; join to JobCodes on jobcode
        public async Task<PagedData<TestRequirementLog>> GetTestRequirementLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.TestRequirementLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    (p.TestCode != null && p.TestCode.ToLower().Contains(search)) ||
                    (p.Buyer != null && p.Buyer.ToLower().Contains(search)) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyTestRequirementLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // join to JobCodes on jobcode to resolve the parentproject association
        public async Task<PagedData<AnimalRequestLog>> GetAnimalRequestLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.AnimalRequestLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    p.JobCode.ToLower().Contains(search) ||
                    p.AnimalType.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyAnimalRequestLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // join to JobCodes on jobcode to resolve the parentproject association
        public async Task<PagedData<AdditionalCostLog>> GetAdditionalCostLogsAsync(
            PaginationParameters<string> query,
            string parentProject,
            DateTime? fromDate,
            DateTime? toDate)
        {
            var q = from log in _dbContext.AdditionalCostLogs.AsNoTracking()
                    join jc in _dbContext.JobCodes.AsNoTracking()
                        on log.JobCode equals jc.JobCodeId
                    where jc.ParentProject == parentProject
                    select log;

            if (fromDate.HasValue)
                q = q.Where(p => p.DateTime >= fromDate.Value);
            if (toDate.HasValue)
                q = q.Where(p => p.DateTime <= toDate.Value);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLower();
                q = q.Where(p =>
                    p.JobCode.ToLower().Contains(search) ||
                    p.Account.ToLower().Contains(search) ||
                    p.Description.ToLower().Contains(search) ||
                    (p.UserId != null && p.UserId.ToLower().Contains(search)));
            }

            q = ApplyAdditionalCostLogSorting(q, query.SortBy, query.Descending);

            var result = await q.ToListAsync();

            await ResolveAdditionalCostLogUserEmailsAsync(result);

            return ApplyPaging(result, query.Page, query.PageSize);
        }

        // fps.additionalcosts_log.user_id was historically populated with a raw login/username
        // for some rows and, more recently, with the authenticated user's email address
        // (AdditionalCostRepository / ProjectRepository both write _requestContext.UserEmailId).
        // To guarantee the Exceptional Cost Changes grid's User_ID column always displays an
        // email address, resolve any legacy, non-email UserId values against fps.tblusers
        // (Username / Dt2Username -> UserEmail). This mirrors the StaffJobLog Name enrichment
        // pattern below (PopulateStaffNamesAsync) and only mutates the in-memory, no-tracking
        // result set — no changes are persisted back to the database.
        private async Task ResolveAdditionalCostLogUserEmailsAsync(List<AdditionalCostLog> logs)
        {
            var rawUserIds = logs
                .Where(l => !string.IsNullOrWhiteSpace(l.UserId) && !l.UserId!.Contains('@'))
                .Select(l => l.UserId!.Trim())
                .Distinct()
                .ToList();

            if (rawUserIds.Count == 0)
                return;

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(u => (u.Username != null && rawUserIds.Contains(u.Username))
                         || (u.Dt2Username != null && rawUserIds.Contains(u.Dt2Username)))
                .ToListAsync();

            var emailByUsername = users
                .Where(u => !string.IsNullOrWhiteSpace(u.Username) && !string.IsNullOrWhiteSpace(u.UserEmail))
                .GroupBy(u => u.Username!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().UserEmail, StringComparer.OrdinalIgnoreCase);

            var emailByDt2Username = users
                .Where(u => !string.IsNullOrWhiteSpace(u.Dt2Username) && !string.IsNullOrWhiteSpace(u.UserEmail))
                .GroupBy(u => u.Dt2Username!.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First().UserEmail, StringComparer.OrdinalIgnoreCase);

            foreach (var log in logs)
            {
                if (string.IsNullOrWhiteSpace(log.UserId) || log.UserId.Contains('@'))
                    continue;

                var key = log.UserId.Trim();

                if (emailByUsername.TryGetValue(key, out var email))
                    log.UserId = email;
                else if (emailByDt2Username.TryGetValue(key, out var email2))
                    log.UserId = email2;
                // else: no matching fps.tblusers record found — legacy value is left as-is.
            }
        }

        // ── Private sorting helpers ──────────────────────────────────────────────────────

        private static IQueryable<ProjectLog> ApplyProjectLogSorting(
            IQueryable<ProjectLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "parentproject" => descending ? q.OrderByDescending(e => e.ParentProject) : q.OrderBy(e => e.ParentProject),
                "projecttitle"  => descending ? q.OrderByDescending(e => e.ProjectTitle)  : q.OrderBy(e => e.ProjectTitle),
                "program"       => descending ? q.OrderByDescending(e => e.Program)        : q.OrderBy(e => e.Program),
                "jobcode"       => descending ? q.OrderByDescending(e => e.JobCode)        : q.OrderBy(e => e.JobCode),
                "date_time"     => descending ? q.OrderByDescending(e => e.DateTime)       : q.OrderBy(e => e.DateTime),
                "insert_delete" => descending ? q.OrderByDescending(e => e.InsertDelete)   : q.OrderBy(e => e.InsertDelete),
                "user_id"       => descending ? q.OrderByDescending(e => e.UserId)         : q.OrderBy(e => e.UserId),
                _               => q.OrderByDescending(e => e.DateTime),
            };
        }

        private static IQueryable<StaffJobLog> ApplyStaffJobLogSorting(
            IQueryable<StaffJobLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "staffid"       => descending ? q.OrderByDescending(e => e.StaffId)      : q.OrderBy(e => e.StaffId),
                "jobcode"       => descending ? q.OrderByDescending(e => e.JobCode)      : q.OrderBy(e => e.JobCode),
                "plannedhours"  => descending ? q.OrderByDescending(e => e.PlannedHours) : q.OrderBy(e => e.PlannedHours),
                "date_time"     => descending ? q.OrderByDescending(e => e.DateTime)     : q.OrderBy(e => e.DateTime),
                "insert_delete" => descending ? q.OrderByDescending(e => e.InsertDelete) : q.OrderBy(e => e.InsertDelete),
                "user_id"       => descending ? q.OrderByDescending(e => e.UserId)       : q.OrderBy(e => e.UserId),
                _               => q.OrderByDescending(e => e.DateTime),
            };
        }

        private static IQueryable<TestRequirementLog> ApplyTestRequirementLogSorting(
            IQueryable<TestRequirementLog> q, string? sortBy, bool descending)
        {
            Expression<Func<TestRequirementLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "testcode"         => e => e.TestCode,
                "buyer"            => e => e.Buyer,
                "unitprice"        => e => e.UnitPrice,
                "norequired"       => e => e.NoRequired,
                "projectbuyercode" => e => e.ProjectBuyerCode,
                "testbuyercode"    => e => e.TestBuyerCode,
                "active"           => e => e.Active,
                "date_time"        => e => e.DateTime,
                "insert_delete"    => e => e.InsertDelete,
                "user_id"          => e => e.UserId,
                _                  => e => e.DateTime,
            };
            return descending ? q.OrderByDescending(keySelector) : q.OrderBy(keySelector);
        }

        private static IQueryable<AnimalRequestLog> ApplyAnimalRequestLogSorting(
            IQueryable<AnimalRequestLog> q, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "jobcode"         => descending ? q.OrderByDescending(e => e.JobCode)         : q.OrderBy(e => e.JobCode),
                "animaltype"      => descending ? q.OrderByDescending(e => e.AnimalType)      : q.OrderBy(e => e.AnimalType),
                "numberofdays"    => descending ? q.OrderByDescending(e => e.NumberOfDays)    : q.OrderBy(e => e.NumberOfDays),
                "numberofanimals" => descending ? q.OrderByDescending(e => e.NumberOfAnimals) : q.OrderBy(e => e.NumberOfAnimals),
                "date_time"       => descending ? q.OrderByDescending(e => e.DateTime)        : q.OrderBy(e => e.DateTime),
                "insert_delete"   => descending ? q.OrderByDescending(e => e.InsertDelete)    : q.OrderBy(e => e.InsertDelete),
                "user_id"         => descending ? q.OrderByDescending(e => e.UserId)          : q.OrderBy(e => e.UserId),
                _                 => q.OrderByDescending(e => e.DateTime),
            };
        }

        private static IQueryable<AdditionalCostLog> ApplyAdditionalCostLogSorting(
            IQueryable<AdditionalCostLog> q, string? sortBy, bool descending)
        {
            Expression<Func<AdditionalCostLog, object?>> keySelector = sortBy?.ToLower() switch
            {
                "jobcode"       => e => e.JobCode,
                "account"       => e => e.Account,
                "description"   => e => e.Description,
                "itemcost"      => e => e.ItemCost,
                "freq"          => e => e.Freq,
                "supplier"      => e => e.Supplier,
                "date_time"     => e => e.DateTime,
                "insert_delete" => e => e.InsertDelete,
                "user_id"       => e => e.UserId,
                _               => e => e.DateTime,
            };
            return descending ? q.OrderByDescending(keySelector) : q.OrderBy(keySelector);
        }
    }
}
