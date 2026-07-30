using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using System.Web;

namespace Apha.FPSApps.Web.Areas.CostBook.Controllers;

[Area("CostBook")]
[Authorize(Roles = "CostbookAdmin,CostbookUser")]
[AuthorizeForScopes(ScopeKeySection = "CostBookApiSettings:Scope")]
public class YearlyDetailsController : Controller
{
    private readonly ICostBookYearlyDetailsService _service;
    private readonly ICostBookProjectSummaryService _summaryService;
    private readonly IMapper _mapper;

    public YearlyDetailsController(
        ICostBookYearlyDetailsService service,
        ICostBookProjectSummaryService summaryService,
        IMapper mapper)
    {
        _service = service;
        _summaryService = summaryService;
        _mapper = mapper;
    }

    // ── INDEX ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(string projectId, int selectedYear = 0)
    {
        var decodedProjectId = HttpUtility.UrlDecode(projectId);

        var headerResponse = await _service.GetProjectHeaderAsync(decodedProjectId);
        if (!headerResponse.Success || headerResponse.Data is null)
            return RedirectToAction("Index", "Projects");

        var header = headerResponse.Data;
        var isDefra = header.IsDefraProject == -1;

        var yearsResponse = await _service.GetProjectYearsAsync(decodedProjectId);
        var projectYears = yearsResponse.Success && yearsResponse.Data != null
            ? yearsResponse.Data.Select(y => y.YearValue).ToList()
            : new List<int>();

        if (selectedYear == 0)
            selectedYear = projectYears.FirstOrDefault();

        if (projectYears.Count == 0)
        {
            var startYear = (int)(header.StartFYear ?? 0);
            if (startYear > 0)
            {
                var dto = new ProjectYearDto
                {
                    Project = decodedProjectId,
                    YearValue = startYear
                };
                var addYearResponse = await _service.AddProjectYearAsync(decodedProjectId, startYear, dto);
                if (addYearResponse.Success && addYearResponse.Data != null)
                {
                    projectYears.Add(startYear);
                    selectedYear = startYear;
                }
            }
        }

        var viewModel = new YearlyDetailsViewModel
        {
            ProjectHeaderDto = header,
            SelectedYear = selectedYear,
            ProjectYears = projectYears,
        };        

        if (selectedYear > 0 && yearsResponse.Success && yearsResponse.Data != null)
        {
            viewModel.YearRates = _mapper.Map<List<ProjectYearRateItem>>(yearsResponse.Data);
        }

        return View(viewModel);
    }

    private static void CalculateYearTotals(YearlyDetailsViewModel viewModel)
    {
        viewModel.StaffCostTotal = viewModel.StaffGrid.Data.Sum(f => f.StaffCost ?? 0);
        viewModel.TestCostTotal = viewModel.TestGrid.Data.Sum(f => f.TestCost ?? 0);
        viewModel.AnimalCostTotal = viewModel.AnimalGrid.Data.Sum(f => f.AnimalCost ?? 0);
        viewModel.AdditionalCostTotal = viewModel.AdditionalCostGrid.Data.Sum(f => f.CostEntered);
    }

    // ── GRID LOADERS ──────────────────────────────────────────────────────

    [HttpPost]
    public async Task<IActionResult> GetYearTotals(string projectId, int year)
    {
        var decodedProjectId = HttpUtility.UrlDecode(projectId);

        var response = await _summaryService.GetProjectYearCostSummaryAsync(decodedProjectId, year);

        if (!response.Success || response.Data is null)
            return Json(new
            {
                staffCostTotal      = 0.0,
                testCostTotal       = 0.0,
                animalCostTotal     = 0.0,
                additionalCostTotal = 0.0,
                grandTotal          = 0.0
            });

        var summary = response.Data;
        return Json(new
        {
            staffCostTotal      = summary.StaffCostTotal,
            testCostTotal       = summary.TestCostTotal,
            animalCostTotal     = summary.AnimalCostTotal,
            additionalCostTotal = summary.AdditionalCostTotal,
            grandTotal          = summary.GrandTotal
        });
    }

