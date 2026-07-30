using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class TestListVlaController : Controller
    {
        private readonly IMapper _mapper;
        private readonly ITestListVlaService _testListVlaService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IFpsApiClient _fpsApiClient;
        private readonly IFpsYearContext _fpsYearContext;

        public TestListVlaController(
            IMapper mapper,
            ITestListVlaService testListVlaService,
            ITestRequirementService testRequirementService,
            ITestCapabilityService testCapabilityService,
            IFpsApiClient fpsApiClient,
            IFpsYearContext fpsYearContext)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _testListVlaService = testListVlaService ?? throw new ArgumentNullException(nameof(testListVlaService));
            _testRequirementService = testRequirementService ?? throw new ArgumentNullException(nameof(testRequirementService));
            _testCapabilityService = testCapabilityService ?? throw new ArgumentNullException(nameof(testCapabilityService));
            _fpsApiClient = fpsApiClient ?? throw new ArgumentNullException(nameof(fpsApiClient));
            _fpsYearContext = fpsYearContext ?? throw new ArgumentNullException(nameof(fpsYearContext));
        }

        // ── INDEX ─────────────────────────────────────────────────────────────

        public IActionResult Index()
        {
            var fpsYear = _fpsYearContext.Year;

            var viewModel = new TestListVlaViewModel
            {
                FpsYear = fpsYear,

                // AllowAdd/Edit/Delete = true — CRUD modals exist in HTML prototype (vlaTestListModal, vlaDeleteModal)
                TestListGrid = new DataGridConfig<TestListVlaItem>
                {
                    GridId              = "testListVlaGrid",
                    Title               = "Test List for VLA",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "ItemCode",
                    AllowAdd            = false,
                    AllowEdit           = false,
                    AllowDelete         = false,
                    AllowRowSelection   = true,
                    RowSelectFunction   = "selectTestListVlaRow",
                    ExtraFilterMethod   = "getTestListVlaExtraFilters",
                    BindGridUrl         = $"/FPS/TestListVla/LoadTestListVlaGrid?year={fpsYear}",
                    Data                = new List<TestListVlaItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestListVlaItem>(),
                    Pagination          = new PaginationModel()
                },

                // Read-only test requirements listing
                TestRequirementsGrid = new DataGridConfig<TestRequirementItem>
                {
                    GridId              = "testRequirementsGrid",
                    Title               = "Test Requirements",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "Buyer",
                    AllowAdd            = false,
                    AllowEdit           = false,
                    AllowDelete         = false,
                    ExtraFilterMethod   = "getTestRequirementExtraFilters",
                    BindGridUrl         = $"/FPS/TestListVla/LoadTestRequirementsGrid?year={fpsYear}",
                    Data                = new List<TestRequirementItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestRequirementItem>(),
                    Pagination          = new PaginationModel()
                },

                // AllowAdd/Edit/Delete = true — tabGridModal + tabDeleteModal in HTML prototype
                ComponentChargesGeneralGrid = new DataGridConfig<TestRCCostItem>
                {
                    GridId              = "componentChargesGeneralGrid",
                    Title               = "Component Charges",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "ProfitCentre",
                    AllowAdd            = false,
                    AllowEdit           = false,
                    AllowDelete         = false,
                    AllowRowSelection   = true,
                    RowSelectFunction   = "selectComponentChargeGeneralRow",
                    ExtraFilterMethod   = "getComponentChargesExtraFilters",
                    BindGridUrl         = $"/FPS/TestListVla/LoadComponentChargesGeneralGrid?year={fpsYear}",
                    Data                = new List<TestRCCostItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestRCCostItem>(),
                    Pagination          = new PaginationModel()
                },

                // AllowAdd/Edit/Delete = true — tabGridModal + tabDeleteModal in HTML prototype
                ComponentChargesProjectGrid = new DataGridConfig<TestRequirementRCCostItem>
                {
                    GridId              = "componentChargesProjectGrid",
                    Title               = "Component Charges for Individual Projects",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "ProfitCentre",
                    AllowAdd            = false,
                    AllowEdit           = false,
                    AllowDelete         = false,
                    ExtraFilterMethod   = "getComponentChargesProjectExtraFilters",
                    BindGridUrl         = $"/FPS/TestListVla/LoadComponentChargesProjectGrid?year={fpsYear}",
                    Data                = new List<TestRequirementRCCostItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestRequirementRCCostItem>(),
                    Pagination          = new PaginationModel()
                },

                // Read-only listing of WorkGroups able to supply the selected test item
                SuppliersGrid = new DataGridConfig<TestCapabilityItem>
                {
                    GridId              = "suppliersGrid",
                    Title               = "WorkGroups able to Supply",
                    ShowCheckboxColumn  = false,
                    ShowPagination      = true,
                    KeyProperty         = "WorkGroup",
                    AllowAdd            = false,
                    AllowEdit           = false,
                    AllowDelete         = false,
                    ExtraFilterMethod   = "getSuppliersExtraFilters",
                    BindGridUrl         = $"/FPS/TestListVla/LoadSuppliersGrid?year={fpsYear}",
                    Data                = new List<TestCapabilityItem>(),
                    Columns             = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(),
                    Pagination          = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        // ── MAIN TEST LIST GRID ───────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestListVlaGrid(PaginationFilter<string> request)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestListVlaGridAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestListVlaItem>> BuildTestListVlaGridAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _testListVlaService.GetAllAsync(query);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<TestListVlaItem>>(response.Data)
                : new List<TestListVlaItem>();

            if (!response.Success)
            {
                Response.Headers.Append("X-Grid-Load-Error", string.Join(" | ", (response.Errors ?? new List<ApiErrorDto>()).Select(e => e.Message)));
            }

            var paginationModel = response.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestListVlaItem>
            {
                GridId              = "testListVlaGrid",
                Title               = "Test List for VLA",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "ItemCode",
                AllowAdd            = false,
                AllowEdit           = false,
                AllowDelete         = false,
                AllowRowSelection   = true,
                RowSelectFunction   = "selectTestListVlaRow",
                ExtraFilterMethod   = "getTestListVlaExtraFilters",
                BindGridUrl         = $"/FPS/TestListVla/LoadTestListVlaGrid?year={fpsYear}",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestListVlaItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

        // ── TEST REQUIREMENTS TAB GRID ────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadTestRequirementsGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildTestRequirementsGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestRequirementItem>> BuildTestRequirementsGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var items = new List<TestRequirementItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                var response = await _testRequirementService.GetPagedTestReqmtAsync(query, testCode);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestRequirementItem>>(response.Data);
                if (response.Pagination is not null)
                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRequirementItem>
            {
                GridId              = "testRequirementsGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "Test Requirements"
                    : $"Test Requirements for {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "Buyer",
                AllowAdd            = false,
                AllowEdit           = false,
                AllowDelete         = false,
                ExtraFilterMethod   = "getTestRequirementExtraFilters",
                BindGridUrl         = $"/FPS/TestListVla/LoadTestRequirementsGrid?year={fpsYear}",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestRequirementItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

        // ── COMPONENT CHARGES GENERAL TAB GRID ───────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadComponentChargesGeneralGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildComponentChargesGeneralGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestRCCostItem>> BuildComponentChargesGeneralGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var items = new List<TestRCCostItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                var response = await _fpsApiClient.FpsTestRCCost.GetByTestCodeAsync(testCode, fpsYear);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestRCCostItem>>(response.Data);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRCCostItem>
            {
                GridId              = "componentChargesGeneralGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "Component Charges"
                    : $"Component charges {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "ProfitCentre",
                AllowAdd            = false,
                AllowEdit           = false,
                AllowDelete         = false,
                AllowRowSelection   = true,
                RowSelectFunction   = "selectComponentChargeGeneralRow",
                ExtraFilterMethod   = "getComponentChargesExtraFilters",
                BindGridUrl         = $"/FPS/TestListVla/LoadComponentChargesGeneralGrid?year={fpsYear}",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestRCCostItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }


        [HttpPost]
        public async Task<IActionResult> LoadComponentChargesProjectGrid(
            PaginationFilter<string> request, string? testCode = null, string? profitCentre = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildComponentChargesProjectGridAsync(request, testCode, profitCentre);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestRequirementRCCostItem>> BuildComponentChargesProjectGridAsync(
            PaginationFilter<string> request, string? testCode, string? profitCentre)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var items = new List<TestRequirementRCCostItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                var response = await _fpsApiClient.FpsTestRequirementRCCost.GetByTestCodeAsync(testCode, fpsYear);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestRequirementRCCostItem>>(response.Data);
            }

            if (!string.IsNullOrWhiteSpace(profitCentre))
            {
                items = items
                    .Where(x => string.Equals(x.ProfitCentre, profitCentre, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestRequirementRCCostItem>
            {
                GridId              = "componentChargesProjectGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "Component Charges for Individual Projects"
                    : $"Component charges {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "ProfitCentre",
                AllowAdd            = false,
                AllowEdit           = false,
                AllowDelete         = false,
                ExtraFilterMethod   = "getComponentChargesProjectExtraFilters",
                BindGridUrl         = $"/FPS/TestListVla/LoadComponentChargesProjectGrid?year={fpsYear}",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestRequirementRCCostItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }

       

        // ── SUPPLIERS / WORKGROUPS TAB GRID ───────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadSuppliersGrid(
            PaginationFilter<string> request, string? testCode = null)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildSuppliersGridAsync(request, testCode);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<TestCapabilityItem>> BuildSuppliersGridAsync(
            PaginationFilter<string> request, string? testCode)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();
            var fpsYear = _fpsYearContext.Year;

            var query = _mapper.Map<QueryParameters<string>>(request);
            var items = new List<TestCapabilityItem>();
            var paginationModel = new PaginationModel();

            if (!string.IsNullOrEmpty(testCode))
            {
                // maps to testCode here (capability items keyed by TestCode)
                var response = await _testCapabilityService.GetPagedByTestCodeAsync(query, testCode);
                if (response.Success && response.Data != null)
                    items = _mapper.Map<List<TestCapabilityItem>>(response.Data);
                if (response.Pagination is not null)
                    paginationModel = _mapper.Map<PaginationModel>(response.Pagination);
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<TestCapabilityItem>
            {
                GridId              = "suppliersGrid",
                Title               = string.IsNullOrEmpty(testCode)
                    ? "WorkGroups able to Supply"
                    : $"WorkGroups able to Supply {testCode}",
                ShowCheckboxColumn  = false,
                ShowPagination      = true,
                KeyProperty         = "WorkGroup",
                AllowAdd            = false,
                AllowEdit           = false,
                AllowDelete         = false,
                ExtraFilterMethod   = "getSuppliersExtraFilters",
                BindGridUrl         = $"/FPS/TestListVla/LoadSuppliersGrid?year={fpsYear}",
                Data                = items,
                Columns             = GridDataProvider.GetColumnsDefination<TestCapabilityItem>(),
                Pagination          = paginationModel,
                CurrentFilters      = filterDict
            };
        }
    }
}
