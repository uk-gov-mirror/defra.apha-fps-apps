using Apha.FPSApps.Application.Dtos;
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

    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]

    public class ProgramMaintenanceController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IProgramService _programService;
        private readonly IEmployeeService _employeeService;

        private static readonly List<string> DefaultDirectorates =
          new List<string> { "CSG", "Surveillance", "Lab Services" };

        public ProgramMaintenanceController(IMapper mapper, IProgramService programService,
            IEmployeeService employeeService)
        {
            _mapper = mapper;
            _programService = programService;
            _employeeService = employeeService;
        }
        public async Task<IActionResult> Index()
        {
            var gridConfig = await GetProgramGridConfig();
            return View(gridConfig);
        }

        [HttpPost]
        public async Task<IActionResult> LoadProgramGrid(PaginationFilter<string> request)
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

            var filterDict = !string.IsNullOrEmpty(request.Filter)
                ? JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter)
                : null;

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            var gridConfig = await GetProgramGridConfig(queryParameters, filterDict);

            return PartialView("_DataGrid", gridConfig);
        }

        // GET: Create
        public async Task<IActionResult> Create()
        {
            var model = new ProgramViewModel
            {
                ProgramNo = string.Empty,
                ProgramName = string.Empty,
                Directorate = string.Empty
            };
            await PopulateDropdownsAsync(model);
            return PartialView("_AddEditProgram", model);
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProgramViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            // Ensure Target is always stored as a positive value
            if (model.Target.HasValue)
                model.Target = Math.Abs(model.Target.Value);

            var dto = _mapper.Map<ProgramDto>(model);
            var response = await _programService.AddProgramAsync(dto);
            if (response.Success)
            {
                return Json(new { success = true, data = response.Data, message = "Program created successfully" });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to create program.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // GET: Edit
        public async Task<IActionResult> Edit(string programNo)
        {
            var response = await _programService.GetProgramByIdAsync(programNo);
            if (!response.Success || response.Data == null)
                return NotFound();
            var model = _mapper.Map<ProgramViewModel>(response.Data);
            await PopulateDropdownsAsync(model);
            return PartialView("_AddEditProgram", model);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody]ProgramViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new
                        {
                            field = kvp.Key,
                            message = e.ErrorMessage
                        }))
                });
            }

            // Ensure Target is always stored as a positive value
            if (model.Target.HasValue)
                model.Target = Math.Abs(model.Target.Value);

            var dto = _mapper.Map<ProgramDto>(model);
            var response = await _programService.UpdateProgramAsync(dto);
            if (response.Success)
            {
                return Json(new
                {
                    success = true,
                    message = "Program updated successfully.",
                    data = response.Data
                });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update program.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // GET: Delete
        public async Task<IActionResult> Delete(string programNo)
        {
            var response = await _programService.DeleteProgramAsync(programNo);
            if (response.Success)
            {
                return Json(new
                {
                    success = true,
                    message = "Program deleted successfully.",
                    data = response.Data
                });
            }

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to delete program.",
                errors = (response.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        private async Task<DataGridConfig<ProgramViewModel>> GetProgramGridConfig(QueryParameters<string>? query = null, Dictionary<string, string>? filterDict = null)
        {
            var response = await _programService.GetAllProgramsAsync(query ?? new QueryParameters<string>());
            var programItems = new List<ProgramViewModel>();
            if (response.Data != null)
            {
                programItems = _mapper.Map<List<ProgramViewModel>>(response.Data.ToList());
            }
            PaginationModel paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();

            paginationModel.SortColumn = query?.SortBy;
            paginationModel.SortDirection = query?.Descending ?? false;

            var programGridConfig = new DataGridConfig<ProgramViewModel>
            {
                GridId = "programGrid",
                Title = "Program Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "ProgramNo",
                AddFunction = "addProgram",
                EditFunction = "editProgram",
                DeleteFunction = "deleteProgram",
                ExtraFilterMethod = "getProgramExtraFilters",
                BindGridUrl = "/FPS/ProgramMaintenance/LoadProgramGrid",
                Data = programItems,
                Columns = GridDataProvider.GetColumnsDefination<ProgramViewModel>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };

            return programGridConfig;
        }

        private async Task PopulateDropdownsAsync(ProgramViewModel model)
        {
            // Directorate dropdown — blank first item
            var directorates = new List<string>(DefaultDirectorates);

            if (!string.IsNullOrWhiteSpace(model.Directorate) &&
                !directorates.Any(d => string.Equals(d, model.Directorate, StringComparison.OrdinalIgnoreCase)))
            {
                directorates.Add(model.Directorate);
            }

            model.DirectorateOptions = directorates
                .Select(d => new SelectListItem
                {
                    Value = d,
                    Text = d,
                    Selected = string.Equals(model.Directorate, d, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = string.Empty, Text = string.Empty, Selected = string.IsNullOrEmpty(model.Directorate) })
                .ToList();

            // Manager dropdown — blank first item
            var managerResponse = await _employeeService.GetAllManagersAsync();
            model.ManagerList = (managerResponse.Data ?? new List<ManagerDto>())
                .Where(m => !string.IsNullOrEmpty(m.Name))
                .Select(m => new SelectListItem
                {
                    Value = m.Name,
                    Text = $"{m.Name} | {m.WorkGroup ?? string.Empty} | {m.GradeCode ?? string.Empty}",
                    Selected = string.Equals(model.Manager, m.Name, StringComparison.OrdinalIgnoreCase)
                })
                .Prepend(new SelectListItem { Value = string.Empty, Text = string.Empty, Selected = string.IsNullOrEmpty(model.Manager) })
                .ToList();
        }
    }
}