using Apha.FPSApps.Application.Dtos;
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
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class GradeMaintenanceController : Controller
    {
        private readonly IMapper _mapper;

        private readonly IGradeService _gradeService;

        public GradeMaintenanceController(IMapper mapper, IGradeService gradeService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _gradeService = gradeService ?? throw new ArgumentNullException(nameof(gradeService));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new GradeMaintenanceViewModel();

            // an empty grid with default Add button regardless of JS-derived operations profile.
            // AllowAdd:true from JS showAddButton:true; AllowEdit+AllowDelete:true from JS
            // actions column containing both edit and delete buttons (fps_grade_maintenance.js).
            var defaultRequest = new PaginationFilter<string>
            {
                Filter = "{}",
                SortBy = "GradeCode",
                Descending = false
            };
            viewModel.GradeGrid = await GetGradeGridConfigAsync(defaultRequest);

            return View(viewModel);
        }

        // ── DataGrid AJAX Reload ─────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadGradeGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetGradeGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<GradeItem>> GetGradeGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var pagedData = await _gradeService.GetAllPagedAsync(queryParameters);

            var items = new List<GradeItem>();
            if (pagedData.Data != null)
            {
                items = _mapper.Map<List<GradeItem>>(pagedData.Data);
            }

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return new DataGridConfig<GradeItem>
            {
                GridId             = "gradeGrid",
                Title              = "Grade Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "GradeCode",
                AllowAdd           = true,
                AddFunction        = "addGrade",
                AllowEdit          = true,
                EditFunction       = "editGrade",
                AllowDelete        = true,
                DeleteFunction     = "deleteGrade",
                BindGridUrl        = "/FPS/GradeMaintenance/LoadGradeGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<GradeItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── CRUD — Create ────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditGrade", new GradeItem());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GradeDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

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

            var result = await _gradeService.CreateAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Grade created successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create grade.";
            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Edit ──────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Grade code is required" });
            }

            var result = await _gradeService.GetByIdAsync(id);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<GradeItem>(result.Data);
                return PartialView("_AddEditGrade", item);
            }

            return Json(new { success = false, message = $"Grade '{id}' not found." });
        }

        // IGradeService.UpdateAsync signature: UpdateAsync(string originalCode, GradeDto dto)
        [HttpPost]
        public async Task<IActionResult> Edit(string id, [FromBody] GradeDto dto)
        {
            if (dto is null)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

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

            // (matches IGradeService.UpdateAsync(string originalCode, GradeDto dto) signature)
            var result = await _gradeService.UpdateAsync(id, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Grade updated successfully" });
            }

            var errorMessage = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update grade.";
            return Json(new
            {
                success = false,
                message = errorMessage,
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD — Delete ────────────────────────────────────────────────────

        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Json(new { success = false, message = "Grade code is required" });
            }

            var result = await _gradeService.DeleteAsync(id);

            if (result.Success && result.Data)
            {
                return Json(new { success = true, message = "Grade deleted successfully" });
            }

            var firstError = result.Errors?.FirstOrDefault();
            var errorMessage = firstError?.Code == "DB_POSTGRES_ERROR"
                ? "This grade cannot be deleted because it is referenced by other records."
                : firstError?.Message ?? "Unable to delete the grade as it may be in use.";

            return Json(new { success = false, message = errorMessage });
        }
    }
}
