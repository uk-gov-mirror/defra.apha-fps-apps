using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope, PACTApiSettings:Scope")]
    public class WorkGroupPeopleController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly ITestCapabilityService _testCapabilityService;

        public WorkGroupPeopleController(
            IMapper mapper,
            IEmployeeService employeeService,
            ITestCapabilityService testCapabilityService)
        {
            _mapper = mapper;
            _employeeService = employeeService;
            _testCapabilityService = testCapabilityService;
        }

        /// <summary>
        /// Renders the Work Group People index page with the initial data grid,
        /// work group dropdown options, and person dropdown options.
        /// An optional <paramref name="workGroup"/> query parameter pre-selects the work group dropdown.
        /// </summary>
        /// <param name="workGroup">Optional work group name used to pre-select the work group dropdown on page load.</param>
        public async Task<IActionResult> Index(string workGroup = "")
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var peopleGrid = await BuildPeopleGridAsync(defaultRequest, string.IsNullOrWhiteSpace(workGroup) ? null : workGroup);
            var workGroupOptions = await GetWorkGroupSelectListAsync();
            var personOptions = await GetPersonSelectListAsync();

            var viewModel = new WorkGroupPeopleViewModel
            {
                SelectedWorkGroup = workGroup,
                PeopleGrid = peopleGrid,
                WorkGroupOptions = workGroupOptions,
                PersonOptions = personOptions
            };

            return View(viewModel);
        }

        // ── GRID ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Handles the AJAX POST request to reload the people data grid.
        /// Accepts pagination, sorting, and filtering parameters along with an optional
        /// work group filter, and returns a rendered <c>_DataGrid</c> partial view.
        /// </summary>
        /// <param name="request">Pagination, sort, and filter parameters from the grid.</param>
        /// <param name="workGroup">Optional work group name to filter results by.</param>
        [HttpPost]
        public async Task<IActionResult> LoadPeopleGrid(PaginationFilter<string> request, string? workGroup)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildPeopleGridAsync(request, workGroup);
            return PartialView("_DataGrid", gridConfig);
        }

        // ── PRIVATE HELPERS ───────────────────────────────────────────────────

        /// <summary>
        /// Builds and returns a <see cref="DataGridConfig{WorkGroupPeopleItem}"/> populated with
        /// people data, pagination state, column definitions, and current filter values.
        /// Delegates data fetching to <see cref="FetchPeopleDataAsync"/> based on the provided filters.
        /// </summary>
        /// <param name="request">Pagination, sort, and filter parameters.</param>
        /// <param name="workGroup">Optional work group name filter.</param>
        private async Task<DataGridConfig<WorkGroupPeopleItem>> BuildPeopleGridAsync(
            PaginationFilter<string> request, string? workGroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);

            var (items, pagination) = await FetchPeopleDataAsync(query, workGroup);

            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<WorkGroupPeopleItem>
            {
                GridId = "peopleGrid",
                Title = "WorkGroup People",
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "Name",
                RowSelectFunction = "onPersonRowSelect",
                ExtraFilterMethod = "getPeopleGridExtraFilters",
                BindGridUrl = "/PACT/WorkGroupPeople/LoadPeopleGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupPeopleItem>(null),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Determines the appropriate fetch strategy based on the provided filters and
        /// returns the matching list of people items along with their pagination metadata.
        /// Prioritises work group filter, then returns all staff.
        /// </summary>
        /// <param name="query">Mapped query parameters including page, sort, and filter.</param>
        /// <param name="workGroup">Optional work group name filter.</param>
        private async Task<(List<WorkGroupPeopleItem> Items, PaginationModel Pagination)> FetchPeopleDataAsync(
            QueryParameters<string> query, string? workGroup)
        {
            if (!string.IsNullOrWhiteSpace(workGroup))
                return await FetchByWorkGroupAsync(query, workGroup);

            return await FetchAllWorkGroupPeoplesAsync(query);
        }


        /// <summary>
        /// Fetches a paginated, filtered, and sorted list of people belonging to the
        /// specified work group from the employee service.
        /// </summary>
        /// <param name="query">Query parameters including pagination, sort, and filter.</param>
        /// <param name="workGroup">The work group name to filter by.</param>
        private async Task<(List<WorkGroupPeopleItem>, PaginationModel)> FetchByWorkGroupAsync(
            QueryParameters<string> query, string workGroup)
        {
            var response = await _employeeService.GetWorkGroupStaffAsync(query, workGroup);
            if (!response.Success || response.Data == null)
                return ([], new PaginationModel());

            return (
                _mapper.Map<List<WorkGroupPeopleItem>>(response.Data.data),
                new PaginationModel
                {
                    TotalRecords = response.Data.TotalCount,
                    PageNumber   = response.Data.PageNumber,
                    PageSize     = response.Data.PageSize
                }
            );
        }

        /// <summary>
        /// Fetches a paginated, filtered, and sorted list of all work group people
        /// with no work group or person name restriction applied.
        /// </summary>
        /// <param name="query">Query parameters including pagination, sort, and filter.</param>
        private async Task<(List<WorkGroupPeopleItem>, PaginationModel)> FetchAllWorkGroupPeoplesAsync(
            QueryParameters<string> query)
        {
            var response = await _employeeService.GetWorkGroupStaffAsync(query);
            if (!response.Success || response.Data == null)
                return ([], new PaginationModel());

            return (
                _mapper.Map<List<WorkGroupPeopleItem>>(response.Data.data),
                new PaginationModel
                {
                    TotalRecords = response.Data.TotalCount,
                    PageNumber   = response.Data.PageNumber,
                    PageSize     = response.Data.PageSize
                }
            );
        }

        /// <summary>
        /// Retrieves the list of available work groups for the work group selection dropdown.
        /// </summary>
        private async Task<List<WorkGroup>> GetWorkGroupSelectListAsync()
        {
            var response = await _testCapabilityService.GetAllWorkGroupsAsync();
            if (!response.Success || response.Data == null)
                return [];

            return _mapper.Map<List<WorkGroup>>(response.Data);
        }

        /// <summary>
        /// Retrieves the list of all persons (PACT staff) for the person selection dropdown.
        /// </summary>
        private async Task<List<WorkGroupPerson>> GetPersonSelectListAsync()
        {
            var response = await _employeeService.GetAllWorkGroupPersonAsync();
            if (!response.Success || response.Data == null)
                return [];

            return _mapper.Map<List<WorkGroupPerson>>(response.Data);
        }
    }
}