    [HttpPost]
    public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var gridConfig = await BuildStaffGridAsync(HttpUtility.UrlDecode(projectId), year, query);

        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadTestGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var gridConfig = await BuildTestGridAsync(HttpUtility.UrlDecode(projectId), year, query);
        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadAnimalGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var gridConfig = await BuildAnimalGridAsync(HttpUtility.UrlDecode(projectId), year, query);
        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadAdditionalCostGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var query = _mapper.Map<QueryParameters<string>>(request);
        var gridConfig = await BuildAdditionalCostGridAsync(HttpUtility.UrlDecode(projectId), year, query);
        return PartialView("_DataGrid", gridConfig);
    }

    [HttpPost]
    public async Task<IActionResult> LoadMarkupAndProfitGrid(PaginationFilter<string> request, string projectId, int year)
    {
        var gridConfig = await BuildMarkupAndProfitGridAsync(HttpUtility.UrlDecode(projectId), year);
        return PartialView("_DataGrid", gridConfig);
    }

    // ── ADD PROJECT YEAR

    [HttpGet]
    [ActionName("AddProjectYear")]
    public IActionResult AddProjectYearGet(string projectId, int year, string? programme = null)
    {
        var model = new ProjectYearRateItem
        {
            Project = projectId,
            YearValue = year,
            Programme = programme
        };
        return PartialView("_AddProjectYear", model);
    }

    [HttpPost]
    public async Task<IActionResult> AddProjectYear(string projectId, int year, ProjectYearRateItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<ProjectYearDto>(item);
        dto.Project = decodedProjectId;
        dto.YearValue = year;
        var response = await _service.AddProjectYearAsync(decodedProjectId, year, dto);
        if (!response.Success)
        {
            if (response.Errors is not null && response.Errors.Count > 0)
                return Json(new { success = false, errors = MapApiErrors(response.Errors) });

            return Json(new { success = false, message = "Failed to add project year." });
        }

        return Json(new { success = true, year = response.Data?.YearValue });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteProjectYear(string projectId, int year)
    {
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var response = await _service.DeleteProjectYearAsync(decodedProjectId, year);
        if (!response.Success)
        {
            var message = response.Errors?.FirstOrDefault()?.Message
                          ?? "Failed to delete project year.";
            return Json(new { success = false, message });
        }
        return Json(new { success = true });
    }

    // ── STAFF CRUD ────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CreateStaff(string projectId, int year, bool isDefra)
    {
        await GetPayRateOptionsAsync(projectId, year,isDefra);
        return PartialView("_AddEditStaffRequirement", new StaffRequirementFormItem { WgGrade = string.Empty });
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff(string projectId, int year, StaffRequirementFormItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<StaffRequirementDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        if (item.StaffCost == null) dto.StaffCost = 0;
        var response = await _service.AddStaffRequirementAsync(decodedProjectId, year, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Staff Record Added Successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> EditStaff(string projectId, int year, int srIdentity, bool isDefra)
    {
        await GetPayRateOptionsAsync(projectId, year, isDefra);

        var query = new QueryParameters<string> { Page = -1, PageSize = int.MaxValue };
        var listResponse = await _service.GetStaffRequirementsAsync(
                               HttpUtility.UrlDecode(projectId), year, query);

        var row = listResponse.Data?.data?.FirstOrDefault(s => s.SrIdentity == srIdentity);
        if (row is null) return NotFound();

        return PartialView("_AddEditStaffRequirement", _mapper.Map<StaffRequirementFormItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditStaff(string projectId, int year, int srIdentity, StaffRequirementFormItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<StaffRequirementDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        dto.SrIdentity = srIdentity;
        if (item.StaffCost == null) dto.StaffCost = 0;
        var response = await _service.UpdateStaffRequirementAsync(decodedProjectId, year, srIdentity, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Staff Record Updated Successfully" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteStaff(string projectId, int year, int srIdentity)
    {
        var response = await _service.DeleteStaffRequirementAsync(HttpUtility.UrlDecode(projectId), year, srIdentity);
        if (!response.Success || !response.Data)
            return Json(new { success = false, message = "Failed to delete Staff record entry." });
        return Json(new { success = true, message = "Staff Record Deleted Successfully" });
    }

    // ── TEST CRUD ─────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CreateTest(string projectId, int year, bool isDefra)
    {
        await GetTestCodeOptionsAsync(projectId, year, isDefra);
        return PartialView("_AddEditTestRequirement", new TestRequirementItem { TestCode = string.Empty });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTest(string projectId, int year, TestRequirementItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        item.NumberOfTests ??= 0;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<TestRequirementDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        if (item.TestCost == null) dto.TestCost = 0;
        var response = await _service.AddTestRequirementAsync(decodedProjectId, year, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Test Record Added Successfully" });        
    }

    [HttpGet]
    public async Task<IActionResult> EditTest(string projectId, int year, string testCode, bool isDefra)
    {
        await GetTestCodeOptionsAsync(projectId, year, isDefra);
        var allQuery = new QueryParameters<string> { Page = -1, PageSize = int.MaxValue };
        var listResponse = await _service.GetTestRequirementsAsync(HttpUtility.UrlDecode(projectId), year, allQuery);
        var row = listResponse.Data?.data?.FirstOrDefault(t => t.TestCode == testCode);
        if (row is null) return NotFound();
        return PartialView("_AddEditTestRequirement", _mapper.Map<TestRequirementItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditTest(string projectId, int year, string testCode, TestRequirementItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        item.NumberOfTests ??= 0;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<TestRequirementDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        dto.TestCode = testCode;
        if (item.TestCost == null) dto.TestCost = 0;
        var response = await _service.UpdateTestRequirementAsync(decodedProjectId, year, testCode, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Test Record Updated Successfully" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteTest(string projectId, int year, string testCode)
    {
        var response = await _service.DeleteTestRequirementAsync(HttpUtility.UrlDecode(projectId), year, testCode);
        if (!response.Success || !response.Data)
            return Json(new { success = false, message = "Failed to delete Test entry." });
        return Json(new { success = true, message = "Test Record Deleted Successfully" });
    }

    // ── ANIMAL CRUD ───────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CreateAnimal(string projectId, int year, bool isDefra)
    {
        await GetAnimalTypeOptionsAsync(projectId, year, isDefra);
        return PartialView("_AddEditAnimalRequirement", new AnimalRequirementItem { AnimalType = string.Empty });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAnimal(string projectId, int year, AnimalRequirementItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        item.NumberOfAnimals ??= 0;
        item.NumberOfDays ??= 0;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<AnimalRequirementDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        if(item.AnimalCost == null) dto.AnimalCost = 0;
        var response = await _service.AddAnimalRequirementAsync(decodedProjectId, year, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Animal Record Added Successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> EditAnimal(string projectId, int year, int arIdentity, bool isDefra)
    {
        await GetAnimalTypeOptionsAsync(projectId, year, isDefra);
        var allQuery = new QueryParameters<string> { Page = -1, PageSize = int.MaxValue };
        var listResponse = await _service.GetAnimalRequirementsAsync(HttpUtility.UrlDecode(projectId), year, allQuery);
        var row = listResponse.Data?.data?.FirstOrDefault(a => a.ArIdentity == arIdentity);
        if (row is null) return NotFound();
        return PartialView("_AddEditAnimalRequirement", _mapper.Map<AnimalRequirementItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditAnimal(string projectId, int year, int arIdentity, AnimalRequirementItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        item.NumberOfAnimals ??= 0;
        item.NumberOfDays ??= 0;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<AnimalRequirementDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        dto.ArIdentity = arIdentity;
        if (item.AnimalCost == null) dto.AnimalCost = 0;
        var response = await _service.UpdateAnimalRequirementAsync(decodedProjectId, year, arIdentity, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Animal Record Updated Successfully" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAnimal(string projectId, int year, int arIdentity)
    {
        var response = await _service.DeleteAnimalRequirementAsync(HttpUtility.UrlDecode(projectId), year, arIdentity);
        if (!response.Success || !response.Data)
            return Json(new { success = false, message = "Failed to delete animal entry." });
        return Json(new { success = true, message = "Animal Record Deleted Successfully" });
    }

    // ── ADDITIONAL COST CRUD ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> CreateAdditionalCost(string projectId, int year)
    {
        await GetAccountCatOptionsAsync();
        return PartialView("_AddEditAdditionalCost",
            new AdditionalCostItem { Description = string.Empty, AccountCat = string.Empty });
    }

    [HttpPost]
    public async Task<IActionResult> CreateAdditionalCost(string projectId, int year, AdditionalCostItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<AdditionalCostDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        if(item.ItemCost==null) dto.ItemCost = 0;
        var response = await _service.AddAdditionalCostAsync(decodedProjectId, year, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Additional Cost Record Added Successfully" });
    }

    [HttpGet]
    public async Task<IActionResult> EditAdditionalCost(string projectId, int year, int acIdentity)
    {
        await GetAccountCatOptionsAsync();
        var allQuery = new QueryParameters<string> { Page = -1, PageSize = int.MaxValue };
        var listResponse = await _service.GetAdditionalCostsAsync(HttpUtility.UrlDecode(projectId), year, allQuery);
        var row = listResponse.Data?.data?.FirstOrDefault(ac => ac.AcIdentity == acIdentity);
        if (row is null) return NotFound();
        return PartialView("_AddEditAdditionalCost", _mapper.Map<AdditionalCostItem>(row));
    }

    [HttpPost]
    public async Task<IActionResult> EditAdditionalCost(string projectId, int year, int acIdentity, AdditionalCostItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        var decodedProjectId = HttpUtility.UrlDecode(projectId);
        var dto = _mapper.Map<AdditionalCostDto>(item);
        dto.Project = decodedProjectId;
        dto.Year = year;
        dto.AcIdentity = acIdentity;
        if (item.ItemCost == null) dto.ItemCost = 0;
        var response = await _service.UpdateAdditionalCostAsync(decodedProjectId, year, acIdentity, dto);
        if (!response.Success)
            return Json(new { success = false, errors = MapApiErrors(response.Errors) });
        return Json(new { success = true, message = "Additional Cost Record Updated Successfully" });
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAdditionalCost(string projectId, int year, int acIdentity)
    {
        var response = await _service.DeleteAdditionalCostAsync(HttpUtility.UrlDecode(projectId), year, acIdentity);
        if (!response.Success || !response.Data)
            return Json(new { success = false, message = "Failed to delete Additional Cost entry." });
        return Json(new { success = true, message = "Additional Cost Record Deleted Successfully" });
      
    }

    // ── MARKUP/PROFIT UPDATE ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EditMarkupAndProfit(string projectId, int year, string? programme)
    {
        var response = await _service.GetProjectYearsAsync(HttpUtility.UrlDecode(projectId));
        var yearDto = response.Data?.FirstOrDefault(y => y.YearValue == year);
        if (yearDto is null) return NotFound();
        var model = _mapper.Map<ProjectYearRateItem>(yearDto);
        model.Programme = programme;
        return PartialView("_AddProjectYear", model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProjectYearRate(string projectId, int year, ProjectYearRateItem item)
    {
        var validationResult = ValidateModel();
        if (validationResult is not null) return validationResult;
        var dto = _mapper.Map<ProjectYearDto>(item);
        var response = await _service.UpdateProjectYearAsync(HttpUtility.UrlDecode(projectId), year, dto);

        if (!response.Success)
        {
            if (response.Errors is not null && response.Errors.Count > 0)
                return Json(new { success = false, errors = MapApiErrors(response.Errors) });

            return Json(new { success = false, message = "Failed to save markup and profit rates." });
        }

        return Json(new { success = true });
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────

    private JsonResult? ValidateModel()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => new { field = e.Key, message = e.Value!.Errors.First().ErrorMessage })
                .ToList();
            return Json(new { success = false, errors });
        }
        return null;
    }

    private static List<object> MapApiErrors(List<ApiErrorDto>? apiErrors)
    {
        if (apiErrors is null || apiErrors.Count == 0)
            return [new { field = string.Empty, message = "An unexpected error occurred." }];

        var errors = new List<object>();
        foreach (var error in apiErrors)
        {
            if (error.Details is Dictionary<string, string[]> validationDetails)
            {
                foreach (var kvp in validationDetails)
                {
                    foreach (var msg in kvp.Value)
                    {
                        errors.Add(new { field = kvp.Key, message = msg });
                    }
                }
            }
            else
            {
                errors.Add(new { field = string.Empty, message = error.Message });
            }
        }

        return errors.Count > 0
            ? errors
            : [new { field = string.Empty, message = "An unexpected error occurred." }];
    }

    private async Task<DataGridConfig<StaffRequirementItem>> BuildStaffGridAsync(
        string projectId, int year, QueryParameters<string>? query = null)
    {
        query ??= new QueryParameters<string>();
        query.Page = -1;

        var response = await _service.GetStaffRequirementsAsync(projectId, year, query);
        var pagedResult = response.Success ? response.Data : null;

        var data = pagedResult?.data != null
            ? _mapper.Map<List<StaffRequirementItem>>(pagedResult.data)
            : new List<StaffRequirementItem>();

        return new DataGridConfig<StaffRequirementItem>
        {
            GridId = "staffGrid",
            Title = "Staff",
            Data = data,
            KeyProperty = nameof(StaffRequirementItem.SrIdentity),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            Pagination = new PaginationModel
            {
                TotalRecords = pagedResult?.TotalCount ?? 0,
                PageNumber = query.Page,
                PageSize = query.PageSize,
                SortColumn = query.SortBy,
                SortDirection = query.Descending
            },
            AddFunction = "gridAddStaff",
            EditFunction = "gridEditStaff",
            DeleteFunction = "gridDeleteStaff",
            ExtraFilterMethod= "allGridExtraFilters",
            BindGridUrl = Url.Action("LoadStaffGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<StaffRequirementItem>(null)
        };
    }

    private async Task<DataGridConfig<TestRequirementItem>> BuildTestGridAsync(string projectId, int year, QueryParameters<string>? query = null)
    {
        query ??= new QueryParameters<string>();
        query.Page = -1;
        var response = await _service.GetTestRequirementsAsync(projectId, year, query);
        var pagedResult = response.Success ? response.Data : null;

        var data = pagedResult?.data != null
            ? _mapper.Map<List<TestRequirementItem>>(pagedResult.data)
            : new List<TestRequirementItem>();

        return new DataGridConfig<TestRequirementItem>
        {
            GridId = "testGrid",
            Title = "Tests",
            Data = data,
            KeyProperty = nameof(TestRequirementItem.TestCode),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            Pagination = new PaginationModel
            {
                TotalRecords = pagedResult?.TotalCount ?? data.Count,
                PageNumber = query.Page,
                PageSize = query.PageSize,
                SortColumn = query.SortBy,
                SortDirection = query.Descending
            },
            AddFunction = "gridAddTest",
            EditFunction = "gridEditTest",
            DeleteFunction = "gridDeleteTest",
            ExtraFilterMethod = "allGridExtraFilters",
            BindGridUrl = Url.Action("LoadTestGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<TestRequirementItem>(null)
        };
    }

    private async Task<DataGridConfig<AnimalRequirementItem>> BuildAnimalGridAsync(
        string projectId, int year, QueryParameters<string>? query = null)
    {
        query ??= new QueryParameters<string>();
        query.Page = -1;

        var response = await _service.GetAnimalRequirementsAsync(projectId, year, query);
        var pagedResult = response.Success ? response.Data : null;

        var data = pagedResult?.data != null
            ? _mapper.Map<List<AnimalRequirementItem>>(pagedResult.data)
            : new List<AnimalRequirementItem>();

        return new DataGridConfig<AnimalRequirementItem>
        {
            GridId = "animalGrid",
            Title = "Animals",
            Data = data,
            KeyProperty = nameof(AnimalRequirementItem.ArIdentity),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            Pagination = new PaginationModel
            {
                TotalRecords = pagedResult?.TotalCount ?? 0,
                PageNumber = query.Page,
                PageSize = query.PageSize,
                SortColumn = query.SortBy,
                SortDirection = query.Descending
            },
            AddFunction = "gridAddAnimal",
            EditFunction = "gridEditAnimal",
            DeleteFunction = "gridDeleteAnimal",
            ExtraFilterMethod = "allGridExtraFilters",
            BindGridUrl = Url.Action("LoadAnimalGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<AnimalRequirementItem>(null)
        };
    }

    private async Task<DataGridConfig<AdditionalCostItem>> BuildAdditionalCostGridAsync(
        string projectId, int year, QueryParameters<string>? query = null)
    {
        query ??= new QueryParameters<string>();
        query.Page = -1;

        var response = await _service.GetAdditionalCostsAsync(projectId, year, query);
        var pagedResult = response.Success ? response.Data : null;

        var data = pagedResult?.data != null
            ? _mapper.Map<List<AdditionalCostItem>>(pagedResult.data)
            : new List<AdditionalCostItem>();

        return new DataGridConfig<AdditionalCostItem>
        {
            GridId = "additionalCostGrid",
            Title = "Additional Costs",
            Data = data,
            KeyProperty = nameof(AdditionalCostItem.AcIdentity),
            AllowAdd = true,
            AllowEdit = true,
            AllowDelete = true,
            AllowCopy = false,
            ShowPagination = false,
            Pagination = new PaginationModel
            {
                TotalRecords = pagedResult?.TotalCount ?? 0,
                PageNumber = query.Page,
                PageSize = query.PageSize,
                SortColumn = query.SortBy,
                SortDirection = query.Descending
            },
            AddFunction = "gridAddAdditionalCost",
            EditFunction = "gridEditAdditionalCost",
            DeleteFunction = "gridDeleteAdditionalCost",
            ExtraFilterMethod = "allGridExtraFilters",
            BindGridUrl = Url.Action("LoadAdditionalCostGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<AdditionalCostItem>(null)
        };
    }

    private async Task<DataGridConfig<ProjectYearRateItem>> BuildMarkupAndProfitGridAsync(string projectId, int year)
    {
        var response = await _service.GetProjectYearsAsync(projectId);
        var data = response.Success && response.Data != null
            ? _mapper.Map<List<ProjectYearRateItem>>(
                  response.Data.ToList())
            : new List<ProjectYearRateItem>();

        return new DataGridConfig<ProjectYearRateItem>
        {
            GridId = "markupAndProfitGrid",
            Title = "Markup and Profit",
            Data = data,
            KeyProperty = nameof(ProjectYearRateItem.YearValue),
            AllowAdd = false,
            AllowEdit = true,
            AllowDelete = false,
            AllowCopy = false,
            ShowPagination = false,
            Pagination = new PaginationModel
            {
                TotalRecords = data.Count,
                PageNumber = 1,
                //PageSize = data.Count > 0 ? data.Count : 10
            },
            EditFunction = "gridEditMarkupAndProfit",
            BindGridUrl = Url.Action("LoadMarkupAndProfitGrid", new { projectId, year }) ?? string.Empty,
            Columns = GridDataProvider.GetColumnsDefination<ProjectYearRateItem>(null),
            ColumnGroups =
                [
                    new() { Label = "",                        Span = 1 },
                    new() { Label = "Contingency Markup %",      Span = 4 },
                    new() { Label = "Profit Margin %",    Span = 4 },

                ]
        };
    }

    private async Task PopulateDropdownsAsync(YearlyDetailsViewModel viewModel, bool isDefra)
    {       

        var accountCats = await _service.GetAccountCategoriesAsync();
        viewModel.AccountCatOptions = accountCats.Success && accountCats.Data != null
            ? accountCats.Data.Select(c => new SelectListItem(c.AccShortName, c.AccShortName)).ToList()
            : new List<SelectListItem>();
    }

    private async Task GetPayRateOptionsAsync(string projectId, int year,bool isDefra)
    {
        var response = await _service.GetPayRatesAsync(projectId, year, isDefra);
        ViewBag.WgGradeOptions = response.Data;
    }

    private async Task GetAnimalTypeOptionsAsync(string projectId, int year, bool isDefra)
    {
        var response = await _service.GetAnimalRatesAsync(projectId, year, isDefra);
        ViewBag.AnimalTypeOptions = response.Success && response.Data != null
            ? response.Data
            : new List<AnimalRateDto>();
    }

    private async Task GetAccountCatOptionsAsync()
    {
        var response = await _service.GetAccountCategoriesAsync();
        ViewBag.AccountCatOptions = response.Success && response.Data != null
            ? response.Data
            : new List<AccountCategoryDto>();
    }

    private async Task GetTestCodeOptionsAsync(string projectId, int year, bool isDefra)
    {
        var response = await _service.GetTestCodeLookupsAsync(projectId, year, isDefra);
        if (!response.Success || response.Data is null)
        {
            ViewBag.TestCodeOptions = new List<TestCodeLookupDto>();
            return;
        }
        ViewBag.TestCodeOptions = response.Data;
    }
}
