using Apha.Common.Utilities.GenericExcelExport;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.FPS.Controllers
{
    /// <summary>
    /// ASU Data View — converted from frmAnimalCosts / fsubAnimalCosts.
    /// Read-only: all Allow* properties in fsubAnimalCosts are NotDefault (disabled).
    /// Displays animal cost records for the current FPS year, filtered by animal type.
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class AnimalCostsController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAnimalPlanService _animalPlanService;
        private readonly IAnimalService _animalService;
        private readonly IGenericExcelExporter _excelExporter;

        public AnimalCostsController(
            IMapper mapper,
            IAnimalPlanService animalPlanService,
            IAnimalService animalService,
            IGenericExcelExporter excelExporter)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _animalPlanService = animalPlanService ?? throw new ArgumentNullException(nameof(animalPlanService));
            _animalService = animalService ?? throw new ArgumentNullException(nameof(animalService));
            _excelExporter = excelExporter ?? throw new ArgumentNullException(nameof(excelExporter));
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AnimalCostsViewModel();
            await PopulateDropdownsAsync(viewModel);

            // Build the full DataGridConfig — do NOT leave it as new() default.
            // DataGridConfig<T> constructor sets AllowAdd=true, GridId="", BindGridUrl="" — those defaults
            // cause the Add button to appear on initial load and gridManager to fail to register.
            viewModel.AnimalCostsGrid = BuildAnimalCostsGridConfig();

            return View(viewModel);
        }

        /// <summary>
        /// AJAX endpoint — reloads the animal costs DataGrid filtered by the selected animal type.
        /// Equivalent to the fsubAnimalCosts subform filtered via LinkMasterFields = "PickAnimalType".
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadAnimalCostsGrid(
            PaginationFilter<string> request, string? animalType = null)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            if (string.IsNullOrWhiteSpace(animalType) || !ModelState.IsValid)
            {
                return PartialView("_DataGrid",
                    BuildAnimalCostsGridConfig(new List<AnimalCostsItem>(), new PaginationModel(), filterDict, request));
            }

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var pagedData = await _animalPlanService.GetAnimalCostByAnimalTypeAsync(queryParameters, animalType);

            List<AnimalCostsItem> items = pagedData.Data != null
                ? _mapper.Map<List<AnimalCostsItem>>(pagedData.Data)
                : new List<AnimalCostsItem>();

            PaginationModel paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination) ?? new PaginationModel();

            var gridConfig = BuildAnimalCostsGridConfig(items, paginationModel, filterDict, request);

            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Exports all animal cost records for the selected animal type to an Excel (.xlsx) file.
        /// Column headers are taken from each property's [Display(Name = ...)] attribute.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Export()
        {

            var pagedData = await _animalService.GetAllAnimalsAsync();

            List<AnimalDto> items = pagedData.Data != null
                ? _mapper.Map<List<AnimalDto>>(pagedData.Data)
                : new List<AnimalDto>();

            byte[] fileContent = _excelExporter.Export(items, "Animal Data");

            var fileName = $"AnimalData_{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx";

            return File(
                fileContent,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }

        private static DataGridConfig<AnimalCostsItem> BuildAnimalCostsGridConfig(
            List<AnimalCostsItem>? items = null,
            PaginationModel? paginationModel = null,
            Dictionary<string, string>? filterDict = null,
            PaginationFilter<string>? request = null)
        {
            paginationModel ??= new PaginationModel();

            if (request != null)
            {
                paginationModel.SortColumn = request.SortBy;
                paginationModel.SortDirection = request.Descending;
            }

            return new DataGridConfig<AnimalCostsItem>
            {
                GridId = "asuUsageList",
                Title = "Animal Type Usage",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "IndCounter",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                ExtraFilterMethod = "getAnimalCostsExtraFilters",
                BindGridUrl = "/FPS/AnimalCosts/LoadAnimalCostsGrid",
                Data = items ?? new List<AnimalCostsItem>(),
                Columns = GridDataProvider.GetColumnsDefination<AnimalCostsItem>(),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        /// <summary>
        /// Returns all animal types as JSON for the client-side multicolumn dropdown.
        /// Mirrors RowSource = "Select [AnimalType],[DailyRate] From [tblAnimals]" on pickAnimalType.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAnimalTypes()
        {
            var result = await _animalService.GetAllAnimalsAsync();
            if (!result.Success || result.Data == null)
                return Json(new { success = false });

            var items = result.Data.Select(a => new
            {
                value     = a.AnimalType,
                label     = a.AnimalType,
                dailyRate = a.DailyRate.HasValue
                    ? a.DailyRate.Value.ToString("C")
                    : "£0.00"
            });

            return Json(new { success = true, data = items });
        }

        private async Task PopulateDropdownsAsync(AnimalCostsViewModel viewModel)
        {
            var result = await _animalService.GetAllAnimalsAsync();
            if (result.Success && result.Data != null)
            {
                viewModel.AnimalTypeList = result.Data
                    .Select(a => new SelectListItem
                    {
                        Value = a.AnimalType,
                        Text  = a.AnimalType
                    })
                    .ToList();
            }
        }
    }
}
