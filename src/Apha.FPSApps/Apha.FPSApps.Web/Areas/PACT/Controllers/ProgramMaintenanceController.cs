using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using FpsDto = Apha.FPSApps.Application.Dtos.FPS;

namespace Apha.FPSApps.Web.Areas.PACT.Controllers
{
    [Area("PACT")]
    [Authorize(Roles = "PACTAdmin,PACTUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ProgramMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IProjectService _projectService;

        public ProgramMaintenanceController( IMapper mapper, IProgramService programService, IProjectService projectService)
        {
            _mapper = mapper;
            _programService = programService;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index(string? programNo = null)
        {
            TempData["NavigationSource"] = "ProgramMaintenance";
            var programList = await GetProgramListInternalAsync();

            var isValid = !string.IsNullOrWhiteSpace(programNo)
                          && programList.Any(p => p.Value == programNo);
            
            var selectedProgramNo = isValid
                ? programNo!
                : programList.FirstOrDefault()?.Value ?? string.Empty;

            var defaultRequest = new PaginationFilter<string>();
            var grid = await BuildProjectsGrid(defaultRequest, string.IsNullOrEmpty(programNo) ? selectedProgramNo : programNo);

            var model = new PactProgramMaintenanceViewModel
            {
                SelectedProgramNo = selectedProgramNo,
                ProgramList = programList,
                ProjectsGrid = grid
            };

            return View(model);
        }

        /// <summary>
        /// Method to fetch program details. Method invoke from AJAX
        /// </summary>
        /// <param name="programNo"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProgram(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return Json(new { success = false, message = "Program not found." });

            var vm = _mapper.Map<ProgramViewModel>(response.Data);
            return Json(new { success = true, data = vm });
        }

        /// <summary>
        /// Method to save program details. Method invoke from AJAX
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] ProgramViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(kvp => kvp.Value?.Errors.Count > 0)
                    .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                    {
                        // [FromBody] with System.Text.Json produces keys like "$.ProgramNo";
                        // strip the "$." prefix so the client JS can remap to "Program.ProgramNo".
                        field = kvp.Key.StartsWith("$.") ? kvp.Key[2..] : kvp.Key,
                        message = string.IsNullOrWhiteSpace(e.ErrorMessage)
                            ? e.Exception?.Message ?? "Validation error"
                            : e.ErrorMessage
                    }))
                    .ToList();

                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors
                });
            }

            var dto = _mapper.Map<FpsDto.ProgramDto>(model);
            var response = await _programService.UpdateProgramAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Program saved successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to save program.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Method to save program's project details. Method invoke from AJAX
        /// </summary>
        /// <param name="request"></param>
        /// <param name="programNo"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> LoadProjectsGrid(PaginationFilter<string> request, string programNo)
        {
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(programNo))
                return BadRequest(ModelState);

            var gridConfig = await BuildProjectsGrid(request, programNo);
           
            return PartialView("_DataGrid", gridConfig);
        }

        /// <summary>
        /// Method to fetch list of programs. Method invoke from AJAX
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ActionName("GetProgramListAsync")]
        public async Task<IActionResult> GetProgramListAsync()
        {
            var programList = await GetProgramListInternalAsync();

            // Use Newtonsoft.Json to serialize with the same settings as the view
            // This ensures property names match (Value, Text) not (value, text)
            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver()
            };

            var json = JsonConvert.SerializeObject(new { success = true, data = programList }, jsonSettings);
            return Content(json, "application/json");
        }

        private async Task<List<SelectListItem>> GetProgramListInternalAsync()
        {
            var response = await _programService.GetAllProgramsForAllUsersAsync();
            if (!response.Success || response.Data == null)
                return [];

            return response.Data
                .Select(p => new SelectListItem
                {
                    Value = p.ProgramNo,
                    Text = $"{p.ProgramNo} - {p.ProgramName}"
                })
                .ToList();
        }

        private async Task<DataGridConfig<ProgramProjectItem>> BuildProjectsGrid(PaginationFilter<string> request, string programNo)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);
            var response = await _projectService.GetPagedPactProjectsByProgramAsync(query, programNo);

            var items = response.Data != null
                ? _mapper.Map<List<ProgramProjectItem>>(response.Data)
                : [];

            var pagination = response.Pagination != null
                ? _mapper.Map<PaginationModel>(response.Pagination)
                : new PaginationModel();
            pagination.SortColumn = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<ProgramProjectItem>
            {
                GridId = "projectsGrid",
                Title = "Projects",
                KeyProperty = "ParentProject",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                AllowAdd = false,
                AllowEdit = false,
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "selectProject",
                ExtraFilterMethod = "getProjectsGridExtraFilters",
                BindGridUrl = "/PACT/ProgramMaintenance/LoadProjectsGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<ProgramProjectItem>(),
                Pagination = pagination,
                CurrentFilters = filterDict
            };
        }
    }
}
