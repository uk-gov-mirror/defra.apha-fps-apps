using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using System.Text.Json;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProjectAuditTrailController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectAuditTrailService _auditTrailService;
        private readonly IProjectService _projectService;

        public ProjectAuditTrailController(
            IMapper mapper,
            IProjectAuditTrailService auditTrailService,
            IProjectService projectService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _auditTrailService = auditTrailService ?? throw new ArgumentNullException(nameof(auditTrailService));
            _projectService = projectService ?? throw new ArgumentNullException(nameof(projectService));
        }

        // 5 empty read-only grids and project dropdown pre-populated from IProjectService
        public async Task<IActionResult> Index()
        {
            var viewModel = new ProjectAuditTrailViewModel();
            await PopulateDropdownsAsync(viewModel);

            viewModel.ProjectLogsGrid = BuildProjectLogsGridConfig(
                new List<ProjectLogItem>(), new PaginationModel(), null);

            viewModel.StaffJobLogsGrid = BuildStaffJobLogsGridConfig(
                new List<StaffJobLogItem>(), new PaginationModel(), null);

            viewModel.TestRequirementLogsGrid = BuildTestRequirementLogsGridConfig(
                new List<TestRequirementLogItem>(), new PaginationModel(), null);

            viewModel.AnimalRequestLogsGrid = BuildAnimalRequestLogsGridConfig(
                new List<AnimalRequestLogItem>(), new PaginationModel(), null);

            viewModel.AdditionalCostLogsGrid = BuildAdditionalCostLogsGridConfig(
                new List<AdditionalCostLogItem>(), new PaginationModel(), null);

            return View(viewModel);
        }

        private async Task PopulateDropdownsAsync(ProjectAuditTrailViewModel model)
        {
            var projectsResult = await _projectService.GetAllProjectsAsync();
            if (projectsResult.Success && projectsResult.Data != null)
            {
                model.ProjectList = projectsResult.Data
                    .OrderBy(p => p.ParentProject)
                    .Select(p => new SelectListItem
                    {
                        Value = p.ParentProject,
                        Text = p.ParentProject,
                        Selected = string.Equals(model.ParentProject, p.ParentProject,
                            StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }

        // ── AJAX Reload Endpoints — one per tab ───────────────────────────────

        // project param required by IProjectAuditTrailService; empty → return empty grid
        [HttpPost]
        public async Task<IActionResult> LoadProjectLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? fromDate = null,
            string? toDate = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildProjectLogsGridConfig(new List<ProjectLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetProjectLogsAsync(queryParameters, project, ParseDate(fromDate), ParseDate(toDate));

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<ProjectLogItem>>(response.Data)
                : new List<ProjectLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildProjectLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadStaffJobLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? fromDate = null,
            string? toDate = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildStaffJobLogsGridConfig(new List<StaffJobLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetStaffJobLogsAsync(queryParameters, project, ParseDate(fromDate), ParseDate(toDate));

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<StaffJobLogItem>>(response.Data)
                : new List<StaffJobLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildStaffJobLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadTestRequirementLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? fromDate = null,
            string? toDate = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildTestRequirementLogsGridConfig(new List<TestRequirementLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetTestRequirementLogsAsync(queryParameters, project, ParseDate(fromDate), ParseDate(toDate));

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestRequirementLogItem>>(response.Data)
                : new List<TestRequirementLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildTestRequirementLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAnimalRequestLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? fromDate = null,
            string? toDate = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildAnimalRequestLogsGridConfig(new List<AnimalRequestLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetAnimalRequestLogsAsync(queryParameters, project, ParseDate(fromDate), ParseDate(toDate));

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<AnimalRequestLogItem>>(response.Data)
                : new List<AnimalRequestLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildAnimalRequestLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadAdditionalCostLogsGrid(
            PaginationFilter<string> request,
            string? project = null,
            string? fromDate = null,
            string? toDate = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // (no TypeNameHandling risk; BCL secure-by-default JSON library for .NET 10)
            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(request.Filter)
                : null;

            if (string.IsNullOrWhiteSpace(project))
            {
                var emptyConfig = BuildAdditionalCostLogsGridConfig(new List<AdditionalCostLogItem>(), new PaginationModel(), filterDict);
                return PartialView("_DataGrid", emptyConfig);
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _auditTrailService.GetAdditionalCostLogsAsync(queryParameters, project, ParseDate(fromDate), ParseDate(toDate));

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<AdditionalCostLogItem>>(response.Data)
                : new List<AdditionalCostLogItem>();

            var pagination = BuildPagination(response.Pagination, request);
            var gridConfig = BuildAdditionalCostLogsGridConfig(items, pagination, filterDict);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── Private Grid Config Builders — one per tab ───────────────────────

        // ExtraFilterMethod wired to JS functions in Index.cshtml that pass project/fromDate/toDate
        private DataGridConfig<ProjectLogItem> BuildProjectLogsGridConfig(
            List<ProjectLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<ProjectLogItem>
            {
                GridId             = "projectAuditTrailGrid",
                Title              = "Project Detail Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getProjectAuditTrailExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadProjectLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ProjectLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<StaffJobLogItem> BuildStaffJobLogsGridConfig(
            List<StaffJobLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<StaffJobLogItem>
            {
                GridId             = "staffPlanChangesGrid",
                Title              = "Staff Plan Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getStaffPlanChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadStaffJobLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<StaffJobLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<TestRequirementLogItem> BuildTestRequirementLogsGridConfig(
            List<TestRequirementLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<TestRequirementLogItem>
            {
                GridId             = "testRequirementChangesGrid",
                Title              = "Test Requirement Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getTestRequirementChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadTestRequirementLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<TestRequirementLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<AnimalRequestLogItem> BuildAnimalRequestLogsGridConfig(
            List<AnimalRequestLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<AnimalRequestLogItem>
            {
                GridId             = "animalRequirementChangesGrid",
                Title              = "Animal Requirement Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getAnimalRequirementChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadAnimalRequestLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AnimalRequestLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private DataGridConfig<AdditionalCostLogItem> BuildAdditionalCostLogsGridConfig(
            List<AdditionalCostLogItem> items,
            PaginationModel pagination,
            Dictionary<string, string>? filterDict)
        {
            return new DataGridConfig<AdditionalCostLogItem>
            {
                GridId             = "exceptionalCostChangesGrid",
                Title              = "Exceptional Cost Changes",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "SequenceNo",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = false,
                EditFunction       = string.Empty,
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getExceptionalCostChangesExtraFilters",
                BindGridUrl        = "/FPS/ProjectAuditTrail/LoadAdditionalCostLogsGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AdditionalCostLogItem>(null),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        private static DateOnly? ParseDate(string? value) =>
            !string.IsNullOrWhiteSpace(value) && DateOnly.TryParseExact(value, "yyyy-MM-dd", out var d) ? d : null;

        private PaginationModel BuildPagination(
            Apha.FPSApps.Application.Dtos.PaginationDto? paginationDto,
            PaginationFilter<string> request)
        {
            if (paginationDto == null)
                return new PaginationModel { SortColumn = request.SortBy, SortDirection = request.Descending };

            var pagination = _mapper.Map<PaginationModel>(paginationDto);
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;
            return pagination;
        }
    }
}
