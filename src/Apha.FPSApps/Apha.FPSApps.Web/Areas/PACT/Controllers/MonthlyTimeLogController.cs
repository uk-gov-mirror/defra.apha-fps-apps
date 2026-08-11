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
    public class MonthlyTimeLogController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IPactMonthlyTimeService _logService;
        private readonly IWorkGroupService _workGroupService;
        private readonly ITestorProductService _testorProductService;
        private readonly IProjectService _projectService;
        private readonly IProjectJobCodeService _jobCodeService;
        private readonly IEmployeeService _employeeService;

        public MonthlyTimeLogController(
            IMapper mapper,
            IPactMonthlyTimeService logService,
            IWorkGroupService workGroupService,
            ITestorProductService testorProductService,
            IProjectService projectService,
            IProjectJobCodeService jobCodeService,
            IEmployeeService employeeService)
        {
            _mapper = mapper;
            _logService = logService;
            _workGroupService = workGroupService;
            _testorProductService = testorProductService;
            _projectService = projectService;
            _jobCodeService = jobCodeService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            var workGroupsResponse = await _workGroupService.GetAllWorkGroupsAsync();
            var testsResponse = await _testorProductService.GetAllTestorProductsAsync();
            var projectsResponse = await _projectService.GetAllPactProjectsAsync();
            var jobCodesResponse = await _jobCodeService.GetJobCodesAsync();
            var staffResponse = await _employeeService.GetPactStaffAsync();

            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var grid = await BuildLogGrid(defaultRequest, null, null, null, null, null, null, null, null);

            var viewModel = new MonthlyTimeLogViewModel
            {
                LogGrid = grid,
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? workGroupsResponse.Data
                        .Select(w => new SelectListItem(w.WorkGroupName, w.WorkGroupName))
                        .OrderBy(x => x.Text)
                        .ToList()
                    : new List<SelectListItem>(),
                TestCodeOptions = testsResponse.Success && testsResponse.Data != null
                    ? testsResponse.Data
                        .Select(t => new SelectListItem(t.ItemCode, t.ItemCode))
                        .DistinctBy(x => x.Value)
                        .OrderBy(x => x.Text)
                        .ToList()
                    : new List<SelectListItem>(),
                ProjectOptions = projectsResponse.Success && projectsResponse.Data != null
                    ? projectsResponse.Data
                        .Select(p => new ProjectDropdownItem
                        {
                            Value = p.ParentProject ?? string.Empty,
                            Code  = p.ParentProject ?? string.Empty,
                            Title = p.ProjectTitle  ?? string.Empty
                        })
                        .OrderBy(x => x.Code)
                        .ToList()
                    : new List<ProjectDropdownItem>(),
                JobCodeOptions = jobCodesResponse.Success && jobCodesResponse.Data != null
                    ? jobCodesResponse.Data
                        .DistinctBy(j => j.JobCodeId)
                        .Select(j => new JobCodeDropdownItem
                        {
                            Value   = j.JobCodeId      ?? string.Empty,
                            Code    = j.JobCodeId      ?? string.Empty,
                            Project = j.ParentProject  ?? string.Empty
                        })
                        .OrderBy(x => x.Code)
                        .ToList()
                    : new List<JobCodeDropdownItem>(),
                StaffOptions = staffResponse.Success && staffResponse.Data != null
                    ? staffResponse.Data
                        .Where(s => !string.IsNullOrWhiteSpace(s.PactId))
                        .Select(s => new StaffDropdownItem
                        {
                            Value    = s.PactId   ?? string.Empty,
                            PactId   = s.PactId   ?? string.Empty,
                            SpNumber = s.SpNumber ?? string.Empty,
                            Name     = s.Name     ?? string.Empty
                        })
                        .OrderBy(x => x.Name)
                        .ToList()
                    : new List<StaffDropdownItem>()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Search(
            PaginationFilter<string> request,
            string? workGroup,
            string? timeCode,
            string? parentProject,
            string? pactStaffId,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            if (!HasSearchCriteria(workGroup, timeCode, parentProject, pactStaffId, dateImported, month, userId, insertDelete))
            {
                var emptyGridConfig = await BuildLogGrid(request, null, null, null, null, null, null, null, null);
                return PartialView("_DataGrid", emptyGridConfig);
            }

            var gridConfig = await BuildLogGrid(request, workGroup, timeCode, parentProject, pactStaffId,
                dateImported, month, userId, insertDelete);

            return PartialView("_DataGrid", gridConfig);
        }

        private static bool HasSearchCriteria(
            string? workGroup,
            string? timeCode,
            string? parentProject,
            string? pactStaffId,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete) =>
                !string.IsNullOrWhiteSpace(workGroup) ||
                !string.IsNullOrWhiteSpace(timeCode) ||
                !string.IsNullOrWhiteSpace(parentProject) ||
                !string.IsNullOrWhiteSpace(pactStaffId) ||
                dateImported.HasValue ||
                month.HasValue ||
                !string.IsNullOrWhiteSpace(userId) ||
                !string.IsNullOrWhiteSpace(insertDelete);

        private async Task<DataGridConfig<MonthlyTimeLogItem>> BuildLogGrid(
            PaginationFilter<string> request,
            string? workGroup,
            string? timeCode,
            string? parentProject,
            string? pactStaffId,
            DateTime? dateImported,
            double? month,
            string? userId,
            string? insertDelete)
        {
            List<MonthlyTimeLogItem> items = [];
            PaginationModel pagination;

            var filter = new MonthlyTimeLogFilterDto
            {
                WorkGroup = workGroup,
                TimeCode = timeCode,
                ParentProject = parentProject,
                PactStaffId = pactStaffId,
                DateImported = dateImported,
                Month = month,
                UserId = userId,
                InsertDelete = insertDelete
            };

            if (HasSearchCriteria(workGroup, timeCode, parentProject, pactStaffId, dateImported, month, userId, insertDelete))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _logService.SearchAsync(query, filter);
                items = response.Data != null ? _mapper.Map<List<MonthlyTimeLogItem>>(response.Data) : [];
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

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            return new DataGridConfig<MonthlyTimeLogItem>
            {
                GridId = "mtLogGrid",
                Title = "Monthly Time Log",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<MonthlyTimeLogItem>(null),
                Pagination = pagination,
                CurrentFilters = filterDict,
                ExtraFilterMethod = "getExtraFilters_mtLogGrid",
                BindGridUrl = "/PACT/MonthlyTimeLog/Search"
            };
        }
    }
}
