using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Models;
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
    public class AnimalSnapshotDataController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IAnimalService _animalService;

        public AnimalSnapshotDataController(IMapper mapper, IAnimalService animalService)
        {
            _mapper = mapper;
            _animalService = animalService;
        }

        /// <summary>
        /// GET /FPS/AnimalSnapshotData — renders the snapshot animal data page with an empty grid.
        /// The grid fetches its first page via the LoadAnimalSnapshotDataGrid AJAX endpoint.
        /// </summary>
        public IActionResult Index()
        {
            var viewModel = new AnimalSnapshotDataViewModel
            {
                SnapShotAnimalDataGrid = new DataGridConfig<AnimalSnapshotItem>
                {
                    GridId = "snapShotAnimalDataGrid",
                    Title = "Snapshot Animal Requirement",
                    ShowCheckboxColumn = false,
                    ShowPagination = true,
                    AllowAdd = false,
                    AllowEdit = false,
                    AllowDelete = false,
                    BindGridUrl = "/FPS/AnimalSnapshotData/LoadAnimalSnapshotDataGrid",
                    Data = new List<AnimalSnapshotItem>(),
                    Columns = GridDataProvider.GetColumnsDefination<AnimalSnapshotItem>(null),
                    Pagination = new PaginationModel()
                }
            };

            return View(viewModel);
        }

        /// <summary>
        /// POST /FPS/AnimalSnapshotData/LoadAnimalSnapshotDataGrid
        /// AJAX DataGrid reload endpoint — provides server-side pagination, filtering and sorting.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadAnimalSnapshotDataGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetAnimalSnapshotDataGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<AnimalSnapshotItem>> GetAnimalSnapshotDataGridConfigAsync(
            PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var response = await _animalService.GetAnimalSnapshotAsync(queryParameters);

            var items = response.Success && response.Data != null
                ? _mapper.Map<List<AnimalSnapshotItem>>(response.Data.ToList())
                : new List<AnimalSnapshotItem>();

            var paginationModel = response.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(response.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<AnimalSnapshotItem>
            {
                GridId = "snapShotAnimalDataGrid",
                Title = "Snapshot Animal Requirement",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                BindGridUrl = "/FPS/AnimalSnapshotData/LoadAnimalSnapshotDataGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<AnimalSnapshotItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}
