using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using Apha.Costbook.DataAccess.Data;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Dynamic;
using System.Linq.Expressions;
using System.Web;


namespace Apha.Costbook.DataAccess.Repositories
{
    public class ProjectRepository : RepositoryBase,IProjectRepository
    {
        private readonly ISettingsRepository _settingsRepository;

        public ProjectRepository(CostbookDbContext context, ISettingsRepository settingsRepository) : base(context)
        {
            _settingsRepository = settingsRepository;
        }
       
        public async Task<PagedData<Project>> GetPaginatedProjectsAsync(PaginationParameters<string> queryFilter)
        {
            var queryProjects = _context.Projects
                .AsNoTracking()
                .AsQueryable();

           
            // Apply general filtering
            queryProjects = ApplyProjectFilter(queryProjects, queryFilter.Filter);            

            // Apply sorting
            queryProjects = (IQueryable<Project>)ApplySorting(queryProjects, queryFilter.SortBy, queryFilter.Descending);

            // Execute query
           
            return await ApplyPaging(queryProjects, queryFilter.Page, queryFilter.PageSize);
            
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(string? contractFilter, string? submittedByFilter)
        {
            var query = _context.Projects.AsQueryable();
            if (!string.IsNullOrEmpty(contractFilter))
                query = query.Where(p => p.ContractNumber == contractFilter);
            if (!string.IsNullOrEmpty(submittedByFilter))
                query = query.Where(p => (p.SubmittedByFName + ", " + p.SubmittedByFName) == submittedByFilter);
            return await query.OrderByDescending(p => p.ProjectId).ToListAsync();
        }
    public async Task<Project?> GetProjectByIdAsync(string id)
    {
            var decodedId = HttpUtility.UrlDecode(id);
          
        return await _context.Set<Project>().FirstOrDefaultAsync(p => p.ProjectId == decodedId);
    }


    public async Task<Project> AddProjectAsync(Project project)
        {
            
            var dbSet = _context.Set<Project>(); 
            dbSet.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<Project> UpdateProjectAsync(Project project)
        {            
            var dbSet = _context.Set<Project>();
            dbSet.Update(project);
            await _context.SaveChangesAsync();
            return project;
        }

        public async Task<bool> DeleteProjectAsync(string id)
        {
            var decodedId = HttpUtility.UrlDecode(id);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // Delete in correct order (children first)
                await _context.Set<AnimalRequirement>()
                    .Where(ar => ar.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<AdditionalCost>()
                    .Where(ac => ac.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<TestRequirement>()
                    .Where(t => t.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<StaffRequirement>()
                    .Where(sr => sr.Project == decodedId)
                    .ExecuteDeleteAsync();

                await _context.Set<ProjectYear>()
                    .Where(py => py.Project == decodedId)
                    .ExecuteDeleteAsync();

                var project = await _context.Set<Project>()
                    .FirstOrDefaultAsync(p => p.ProjectId == decodedId);

                if (project == null)
                {
                    await transaction.RollbackAsync();
                    return false;
                }

                _context.Set<Project>().Remove(project);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return true;
            });
        }
        public async Task<Project> CopyProjectAsync(Project project, string sourceProjectId)
        {
            var decodedSourceId = HttpUtility.UrlDecode(sourceProjectId);

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();

                // 1. Insert the new project record
                _context.Set<Project>().Add(project);
                await _context.SaveChangesAsync();

                // 2. Copy ProjectYear records
                var sourceProjectYears = await _context.Set<ProjectYear>()
                    .AsNoTracking()
                    .Where(py => py.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourcePY in sourceProjectYears)
                {
                    var newPY = new ProjectYear
                    {
                        Project = project.ProjectId,
                        YearValue = sourcePY.YearValue

                    };
                    _context.Set<ProjectYear>().Add(newPY);
                }

                // Save ProjectYear records before inserting child records that FK reference them
                await _context.SaveChangesAsync();

                // 3. Copy AnimalRequirement records
                var sourceAnimalReqs = await _context.Set<AnimalRequirement>()
                    .AsNoTracking()
                    .Where(ar => ar.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceAR in sourceAnimalReqs)
                {
                    var newAR = new AnimalRequirement
                    {
                        Project = project.ProjectId,
                        Year = sourceAR.Year,
                        AnimalType = sourceAR.AnimalType,
                        NumberOfDays = sourceAR.NumberOfDays,
                        NumberOfAnimals = sourceAR.NumberOfAnimals,
                        DailyRate = sourceAR.DailyRate
                    };
                    _context.Set<AnimalRequirement>().Add(newAR);
                }

                // 4. Copy AdditionalCost records
                var sourceAdditionalCosts = await _context.Set<AdditionalCost>()
                    .AsNoTracking()
                    .Where(ac => ac.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceAC in sourceAdditionalCosts)
                {
                    var newAC = new AdditionalCost
                    {
                        Project = project.ProjectId,
                        Year = sourceAC.Year,
                        AccountCat = sourceAC.AccountCat,
                        Description = sourceAC.Description,
                        ItemCost = sourceAC.ItemCost,
                        CostEntered = sourceAC.CostEntered,
                        Freq = sourceAC.Freq
                    };
                    _context.Set<AdditionalCost>().Add(newAC);
                }

                // 5. Copy StaffRequirement records
                var sourceStaffReqs = await _context.Set<StaffRequirement>()
                    .AsNoTracking()
                    .Where(sr => sr.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceSR in sourceStaffReqs)
                {
                    var newSR = new StaffRequirement
                    {
                        Project = project.ProjectId,
                        Year = sourceSR.Year,
                        WgGrade = sourceSR.WgGrade,
                        Name = sourceSR.Name,
                        Nohours = sourceSR.Nohours,
                        Nodays = sourceSR.Nodays,
                        Chargerate = sourceSR.Chargerate,
                        Payrate = sourceSR.Payrate,
                        Npr = sourceSR.Npr,
                        Ohr = sourceSR.Ohr
                    };
                    _context.Set<StaffRequirement>().Add(newSR);
                }

                // 6. Copy TestRequirement records
                var sourceTestReqs = await _context.Set<TestRequirement>()
                    .AsNoTracking()
                    .Where(tr => tr.Project == decodedSourceId)
                    .ToListAsync();

                foreach (var sourceTR in sourceTestReqs)
                {
                    var newTR = new TestRequirement
                    {
                        Project = project.ProjectId,
                        Year = sourceTR.Year,
                        TestCode = sourceTR.TestCode,
                        NumberOfTests = sourceTR.NumberOfTests,
                        UnitPrice = sourceTR.UnitPrice
                    };
                    _context.Set<TestRequirement>().Add(newTR);
                }

                // Save all copied records
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return project;
            });
        }
        public async Task<string> GetNextProjectNumberAsync(string? baseNumber)
        {
            if (!string.IsNullOrEmpty(baseNumber))
            {
                baseNumber = HttpUtility.UrlDecode(baseNumber);
            }

            var currentYear = GetCurrentFinancialYear();

            var dbSet = _context.Set<Project>();

            if (string.IsNullOrEmpty(baseNumber))
            {
                // Get only the highest ProjectId for the year (NO full list)
                var maxProjectId = await dbSet
                    .Where(p => p.ProjectId.StartsWith($"{currentYear}/"))
                    .OrderByDescending(p => p.ProjectId)
                    .Select(p => p.ProjectId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(maxProjectId))
                {
                    return $"{currentYear}/001";
                }

                // Extract numeric part safely
                var parts = maxProjectId.Split('/');
                if (parts.Length == 2 && parts[1].Length >= 3 &&
                    int.TryParse(parts[1].Substring(0, 3), out int num))
                {
                    return $"{currentYear}/{(num + 1):D3}";
                }

                return $"{currentYear}/001";
            }
            else
            {
                // CASE 1: 2024/001a → increment letter
                if (baseNumber.Length == 9 && char.IsLetter(baseNumber[8]))
                {
                    var basePattern = baseNumber.Substring(0, 8);

                    var maxProject = await dbSet
                        .Where(p => p.ProjectId.StartsWith(basePattern))
                        .OrderByDescending(p => p.ProjectId)
                        .Select(p => p.ProjectId)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrEmpty(maxProject))
                    {
                        return baseNumber;
                    }

                    if (maxProject.Length == 9 && char.IsLetter(maxProject[8]))
                    {
                        var nextChar = (char)(maxProject[8] + 1);
                        return $"{basePattern}{nextChar}";
                    }

                    return $"{basePattern}a";
                }

                // CASE 2: 2024/001 → find next suffix
                if (baseNumber.Length == 8)
                {
                    var maxProject = await dbSet
                        .Where(p => p.ProjectId.StartsWith(baseNumber))
                        .OrderByDescending(p => p.ProjectId)
                        .Select(p => p.ProjectId)
                        .FirstOrDefaultAsync();

                    if (string.IsNullOrEmpty(maxProject))
                    {
                        return baseNumber;
                    }

                    if (maxProject.Length == 9 && char.IsLetter(maxProject[8]))
                    {
                        var nextChar = (char)(maxProject[8] + 1);
                        return $"{baseNumber}{nextChar}";
                    }

                    return $"{baseNumber}a";
                }

                // CASE 3: fallback
                var similarProject = await dbSet
                    .Where(p => p.ProjectId.StartsWith(baseNumber))
                    .OrderByDescending(p => p.ProjectId)
                    .Select(p => p.ProjectId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(similarProject))
                {
                    return baseNumber;
                }

                return $"{similarProject}a";
            }
        }
        public async Task<bool> RecostProjectAsync(string projectID)
        {
            bool result = false;

            var decodedId = HttpUtility.UrlDecode(projectID);
            var currentYearSetting = await _settingsRepository.GetSettingValueByIdAsync("CurrentYear");
            

            if (string.IsNullOrEmpty(currentYearSetting) || !int.TryParse(currentYearSetting, out int fyear))
            {
                throw new InvalidOperationException("CurrentYear setting not found or invalid in settings table.");
            }
            bool isDefraProject = (await _context.Projects
                .Where(p => p.ProjectId == decodedId)
                .Select(p => (int?)p.IsDefraProject)
                .FirstOrDefaultAsync() ?? 0) != 0;
            #region TestorProducts
            // Step 2: Get all relevant records
            var records = await _context.TestRequirements
                .Where(r => r.Project ==decodedId && r.Year >= fyear)
                .ToListAsync();

            // Step 3: Preload test data (avoid repeated DB calls like DLookup)
            var testData = await _context.FpsTestorProducts
                .ToDictionaryAsync(t => t.ItemCode);

            // Step 4: Loop and update
            foreach (var rec in records)
            {
                if (!testData.TryGetValue(rec.TestCode, out var test))
                    continue;

                decimal basePriceDecimal = isDefraProject
                    ? test.DefraUnitPrice
                    : test.UnitPriceVla.GetValueOrDefault(0);

                double basePrice = (double)basePriceDecimal;
                double inflationFactor = await fnInflation("InflationTests", decodedId, rec.Year, fyear);

                rec.UnitPrice = basePrice * inflationFactor;
            }

            // Step 5: Save changes
            await _context.SaveChangesAsync();
            #endregion

            #region AnimalRequirements
            // Get all animal requirement records
            var animalRecords = await _context.AnimalRequirements
                .Where(ar => ar.Project == decodedId && ar.Year >= fyear)
                .ToListAsync();

            // Preload animal data (avoid repeated DB calls like DLookup)
            var animalData = await _context.FpsAnimals
                .ToDictionaryAsync(a => a.AnimalType);

            // Loop and update
            foreach (var rec in animalRecords)
            {
                if (!animalData.TryGetValue(rec.AnimalType, out var animal))
                    continue;

                decimal? baseRateDecimal = isDefraProject
                    ? animal.DefraDailyRate
                    : animal.DailyRate;

                double baseRate = (double)(baseRateDecimal ?? 0);

                double inflationFactor = await fnInflation("InflationAnimals", decodedId, rec.Year ?? 0, fyear);

                rec.DailyRate = baseRate * inflationFactor;
            }

            // Save changes
            await _context.SaveChangesAsync();
            #endregion

            #region AdditionalCosts
            // Get all additional cost records
            var additionalCostRecords = await _context.AdditionalCosts
                .Where(ac => ac.Project == decodedId && ac.Year >= fyear)
                .ToListAsync();

            // Loop and update
            foreach (var rec in additionalCostRecords)
            {
                double inflatedCost;

                if (await fnUseInflation(rec.AccountCat))
                {
                    double inflationFactor = await fnInflation("InflationExceptional", decodedId, rec.Year ?? 0, fyear);
                    inflatedCost = rec.CostEntered * inflationFactor;
                }
                else
                {
                    inflatedCost = rec.CostEntered;
                }

                rec.ItemCost = inflatedCost;
            }

            // Save changes
            await _context.SaveChangesAsync();
            #endregion

            #region StaffRequirements
            // Get all staff requirement records
            var staffRecords = await _context.StaffRequirements
                .Where(sr => sr.Project == decodedId && sr.Year >= fyear)
                .ToListAsync();

            // Preload pay rates data based on project type
            // Equivalent to qrypayRates_defra or qrypayRates_nondefra
            var payRatesQuery = from wg in _context.WorkGroupGrades
                                join pc in _context.ProfitCentreGrades
                                on wg.ProfitCentreGrade equals pc.PcGrade
                                select new
                                {
                                    wg.WgGrade,
                                    ChargeRate = isDefraProject 
                                        ? pc.DefraChargeRate 
                                        : pc.ChargeRate,
                                    pc.PayRate,
                                    pc.Npr,
                                    Ohr = isDefraProject ? 0 : pc.Ohr
                                };

            // Filter out zero charge rates (applies to both DEFRA and non-DEFRA)
            payRatesQuery = payRatesQuery.Where(x => x.ChargeRate != null && x.ChargeRate != 0);

            var payRatesData = await payRatesQuery.ToDictionaryAsync(x => x.WgGrade);

            // Loop and update
            foreach (var rec in staffRecords)
            {
                if (!payRatesData.TryGetValue(rec.WgGrade, out var payRate))
                    continue;

                double inflationFactor = await fnInflation("InflationStaff", decodedId, rec.Year ?? 0, fyear);

                rec.Chargerate = (double)(payRate.ChargeRate ?? 0) * inflationFactor;
                rec.Payrate = (double)(payRate.PayRate ?? 0) * inflationFactor;
                rec.Npr = (double)(payRate.Npr ?? 0) * inflationFactor;
                rec.Ohr = (double)(payRate.Ohr ?? 0) * inflationFactor;
            }

            // Save changes
            await _context.SaveChangesAsync();
            #endregion

            result = true;
            return result;
        }

