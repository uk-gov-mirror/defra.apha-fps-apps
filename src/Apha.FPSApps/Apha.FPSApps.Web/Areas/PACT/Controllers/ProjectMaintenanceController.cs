using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class ProjectMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IPactTimeCodeValidService _timeCodeService;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;

        public ProjectMaintenanceController(
            IMapper mapper,
            IProjectService projectService,
            IProjectJobCodeService jobCodeService,
            IPactTimeCodeValidService timeCodeService,
            IProgramService programService,
            IEmployeeService empployeeService)
        {
            _mapper = mapper;
            _projectService = projectService;
            _jobCodeService = jobCodeService;
            _timeCodeService = timeCodeService;
            _programService = programService;
            _employeeService = empployeeService;
        }

        // ── INDEX ────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            TempData["NavigationSource"] = "ProjectMaintenance";
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var gridConfig = await BuildPactProjectCodeGridAsync(defaultRequest);
            return View(new ProjectListViewModel { ProjectGrid = gridConfig });
        }

        [HttpPost]
        public async Task<IActionResult> LoadProjectGrid(PaginationFilter<string> request, int viewBy)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (viewBy == 1)
            {
                var projectCodeGridConfig = await BuildPactProjectCodeGridAsync(request);
                return PartialView("_DataGrid", projectCodeGridConfig);
            }
            else
            {
                var jobCodeGridConfig = await BuildPactJobCodeGridAsync(request);
                return PartialView("_DataGrid", jobCodeGridConfig);
            }
        }

        private async Task<DataGridConfig<PactProjectViewModel>> BuildPactProjectCodeGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _projectService.GetPagedPactProjectsAsync(query);

            var items = response.Data != null
                ? _mapper.Map<List<PactProjectViewModel>>(response.Data)
                : new List<PactProjectViewModel>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<PactProjectViewModel>
            {
                GridId = "projectGrid",
                Title = "Project Maintenance",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = true,
                KeyProperty = "ParentProject",
                ViewFunction = "viewProject",
                ExtraFilterMethod = "getProjectMaintenanceExtraFilters",
                BindGridUrl = "/PACT/ProjectMaintenance/LoadProjectGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<PactProjectViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<ProjectJobCodeViewModel>> BuildPactJobCodeGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _jobCodeService.GetPagedJobCodesAsync(query, null);

            var items = response.Data != null
                ? _mapper.Map<List<ProjectJobCodeViewModel>>(response.Data)
                : new List<ProjectJobCodeViewModel>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProjectJobCodeViewModel>
            {
                GridId = "projectGrid",
                Title = "Project Maintenance",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowView = true,
                KeyProperty = "ParentProject",
                ViewFunction = "viewProject",
                ExtraFilterMethod = "getProjectMaintenanceExtraFilters",
                BindGridUrl = "/PACT/ProjectMaintenance/LoadProjectGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectJobCodeViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        // ── DETAILS ──────────────────────────────────────────────────────────
        public async Task<IActionResult> Details([FromRoute(Name = "id")] string parentProject)
        {
            TempData["PactOrigin"] = "Project";
            ViewBag.NavigationSource = TempData["NavigationSource"]?.ToString();
            var projectResponse = await _projectService.GetProjectByIdAsync(parentProject);
            if (!projectResponse.Success || projectResponse.Data == null)
                return NotFound();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var jobCodeGrid = await BuildJobCodeGridAsync(defaultRequest, parentProject);

            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            var programs = await _programService.GetAllProgramsAsync();
            var statuses = await _projectService.GetAllStatusesAsync();
            var diseases = await _projectService.GetAllDiseasesAsync();
            var customers = await _projectService.GetAllCustomersAsync();
            var contracts = await _projectService.GetAllPactContractsAsync();
            var managers = await _employeeService.GetAllPactManagersAsync();

            var viewModel = new ProjectMaintenanceViewModel
            {
                Project = _mapper.Map<PactProjectViewModel>(projectResponse.Data),
                JobCodeGrid = jobCodeGrid,
                TimeCodeGrid = BuildEmptyTimeCodeGrid(parentProject),
                Statuses = statuses.Data?.Select(s => new SelectListItem(s.Status, s.Status)).ToList() ?? [],
                Diseases = diseases.Data?.Select(d => new SelectListItem(d.Disease, d.Disease)).ToList() ?? [],
                Customers = customers.Data?.Select(c => new SelectListItem(c.Customer, c.Customer)).ToList() ?? [],
                Contracts = contracts.Data?.Select(c => new SelectListItem(c.ContractNo, c.ContractNo)).ToList() ?? [],
                Programs = programs.Data?.Select(p => new SelectListItem(p.ProgramName ?? p.ProgramNo, p.ProgramNo)).ToList() ?? [],
                WorkGroups = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [],
                Managers = managers.Data?.Select(w => new SelectListItem(w.Name, w.Name)).ToList() ?? []
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoadJobCodeGrid(PaginationFilter<string> request, string parentProject)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (String.IsNullOrEmpty(parentProject))
                return BadRequest("Parent project is required");

            var gridConfig = await BuildJobCodeGridAsync(request, parentProject);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTimeCodeGrid(PaginationFilter<string> request, string parentProject, string jobCodeId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (String.IsNullOrEmpty(parentProject) || String.IsNullOrEmpty(jobCodeId))
                return BadRequest("Parent project and job code are required");

            var gridConfig = await BuildTimeCodeGridAsync(request, parentProject, jobCodeId);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<JobCodeViewModel>> BuildJobCodeGridAsync(PaginationFilter<string> request, string parentProject)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _jobCodeService.GetPagedJobCodesAsync(query, parentProject);

            var items = response.Data != null
                ? _mapper.Map<List<JobCodeViewModel>>(response.Data)
                : new List<JobCodeViewModel>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            return new DataGridConfig<JobCodeViewModel>
            {
                GridId = "jobCodeGrid",
                Title = "Job Codes",
                AllowRowSelection = true,
                AllowCopy = true,
                CopyFunction = "copyJobCode",
                KeyProperty = "JobCodeId",
                AddFunction = "addJobCode",
                EditFunction = "editJobCode",
                DeleteFunction = "deleteJobCode",
                RowSelectFunction = "selectJobCode",
                BindGridUrl = $"/PACT/ProjectMaintenance/LoadJobCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<JobCodeViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task<DataGridConfig<TimeCodeViewModel>> BuildTimeCodeGridAsync(PaginationFilter<string> request, string parentProject, string jobCodeId)
        {
            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _timeCodeService.GetPagedTimeCodesAsync(query, jobCodeId, parentProject);

            var items = response.Data != null
                ? _mapper.Map<List<TimeCodeViewModel>>(response.Data)
                : new List<TimeCodeViewModel>();

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            return new DataGridConfig<TimeCodeViewModel>
            {
                GridId = "timeCodeGrid",
                Title = "Time Code Validity",
                KeyProperty = "TimeCode",
                ShowCheckboxColumn = true,
                AllowBulkCopy = true,
                AllowBulkDelete = true,
                BulkCopyButtonText = "Copy Work Group",
                AddFunction = "addTimeCode",
                EditFunction = "editTimeCode",
                DeleteFunction = "deleteTimeCode",
                BulkCopyFunction = "copyBulkWorkGroup",
                BulkDeleteFunction = "deleteBulkTimeCode",
                ExtraFilterMethod = "getTimeCodeExtraFilters",
                BindGridUrl = $"/PACT/ProjectMaintenance/LoadTimeCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<TimeCodeViewModel>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private static DataGridConfig<TimeCodeViewModel> BuildEmptyTimeCodeGrid(string parentProject)
        {
            return new DataGridConfig<TimeCodeViewModel>
            {
                GridId = "timeCodeGrid",
                Title = "Time Code Validity",
                ShowPagination = true,
                KeyProperty = "TimeCode",
                AddFunction = "addTimeCode",
                EditFunction = "editTimeCode",
                DeleteFunction = "deleteTimeCode",
                ExtraFilterMethod = "getTimeCodeExtraFilters",
                BindGridUrl = $"/PACT/ProjectMaintenance/LoadTimeCodeGrid?parentProject={Uri.EscapeDataString(parentProject)}",
                Data = [],
                Columns = GridDataProvider.GetColumnsDefination<TimeCodeViewModel>()
            };
        }

        // ── PROJECT UPDATE ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Edit(string parentProject)
        {
            var result = await _projectService.GetProjectByIdAsync(parentProject);
            if (!result.Success || result.Data == null) return NotFound();
            return PartialView("_AddEditProject", _mapper.Map<PactProjectViewModel>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] PactProjectViewModel model)
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
                            field = "Project." + x.Key,
                            message = e.ErrorMessage
                        }))
                });

            var projectdto = _mapper.Map<ProjectDto>(model);
            var result = await _projectService.UpdatePactProjectAsync(projectdto);

            if (result.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update project.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string parentProject)
        {
            var result = await _projectService.DeleteProjectAsync(parentProject);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        // ── JOB CODE CRUD ─────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateJobCode(string parentProject)
        {
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            var types = await _jobCodeService.GetTypesAsync();
            ViewBag.WorkGroupsData = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName ??  w.WorkGroupName, w.ProfitCentre)).ToList() ?? [];
            ViewBag.Types = types.Data?.Select(t => new SelectListItem(t, t)).ToList() ?? [];
            return PartialView("_AddEditJobCode", new JobCodeViewModel { ParentProject = parentProject });
        }

        [HttpPost]
        public async Task<IActionResult> CreateJobCode([FromBody] JobCodeViewModel model)
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
            ViewBag.WorkGroupsData = workGroups.Data?.Select(w => new { Value = w.WorkGroupName, Text = (string.IsNullOrEmpty(w.ProfitCentre) ? "" : w.ProfitCentre) }).ToList() ?? [];
            ViewBag.Types = types.Data?.Select(t => new SelectListItem(t, t)).ToList() ?? [];
            return PartialView("_AddEditJobCode", _mapper.Map<JobCodeViewModel>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> EditJobCode([FromBody] JobCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                   .Where(kvp => kvp.Value!.Errors.Any())
                   .SelectMany(kvp => kvp.Value!.Errors.Select(e => new {
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

        // ── TIME CODE CRUD ────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> CreateTimeCode(string parentProject, string jobCodeId)
        {
            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];

            // TimeCode is set equal to JobCode — the user selects only WorkGroup and Active
            return PartialView("_AddEditTimeCode", new TimeCodeViewModel
            {
                ParentProject = parentProject,
                JobCode = jobCodeId,
                TimeCode = jobCodeId
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateTimeCode([FromBody] TimeCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                    .Where(kvp => kvp.Value!.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => new {
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
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        [HttpGet]
        public async Task<IActionResult> EditTimeCode(string? workGroup, string timeCode, string jobCodeId, string parentProject)
        {
            var result = await _timeCodeService.GetByJobCodeAsync(jobCodeId, parentProject);
            var item = string.IsNullOrEmpty(workGroup)
                ? result.Data?.FirstOrDefault(t => t.TimeCode == timeCode)
                : result.Data?.FirstOrDefault(t => t.WorkGroup == workGroup && t.TimeCode == timeCode);

            if (item == null) return NotFound();

            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];

            var model = _mapper.Map<TimeCodeViewModel>(item);

            // Store the original WorkGroup so the POST action can detect key changes
            // and locate the existing composite-key record (ParentProject + WorkGroup + TimeCode)
            model.OriginalWorkGroup = item.WorkGroup;

            return PartialView("_AddEditTimeCode", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditTimeCode([FromBody] TimeCodeViewModel model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                    .Where(kvp => kvp.Value!.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => new {
                        field = kvp.Key,
                        message = e.ErrorMessage
                    }))
                });

            // Detect whether the user changed the WorkGroup value.
            // Because WorkGroup forms part of the composite key
            // (ParentProject + WorkGroup + TimeCode), a change cannot be done
            // via a simple UPDATE — the old record must be removed and a new
            // one inserted with the updated WorkGroup.
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

            // WorkGroup unchanged — standard update path
            var updateDto = _mapper.Map<TimeCodeValidDto>(model);
            var updateResult = await _timeCodeService.UpdateTimeCodeValidAsync(updateDto);

            if (updateResult.Success)
                return Json(new { success = true });

            return Json(new
            {
                success = false,
                message = "Failed to update time code.",
                errors = (updateResult.Errors ?? new List<ApiErrorDto>()).Select(e => new {
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

        [HttpGet]
        public async Task<IActionResult> CopyProjectJobCode(string parentProject, string jobCodeId)
        {
            var result = await _jobCodeService.GetJobCodeByIdAsync(jobCodeId);
            if (!result.Success || result.Data == null) return NotFound();

            var workGroups = await _jobCodeService.GetAllWorkGroupsAsync();
            var types = await _jobCodeService.GetTypesAsync();
            ViewBag.SourceJobCodeId = jobCodeId;
            ViewBag.WorkGroups = workGroups.Data?.Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName)).ToList() ?? [];
            ViewBag.Types = types.Data?.Select(t => new SelectListItem(t, t)).ToList() ?? [];
            return PartialView("_CopyJobCode", _mapper.Map<JobCodeViewModel>(result.Data));
        }

        [HttpPost]
        public async Task<IActionResult> CopyProjectJobCode([FromBody] CopyJobCodeRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                    .Where(kvp => kvp.Value!.Errors.Any())
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => new {
                        field = kvp.Key,
                        message = e.ErrorMessage
                    }))
                });

            var dto = new JobCodeDto
            {
                JobCodeId = model.JobCodeId,
                JobCodeName = model.JobCodeName,
                Type = model.Type,
                JobCodeWorkGroup = model.JobCodeWorkGroup,
                ParentProject = model.ParentProject
            };

            var createResult = await _jobCodeService.CreateJobCodeAsync(dto);
            if (!createResult.Success)
                return Json(new
                {
                    success = false,
                    message = "Failed to create project job code.",
                    errors = (createResult.Errors ?? new List<ApiErrorDto>()).Select(e => new {
                        field = e.Code ?? string.Empty,
                        message = e.Message ?? "An unexpected error occurred."
                    })
                });

            if (model.CopyWorkGroup)
            {
                var copyResult = await _timeCodeService.CopyWorkGroupAsync(model.SourceJobCode, model.JobCodeId, model.ParentProject);
                if (!copyResult.Success)
                    return Json(new { success = false, message = copyResult.Errors?.FirstOrDefault()?.Message ?? "Copy time codes failed" });
            }

            return Json(new { success = true });
        }

        // ── BULK TIME CODE OPERATIONS ─────────────────────────────────────────

        /// <summary>Loads the Copy Work Group modal partial, populating the target job code dropdown.</summary>
        [HttpGet]
        public async Task<IActionResult> CopyWorkGroupPartial(string parentProject, string sourceJobCodeId)
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 1000 };
            var query = _mapper.Map<QueryParameters<string>>(defaultRequest);
            var jobCodesResult = await _jobCodeService.GetPagedJobCodesAsync(query, parentProject);

            var targetJobCodes = jobCodesResult.Data?
                .Where(j => j.JobCodeId != sourceJobCodeId)
                .Select(j => new SelectListItem(j.JobCodeId, j.JobCodeId))
                .ToList() ?? [];

            ViewBag.TargetJobCodes = targetJobCodes;
            ViewBag.SourceJobCodeId = sourceJobCodeId;
            ViewBag.ParentProject = parentProject;
            return PartialView("_CopyWorkGroup");
        }

        [HttpPost]
        public async Task<IActionResult> CopyBulkWorkGroup([FromBody] CopyBulkWorkGroupRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var dto = new BulkCopyWorkGroupRequestDto
            {
                ParentProject = model.ParentProject,
                SourceJobCode = model.SourceJobCodeId,
                TargetJobCode = model.TargetJobCodeId,
                WorkGroups = model.WorkGroups
            };

            var result = await _timeCodeService.CopySelectedWorkGroupsAsync(dto);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Copy failed" });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteBulkTimeCode([FromBody] BulkDeleteTimeCodeRequest model)
        {
            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)) });

            var dto = new BulkDeleteTimeCodeRequestDto
            {
                ParentProject = model.ParentProject,
                Items = model.Items
                    .Select(i => new TimeCodeKeyItemDto { WorkGroup = i.WorkGroup, TimeCode = i.TimeCode })
                    .ToList()
            };

            var result = await _timeCodeService.DeleteBulkAsync(dto);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        /// <summary>Deletes ALL time codes for a job code (used when select-all spans pages).</summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAllJobCodeTimeCodes(string parentProject, string jobCodeId)
        {
            var result = await _timeCodeService.DeleteAllByJobCodeAsync(jobCodeId, parentProject);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Delete failed" });
        }

        /// <summary>Copies ALL work groups from source to target job code (used when select-all spans pages).</summary>
        [HttpPost]
        public async Task<IActionResult> CopyAllJobCodeWorkGroups(string parentProject, string sourceJobCodeId, string targetJobCodeId)
        {
            var result = await _timeCodeService.CopyWorkGroupAsync(sourceJobCodeId, targetJobCodeId, parentProject);
            if (result.Success)
                return Json(new { success = true });

            return Json(new { success = false, message = result.Errors?.FirstOrDefault()?.Message ?? "Copy failed" });
        }
    }
}