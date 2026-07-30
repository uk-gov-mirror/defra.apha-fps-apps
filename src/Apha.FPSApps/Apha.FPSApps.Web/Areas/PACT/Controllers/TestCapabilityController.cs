using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class TestCapabilityController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestCapabilityService _service;
        private readonly ITestRequirementService _testReqmtService;
        private readonly IProjectService _projectService;
        private readonly IExcelExportService _excelExportService;
        private readonly ITestorProductService _testorProductService;
        public TestCapabilityController(
            IMapper mapper,
            ITestCapabilityService service,
            ITestRequirementService testReqmtService,
            IProjectService projectService,
            IExcelExportService excelExportService,
            ITestorProductService testorProductService
            )
        {
            _mapper = mapper;
            _service = service;
            _testReqmtService = testReqmtService;
            _projectService = projectService;
            _excelExportService = excelExportService;
            _testorProductService = testorProductService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var testCapabilityGrid = await BuildTestCapabilityGridAsync(defaultRequest, viewBy: 1, filterValue: null);
            var testReqmtGrid = BuildEmptyTestReqmtGrid();

            var workGroupsResponse = await _service.GetAllWorkGroupsAsync();
            var testsResponse = await _testorProductService.GetAllTestorProductsAsync();

            var viewModel = new TestCapabilityViewModel
            {
                TestCapabilityGrid = testCapabilityGrid,
                TestReqmtGrid = testReqmtGrid,
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? workGroupsResponse.Data
                        .Select(w => new SelectListItem(w.ProfitCentre, w.WorkGroupName))
                        .ToList()
                    : new List<SelectListItem>(),
                TestorProductOptions = testsResponse.Success && testsResponse.Data != null
                    ? testsResponse.Data
                        .Select(t => new SelectListItem(
                            string.IsNullOrWhiteSpace(t.ItemDescription)
                                ? t.ItemCode
                                : $"{t.ItemDescription}",
                            t.ItemCode))
                        .ToList()
                    : new List<SelectListItem>(),
                SelectedWorkGroup= Request.Query["workgroup"].ToString()
            };

            return View(viewModel);
        }

        // ── GRID 1: TEST CAPABILITY ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestCapabilityGrid(
            PaginationFilter<string> request, int viewBy, string? filterValue)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestCapabilityGridAsync(request, viewBy, filterValue);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── GRID 2: TEST REQMT ────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestReqmtGrid(
            PaginationFilter<string> request, string testCode)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestReqmtGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── TEST CAPABILITY CRUD ──────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTestCapability()
        {
            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.Projects = await GetProjectsAsync();
            return PartialView("_AddEditTestCapability", new TestCapabilityItem());
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestCapability([FromBody] TestCapabilityItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _service.CreateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Capability created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create Test Capability.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestCapability(string testCode, string workGroup)
        {
            var result = await _service.GetTestCapabilityByIdAsync(testCode, workGroup);
            if (!result.Success)
                return NotFound($"Test Capability with TestCode '{testCode}' and WorkGroup '{workGroup}' not found.");

            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            ViewBag.Projects = await GetProjectsAsync();
            var item = _mapper.Map<TestCapabilityItem>(result.Data);
            return PartialView("_AddEditTestCapability", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestCapability([FromBody] TestCapabilityItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _service.UpdateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Capability updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update Test Capability.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestCapability(string testCode, string workGroup)
        {
            var result = await _service.DeleteTestCapabilityAsync(testCode, workGroup);
            return result.Success
                ? Json(new { success = true, message = "Test Capability deleted successfully" })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Test Capability.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── TEST REQMT CRUD ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTestReqmt(string testCode)
        {
            ViewBag.Projects = await GetProjectsAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();

            var model = new TestRequirementItem { TestCode = testCode, Active = 1, NoRequired = 0 };

            if (!string.IsNullOrWhiteSpace(testCode))
            {
                var pricing = await _testReqmtService.GetTestReqmtPricingAsync(testCode, null);
                if (pricing.Success && pricing.Data is not null)
                {
                    model.RecUnitPrice = pricing.Data.RecUnitPrice;
                    model.UnitPrice = pricing.Data.RecUnitPrice;
                }
            }

            return PartialView("_AddEditTestReqmt", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateTestReqmt([FromBody] TestRequirementItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestRequirementDto>(model);
            var result = await _testReqmtService.CreateTestReqmtAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Requirement created successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditTestReqmt(string testCode, string buyer)
        {
            var result = await _testReqmtService.GetTestReqmtByIdAsync(testCode, buyer);
            if (!result.Success)
                return NotFound($"Test Requirement with TestCode '{testCode}' and Buyer '{buyer}' not found.");

            ViewBag.Projects = await GetProjectsAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            var item = _mapper.Map<TestRequirementItem>(result.Data);
            return PartialView("_AddEditTestReqmt", item);
        }

        [HttpPost]
        public async Task<IActionResult> EditTestReqmt([FromBody] TestRequirementItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestRequirementDto>(model);
            var result = await _testReqmtService.UpdateTestReqmtAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Test Requirement updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTestReqmt(string testCode, string buyer)
        {
            var result = await _testReqmtService.DeleteTestReqmtAsync(testCode, buyer);
            return result.Success
                ? Json(new { success = true, message = "Test Requirement deleted successfully" })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete Test Requirement.",
                    errors = (result.Errors ?? new List<ApiErrorDto>())
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> ExportTestReqmt(string testCode, string? filter = null)
        {
            var response = await _testReqmtService.GetAllTestReqmtForExportAsync(testCode, filter);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestRequirementItem>>(response.Data)
                : new List<TestRequirementItem>();

            var bytes = _excelExportService.ExportToExcel(items, "Test Requirements");
            var fileName = $"TestRequirements_{testCode}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> GetTestReqmtPricing(string testCode, string? projectCode = null)
        {
            if (string.IsNullOrWhiteSpace(testCode))
                return Json(new { success = false });

            var result = await _testReqmtService.GetTestReqmtPricingAsync(testCode, projectCode);
            if (!result.Success || result.Data is null)
                return Json(new { success = false });

            return Json(new
            {
                success = true,
                recUnitPrice = result.Data.RecUnitPrice,
                isDefraProject = string.IsNullOrWhiteSpace(projectCode) ? (short?)null : result.Data.IsDefraProject
            });
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        private async Task<DataGridConfig<TestCapabilityItem>> BuildTestCapabilityGridAsync(
            PaginationFilter<string> request, int viewBy, string? filterValue)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);

            var response = viewBy == 2
                ? await _service.GetPagedByTestCodeAsync(query, filterValue)
                : await _service.GetPagedByWorkGroupAsync(query, filterValue);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestCapabilityItem>>(response.Data)
                : new List<TestCapabilityItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestCapabilityItem>
            {
                GridId = "testCapabilityGrid",
                Title = "Test Capabilities",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "TestCode",
                AddFunction = "addTestCapability",
                EditFunction = "editTestCapability",
                DeleteFunction = "deleteTestCapability",
                RowSelectFunction = "onTestCapabilityRowSelect",
                ExtraFilterMethod = "getTestCapabilityExtraFilters",
                BindGridUrl = "/PACT/TestCapability/LoadTestCapabilityGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<TestRequirementItem>> BuildTestReqmtGridAsync(
            PaginationFilter<string> request, string testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testReqmtService.GetPagedTestReqmtAsync(query, testCode);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestRequirementItem>>(response.Data)
                : new List<TestRequirementItem>();

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRequirementItem>
            {
                GridId = "testReqmtGrid",
                Title = "Test Requirement Records for Test",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Buyer",
                AllowExport = true,
                ExportUrl = "/PACT/TestCapability/ExportTestReqmt",
                AddFunction = "addTestReqmt",
                EditFunction = "editTestReqmt",
                DeleteFunction = "deleteTestReqmt",
                ExtraFilterMethod = "getTestReqmtExtraFilters",
                BindGridUrl = "/PACT/TestCapability/LoadTestReqmtGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TestRequirementItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        private static DataGridConfig<TestRequirementItem> BuildEmptyTestReqmtGrid()
        {
            return new DataGridConfig<TestRequirementItem>
            {
                GridId = "testReqmtGrid",
                Title = "Test Requirement Records for Test",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Buyer",
                AllowExport = true,
                ExportUrl = "/PACT/TestCapability/ExportTestReqmt",
                AddFunction = "addTestReqmt",
                EditFunction = "editTestReqmt",
                DeleteFunction = "deleteTestReqmt",
                ExtraFilterMethod = "getTestReqmtExtraFilters",
                BindGridUrl = "/PACT/TestCapability/LoadTestReqmtGrid",
                Data = new List<TestRequirementItem>(),
                Columns = GridDataProvider.GetColumnsDefination<TestRequirementItem>(null),
                Pagination = new PaginationModel()
            };
        }

        private async Task<List<SelectListItem>> GetWorkGroupSelectListAsync()
        {
            var response = await _service.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                    .ToList()
                : new List<SelectListItem>();
        }

        private async Task<List<ProjectDto>> GetProjectsAsync()
        {
            var response = await _projectService.GetAllProjectsAsync();
            return response.Success && response.Data != null
                ? response.Data
                : new List<ProjectDto>();
        }

        private async Task<List<SelectListItem>> GetTestorProductSelectListAsync()
        {
            var response = await _testorProductService.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data
                    .Select(t => new SelectListItem(
                        string.IsNullOrWhiteSpace(t.ItemDescription)
                            ? t.ItemCode
                            : $"{t.ItemCode}",
                        t.ItemCode))
                    .ToList()
                : new List<SelectListItem>();
        }
    }
}