        public async Task<StaffYearsPivotData> GetStaffYearsPivotAsync(string projectId, PaginationParameters<string>? parameters = null)
        {
            var decodedProjectId = HttpUtility.UrlDecode(projectId);

            var daysInYearSetting = await _settingsRepository.GetSettingValueByIdAsync("DaysInYear");
            double daysInYear = !string.IsNullOrEmpty(daysInYearSetting) && double.TryParse(daysInYearSetting, out double d) ? d : 220.0;

            var rows = await _context.StaffRequirements
                .AsNoTracking()
                .Where(s => s.Project == decodedProjectId && s.Year.HasValue && s.Nodays.HasValue)
                .ToListAsync();

            var years = rows.Select(s => s.Year!.Value).Distinct().OrderBy(y => y).ToList();

            var pivotRows = rows
                .GroupBy(s => s.WgGrade.StartsWith("GD5", StringComparison.OrdinalIgnoreCase)
                    ? "GD5"
                    : s.WgGrade.Length > 0 ? s.WgGrade[..1] : s.WgGrade)
                .Select(g => new StaffYearsRowData
                {
                    Project = decodedProjectId,
                    Grade = g.Key,
                    YearlyAmounts = g.GroupBy(s => s.Year!.Value)
                        .ToDictionary(yg => yg.Key, yg => yg.Sum(s => s.Nodays!.Value / daysInYear)),
                    Total = g.Sum(s => s.Nodays!.Value / daysInYear)
                })
                .ToList();


            if (!string.IsNullOrWhiteSpace(parameters?.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(parameters.Filter);
                if (filterModel != null)
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

                    if (dict.TryGetValue("Grade", out object? grade) && grade != null)
                        pivotRows = pivotRows
                            .Where(r => r.Grade.Contains(grade.ToString()!, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters?.SortBy))
            {
                var sortBy = parameters.SortBy.Trim().ToLowerInvariant();
                var descending = parameters.Descending;

                pivotRows = sortBy switch
                {
                    "project" => descending
                        ? pivotRows.OrderByDescending(r => r.Project).ToList()
                        : pivotRows.OrderBy(r => r.Project).ToList(),
                    "grade" => descending
                        ? pivotRows.OrderByDescending(r => r.Grade).ToList()
                        : pivotRows.OrderBy(r => r.Grade).ToList(),
                    "total" => descending
                        ? pivotRows.OrderByDescending(r => r.Total).ToList()
                        : pivotRows.OrderBy(r => r.Total).ToList(),
                    _ => pivotRows.OrderBy(r => r.Grade).ToList()
                };
            }
            else
            {
                pivotRows = pivotRows.OrderBy(r => r.Grade).ToList();
            }

            var totalCount = pivotRows.Count;

            if (parameters?.Page > 0 && parameters.PageSize > 0)
            {
                pivotRows = pivotRows
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();
            }

            return new StaffYearsPivotData { Years = years, Rows = pivotRows, TotalCount = totalCount };
        }

        public async Task<StaffEffortPivotData> GetStaffEffortAsync(string projectId, PaginationParameters<string>? parameters = null)
        {
            var decodedProjectId = HttpUtility.UrlDecode(projectId);

            var staffRows = await _context.StaffRequirements
                .AsNoTracking()
                .Where(s => s.Project == decodedProjectId && s.Year.HasValue && s.Nodays.HasValue)
                .ToListAsync();

            var wgLookup = await _context.WorkGroupGrades
                .AsNoTracking()
                .ToDictionaryAsync(w => w.WgGrade, w => w.WorkGroup);

            var joined = staffRows.Select(s => new
            {
                Project = s.Project ?? decodedProjectId,
                WorkGroup = wgLookup.TryGetValue(s.WgGrade, out var wg) ? wg : string.Empty,
                GradeCode = s.WgGrade.StartsWith("GD5", StringComparison.OrdinalIgnoreCase)
                    ? "GD5"
                    : s.WgGrade.Length > 0 ? s.WgGrade[..1] : s.WgGrade,
                Name = s.Name ?? string.Empty,
                Year = s.Year!.Value,
                NoDays = s.Nodays!.Value
            }).ToList();

            var years = joined.Select(r => r.Year).Distinct().OrderBy(y => y).ToList();

            var pivotRows = joined
                .GroupBy(r => new { r.Project, r.WorkGroup, r.GradeCode, r.Name })
                .Select(g => new StaffEffortRowData
                {
                    Project = g.Key.Project,
                    WorkGroup = g.Key.WorkGroup,
                    GradeCode = g.Key.GradeCode,
                    Name = g.Key.Name,
                    YearlyAmounts = g.GroupBy(r => r.Year)
                        .ToDictionary(yg => yg.Key, yg => yg.Sum(r => r.NoDays)),
                    Total = g.Sum(r => r.NoDays)
                })               
                .ToList();

            if (!string.IsNullOrWhiteSpace(parameters?.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(parameters.Filter);
                if (filterModel != null)
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

                    if (dict.TryGetValue("GradeCode", out object? gradeCode) && gradeCode != null)
                        pivotRows = pivotRows
                            .Where(r => r.GradeCode.Contains(gradeCode.ToString()!, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    if (dict.TryGetValue("Name", out object? name) && name != null)
                        pivotRows = pivotRows
                            .Where(r => r.Name.Contains(name.ToString()!, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                    if (dict.TryGetValue("WorkGroup", out object? workGroup) && workGroup != null)
                        pivotRows = pivotRows
                            .Where(r => r.WorkGroup.Contains(workGroup.ToString()!, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters?.SortBy))
            {
                var sortBy = parameters.SortBy.Trim().ToLowerInvariant();
                var descending = parameters.Descending;

                pivotRows = sortBy switch
                {
                    "project" => descending
                        ? pivotRows.OrderByDescending(r => r.Project).ToList()
                        : pivotRows.OrderBy(r => r.Project).ToList(),
                    "workgroup" => descending
                        ? pivotRows.OrderByDescending(r => r.WorkGroup).ToList()
                        : pivotRows.OrderBy(r => r.WorkGroup).ToList(),
                    "gradecode" => descending
                        ? pivotRows.OrderByDescending(r => r.GradeCode).ToList()
                        : pivotRows.OrderBy(r => r.GradeCode).ToList(),
                    "name" => descending
                        ? pivotRows.OrderByDescending(r => r.Name).ToList()
                        : pivotRows.OrderBy(r => r.Name).ToList(),
                    "total" => descending
                        ? pivotRows.OrderByDescending(r => r.Total).ToList()
                        : pivotRows.OrderBy(r => r.Total).ToList(),
                    _ => pivotRows.OrderBy(r => r.GradeCode).ThenBy(r => r.Name).ToList()
                };
            }
            else
            {
                pivotRows = pivotRows.OrderBy(r => r.WorkGroup).ThenBy(r => r.GradeCode).ToList();
            }

            var totalCount = pivotRows.Count;

            if (parameters?.Page > 0 && parameters.PageSize > 0)
            {
                pivotRows = pivotRows
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();
            }

            return new StaffEffortPivotData { Years = years, Rows = pivotRows, TotalCount = totalCount };
        }

        public async Task<ProjectCostsPivotData> GetProjectCostsPivotAsync(string projectId, PaginationParameters<string>? parameters = null)
        {
            var decodedProjectId = HttpUtility.UrlDecode(projectId);

            // qryCSG7_Animals: Sum(NumberOfDays * NumberOfAnimals * DailyRate) AS Cost, "Other Costs"
            var animalsRows = await _context.AnimalRequirements
                .AsNoTracking()
                .Where(a => a.Project == decodedProjectId && a.Year.HasValue)
                .GroupBy(a => new { a.Project, a.Year })
                .Select(g => new
                {
                    Project = g.Key.Project,
                    YearNo = g.Key.Year!.Value,
                    Cost = g.Sum(a => (a.NumberOfDays ?? 0) * (a.NumberOfAnimals ?? 0) * (a.DailyRate ?? 0)),
                    Category = "Other Costs"
                })
                .ToListAsync();

            // qryCSG7_Exceptional: Sum(ItemCost), CSG7_Group from tblkpAccountCategory
            var exceptionalRows = await (
                from ac in _context.AdditionalCosts.AsNoTracking()
                join cat in _context.FpsAccountCategories.AsNoTracking()
                    on ac.AccountCat equals cat.AccShortName
                where ac.Project == decodedProjectId && ac.Year.HasValue
                group new { ac, cat } by new { ac.Project, ac.Year, cat.Csg7Group } into g
                select new
                {
                    Project = g.Key.Project,
                    YearNo = g.Key.Year!.Value,
                    Cost = g.Sum(x => x.ac.ItemCost ?? 0.0),
                    Category = g.Key.Csg7Group ?? "Other"
                }
            ).ToListAsync();

            // qryCSG7_OHR: Sum((NoHours * ChargeRate) * (OHR + NPR) / ChargeRate) AS Cost, "Overheads"
            var ohrRows = await _context.StaffRequirements
                .AsNoTracking()
                .Where(s => s.Project == decodedProjectId && s.Year.HasValue && s.Chargerate != 0)
                .GroupBy(s => new { s.Project, s.Year })
                .Select(g => new
                {
                    Project = g.Key.Project,
                    YearNo = g.Key.Year!.Value,
                    Cost = g.Sum(s => (s.Nohours ?? 0) * (s.Chargerate ?? 0) * ((s.Ohr ?? 0) + (s.Npr ?? 0)) / (s.Chargerate ?? 1)),
                    Category = "Overheads"
                })
                .ToListAsync();

            // qryCSG7_Pay: Sum((NoHours * ChargeRate) * PayRate / ChargeRate) AS Cost, "Pay"
            var payRows = await _context.StaffRequirements
                .AsNoTracking()
                .Where(s => s.Project == decodedProjectId && s.Year.HasValue && s.Chargerate != 0)
                .GroupBy(s => new { s.Project, s.Year })
                .Select(g => new
                {
                    Project = g.Key.Project,
                    YearNo = g.Key.Year!.Value,
                    Cost = g.Sum(s => (s.Nohours ?? 0) * (s.Chargerate ?? 0) * (s.Payrate ?? 0) / (s.Chargerate ?? 1)),
                    Category = "Pay"
                })
                .ToListAsync();

            // qryCSG7_Tests: Sum(NoTests * UnitPrice) AS Cost, "Other Costs"
            var testsRows = await _context.TestRequirements
                .AsNoTracking()
                .Where(t => t.Project == decodedProjectId)
                .GroupBy(t => new { t.Project, t.Year })
                .Select(g => new
                {
                    Project = g.Key.Project,
                    YearNo = g.Key.Year,
                    Cost = g.Sum(t => (t.NumberOfTests ?? 0) * (t.UnitPrice ?? 0)),
                    Category = "Other Costs"
                })
                .ToListAsync();

            // UNION ALL in memory then GROUP BY Project, Category, pivot by YearNo
            var union = animalsRows
                .Concat(exceptionalRows.Select(r => new { Project = (string?)r.Project, r.YearNo, r.Cost, r.Category }))
                .Concat(ohrRows.Select(r => new { Project = (string?)r.Project, r.YearNo, r.Cost, r.Category }))
                .Concat(payRows.Select(r => new { Project = (string?)r.Project, r.YearNo, r.Cost, r.Category }))
                .Concat(testsRows.Select(r => new { Project = (string?)r.Project, r.YearNo, r.Cost, r.Category }))
                .ToList();

            var years = union.Select(r => r.YearNo).Distinct().OrderBy(y => y).ToList();

            var pivotRows = union
                .GroupBy(r => new { r.Project, r.Category })
                .Select(g => new ProjectCostsRowData
                {
                    Project = g.Key.Project ?? decodedProjectId,
                    Category = g.Key.Category,
                    YearlyAmounts = g.GroupBy(r => r.YearNo)
                        .ToDictionary(yg => yg.Key, yg => yg.Sum(r => r.Cost)),
                    Total = g.Sum(r => r.Cost)
                })
                .ToList();

            // Apply filter
            if (!string.IsNullOrWhiteSpace(parameters?.Filter))
            {
                dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(parameters.Filter);
                if (filterModel != null)
                {
                    IDictionary<string, object> dict = (IDictionary<string, object>)filterModel;

                    if (dict.TryGetValue("Category", out object? category) && category != null)
                        pivotRows = pivotRows
                            .Where(r => r.Category.Contains(category.ToString()!, StringComparison.OrdinalIgnoreCase))
                            .ToList();
                }
            }

            if (!string.IsNullOrWhiteSpace(parameters?.SortBy))
            {
                var sortBy = parameters.SortBy.Trim().ToLowerInvariant();
                var descending = parameters.Descending;

                pivotRows = sortBy switch
                {
                    "project" => descending
                        ? pivotRows.OrderByDescending(r => r.Project).ToList()
                        : pivotRows.OrderBy(r => r.Project).ToList(),
                    "category" => descending
                        ? pivotRows.OrderByDescending(r => r.Category).ToList()
                        : pivotRows.OrderBy(r => r.Category).ToList(),
                    "total" => descending
                        ? pivotRows.OrderByDescending(r => r.Total).ToList()
                        : pivotRows.OrderBy(r => r.Total).ToList(),
                    _ => pivotRows.OrderBy(r => r.Category).ToList()
                };
            }
            else
            {
                pivotRows = pivotRows.OrderBy(r => r.Category).ToList();
            }

            var totalCount = pivotRows.Count;

            if (parameters?.Page > 0 && parameters.PageSize > 0)
            {
                pivotRows = pivotRows
                    .Skip((parameters.Page - 1) * parameters.PageSize)
                    .Take(parameters.PageSize)
                    .ToList();
            }

            return new ProjectCostsPivotData { Years = years, Rows = pivotRows, TotalCount = totalCount };
        }

        private static int GetCurrentFinancialYear()
        {
            var now = DateTime.Now;
            // MS Access logic: if month <= 3 (Jan-Mar), use previous year, otherwise current year
            return now.Month <= 3 ? now.Year - 1 : now.Year;
        }

        // Filtering logic similar to FPS ApplyEmployeeFilter
        private static IQueryable<Project> ApplyProjectFilter(IQueryable<Project> queryProjects, string? filter)
        { 
        if (string.IsNullOrEmpty(filter))
            {
                return queryProjects;
            }

            dynamic? filterModel = JsonConvert.DeserializeObject<ExpandoObject>(filter);
            if (filterModel == null)
            {
                return queryProjects;
            }

            var dict = (IDictionary<string, object>)filterModel;

            if (dict.TryGetValue("ProjectId", out var projectId) && projectId != null)
            {                
                queryProjects = queryProjects.Where(x => EF.Functions.ILike(x.ProjectId, $"%{projectId}%"));
            }           

            return queryProjects;
        }

        // Sorting logic similar to FPS ApplySorting
        private static IQueryable ApplySorting(IQueryable<Project> query, string? sortBy, bool descending)
        {
            if (string.IsNullOrEmpty(sortBy))
            {
                return query.OrderByDescending(p => p.ProjectId);
            }

            return ApplySortingByProperty(query, sortBy.ToLower(), descending);
        }

        // Property-based sorting similar to FPS ApplySortingByProperty
        private static IQueryable ApplySortingByProperty(IQueryable<Project> query, string property, bool descending)
        {
            return property switch
            {
                "projectid" => ApplyOrder(query, p => p.ProjectId, descending),
                "projecttitle" => ApplyOrder(query, p => p.ProjectTitle, descending),
                "programme" => ApplyOrder(query, p => p.Programme, descending),
                "contractnumber" => ApplyOrder(query, p => p.ContractNumber, descending),
                "customername" => ApplyOrder(query, p => p.CustomerName, descending),
                "disease" => ApplyOrder(query, p => p.Disease, descending),
                "startdate" => ApplyOrder(query, p => p.StartDate, descending),
                "contractprice" => ApplyOrder(query, p => p.ContractPrice, descending),
                "preparedby" => ApplyOrder(query, p => p.PreparedBy, descending),
                "dateofsubmission" => ApplyOrder(query, p => p.DateOfSubmission, descending),
                _ => query.OrderByDescending(p => p.ProjectId)
            };
        }

        // Order application helper similar to FPS ApplyOrder
        private static IQueryable ApplyOrder<T>(IQueryable<Project> query, Expression<Func<Project, T>> keySelector, bool descending)
        {
            return descending ? query.OrderByDescending(keySelector) : query.OrderBy(keySelector);
        }

        
        public async Task<ProjectSummaryExportData> GetProjectSummaryExportDataAsync(string projectId)
        {
            var decodedProjectId = HttpUtility.UrlDecode(projectId);

            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProjectId == decodedProjectId);

            var years = await _context.ProjectYears
                .AsNoTracking()
                .Where(py => py.Project == decodedProjectId)
                .OrderBy(py => py.YearValue)
                .ToListAsync();

            var staffRows = await _context.StaffRequirements
                .AsNoTracking()
                .Where(s => s.Project == decodedProjectId)
                .OrderBy(s => s.Year).ThenBy(s => s.WgGrade)
                .ToListAsync();

            var testRows = await _context.TestRequirements
                .AsNoTracking()
                .Where(t => t.Project == decodedProjectId)
                .OrderBy(t => t.Year).ThenBy(t => t.TestCode)
                .ToListAsync();

            var animalRows = await _context.AnimalRequirements
                .AsNoTracking()
                .Where(a => a.Project == decodedProjectId)
                .OrderBy(a => a.Year).ThenBy(a => a.AnimalType)
                .ToListAsync();

            var additionalRows = await _context.AdditionalCosts
                .AsNoTracking()
                .Where(ac => ac.Project == decodedProjectId)
                .OrderBy(ac => ac.Year).ThenBy(ac => ac.Description)
                .ToListAsync();

            return new ProjectSummaryExportData
            {
                Project = project,
                Years = years,
                StaffRequirements = staffRows,
                TestRequirements = testRows,
                AnimalRequirements = animalRows,
                AdditionalCosts = additionalRows
            };
        }

        private static int fnYearGapSign(int yearGap)
        {
            if (yearGap == 0) return 0;
            if (yearGap > 0) return 1;
            return -1;
        }

        private async Task<bool> fnUseInflation(string accountCat)
        {
            
            var useInflation = await (from ac in _context.FpsAccountCategories
                                       join ag in _context.AccountGroups 
                                       on ac.Csg7Group equals ag.Csg7group
                                       where ac.AccShortName == accountCat
                                       select ag.Useinflation)
                                      .FirstOrDefaultAsync();

            // Return false if null (no match found), otherwise return the value
            return useInflation ?? false;
        }

        private async Task<double> fnInflation(string infType, string proj, int year,int currentYear)
        {
            // Get project data
            var project = await _context.Projects
                .Where(p => p.ProjectId == proj)
                .Select(p => new
                {
                    p.Inflation,
                    p.StartDate,
                    p.StartFYear,
                    p.FinancialYears
                })
                .FirstOrDefaultAsync();

            if (project == null)
            {
                throw new InvalidOperationException($"Project {proj} not found.");
            }

            // If inflation is disabled, return 1 (no inflation)
            if (project.Inflation != -1)
            {
                return 1.0;
            }

            // Get inflation rate from settings
            var inflationRateSetting = await _settingsRepository.GetSettingValueByIdAsync(infType);
            
            if (string.IsNullOrEmpty(inflationRateSetting) || !double.TryParse(inflationRateSetting, out double inflationRate))
            {
                throw new InvalidOperationException($"Inflation setting '{infType}' not found or invalid.");
            }

            if (project.FinancialYears == -1)
            {
                // Simple compound inflation based on financial years
              

                int yearGap = year - currentYear;
                if (yearGap < 0) yearGap = 0;

                return Math.Pow(1 + inflationRate / 100, yearGap);
            }
            else
            {
                // Complex calculation with partial year logic
                if (!project.StartFYear.HasValue || !project.StartDate.HasValue)
                {
                    throw new InvalidOperationException($"Project {proj} missing StartFYear or StartDate.");
                }

                var fyearStart = new DateTime((int)project.StartFYear.Value, 4, 1);
                var startDate = project.StartDate.Value;               

                int yearGap = year - currentYear;
                double percentOfYear = Math.Abs((fyearStart - startDate).TotalDays) / 364.0;

                double inflation;
                double inflation2;

                if (startDate < fyearStart)
                {
                    double inflationAsNumber = 1 + (fnYearGapSign(yearGap - 1) * inflationRate) / 100;
                    inflation = percentOfYear * Math.Pow(inflationAsNumber, Math.Abs(yearGap - 1));

                    double inflationAsNumber2 = 1 + (fnYearGapSign(yearGap) * inflationRate) / 100;
                    inflation2 = (1 - percentOfYear) * Math.Pow(inflationAsNumber2, Math.Abs(yearGap));
                }
                else
                {
                    double inflationAsNumber = 1 + (fnYearGapSign(yearGap) * inflationRate) / 100;
                    inflation = (1 - percentOfYear) * Math.Pow(inflationAsNumber, Math.Abs(yearGap));

                    double inflationAsNumber2 = 1 + (fnYearGapSign(yearGap + 1) * inflationRate) / 100;
                    inflation2 = percentOfYear * Math.Pow(inflationAsNumber2, Math.Abs(yearGap + 1));
                }

                return inflation + inflation2;
            }
        }

        public async Task<ProjectYearCostSummary> GetProjectYearCostSummaryAsync(string projectId, int year)
        {
            var decodedId = HttpUtility.UrlDecode(projectId);

            // SELECT DISTINCTROW tblStaffRequ.Project, tblStaffRequ.Year,
            //        Sum([ChargeRate]*[NoHours]) AS StaffCost
            double staffCostTotal = await _context.StaffRequirements
                .AsNoTracking()
                .Where(s => s.Project == decodedId && s.Year == year)
                .SumAsync(s => s.Chargerate.HasValue && s.Nohours.HasValue
                    ? s.Chargerate.Value * s.Nohours.Value
                    : 0.0);

            // SELECT DISTINCTROW tblTestRequ.Project, tblTestRequ.Year,
            //        Sum([UnitPrice]*[NoTests]) AS TestCost
            double testCostTotal = await _context.TestRequirements
                .AsNoTracking()
                .Where(t => t.Project == decodedId && t.Year == year)
                .SumAsync(t => t.UnitPrice.HasValue && t.NumberOfTests.HasValue
                    ? t.UnitPrice.Value * t.NumberOfTests.Value
                    : 0.0);

            // SELECT DISTINCTROW tblAnimalReq.Project, tblAnimalReq.Year,
            //        Sum([Number of Days]*[Number of Animals]*[DailyRate]) AS AnimalCost
            double animalCostTotal = await _context.AnimalRequirements
                .AsNoTracking()
                .Where(a => a.Project == decodedId && a.Year == year)
                .SumAsync(a => a.NumberOfDays.HasValue && a.NumberOfAnimals.HasValue && a.DailyRate.HasValue
                    ? a.NumberOfDays.Value * a.NumberOfAnimals.Value * a.DailyRate.Value
                    : 0.0);

            // SELECT DISTINCTROW tblAdditionalCosts.Project, tblAdditionalCosts.Year,
            //        Sum(ItemCost) AS LineCost
            double additionalCostTotal = (double)await _context.AdditionalCosts
                .AsNoTracking()
                .Where(ac => ac.Project == decodedId && ac.Year == year)
                .SumAsync(ac => ac.ItemCost ?? 0.0);

            return new ProjectYearCostSummary
            {
                Project             = decodedId,
                Year                = year,
                StaffCostTotal      = staffCostTotal,
                TestCostTotal       = testCostTotal,
                AnimalCostTotal     = animalCostTotal,
                AdditionalCostTotal = additionalCostTotal
            };
        }

        public async Task<double> GetInflationFactorAsync(string infType, string projectId, int year, int currentYear)
        {
            var decodedId = HttpUtility.UrlDecode(projectId);
            return await fnInflation(infType, decodedId, year, currentYear);
        }

        public async Task<double> GetProfitIncludedTotalAsync(string projectId, int year)
        {
            var decodedId = HttpUtility.UrlDecode(projectId);

            double staffCostTotal = await _context.StaffRequirements
                .Where(sr => sr.Project == decodedId && sr.Year == year)
                .SumAsync(sr => sr.Chargerate.HasValue && sr.Nohours.HasValue
                    ? sr.Chargerate.Value * sr.Nohours.Value
                    : 0.0);

            double testCostTotal = await _context.TestRequirements
                .Where(tr => tr.Project == decodedId && tr.Year == year)
                .SumAsync(tr => tr.UnitPrice.HasValue && tr.NumberOfTests.HasValue
                    ? tr.UnitPrice.Value * tr.NumberOfTests.Value
                    : 0.0);

            double animalCostTotal = await _context.AnimalRequirements
                .Where(ar => ar.Project == decodedId && ar.Year == year)
                .SumAsync(ar => ar.DailyRate.HasValue && ar.NumberOfDays.HasValue && ar.NumberOfAnimals.HasValue
                    ? ar.DailyRate.Value * ar.NumberOfDays.Value * ar.NumberOfAnimals.Value
                    : 0.0);

            double additionalCostTotal = (double)await _context.AdditionalCosts
                .Where(ac => ac.Project == decodedId && ac.Year == year)
                .SumAsync(ac => ac.ItemCost);            

            double profitStaff       = await GetProfitFactorAsync("Profitstaff");
            double profitTests       = await GetProfitFactorAsync("Profittests");
            double profitExceptional = await GetProfitFactorAsync("ProfitExceptional");
            double profitAnimals     = await GetProfitFactorAsync("Profitanimals");            

            return (staffCostTotal      * profitStaff)       +
                   (testCostTotal       * profitTests)       +
                   (additionalCostTotal * profitExceptional) +
                   (animalCostTotal     * profitAnimals);
        }

        private async Task<double> GetProfitFactorAsync(string ptype)
        {
            var settingValue = await _settingsRepository.GetSettingValueByIdAsync(ptype);

            if (string.IsNullOrEmpty(settingValue) || !double.TryParse(settingValue, out double rate))
                return 1.0; // fallback: no markup if setting missing

            double p = rate / 100.0;

            if (p >= 1.0) return 1.0; // guard against division by zero

            return 1.0 + (p / (1.0 - p));
        }
    }
}
