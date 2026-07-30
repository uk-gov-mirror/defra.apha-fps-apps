using Apha.FPSApps.Application.Dtos;
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
    public class PortfolioTimeCodesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IPactTimeCodeValidService _timeCodeService;

        public PortfolioTimeCodesController(
            IMapper mapper,
            IProjectService projectService,
            IProjectJobCodeService jobCodeService,
            IPactTimeCodeValidService timeCodeService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _jobCodeService = jobCodeService;
            _timeCodeService = timeCodeService;
        }

        // ── INDEX ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index(string? parentProject)
        {
            var portfolios = await _projectService.GetAllPactProjectsAsync();
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();

            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}",
                Page = 1,
                PageSize = 10,
                SortBy = "",
                Descending = false
            };

            var jobCodeGrid = string.IsNullOrEmpty(parentProject)
                ? BuildEmptyJobCodeGrid()
                : await BuildJobCodeGridAsync(defaultRequest, parentProject);

            var timeCodeGrid = string.IsNullOrEmpty(parentProject)
                ? BuildEmptyTimeCodeGrid()
                : await BuildTimeCodeTestCodeGridAsync(defaultRequest, parentProject, null, null);

            var viewModel = new PortfolioTimeCodesViewModel
            {
                SelectedPortfolio = parentProject,
                PortfolioOptions = portfolios.Data?
                    .Select(p => new SelectListItem($"{p.ParentProject} - {p.ProjectTitle}", p.ParentProject))
                    .ToList() ?? [],
                WorkGroups = workGroups.Data?
                    .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                    .ToList() ?? [],
                JobCodeGrid = jobCodeGrid,
                TimeCodeGrid = timeCodeGrid
            };

            return View(viewModel);
        }

        // ── JOB CODE GRID ────────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadJobCodeGrid(PaginationFilter<string> request, string parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(parentProject))
                return BadRequest("Parent project is required");

            var gridConfig = await BuildJobCodeGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<PortfolioJobCodeViewModel>> BuildJobCodeGridAsync(
            PaginationFilter<string> request, string parentProject)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _jobCodeService.GetPagedJobCodesAsync(query, parentProject);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<PortfolioJobCodeViewModel>>(response.Data)
                : new List<PortfolioJobCodeViewModel>();

            var pagination = response.Success && response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            return new DataGridConfig<PortfolioJobCodeViewModel>
            {
                GridId = "jobCodeGrid",
                Title = "Project Job Codes",
                ShowPagination = true,
                AllowRowSelection = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                KeyProperty = "JobCodeId",
                AddFunction = "addJobCode",
                EditFunction = "editJobCode",
                DeleteFunction = "deleteJobCode",
                RowSelectFunction = "selectJobCode",
                BindGridUrl = $"/PACT/PortfolioTimeCodes/LoadJobCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PortfolioJobCodeViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private static DataGridConfig<PortfolioJobCodeViewModel> BuildEmptyJobCodeGrid()
        {
            return new DataGridConfig<PortfolioJobCodeViewModel>
            {
                GridId = "jobCodeGrid",
                Title = "Project Job Codes",
                ShowPagination = true,
                KeyProperty = "JobCodeId",
                AllowRowSelection = true,
                AddFunction = "addJobCode",
                EditFunction = "editJobCode",
                DeleteFunction = "deleteJobCode",
                RowSelectFunction = "selectJobCode",
                BindGridUrl = "/PACT/PortfolioTimeCodes/LoadJobCodeGrid",
                Data = [],
                Columns = GridDataProvider.GetColumnsDefination<PortfolioJobCodeViewModel>()
            };
        }

        // ── TIME CODE GRID ───────────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTimeCodeGrid(
            [FromForm] PaginationFilter<string> request, 
            [FromQuery] string parentProject, 
            [FromForm] string? jobCodeId, 
            [FromForm] string? testCode)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(parentProject))
                return BadRequest("Parent project is required");

            var gridConfig = await BuildTimeCodeTestCodeGridAsync(request, parentProject, jobCodeId, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ValidTimeCodeViewModel>> BuildTimeCodeTestCodeGridAsync(
            PaginationFilter<string> request, string parentProject, string? jobCodeId, string? testCodeId)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);

            // Option 1: Show ALL time codes for the project (regardless of Job Code selection)
            // Pass null for jobCode to not filter by it
            //var response = await _timeCodeService.GetPagedTimeCodesTestCodeAsync(query, null, testCodeId, parentProject);
            var response = await _timeCodeService.GetPagedTimeCodesAsync(query, null, parentProject);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ValidTimeCodeViewModel>>(response.Data)
                : new List<ValidTimeCodeViewModel>();

            var pagination = response.Success && response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            // Option 1: Always show all time codes for the project
            var title = $"Time Code Validity for Project: {parentProject}";

            return new DataGridConfig<ValidTimeCodeViewModel>
            {
                GridId = "timeCodeGrid",
                Title = title,
                KeyProperty = "TimeCode",
                ShowPagination = true,
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addTimeCode",
                EditFunction = "editTimeCode",
                DeleteFunction = "deleteTimeCode",
                ExtraFilterMethod = "getTimeCodeExtraFilters",
                BindGridUrl = $"/PACT/PortfolioTimeCodes/LoadTimeCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ValidTimeCodeViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private static DataGridConfig<ValidTimeCodeViewModel> BuildEmptyTimeCodeGrid()
        {
            return new DataGridConfig<ValidTimeCodeViewModel>
            {
                GridId = "timeCodeGrid",
                Title = "Time Code Validity",
                ShowPagination = true,
                KeyProperty = "TimeCode",
                AllowAdd = true,
                AllowEdit = true,
                AllowDelete = true,
                AddFunction = "addTimeCode",
                EditFunction = "editTimeCode",
                DeleteFunction = "deleteTimeCode",
                ExtraFilterMethod = "getTimeCodeExtraFilters",
                BindGridUrl = "/PACT/PortfolioTimeCodes/LoadTimeCodeGrid",
                Data = [],
                Columns = GridDataProvider.GetColumnsDefination<ValidTimeCodeViewModel>()
            };
        }

        // ── JOB CODE CRUD ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateJobCode(string parentProject)
        {
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            var types = await _jobCodeService.GetTypesAsync();
            var projects = await _projectService.GetAllPactProjectsAsync();

            ViewBag.WorkGroupsData = workGroups.Data?.Select(w => new { Value = w.WorkGroupName, Text = (string.IsNullOrEmpty(w.ProfitCentre) ? "" : w.ProfitCentre) }).ToList() ?? [];
            ViewBag.Types = types.Data?.Select(t => new SelectListItem(t, t)).ToList() ?? [];
            ViewBag.Projects = projects.Data?.Select(p => new SelectListItem(p.ParentProject, p.ParentProject)).ToList() ?? [];

            return PartialView("_AddEditJobCode", new PortfolioJobCodeViewModel { ParentProject = parentProject });
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobCode([FromBody] PortfolioJobCodeViewModel model)
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

            var dto = _mapper.Map<JobCodeDto>(model);
            var result = await _jobCodeService.CreateJobCodeAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to create project job code.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditJobCode(string jobCodeId)
        {
            var result = await _jobCodeService.GetJobCodeByIdAsync(jobCodeId);
            if (!result.Success || result.Data == null) return NotFound();

            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            var types = await _jobCodeService.GetTypesAsync();
            var projects = await _projectService.GetAllPactProjectsAsync();

            ViewBag.WorkGroupsData = workGroups.Data?.Select(w => new { Value = w.WorkGroupName, Text = (string.IsNullOrEmpty(w.ProfitCentre) ? "" : w.ProfitCentre) }).ToList() ?? [];
            ViewBag.Types = types.Data?.Select(t => new SelectListItem(t, t)).ToList() ?? [];
            ViewBag.Projects = projects.Data?.Select(p => new SelectListItem(p.ParentProject, p.ParentProject)).ToList() ?? [];

            return PartialView("_AddEditJobCode", _mapper.Map<PortfolioJobCodeViewModel>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> EditJobCode([FromBody] PortfolioJobCodeViewModel model)
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

            var dto = _mapper.Map<JobCodeDto>(model);
            var result = await _jobCodeService.UpdateJobCodeAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update project job code.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteJobCode(string jobCodeId, string parentProject)
        {
            var result = await _jobCodeService.DeleteJobCodeAsync(jobCodeId);

            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        // ── TIME CODE CRUD ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTimeCode(string parentProject, string jobCodeId)
        {
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            var projects = await _projectService.GetAllPactProjectsAsync();

            ViewBag.WorkGroups = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
            ViewBag.Projects = projects.Data?.Select(p => new SelectListItem(p.ParentProject, p.ParentProject)).ToList() ?? [];

            // Option 1: Don't pre-populate JobCode - let user choose between JobCode or Portfolio/TestCode
            return PartialView("_AddEditTimeCode", new ValidTimeCodeViewModel
            {
                ParentProject = parentProject,
                JobCode = null,  // Leave empty to allow user choice
                TimeCode = string.Empty,
                Project = parentProject  // Set Project to parent project value
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimeCode([FromBody] ValidTimeCodeViewModel model)
        {
            // Business Rule: Enforce mutual exclusivity
            // If JobCode has value, clear Portfolio and TestCode
            // If Portfolio or TestCode has value, clear JobCode
            if (!string.IsNullOrWhiteSpace(model.JobCode))
            {
                model.Portfolio = null;
                model.TestCode = null;
            }
            else if (!string.IsNullOrWhiteSpace(model.Portfolio) || !string.IsNullOrWhiteSpace(model.TestCode))
            {
                model.JobCode = null;
            }

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
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });

            var dto = _mapper.Map<TimeCodeValidDto>(model);
            var result = await _timeCodeService.CreateTimeCodeValidAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to create time code.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditTimeCode(string? workGroup, string timeCode, string? jobCodeId, string parentProject)
        {
            try
            {
                if (string.IsNullOrEmpty(timeCode))
                    return BadRequest("Time code is required");

                if (string.IsNullOrEmpty(parentProject))
                    return BadRequest("Parent project is required");

                if (string.IsNullOrEmpty(workGroup))
                    return BadRequest("Work group is required");

                var queryParams = new QueryParameters<string>
                {
                    Page = 1,
                    PageSize = 10000,
                    Filter = null
                };

                var result = await _timeCodeService.GetPagedTimeCodesAsync(queryParams, null, parentProject);

                if (!result.Success || result.Data == null || !result.Data.Any())
                    return NotFound($"No time codes found for project '{parentProject}'");

                var item = result.Data.FirstOrDefault(t => 
                    string.Equals(t.WorkGroup?.Trim(), workGroup?.Trim(), StringComparison.OrdinalIgnoreCase) && 
                    string.Equals(t.TimeCode?.Trim(), timeCode?.Trim(), StringComparison.OrdinalIgnoreCase));

                if (item == null)
                    return NotFound($"Time code '{timeCode}' with work group '{workGroup}' not found in project '{parentProject}'");

                var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
                var projects = await _projectService.GetAllPactProjectsAsync();

                ViewBag.WorkGroups = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
                ViewBag.Projects = projects.Data?.Select(p => new SelectListItem(p.ParentProject, p.ParentProject)).ToList() ?? [];

                var model = _mapper.Map<ValidTimeCodeViewModel>(item);
                model.OriginalWorkGroup = item.WorkGroup;

                return PartialView("_AddEditTimeCode", model);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> EditTimeCode([FromBody] ValidTimeCodeViewModel model)
        {
            // Business Rule: Enforce mutual exclusivity
            // If JobCode has value, clear Portfolio and TestCode
            // If Portfolio or TestCode has value, clear JobCode
            if (!string.IsNullOrWhiteSpace(model.JobCode))
            {
                model.Portfolio = null;
                model.TestCode = null;
            }
            else if (!string.IsNullOrWhiteSpace(model.Portfolio) || !string.IsNullOrWhiteSpace(model.TestCode))
            {
                model.JobCode = null;
            }

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

            bool workGroupChanged = !string.IsNullOrEmpty(model.OriginalWorkGroup)
                && !string.Equals(model.OriginalWorkGroup, model.WorkGroup, StringComparison.OrdinalIgnoreCase);

            if (workGroupChanged)
            {
                var deleteResult = await _timeCodeService.DeleteTimeCodeValidAsync(
                    model.OriginalWorkGroup!, model.TimeCode, model.ParentProject);

                if (!deleteResult.Success)
                    return Json(new
                    {
                        success = false,
                        message = deleteResult.Errors?.FirstOrDefault()?.Message
                                  ?? "Failed to remove the previous work group record."
                    });

                var dto = _mapper.Map<TimeCodeValidDto>(model);
                var createResult = await _timeCodeService.CreateTimeCodeValidAsync(dto);

                if (createResult.Success)
                    return Json(new { success = true });

                return Json(new { success = false, message = createResult.Errors?.FirstOrDefault()?.Message ?? "Create failed" });
            }

            var updateDto = _mapper.Map<TimeCodeValidDto>(model);
            var updateResult = await _timeCodeService.UpdateTimeCodeValidAsync(updateDto);

            if (updateResult.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update time code.",
                errors = (updateResult.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTimeCode(string workGroup, string timeCode, string parentProject)
        {
            var result = await _timeCodeService.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        // ── NAVIGATION ───────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult NavigateToTestPurchaseRequirements(string parentProject)
        {
            // Set TempData to indicate navigation came from Portfolio Time Codes
            TempData["PactOrigin"] = "PortfolioTimeCodes";

            // Redirect to Test Purchase Requirements
            return RedirectToAction("Index", "TestPurchaseRequirement", new { area = "PACT", parentProject });
        }

    }
}
