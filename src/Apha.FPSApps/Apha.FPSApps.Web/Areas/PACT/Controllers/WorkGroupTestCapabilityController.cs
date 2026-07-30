using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
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
    [AuthorizeForScopes(ScopeKeySection = "PACTApiSettings:Scope")]
    public class WorkGroupTestCapabilityController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupService _workGroupService;
        private readonly ITestCapabilityService _testCapabilityService;

        public WorkGroupTestCapabilityController(
            IMapper mapper,
            IWorkGroupService workGroupService,
            ITestCapabilityService testCapabilityService)
        {
            _mapper = mapper;
            _workGroupService = workGroupService;
            _testCapabilityService = testCapabilityService;
        }

        // ── INDEX (Main View) ─────────────────────────────────────────────────

        /// <summary>
        /// Displays the WorkGroup-focused Test Capability view.
        /// </summary>
        public async Task<IActionResult> Index(string workGroup = "")
        {
            TempData["NavigationSource"] = "WorkGroupTestCapability";
            var workGroupsResponse = await _workGroupService.GetAllWorkGroupsAsync();

            var viewModel = new WorkGroupTestCapabilityViewModel
            {
                SelectedWorkGroup = workGroup,
                TestCapabilityGrid = BuildEmptyTestCapabilityGrid(),
                WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                    ? _mapper.Map<List<WorkGroup>>(workGroupsResponse.Data)
                    : new List<WorkGroup>()
            };

            return View(viewModel);
        }

        // ── GRID OPERATIONS ───────────────────────────────────────────────────

        /// <summary>
        /// Loads the Test Capability grid filtered by WorkGroup.
        /// </summary>
        /// <param name="request">Pagination and filtering parameters</param>
        /// <param name="workGroup">Optional WorkGroup filter value</param>
        /// <returns>Partial view containing the data grid</returns>
        [HttpPost]
        public async Task<IActionResult> LoadTestCapabilityGrid(
            PaginationFilter<string> request, string? workGroup)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            try
            {
                var gridConfig = await BuildTestCapabilityGridAsync(request, workGroup);
                return PartialView("_DataGrid", gridConfig);
            }
            catch (Exception ex)
            {
                // Log the exception here if logging is available
                return Json(new
                {
                    success = false,
                    message = "An error occurred while loading the grid",
                    errors = new[] { ex.Message }
                });
            }
        }

        // ── GRID CONFIGURATION BUILDERS ───────────────────────────────────────

        /// <summary>
        /// Builds the Test Capability grid configuration with data fetched from the service.
        /// </summary>
        /// <param name="request">Pagination and filtering request parameters</param>
        /// <param name="workGroup">Optional WorkGroup to filter results</param>
        /// <returns>Configured data grid with test capability data</returns>
        private async Task<DataGridConfig<WorkGroupTestCapabilityItem>> BuildTestCapabilityGridAsync(
            PaginationFilter<string> request, string? workGroup)
        {
            // Parse filter dictionary from JSON
            var filterDict = ParseFilterDictionary(request.Filter);

            // Map request to query parameters
            var query = _mapper.Map<QueryParameters<string>>(request);

            // Fetch data from service
            var response = await _testCapabilityService.GetPagedByWorkGroupAsync(query, workGroup);

            // Map response data to view model items
            var items = MapTestCapabilityItems(response);

            // Build pagination model
            var paginationModel = BuildPaginationModel(response.Pagination, request);

            // Create and return configured grid
            return CreateGridConfiguration(items, paginationModel, filterDict);
        }

        /// <summary>
        /// Builds an empty Test Capability grid configuration for initial page load.
        /// </summary>
        /// <returns>Empty data grid configuration</returns>
        private static DataGridConfig<WorkGroupTestCapabilityItem> BuildEmptyTestCapabilityGrid()
        {
            return CreateGridConfiguration(
                data: new List<WorkGroupTestCapabilityItem>(),
                pagination: new PaginationModel(),
                filters: new Dictionary<string, string>());
        }

        // ── HELPER METHODS ────────────────────────────────────────────────────

        /// <summary>
        /// Parses the filter JSON string into a dictionary.
        /// </summary>
        /// <param name="filterJson">JSON string containing filter parameters</param>
        /// <returns>Dictionary of filter key-value pairs</returns>
        private static Dictionary<string, string> ParseFilterDictionary(string? filterJson)
        {
            if (string.IsNullOrWhiteSpace(filterJson))
            {
                return new Dictionary<string, string>();
            }

            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(filterJson)
                       ?? new Dictionary<string, string>();
            }
            catch (JsonException)
            {
                // If JSON parsing fails, return empty dictionary
                return new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Maps API response data to view model items.
        /// </summary>
        /// <param name="response">API response containing test capability data</param>
        /// <returns>List of mapped WorkGroupTestCapabilityItem objects</returns>
        private List<WorkGroupTestCapabilityItem> MapTestCapabilityItems(
            ApiResponseDto<List<TestCapabilityDto>> response)
        {
            if (!response.Success || response.Data == null || response.Data.Count == 0)
            {
                return new List<WorkGroupTestCapabilityItem>();
            }

            return _mapper.Map<List<WorkGroupTestCapabilityItem>>(response.Data);
        }

        /// <summary>
        /// Builds pagination model from API response and request parameters.
        /// </summary>
        /// <param name="paginationDto">Pagination data from API response</param>
        /// <param name="request">Original pagination request</param>
        /// <returns>Configured pagination model</returns>
        private PaginationModel BuildPaginationModel(
            PaginationDto? paginationDto,
            PaginationFilter<string> request)
        {
            var paginationModel = paginationDto != null
                ? _mapper.Map<PaginationModel>(paginationDto)
                : new PaginationModel();

            // Apply sorting from request
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return paginationModel;
        }

        /// <summary>
        /// Creates a data grid configuration with the specified data, pagination, and filters.
        /// This method centralizes grid configuration to ensure consistency and reduce duplication.
        /// </summary>
        /// <param name="data">List of items to display in the grid</param>
        /// <param name="pagination">Pagination model for the grid</param>
        /// <param name="filters">Current filter values</param>
        /// <returns>Configured DataGridConfig instance</returns>
        private static DataGridConfig<WorkGroupTestCapabilityItem> CreateGridConfiguration(
            List<WorkGroupTestCapabilityItem> data,
            PaginationModel pagination,
            Dictionary<string, string> filters)
        {
            return new DataGridConfig<WorkGroupTestCapabilityItem>
            {
                // Grid identity and display settings
                GridId = "testCapabilitiesWGGrid",
                Title = string.Empty,

                // Row and column settings
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowRowSelection = true,
                KeyProperty = "TestCode",

                // Action permissions (read-only grid)
                AllowExport = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowAdd=false,

                // JavaScript callback functions
                RowSelectFunction = "onTestCapabilityRowSelect",
                ExtraFilterMethod = "getTestCapabilityExtraFilters",

                // Data binding
                BindGridUrl = "/PACT/WorkGroupTestCapability/LoadTestCapabilityGrid",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<WorkGroupTestCapabilityItem>(null),
                Pagination = pagination,
                CurrentFilters = filters
            };
        }
    }
}
