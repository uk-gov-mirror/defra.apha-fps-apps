using Apha.Common.Utilities.ExcelExport;
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
    public class BosworthInterfaceController : Controller
    {
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        private readonly IMapper _mapper;
        private readonly IBosworthInterfaceService _bosworthInterfaceService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IProjectService _projectService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IExcelExportService _excelExportService;
        private readonly ITestCapabilityService _testCapabilityService;

        public BosworthInterfaceController(
            IMapper mapper,
            IBosworthInterfaceService bosworthInterfaceService,
            IWorkGroupService workGroupService,
            IProjectService projectService,
            IProfitCentreService profitCentreService,
            IExcelExportService excelExportService,
            ITestCapabilityService testCapabilityService)
        {
            _mapper = mapper;
            _bosworthInterfaceService = bosworthInterfaceService;
            _workGroupService = workGroupService;
            _projectService = projectService;
            _profitCentreService = profitCentreService;
            _excelExportService = excelExportService;
            _testCapabilityService = testCapabilityService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new BosworthInterfaceViewModel();
            await PopulateDropdownsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTimePurchaseProjectReport(string project)
        {   
            var response = await _bosworthInterfaceService.GetTimePurchaseProjectAsync(project);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TimePurchaseProject");
            var fileName = $"TimePurchaseProject_{project}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTimeSaleProfitCentreReport(string profitCentre)
        {
            var response = await _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(profitCentre);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TimeSaleProfitCentre");
            var fileName = $"TimeSaleProfitCentre_{profitCentre}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTimeSaleWorkgroupReport(string workGroup)
        {
            var response = await _bosworthInterfaceService.GetTimeSaleProfitCentreAsync(workGroup);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TimeSaleWorkgroup");
            var fileName = $"TimeSaleWorkgroup_{workGroup}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTestSaleSellingWorkgroupReport(string workGroup)
        {
            var response = await _bosworthInterfaceService.GetTestSaleSellingWorkgroupAsync(workGroup);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TestSaleSellingWorkgroup");
            var fileName = $"TestSaleSellingWorkgroup_{workGroup}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        [HttpPost]
        public async Task<IActionResult> GenerateTestSaleBuyingProjectReport(string parentProject)
        {
            var response = await _bosworthInterfaceService.GetTestSaleBuyingProjectAsync(parentProject);

            var excelBytes = _excelExportService.ExportToExcel(response.Data ?? [], "TestSaleBuyingProject");
            var fileName = $"TestSaleBuyingProject_{parentProject}.xlsx";

            return File(excelBytes, ExcelContentType, fileName);
        }

        public async Task<IActionResult> ListTestCapability(string? workGroup)
        {
            TempData["NavigationSource"] = "BosworthInterface";
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            var capabilitiesGrid = await BuildCapabilitiesGridAsync(defaultRequest, workGroup);

            return View(new BosworthInterfaceViewModel
            {
                CapabilitiesGrid = capabilitiesGrid,
                WorkGroup = workGroup
            });
        }


        [HttpPost]
        public async Task<IActionResult> LoadCapabilitiesGrid(PaginationFilter<string> request, string workGroup)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });

            var gridConfig = await BuildCapabilitiesGridAsync(request, workGroup);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<WgTestCapabilitiesWithDescriptionItem>> BuildCapabilitiesGridAsync(PaginationFilter<string> request, string? workGroup)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? [];

            var items = new List<WgTestCapabilitiesWithDescriptionItem>();
            var pagination = new PaginationModel { SortColumn = request.SortBy, SortDirection = request.Descending };

            if (!string.IsNullOrWhiteSpace(workGroup))
            {
                var query = _mapper.Map<QueryParameters<string>>(request);
                var response = await _testCapabilityService.GetPagedWgTestCapabilitiesWithDescriptionAsync(query, workGroup);

                items = response.Success && response.Data != null
                    ? _mapper.Map<List<WgTestCapabilitiesWithDescriptionItem>>(response.Data)
                    : new List<WgTestCapabilitiesWithDescriptionItem>();

                if (response.Pagination != null)
                {
                    pagination = new PaginationModel
                    {
                        TotalRecords = response.Pagination.TotalRecords,
                        PageNumber = response.Pagination.PageNumber,
                        PageSize = response.Pagination.PageSize,
                        SortColumn = request.SortBy,
                        SortDirection = request.Descending
                    };
                }
            }

            var bindGridUrl = string.IsNullOrWhiteSpace(workGroup)
                ? string.Empty
                : $"/PACT/BosworthInterface/LoadCapabilitiesGrid?workGroup={Uri.EscapeDataString(workGroup)}";

            return new DataGridConfig<WgTestCapabilitiesWithDescriptionItem>
            {
                GridId = "wgTestCapabilitiesGrid",
                Title = "",
                BindGridUrl = bindGridUrl,
                ShowCheckboxColumn = false,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowExport = false,
                AllowRowSelection = false,
                ShowPagination = true,
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WgTestCapabilitiesWithDescriptionItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }

        private async Task PopulateDropdownsAsync(BosworthInterfaceViewModel viewModel)
        {
            var projectsResponse = await _projectService.GetAllPactProjectsAsync();
            var profitCentresResponse = await _profitCentreService.GetAllProfitCentresAsync();
            var workGroupsResponse = await _workGroupService.GetAllWorkGroupsAsync();

            viewModel.ProjectOptions = projectsResponse.Success && projectsResponse.Data != null
                ? _mapper.Map<List<Project>>(projectsResponse.Data)
                    .OrderBy(p => p.ParentProject)
                    .ToList()
                : [];

            viewModel.ProfitCentreOptions = profitCentresResponse.Success && profitCentresResponse.Data != null
                ? _mapper.Map<List<ProfitCentre>>(profitCentresResponse.Data)
                    .OrderBy(pc => pc.Division)
                    .ToList()
                : [];

            viewModel.WorkGroupOptions = workGroupsResponse.Success && workGroupsResponse.Data != null
                ? _mapper.Map<List<WorkGroup>>(workGroupsResponse.Data)
                    .OrderBy(w => w.WorkGroupName)
                    .ToList()
                : [];
        }
    }
}