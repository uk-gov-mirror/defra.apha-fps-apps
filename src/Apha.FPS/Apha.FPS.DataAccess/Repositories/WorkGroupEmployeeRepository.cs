using System.Dynamic;
using System.Linq.Expressions;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Apha.FPS.DataAccess.Repositories
{
    public class WorkGroupEmployeeRepository : BaseRepository, IWorkGroupEmployeeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _requestContext;

        public WorkGroupEmployeeRepository(FpsDbContext dbContext, IFpsRequestContext requestContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _requestContext = requestContext;
        }

        public async Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _dbContext.WorkGroupEmployees
                .AsNoTracking()
                .Where(wg => wg.PactId == pactId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
                        TimeRecorder = wg.TimeRecorder,
                        StartDate = wg.StartDate,
                        EndDate = wg.EndDate,
                        HoursPerWeek = wg.HoursPerWeek,
                    })
                .FirstOrDefaultAsync(default);
        }

        public async Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdForStaffAsync(string pactId)
        {
            return await _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(wg => wg.PactId == pactId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId = wg.PactId,
                        SpNumber = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus = wg.PersonStatus,
                        PersonClass = wg.PersonClass,
                        HrsPaid = wg.HrsPaid,
                        Leave = wg.Leave,
                        SickSpecial = wg.SickSpecial,
                        HrsAvail = wg.HrsAvail,
                        MakeAvailable = wg.MakeAvailable,
                        TimeRecorder = wg.TimeRecorder,
                        StartDate = wg.StartDate,
                        EndDate = wg.EndDate,
                        HoursPerWeek = wg.HoursPerWeek,
                        FpsYear = wg.FpsYear,
                        UserId = wg.UserId,
                        Dt2Username = wg.Dt2Username,
                        UserEmail = wg.UserEmail
                    })
                .FirstOrDefaultAsync(default);
        }

        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId);
            if (existing == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{entity.PactId}' was not found.");

            existing.HrsPaid = entity.HrsPaid;
            existing.Leave = entity.Leave;
            existing.SickSpecial = entity.SickSpecial;
            existing.HrsAvail = entity.HrsPaid - (entity.Leave + entity.SickSpecial);
            existing.PersonStatus = entity.PersonStatus;
            existing.PersonClass = entity.PersonClass;
            existing.MakeAvailable = entity.MakeAvailable;

            await _dbContext.SaveChangesAsync(default);
            return existing;
        }

        public async Task<WorkGroupEmployee> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            var existing = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == entity.PactId);
            if (existing == null)
                throw new KeyNotFoundException($"WorkGroupEmployee with PACTid '{entity.PactId}' was not found.");

            existing.SpNumber = entity.SpNumber;
            existing.WorkGroupGrade = entity.WorkGroupGrade;
            existing.HrsPaid = entity.HrsPaid;
            existing.Leave = entity.Leave;
            existing.SickSpecial = entity.SickSpecial;
            existing.HrsAvail = entity.HrsAvail;
            existing.PersonStatus = entity.PersonStatus;
            existing.PersonClass = entity.PersonClass;
            existing.MakeAvailable = entity.MakeAvailable;
            existing.TimeRecorder = entity.TimeRecorder;
            existing.StartDate = entity.StartDate;
            existing.EndDate = entity.EndDate;
            existing.HoursPerWeek = entity.HoursPerWeek;

            await _dbContext.SaveChangesAsync(default);
            return existing;
        }

        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            var all = await _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(x => x.WorkGroupGrade == wgGrade
                         && x.PersonStatus != "I"
                         && x.UserEmail != null && x.UserEmail.ToLower() == _requestContext.UserEmailId)
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new WorkGroupEmployeeView
                    {
                        PactId         = wg.PactId,
                        SpNumber       = wg.SpNumber,
                        WorkGroupGrade = wg.WorkGroupGrade,
                        Name           = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        PersonStatus   = wg.PersonStatus,
                        PersonClass    = wg.PersonClass,
                        HrsPaid        = wg.HrsPaid,
                        Leave          = wg.Leave,
                        SickSpecial    = wg.SickSpecial,
                        HrsAvail       = wg.HrsAvail,
                        MakeAvailable  = wg.MakeAvailable,
                        TimeRecorder   = wg.TimeRecorder,
                        StartDate      = wg.StartDate,
                        EndDate        = wg.EndDate,
                        HoursPerWeek   = wg.HoursPerWeek,
                        FpsYear        = wg.FpsYear,
                        UserId         = wg.UserId,
                        Dt2Username    = wg.Dt2Username,
                        UserEmail      = wg.UserEmail,
                    })
                .ToListAsync();

            var filtered = ApplyFilter(all.AsQueryable(), query.Filter);
            var sorted   = ApplySorting(filtered, query.SortBy, query.Descending);

            return ApplyPaging(sorted, query.Page, query.PageSize);
        }

        public async Task<PagedData<WorkGroupEmployeeView>> GetAllActiveWorkGroupEmployeesAsync(
            PaginationParameters<string> query, string wgGrade)
        {
            var workGroupEmployeeQuery = _dbContext.WorkGroupEmployees
                .AsNoTracking()
                .Where(wg => wg.WorkGroupGrade == wgGrade && wg.PersonStatus.ToUpper() != "I")
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new
                    {
                        wg.PactId,
                        wg.SpNumber,
                        wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        wg.PersonStatus,
                        wg.PersonClass,
                        wg.HrsPaid,
                        wg.Leave,
                        wg.SickSpecial,
                        wg.HrsAvail,
                        wg.MakeAvailable,
                        wg.TimeRecorder,
                        wg.StartDate,
                        wg.EndDate,
                        wg.HoursPerWeek
                    })
                .Distinct()
                .Select(x => new WorkGroupEmployeeView
                {
                    PactId = x.PactId,
                    SpNumber = x.SpNumber,
                    WorkGroupGrade = x.WorkGroupGrade,
                    Name = x.Name,
                    PersonStatus = x.PersonStatus,
                    PersonClass = x.PersonClass,
                    HrsPaid = x.HrsPaid,
                    Leave = x.Leave,
                    SickSpecial = x.SickSpecial,
                    HrsAvail = x.HrsAvail,
                    MakeAvailable = x.MakeAvailable,
                    TimeRecorder = x.TimeRecorder,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    HoursPerWeek = x.HoursPerWeek
                })
                .AsQueryable();

            workGroupEmployeeQuery = ApplyFilter(workGroupEmployeeQuery, query.Filter);
            workGroupEmployeeQuery = ApplySorting(workGroupEmployeeQuery, query.SortBy, query.Descending);

            var result = await workGroupEmployeeQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<bool> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            var entity = await _dbContext.WorkGroupEmployees
                .FirstOrDefaultAsync(x => x.PactId == pactId);
            if (entity == null)
                return false;

            _dbContext.WorkGroupEmployees.Remove(entity);
            await _dbContext.SaveChangesAsync(default);
            return true;
        }

        public async Task<bool> HasAssociatedStaffAsync(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return false;

            return await _dbContext.WorkGroupEmployees
                .AnyAsync(e => e.WorkGroupGrade == wgGrade);
        }

        public async Task<WorkGroupEmployee> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            entity.FpsYear = _requestContext.FpsYear;
            await _dbContext.WorkGroupEmployees.AddAsync(entity);
            await _dbContext.SaveChangesAsync(default);
            return entity;
        }

       
        private static IQueryable<WorkGroupEmployeeView> ApplyFilter(IQueryable<WorkGroupEmployeeView> query, string? filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("PactId", out var pactId) && pactId != null)
                query = query.Where(x => EF.Functions.ILike(x.PactId, $"%{pactId}%"));

            if (dict.TryGetValue("SpNumber", out var spNumber) && spNumber != null)
                query = query.Where(x => x.SpNumber != null && EF.Functions.ILike(x.SpNumber, $"%{spNumber}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => x.Name != null && EF.Functions.ILike(x.Name, $"%{name}%"));

            if (dict.TryGetValue("WorkGroupGrade", out var workGroupGrade) && workGroupGrade != null)
                query = query.Where(x => x.WorkGroupGrade != null && EF.Functions.ILike(x.WorkGroupGrade, $"%{workGroupGrade}%"));

            return query;
        }
       

        public async Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeForStaffAsync(
            PaginationParameters<string> query,
            string wgGrade)
        {
            var workGroupEmployeeQuery = _dbContext.WorkGroupEmployeeViews
                .AsNoTracking()
                .Where(wg => (string.IsNullOrWhiteSpace(wgGrade) || wg.WorkGroupGrade == wgGrade)
                          && wg.UserEmail != null
                          && wg.UserEmail.ToLower() == _requestContext.UserEmailId.ToLower())
                .Join(
                    _dbContext.Employees.AsNoTracking(),
                    wg => wg.SpNumber,
                    e => e.SPNumber,
                    (wg, e) => new
                    {
                        wg.PactId,
                        wg.SpNumber,
                        wg.WorkGroupGrade,
                        Name = (e.LastName ?? "") + " " + (e.FirstName ?? ""),
                        wg.PersonStatus,
                        wg.PersonClass,
                        wg.HrsPaid,
                        wg.Leave,
                        wg.SickSpecial,
                        wg.HrsAvail,
                        wg.MakeAvailable,
                        wg.TimeRecorder,
                        wg.StartDate,
                        wg.EndDate,
                        wg.HoursPerWeek,
                        wg.FpsYear,
                        wg.UserId,
                        wg.Dt2Username,
                        wg.UserEmail
                    })
                .Distinct()
                .Select(x => new WorkGroupEmployeeView
                {
                    PactId = x.PactId,
                    SpNumber = x.SpNumber,
                    WorkGroupGrade = x.WorkGroupGrade,
                    Name = x.Name,
                    PersonStatus = x.PersonStatus,
                    PersonClass = x.PersonClass,
                    HrsPaid = x.HrsPaid,
                    Leave = x.Leave,
                    SickSpecial = x.SickSpecial,
                    HrsAvail = x.HrsAvail,
                    MakeAvailable = x.MakeAvailable,
                    TimeRecorder = x.TimeRecorder,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    HoursPerWeek = x.HoursPerWeek,
                    FpsYear = x.FpsYear,
                    UserId = x.UserId,
                    Dt2Username = x.Dt2Username,
                    UserEmail = x.UserEmail
                })
                .AsQueryable();

            workGroupEmployeeQuery = ApplyFilter(workGroupEmployeeQuery, query.Filter);
            workGroupEmployeeQuery = ApplySorting(workGroupEmployeeQuery, query.SortBy, query.Descending);

            var result = await workGroupEmployeeQuery.ToListAsync();
            return base.ApplyPaging(result, query.Page, query.PageSize);
        }

           

        private static IQueryable<WorkGroupEmployeeView> ApplySorting(IQueryable<WorkGroupEmployeeView> query, string? sortBy, bool descending)
        {
            return sortBy?.ToLower() switch
            {
                "pactid" => descending ? query.OrderByDescending(x => x.PactId) : query.OrderBy(x => x.PactId),
                "spnumber" => descending ? query.OrderByDescending(x => x.SpNumber) : query.OrderBy(x => x.SpNumber),
                "name" or "staffname" => descending ? query.OrderByDescending(x => x.Name) : query.OrderBy(x => x.Name),
                "workgroupgrade" or "wggrade" => descending ? query.OrderByDescending(x => x.WorkGroupGrade) : query.OrderBy(x => x.WorkGroupGrade),
                "personstatus" => descending ? query.OrderByDescending(x => x.PersonStatus) : query.OrderBy(x => x.PersonStatus),
                _ => query.OrderBy(x => x.Name)
            };
        }
    }
}
