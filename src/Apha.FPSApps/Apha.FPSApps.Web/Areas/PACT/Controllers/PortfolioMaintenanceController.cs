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
    public class PortfolioMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly ITestorProductService _testorProductService;
        private readonly IPactTimeCodeValidService _timeCodeService;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;

        public PortfolioMaintenanceController(
            IMapper mapper,
            IProjectService projectService,
            ITestCapabilityService testCapabilityService,
            ITestorProductService testorProductService,
            IPactTimeCodeValidService timeCodeService,
            IProgramService programService,
            IEmployeeService employeeService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _testCapabilityService = testCapabilityService;
            _testorProductService = testorProductService;
            _timeCodeService = timeCodeService;
            _programService = programService;
            _employeeService = employeeService;
        }

        // ── INDEX — single page with portfolio picker ─────────────────────────

        public async Task<IActionResult> Index(string? portfolio, string? workgroup)
        {
            TempData["PactOrigin"] = "PortfolioMaintenance";

            // Store parameters in ViewBag for the view to use
            ViewBag.SelectedPortfolio = portfolio;
            ViewBag.SourceWorkGroup = workgroup;

            var allPortfolios = await _projectService.GetAllPactProjectsAsync();
            var programs = await _programService.GetAllProgramsAsync();
            var managers = await _employeeService.GetAllPactManagersAsync();
            var workGroups = await _testCapabilityService.GetAllWorkGroupsAsync();
            var testorProducts = await _testorProductService.GetAllTestorProductsAsync();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };

            var viewModel = new PortfolioMaintenanceViewModel
            {
                PortfolioOptions = allPortfolios.Data?
                    .Select(p => new SelectListItem(
                        $"{p.ParentProject} - {p.ProjectTitle}", p.ParentProject))
                    .ToList() ?? [],
                Programs = programs.Data?
                    .Select(p => new SelectListItem(p.ProgramName ?? p.ProgramNo, p.ProgramNo))
                    .ToList() ?? [],
                Managers = managers.Data?
                    .Select(m => new SelectListItem(m.Name, m.Name))
                    .ToList() ?? [],
                WorkGroups = workGroups.Data?
                    .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                    .ToList() ?? [],
                TestorProducts = testorProducts.Data?
                    .Select(t => new SelectListItem(t.ItemDescription, t.ItemCode))
                    .ToList() ?? [],
                ConstituentTestGrid = await BuildConstituentTestGridAsync(defaultRequest, null),
                TimeCodeGrid = await BuildPortfolioTimeCodeGridAsync(defaultRequest, null)
            };

            return View(viewModel);
        }

        // ── CONSTITUENT TESTS GRID ────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadConstituentTestGrid(PaginationFilter<string> request, string parentProject)
        {
            if (string.IsNullOrEmpty(parentProject))
                return BadRequest("Parent project is required");

            var gridConfig = await BuildConstituentTestGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ConstituentTestItem>> BuildConstituentTestGridAsync(
            PaginationFilter<string> request, string? parentProject)
        {
            List<ConstituentTestItem> items = [];
            PaginationModel pagination;

            if (!string.IsNullOrEmpty(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(query, parentProject);
                items = response.Data != null ? _mapper.Map<List<ConstituentTestItem>>(response.Data) : [];
                pagination = response.Pagination != null
                    ? _mapper.Map<PaginationModel>(response.Pagination)
                    : new PaginationModel();
            }
            else
            {
                pagination = new PaginationModel();
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];

            return new DataGridConfig<ConstituentTestItem>
            {
                GridId = "constituentTestGrid",
                Title = "Constituent Tests",
                AllowAdd = true,
                AllowEdit = false,
                AllowDelete = true,
                AllowRowSelection = true,
                KeyProperty = "TestCode",
                AddFunction = "addConstituentTest",
                DeleteFunction = "deleteConstituentTest",
                RowSelectFunction = "selectConstituentTest",
                BindGridUrl = string.IsNullOrEmpty(parentProject)
                    ? "/PACT/PortfolioMaintenance/LoadConstituentTestGrid"
                    : $"/PACT/PortfolioMaintenance/LoadConstituentTestGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ConstituentTestItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        // ── GET PORTFOLIO BY ID (AJAX) ────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetPortfolio(string parentProject)
        {
            var result = await _projectService.GetProjectByIdAsync(parentProject);
            if (!result.Success || result.Data == null)
                return Json(new { success = false, message = "Portfolio not found." });

            var vm = _mapper.Map<PactProjectViewModel>(result.Data);
            return Json(new { success = true, data = vm });
        }

        // ── SAVE PORTFOLIO DETAILS ────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] PortfolioDetailModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new
                        {
                            field = "CurrentPortfolio." + x.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = new ProjectDto
            {
                ParentProject = model.ParentProject ?? string.Empty,
                ProjectTitle = model.ProjectTitle ?? string.Empty,
                Finished = model.Finished ? (short)-1 : (short)0,
                Program = model.Program,
                Manager = model.ProjectManager,
                BudgetCvl = model.BudgetCvl,
                TransferIncome = model.TransferIncome ?? 0,
                Comments = model.Comments
            };

            var result = await _projectService.UpdatePactPortfolioAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Portfolio updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update portfolio.",
                    errors = (result.Errors ?? [])
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── CONSTITUENT TEST CRUD ─────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateConstituentTest(string parentProject)
        {
            ViewBag.WorkGroupOptions = await GetWorkGroupSelectListAsync();
            ViewBag.TestorProductOptions = await GetTestorProductSelectListAsync();
            return PartialView("_AddConstituentTest", new ConstituentTestItem { PlanPortfolio = parentProject });
        }

        [HttpPost]
        public async Task<IActionResult> CreateConstituentTest([FromBody] ConstituentTestItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TestCapabilityDto>(model);
            var result = await _testCapabilityService.CreateTestCapabilityAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Constituent test added successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to add constituent test.",
                    errors = (result.Errors ?? [])
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteConstituentTest(string testCode, string workGroup)
        {
            var result = await _testCapabilityService.DeleteTestCapabilityAsync(testCode, workGroup);
            return result.Success
                ? Json(new { success = true, message = "Constituent test deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete constituent test.",
                    errors = (result.Errors ?? [])
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── TIME CODE VALIDITY GRID ───────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTimeCodeGrid(PaginationFilter<string> request, string parentProject, string? testCode)
        {
            if (string.IsNullOrEmpty(parentProject))
                return BadRequest("Parent project is required");

            var gridConfig = await BuildPortfolioTimeCodeGridAsync(request, parentProject, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<PortfolioTimeCodeViewModel>> BuildPortfolioTimeCodeGridAsync(
            PaginationFilter<string> request, string? parentProject, string? testCode = null)
        {
            List<PortfolioTimeCodeViewModel> items = [];
            PaginationModel pagination;

            if (!string.IsNullOrEmpty(parentProject) && !string.IsNullOrEmpty(testCode))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _timeCodeService.GetPagedByProjectAndTestCodeAsync(query, parentProject, testCode);
                items = response.Data != null ? _mapper.Map<List<PortfolioTimeCodeViewModel>>(response.Data) : [];

                // Fill in Portfolio from context where the DB holds nulls
                foreach (var item in items)
                {
                    if (string.IsNullOrEmpty(item.Portfolio))
                        item.Portfolio = parentProject;
                }

                pagination = response.Pagination != null
                    ? _mapper.Map<PaginationModel>(response.Pagination)
                    : new PaginationModel();
            }
            else
            {
                pagination = new PaginationModel();
            }

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];

            return new DataGridConfig<PortfolioTimeCodeViewModel>
            {
                GridId = "portfolioTimeCodeGrid",
                Title = "Work Groups who can record Time to this Portfolio Test",
                KeyProperty = "TimeCode",
                ShowCheckboxColumn = false,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addPortfolioTimeCode",
                EditFunction = "editPortfolioTimeCode",
                DeleteFunction = "deletePortfolioTimeCode",
                BindGridUrl = "/PACT/PortfolioMaintenance/LoadTimeCodeGrid",
                ExtraFilterMethod = "getTimeCodeGridExtraFilters",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PortfolioTimeCodeViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        // ── TIME CODE CRUD ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreatePortfolioTimeCode(string parentProject, string? selectedTestCode, string? selectedPortfolio)
        {
            ViewBag.WorkGroups = await GetWorkGroupSelectListAsync();
            ViewBag.ProjectOptions = await GetProjectSelectListAsync();
            return PartialView("_AddEditPortfolioTimeCode", new PortfolioTimeCodeViewModel
            {
                ParentProject = parentProject,
                TestCode = selectedTestCode,
                Portfolio = selectedPortfolio
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePortfolioTimeCode([FromBody] PortfolioTimeCodeViewModel model)
        {
            if (!model.Active)
                ModelState.AddModelError(
                       nameof(model.Active),
                       "The time code must be active.");

            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TimeCodeValidDto>(model);
            var result = await _timeCodeService.CreateTimeCodeValidAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Work group added successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to add work group.",
                    errors = (result.Errors ?? [])
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpGet]
        public async Task<IActionResult> EditPortfolioTimeCode(string workGroup, string timeCode, string parentProject, string? selectedTestCode)
        {
            ViewBag.WorkGroups = await GetWorkGroupSelectListAsync();
            ViewBag.ProjectOptions = await GetProjectSelectListAsync();

            var existing = await _timeCodeService.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);
            var data = existing.Success ? existing.Data : null;

            return PartialView("_AddEditPortfolioTimeCode", new PortfolioTimeCodeViewModel
            {
                WorkGroup = workGroup,
                TimeCode = timeCode,
                ParentProject = parentProject,
                Portfolio = data?.Portfolio,
                Active = data?.Active ?? false,
                IsEdit = true,
                TestCode = selectedTestCode
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditPortfolioTimeCode([FromBody] PortfolioTimeCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any() && kvp.Key != "$")
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TimeCodeValidDto>(model);
            var result = await _timeCodeService.UpdateTimeCodeValidAsync(dto);

            return result.Success
                ? Json(new { success = true, message = "Work group updated successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update work group.",
                    errors = (result.Errors ?? [])
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePortfolioTimeCode(string workGroup, string timeCode, string parentProject)
        {
            var result = await _timeCodeService.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
            return result.Success
                ? Json(new { success = true, message = "Work group deleted successfully." })
                : Json(new
                {
                    success = false,
                    message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete work group.",
                    errors = (result.Errors ?? [])
                        .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
                });
        }

        // ── HELPERS ───────────────────────────────────────────────────────────

        private async Task<List<SelectListItem>> GetWorkGroupSelectListAsync()
        {
            var response = await _testCapabilityService.GetAllWorkGroupsAsync();
            return response.Success && response.Data != null
                ? response.Data.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList()
                : [];
        }

        private async Task<List<SelectListItem>> GetTestorProductSelectListAsync()
        {
            var response = await _testorProductService.GetAllTestorProductsAsync();
            return response.Success && response.Data != null
                ? response.Data.Select(t => new SelectListItem(t.ItemDescription ?? t.ItemCode, t.ItemCode)).ToList()
                : [];
        }

        private async Task<List<SelectListItem>> GetProjectSelectListAsync()
        {
            var response = await _projectService.GetAllPactProjectsAsync();
            return response.Success && response.Data != null
                ? response.Data.Select(p => new SelectListItem(
                    $"{p.ParentProject} - {p.ProjectTitle}", p.ParentProject)).ToList()
                : [];
        }
    }
}
