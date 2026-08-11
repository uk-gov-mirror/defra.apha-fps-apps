using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;

namespace Apha.FPS.DataAccess.Repositories
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        private readonly FpsDbContext _dbContext;
        private readonly IFpsRequestContext _fpsYearContext;

        public EmployeeRepository(FpsDbContext dbContext, IFpsRequestContext fpsYearContext) : base(dbContext)
        {
            _dbContext = dbContext;
            _fpsYearContext = fpsYearContext;
        }

        public async Task<IEnumerable<Employee>> GetAllEmployeesAsync()
        {
            return await _dbContext.Employees
                .AsNoTracking()
                .OrderBy(e => e.SPNumber)
                .ToListAsync();
        }

        public async Task<PagedData<Employee>> GetEmployeesByPrefixAsync(PaginationParameters<string> query, string prefix)
        {
            var queryEmployees = _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.SPNumber.StartsWith(prefix))
                .AsQueryable();

            // Apply filtering
            queryEmployees = ApplyEmployeeFilter(queryEmployees, query.Filter);

            // Apply sorting
            queryEmployees = (IQueryable<Employee>)ApplySorting(queryEmployees, query.SortBy, query.Descending);

            // Execute query
            var result = await queryEmployees.ToListAsync();

            // Apply paging
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<Employee>> GetEmployeesByPrefixAsync(string prefix)
        {
            return await _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.SPNumber.StartsWith(prefix))
                .OrderBy(e => e.SPNumber)
                .ToListAsync();
        }

        public async Task<Employee?> GetEmployeeByIdAsync(string spNumber)
        {
            return await _dbContext.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.SPNumber == spNumber);
        }

        public async Task<Employee> AddEmployeeAsync(Employee employee)
        {
            employee.FpsYear = _fpsYearContext.FpsYear;
            await _dbContext.Employees.AddAsync(employee);
            await _dbContext.SaveChangesAsync();

            return employee;
        }

        public async Task<Employee> UpdateEmployeeAsync(Employee employee)
        {
            employee.FpsYear = _fpsYearContext.FpsYear;
            _dbContext.Entry(employee).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            return employee;
        }

        public async Task<bool> DeleteEmployeeAsync(string spNumber)
        {
            var existingWgEmployee = await _dbContext.WorkGroupEmployees
                      .AsNoTracking()
                      .Where(e => e.SpNumber == spNumber && e.FpsYear == _fpsYearContext.FpsYear)
                      .FirstOrDefaultAsync();

            if (existingWgEmployee is not null)
                throw new InvalidOperationException(
                     $"Cannot delete SPNumber {spNumber} because linked Employee exist.");

            var employee = await _dbContext.Employees
                .AsNoTracking()
                .Where(e => e.SPNumber == spNumber && e.FpsYear == _fpsYearContext.FpsYear)
                .FirstOrDefaultAsync();

            if (employee is null)
                throw new InvalidOperationException(
                    $"Employee with SPNumber {spNumber} does not exist.");                                           

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Manager>> GetAllManagersAsync()
        {
            var query = (
                 from staff in _dbContext.StaffActiveView
                 join grade in _dbContext.WorkgroupGradeGeneralViews
                     on staff.WorkgroupGrade equals grade.WgGrade
                 where
                    staff.Name != null &&
                    !EF.Functions.ILike(staff.Name, "%general%") &&
                    !EF.Functions.ILike(staff.Name, "%vacancy%") &&
                    grade.GradeCode != null &&
                    grade.GradeCode.Length > 0 &&
                    grade.GradeCode.Substring(0, 1) != "G"
                 select new Manager
                 {
                     Name = staff.Name,
                     WorkGroup = grade.WorkGroup,
                     GradeCode = grade.GradeCode,
                     Expr1 = grade.GradeCode!.Substring(0, 1)
                 }
             )
             .Distinct()
             .OrderBy(x => x.Name);
            
            var managers = await query.ToListAsync();
            return managers;
        }

        public async Task<IEnumerable<Manager>> GetAllPactManagersAsync()
        {
            var query = (
                from grade in _dbContext.PactWorkGroupGradeViews
                join staff in _dbContext.StaffGeneralViews
                    on grade.WgGrade equals staff.WorkGroupGrade
                where
                    staff.Name != null &&
                    !EF.Functions.ILike(staff.Name, "%gen%") &&
                    !EF.Functions.ILike(staff.Name, "%vacancy%") &&
                    grade.GradeCode != null &&
                    (string.Compare(grade.GradeCode, "E") <= 0 || grade.GradeCode == "GD5")
                select new Manager
                {
                    Name = staff.Name,
                    WorkGroup = grade.WorkGroup,
                    GradeCode = grade.GradeCode,
                    Expr1 = grade.GradeCode!.Substring(0, 1)
                }
            )
            .Distinct()
            .OrderBy(x => x.Name);

            return await query.ToListAsync();
        }

        private static IQueryable<Employee> ApplyEmployeeFilter(IQueryable<Employee> queryEmployees, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return queryEmployees;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryEmployees;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("SPNumber", out var spNumber) && spNumber != null)
                queryEmployees = queryEmployees.Where(x => EF.Functions.ILike(x.SPNumber, $"%{spNumber}%"));

            if (dict.TryGetValue("FirstName", out var firstName) && firstName != null)
                queryEmployees = queryEmployees.Where(x => EF.Functions.ILike(x.FirstName!, $"%{firstName}%"));

            if (dict.TryGetValue("LastName", out var lastName) && lastName != null)
                queryEmployees = queryEmployees.Where(x => EF.Functions.ILike(x.LastName!, $"%{lastName}%"));

            if (dict.TryGetValue("Title", out var title) && title != null)
                queryEmployees = queryEmployees.Where(x => EF.Functions.ILike(x.Title!, $"%{title}%"));

            return queryEmployees;
        }

        private static IQueryable ApplySorting(IQueryable<Employee> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderBy(e => e.SPNumber);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        private static IQueryable ApplySortingByProperty(IQueryable<Employee> query, string property, bool descending)
        {
            return property switch
            {
                "spnumber" => ApplyOrder(query, i => i.SPNumber, descending),
                "firstname" => ApplyOrder(query, i => i.FirstName, descending),
                "lastname" => ApplyOrder(query, i => i.LastName, descending),
                "title" => ApplyOrder(query, i => i.Title, descending),
                "fpscalyear" => ApplyOrder(query, i => i.FpsYear, descending),
                _ => query.OrderBy(e => e.SPNumber)
            };
        }

        private static IQueryable ApplyOrder<T>(IQueryable<Employee> query, Expression<Func<Employee, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        private static IQueryable ApplyOrder<T>(IQueryable<PactStaff> query, Expression<Func<PactStaff, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        public async Task<IEnumerable<WorkGroupPerson>> GetAllWorkGroupPersonAsync()
        {
            return await _dbContext.PactStaffs
                .AsNoTracking()
                .Join(_dbContext.WorkgroupGrades,
                    s => s.WorkGroupGrade,
                    g => g.WgGrade,
                    (s, g) => new WorkGroupPerson
                    {
                        Name = s.Name,
                        WorkGroupGrade = s.WorkGroupGrade,
                        WorkGroup = g.Workgroup
                    })
                .Distinct()
                .OrderBy(p => p.Name)
                .ToListAsync();
        }

        public async Task<PagedData<PactStaff>> GetPagedWorkGroupStaffAsync(PaginationParameters<string> query, string? workGroup = null)
        {
            IQueryable<PactStaff> queryStaff;

            if (string.IsNullOrWhiteSpace(workGroup))
            {
                queryStaff = _dbContext.PactStaffs.AsNoTracking();
            }
            else
            {
                queryStaff = _dbContext.Workgroups
                    .AsNoTracking()
                    .Join(_dbContext.PactWorkGroupGradeViews.AsNoTracking(),
                        wg    => wg.WorkGroupName,
                        grade => grade.WorkGroup,
                        (wg, grade) => new { wg, grade })
                    .Join(_dbContext.PactStaffs.AsNoTracking(),
                        wgGrade => wgGrade.grade.WgGrade,
                        staff   => staff.WorkGroupGrade,
                        (wgGrade, staff) => new { wgGrade.wg, staff })
                    .Where(x => x.wg.WorkGroupName == workGroup)
                    .Select(x => x.staff);
            }

            queryStaff = ApplyWorkGroupStaffFilter(queryStaff, query.Filter);
            queryStaff = (IQueryable<PactStaff>)ApplyWorkGroupStaffSorting(queryStaff, query.SortBy, query.Descending);

            var result = await queryStaff.ToListAsync();
            result = ApplyWorkGroupStaffNumericFilter(result, query.Filter);
            return ApplyPaging(result, query.Page, query.PageSize);
        }

        public async Task<IEnumerable<PactStaff>> GetPactStaffAsync()
        {
            return await _dbContext.PactStaffs
                .AsNoTracking()
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<PactStaff>> GetPactWorkGroupStaffAsync(string? workGroup)
        {            
            if (string.IsNullOrEmpty(workGroup))
            {
               return await _dbContext.Workgroups
                    .AsNoTracking()
                    .Join(_dbContext.PactWorkGroupGradeViews.AsNoTracking(),
                        wg => wg.WorkGroupName,
                        grade => grade.WorkGroup,
                        (wg, grade) => new { wg, grade })
                    .Join(_dbContext.PactStaffs.AsNoTracking(),
                        wgGrade => wgGrade.grade.WgGrade,
                        staff => staff.WorkGroupGrade,
                        (wgGrade, staff) => new { wgGrade.wg, staff })                    
                    .Select(x => x.staff)
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.WorkGroupGrade)
                    .ToListAsync();
            }
            else
            {
                return await _dbContext.Workgroups
                    .AsNoTracking()
                    .Join(_dbContext.PactWorkGroupGradeViews.AsNoTracking(),
                        wg => wg.WorkGroupName,
                        grade => grade.WorkGroup,
                        (wg, grade) => new { wg, grade })
                    .Join(_dbContext.PactStaffs.AsNoTracking(),
                        wgGrade => wgGrade.grade.WgGrade,
                        staff => staff.WorkGroupGrade,
                        (wgGrade, staff) => new { wgGrade.wg, staff })
                    .Where(x => x.wg.WorkGroupName == workGroup)
                    .Select(x => x.staff)
                    .OrderBy(x => x.Name)
                    .ThenBy(x => x.WorkGroupGrade)
                    .ToListAsync();
            }
            
        }

        private static IQueryable<PactStaff> ApplyWorkGroupStaffFilter(IQueryable<PactStaff> query, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return query;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return query;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("PactId", out var pactId) && pactId != null)
                query = query.Where(x => EF.Functions.ILike(x.PactId!.ToLower(), $"%{pactId.ToString()!.ToLower()}%"));

            if (dict.TryGetValue("SpNumber", out var spNumber) && spNumber != null)
                query = query.Where(x => EF.Functions.ILike(x.SpNumber!.ToLower(), $"%{spNumber.ToString()!.ToLower()}%"));

            if (dict.TryGetValue("Name", out var name) && name != null)
                query = query.Where(x => EF.Functions.ILike(x.Name!.ToLower(), $"%{name.ToString()!.ToLower()}%"));

            if (dict.TryGetValue("WorkGroupGrade", out var wgg) && wgg != null)
                query = query.Where(x => EF.Functions.ILike(x.WorkGroupGrade!.ToLower(), $"%{wgg.ToString()!.ToLower()}%"));

            if (dict.TryGetValue("Title", out var title) && title != null)
                query = query.Where(x => EF.Functions.ILike(x.Title!.ToLower(), $"%{title.ToString()!.ToLower()}%"));

            if (dict.TryGetValue("PersonStatus", out var personStatus) && personStatus != null)
                query = query.Where(x => EF.Functions.ILike(x.PersonStatus!.ToLower(), $"%{personStatus.ToString()!.ToLower()}%"));

            return query;
        }
        private static List<PactStaff> ApplyWorkGroupStaffNumericFilter(List<PactStaff> list, string? filter)
        {
            if (string.IsNullOrEmpty(filter))
                return list;

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
                return list;

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("HrsPaid", out var hrsPaid) && hrsPaid != null && !string.IsNullOrWhiteSpace(hrsPaid.ToString()))
                list = list.Where(x => x.HrsPaid.HasValue && x.HrsPaid.Value.ToString("G").Contains(hrsPaid.ToString()!, StringComparison.OrdinalIgnoreCase)).ToList();

            if (dict.TryGetValue("Leave", out var leave) && leave != null && !string.IsNullOrWhiteSpace(leave.ToString()))
                list = list.Where(x => x.Leave.HasValue && x.Leave.Value.ToString("G").Contains(leave.ToString()!, StringComparison.OrdinalIgnoreCase)).ToList();

            if (dict.TryGetValue("SickSpecial", out var sickSpecial) && sickSpecial != null && !string.IsNullOrWhiteSpace(sickSpecial.ToString()))
                list = list.Where(x => x.SickSpecial.HasValue && x.SickSpecial.Value.ToString("G").Contains(sickSpecial.ToString()!, StringComparison.OrdinalIgnoreCase)).ToList();

            if (dict.TryGetValue("HrsAvail", out var hrsAvail) && hrsAvail != null && !string.IsNullOrWhiteSpace(hrsAvail.ToString()))
                list = list.Where(x => x.HrsAvail.HasValue && x.HrsAvail.Value.ToString("G").Contains(hrsAvail.ToString()!, StringComparison.OrdinalIgnoreCase)).ToList();

            return list;
        }

        private static IQueryable ApplyWorkGroupStaffSorting(IQueryable<PactStaff> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
                return query.OrderBy(s => s.Name);

            return sortBy.ToLower() switch
            {
                "pactid" => ApplyOrder(query, s => s.PactId, descending),
                "name" => ApplyOrder(query, s => s.Name, descending),
                "spnumber" => ApplyOrder(query, s => s.SpNumber, descending),
                "title" => ApplyOrder(query, s => s.Title, descending),
                "workgroupgrade" => ApplyOrder(query, s => s.WorkGroupGrade, descending),
                "personstatus" => ApplyOrder(query, s => s.PersonStatus, descending),
                "hrspaid" => ApplyOrder(query, s => s.HrsPaid, descending),
                "leave" => ApplyOrder(query, s => s.Leave, descending),
                "sickspecial" => ApplyOrder(query, s => s.SickSpecial, descending),
                "hrsavail" => ApplyOrder(query, s => s.HrsAvail, descending),
                _ => query.OrderBy(s => s.Name)
            };
        }

            }
        }
