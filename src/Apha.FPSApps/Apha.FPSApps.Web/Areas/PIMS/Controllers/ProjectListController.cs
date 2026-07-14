using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Newtonsoft.Json;

namespace Apha.FPSApps.Web.Areas.PIMS.Controllers
{
    [Area("PIMS")]
    [Authorize(Roles = "PIMSAdmin,PIMSUser")]
    [AuthorizeForScopes(ScopeKeySection = "PIMSApiSettings:Scope")]
    public class ProjectListController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProjectListService _projectListService;

        public ProjectListController(IMapper mapper, IProjectListService projectListService)
        {
            _mapper = mapper;
            _projectListService = projectListService;
        }

        public async Task<IActionResult> Index()
        {
            PaginationFilter<string> defaultRequest = new() { Filter = "{}" };
            DataGridConfig<ProjectListItem> gridConfig = await BuildProjectListGridAsync(defaultRequest, 1);
            return View(new ProjectListViewModel { ProjectGrid = gridConfig, FilterOption = 1 });
        }

        [HttpPost]
        public async Task<IActionResult> LoadProjectListGrid(PaginationFilter<string> request, int filterOption = 2)
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

            DataGridConfig<ProjectListItem> gridConfig = await BuildProjectListGridAsync(request, filterOption);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ProjectListItem>> BuildProjectListGridAsync(PaginationFilter<string> request, int filterOption)
        {
            Dictionary<string, string> filterDict =
                JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}") ?? new();

            QueryParameters<string> queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _projectListService.GetAllProjectsAsync(queryParameters, filterOption);

            List<ProjectListItem> items = new();
            if (pagedData.Success && pagedData.Data != null)
            {
                items = _mapper.Map<List<ProjectListItem>>(pagedData.Data);
            }
            else if (pagedData.Errors != null)
            {
                foreach (var error in pagedData.Errors)
                    Console.WriteLine($"Project list error: {error.Message}");
            }

            PaginationModel paginationModel = pagedData.Pagination is null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<ProjectListItem>
            {
                GridId = "projectListGrid",
                Title = "Select a Project",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "Parentproject",
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                EditFunction = "editProject",
                AllowView = true,
                ViewFunction = "viewProject",
                ExtraFilterMethod = "getProjectExtraFilters",
                BindGridUrl = "/PIMS/ProjectList/LoadProjectListGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProjectListItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }
    }
}