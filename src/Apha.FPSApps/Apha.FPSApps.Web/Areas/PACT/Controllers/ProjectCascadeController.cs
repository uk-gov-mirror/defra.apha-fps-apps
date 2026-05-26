using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Services.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "FPSAdmin,FPSUser,PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class ProjectCascadeController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IPactTimeCodeValidService _timeCodeService;
        private readonly IMonthlyTimeService _monthlyTimeService;

        public ProjectCascadeController(
            IMapper mapper,
            IProjectService projectService,
            IProjectJobCodeService jobCodeService,
            IPactTimeCodeValidService timeCodeService,
            IMonthlyTimeService monthlyTimeService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _jobCodeService = jobCodeService;
            _timeCodeService = timeCodeService;
            _monthlyTimeService = monthlyTimeService;
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var projectsResponse = await _projectService.GetAllPactProjectsAsync();
            var projects = projectsResponse.Data != null
                ? _mapper.Map<List<PactProjectViewModel>>(projectsResponse.Data)
                : new List<PactProjectViewModel>();

            var viewModel = new ProjectCascadeViewModel
            {
                Projects = projects,
                JobCodeGrid = await BuildJobCodeGridAsync(null, null),
                TimeCodeGrid = await BuildTimeCodeGridAsync(null, null, null),
                MonthlyTimeGrid = await BuildMonthlyTimeGridAsync(null, null, null, null)
            };

            return View(viewModel);
        }

        // ── GRID LOADERS ──────────────────────────────────────────────────────

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

        [HttpPost]
        public async Task<IActionResult> LoadTimeCodeGrid(PaginationFilter<string> request, string parentProject, string jobCodeId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(parentProject) || string.IsNullOrEmpty(jobCodeId))
            {
                var emptyGrid = await BuildTimeCodeGridAsync(null, null, null);
                return PartialView("_DataGrid", emptyGrid);
            }

            var gridConfig = await BuildTimeCodeGridAsync(request, parentProject, jobCodeId);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadMonthlyTimeGrid(PaginationFilter<string> request, string parentProject, string timeCode, string workGroup)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(parentProject) || string.IsNullOrEmpty(timeCode))
            {
                var emptyGrid = await BuildMonthlyTimeGridAsync(null, null, null, null);
                return PartialView("_DataGrid", emptyGrid);
            }

            var gridConfig = await BuildMonthlyTimeGridAsync(request, parentProject, timeCode, workGroup);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── JOB CODE CRUD ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateJobCode(string parentProject)
        {
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
            return PartialView("_AddEditCascadeJobCode", new CascadeJobCodeItem { ParentProject = parentProject });
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobCode([FromBody] CascadeJobCodeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { field = x.Key, message = e.ErrorMessage }))
                });

            var dto = _mapper.Map<JobCodeDto>(model);
            var result = await _jobCodeService.CreateJobCodeAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to create job code.",
                errors = (result.Errors ?? []).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditJobCode(string jobCodeId)
        {
            var result = await _jobCodeService.GetJobCodeByIdAsync(jobCodeId);
            if (!result.Success || result.Data == null) return NotFound();

            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
            return PartialView("_AddEditCascadeJobCode", _mapper.Map<CascadeJobCodeItem>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> EditJobCode([FromBody] CascadeJobCodeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { field = x.Key, message = e.ErrorMessage }))
                });

            var dto = _mapper.Map<JobCodeDto>(model);
            var result = await _jobCodeService.UpdateJobCodeAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update job code.",
                errors = (result.Errors ?? []).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteJobCode(string jobCodeId)
        {
            var result = await _jobCodeService.DeleteJobCodeAsync(jobCodeId);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        // ── TIME CODE CRUD ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTimeCode(string parentProject, string jobCodeId)
        {
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
            return PartialView("_AddEditCascadeTimeCode", new CascadeTimeCodeItem
            {
                ParentProject = parentProject,
                JobCode = jobCodeId,
                TimeCode = jobCodeId
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimeCode([FromBody] CascadeTimeCodeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { field = x.Key, message = e.ErrorMessage }))
                });

            var dto = _mapper.Map<TimeCodeValidDto>(model);
            var result = await _timeCodeService.CreateTimeCodeValidAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to create time code.",
                errors = (result.Errors ?? []).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditTimeCode(string timeCode, string workGroup, string parentProject)
        {
            var result = await _timeCodeService.GetTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (!result.Success || result.Data == null) return NotFound();

            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
            return PartialView("_AddEditCascadeTimeCode", _mapper.Map<CascadeTimeCodeItem>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> EditTimeCode([FromBody] CascadeTimeCodeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { field = x.Key, message = e.ErrorMessage }))
                });

            var dto = _mapper.Map<TimeCodeValidDto>(model);
            var result = await _timeCodeService.UpdateTimeCodeValidAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update time code.",
                errors = (result.Errors ?? []).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteTimeCode(string timeCode, string workGroup, string parentProject)
        {
            var result = await _timeCodeService.DeleteTimeCodeValidAsync(workGroup, timeCode, parentProject);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        // ── MONTHLY TIME CRUD ─────────────────────────────────────────────────

        [HttpGet]
        public IActionResult CreateMonthlyTime(string parentProject, string timeCode, string workGroup)
        {
            return PartialView("_AddEditCascadeMonthlyTime", new CascadeMonthlyTimeItem
            {
                ParentProject = parentProject,
                TimeCode = timeCode,
                WorkGroup = workGroup
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateMonthlyTime([FromBody] CascadeMonthlyTimeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { field = x.Key, message = e.ErrorMessage }))
                });

            var dto = _mapper.Map<MonthlyTimeDto>(model);
            var result = await _monthlyTimeService.CreateMonthlyTimeAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to create monthly time entry.",
                errors = (result.Errors ?? []).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditMonthlyTime(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var result = await _monthlyTimeService.GetMonthlyTimeByIdAsync(pactStaffId, timeCode, month, parentProject);
            if (!result.Success || result.Data == null) return NotFound();

            return PartialView("_AddEditCascadeMonthlyTime", _mapper.Map<CascadeMonthlyTimeItem>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> EditMonthlyTime([FromBody] CascadeMonthlyTimeItem model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState.Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors.Select(e => new { field = x.Key, message = e.ErrorMessage }))
                });

            var dto = _mapper.Map<MonthlyTimeDto>(model);
            var result = await _monthlyTimeService.UpdateMonthlyTimeAsync(dto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update monthly time entry.",
                errors = (result.Errors ?? []).Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMonthlyTime(string pactStaffId, string timeCode, double month, string parentProject)
        {
            var result = await _monthlyTimeService.DeleteMonthlyTimeAsync(pactStaffId, timeCode, month, parentProject);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        // ── PRIVATE GRID BUILDERS ─────────────────────────────────────────────

        private async Task<DataGridConfig<CascadeJobCodeItem>> BuildJobCodeGridAsync(PaginationFilter<string>? request, string? parentProject)
        {
            List<CascadeJobCodeItem> items = [];
            var pagination = new PaginationModel();
            Dictionary<string, string> filterDict = [];

            if (request != null && !string.IsNullOrEmpty(parentProject))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _jobCodeService.GetPagedJobCodesAsync(query, parentProject);
                items = response.Data != null ? _mapper.Map<List<CascadeJobCodeItem>>(response.Data) : [];
                if (response.Pagination != null)
                    pagination = _mapper.Map<PaginationModel>(response.Pagination);
                pagination.SortColumn = request.SortBy;
                pagination.SortDirection = request.Descending;
                filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];
            }

            return new DataGridConfig<CascadeJobCodeItem>
            {
                GridId = "gridContainer_JobcodeBelongsToProjectGrid",
                Title = "Job Codes",
                AllowRowSelection = true,
                KeyProperty = "JobCodeId",
                AddFunction = "addJobcode",
                EditFunction = "editJobcode",
                DeleteFunction = "deleteJobcode",
                RowSelectFunction = "selectJobcode",
                BindGridUrl = string.IsNullOrEmpty(parentProject)
                    ? "/PACT/ProjectCascade/LoadJobCodeGrid"
                    : $"/PACT/ProjectCascade/LoadJobCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<CascadeJobCodeItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<CascadeTimeCodeItem>> BuildTimeCodeGridAsync(PaginationFilter<string>? request, string? parentProject, string? jobCodeId)
        {
            List<CascadeTimeCodeItem> items = [];
            var pagination = new PaginationModel();
            Dictionary<string, string> filterDict = [];

            if (request != null && !string.IsNullOrEmpty(parentProject) && !string.IsNullOrEmpty(jobCodeId))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _timeCodeService.GetPagedTimeCodesAsync(query, jobCodeId, parentProject);
                items = response.Data != null ? _mapper.Map<List<CascadeTimeCodeItem>>(response.Data) : [];
                if (response.Pagination != null)
                    pagination = _mapper.Map<PaginationModel>(response.Pagination);
                pagination.SortColumn = request.SortBy;
                pagination.SortDirection = request.Descending;
                filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];
            }

            return new DataGridConfig<CascadeTimeCodeItem>
            {
                GridId = "gridContainer_TimeCodeValidOptionGrid",
                Title = "Time Code Valid",
                AllowRowSelection = true,
                KeyProperty = "TimeCode",
                AddFunction = "addTimecode",
                EditFunction = "editTimecode",
                DeleteFunction = "deleteTimecode",
                RowSelectFunction = "selectTimecode",
                BindGridUrl = string.IsNullOrEmpty(parentProject) || string.IsNullOrEmpty(jobCodeId)
                    ? "/PACT/ProjectCascade/LoadTimeCodeGrid"
                    : $"/PACT/ProjectCascade/LoadTimeCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}&jobCodeId={Uri.EscapeDataString(jobCodeId)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<CascadeTimeCodeItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<CascadeMonthlyTimeItem>> BuildMonthlyTimeGridAsync(PaginationFilter<string>? request, string? parentProject, string? timeCode, string? workGroup)
        {
            List<CascadeMonthlyTimeItem> items = [];
            var pagination = new PaginationModel();
            Dictionary<string, string> filterDict = [];

            if (request != null && !string.IsNullOrEmpty(parentProject) && !string.IsNullOrEmpty(timeCode))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _monthlyTimeService.GetPagedMonthlyTimeAsync(query, timeCode, workGroup ?? string.Empty, parentProject);
                items = response.Data != null ? _mapper.Map<List<CascadeMonthlyTimeItem>>(response.Data) : [];
                if (response.Pagination != null)
                    pagination = _mapper.Map<PaginationModel>(response.Pagination);
                pagination.SortColumn = request.SortBy;
                pagination.SortDirection = request.Descending;
                filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? [];
            }

            return new DataGridConfig<CascadeMonthlyTimeItem>
            {
                GridId = "gridContainer_TimeRecordsGrid",
                Title = "Monthly Time Records",
                KeyProperty = "PactStaffId",
                AddFunction = "addTimeentry",
                EditFunction = "editTimeentry",
                DeleteFunction = "deleteTimeentry",
                BindGridUrl = string.IsNullOrEmpty(parentProject) || string.IsNullOrEmpty(timeCode)
                    ? "/PACT/ProjectCascade/LoadMonthlyTimeGrid"
                    : $"/PACT/ProjectCascade/LoadMonthlyTimeGrid?parentProject={Uri.EscapeDataString(parentProject)}&timeCode={Uri.EscapeDataString(timeCode)}&workGroup={Uri.EscapeDataString(workGroup ?? string.Empty)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<CascadeMonthlyTimeItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }
    }
}
