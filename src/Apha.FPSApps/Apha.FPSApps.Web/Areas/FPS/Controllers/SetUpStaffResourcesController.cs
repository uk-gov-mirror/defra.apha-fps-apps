using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class SetUpStaffResourcesController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupEmployeeService _workGroupEmployeeService;
        private readonly IProfitCentreService _profitCentreService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IWorkGroupGradeService _workGroupGradeService;

        public SetUpStaffResourcesController(
            IMapper mapper,
            IWorkGroupEmployeeService workGroupEmployeeService,
            IProfitCentreService profitCentreService,
            IWorkGroupGradeService workGroupGradeService,
            IWorkGroupService workGroupService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _workGroupEmployeeService = workGroupEmployeeService ?? throw new ArgumentNullException(nameof(workGroupEmployeeService));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
            _workGroupGradeService = workGroupGradeService ?? throw new ArgumentNullException(nameof(workGroupGradeService));
            _workGroupService = workGroupService ?? throw new ArgumentNullException(nameof(workGroupService));
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? resourceCentre = null)
        {
            var resourceCentres = await PopulateResourceCentresAsync();
            var selectedRc = resourceCentre ?? string.Empty;

            var viewModel = new SetUpStaffResourcesViewModel
            {
                ResourceCentres = resourceCentres,
                SelectedResourceCentre = selectedRc
            };

            if (!string.IsNullOrWhiteSpace(selectedRc))
            {
                var gradesResponse = await _workGroupGradeService.GetWorkGroupGradeAsync(selectedRc);
                if (gradesResponse.Success && gradesResponse.Data != null)
                {
                    viewModel.GradeList = gradesResponse.Data.Select(g => g.WgGrade).ToList();
                    viewModel.GradeCodeMap = gradesResponse.Data
                        .ToDictionary(g => g.WgGrade, g => g.GradeCode ?? string.Empty);
                }
            }

            viewModel.StaffGrid = BuildStaffGridConfig(new List<SetUpStaffResourcesItem>(), new PaginationModel());

            return View(viewModel);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> LoadStaffGrid(PaginationFilter<string> request, string wgGrade)
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

            if (string.IsNullOrWhiteSpace(wgGrade))
            {
                return Json(new { success = false, message = "WG Grade is required." });
            }

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);
            var response = await _workGroupEmployeeService.GetAllActiveWorkGroupEmployeesAsync(queryParameters, wgGrade);

            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load staff data." });
            }

            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                             ?? new Dictionary<string, string>();

            var rawData = response.Data ?? new List<WorkGroupEmployeeStaffDto>();
            var staffItems = rawData.Select(d => _mapper.Map<SetUpStaffResourcesItem>(d)).ToList();

            var paginationModel = _mapper.Map<PaginationModel>(response.Pagination) ?? new PaginationModel();
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            return PartialView("_DataGrid", BuildStaffGridConfig(staffItems, paginationModel, filterDict));
        }

        [HttpGet]
        public async Task<IActionResult> GetGroupsByResourceCentre(string resourceCentre)
        {
            if (string.IsNullOrWhiteSpace(resourceCentre))
            {
                return Json(new { success = false, message = "Resource Centre is required." });
            }

            var response = await _workGroupService.GetWorkGroupsByProfitCentreForBudgetAsync(resourceCentre);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load grades." });
            }

            var workgroups = response.Data != null ?
                [.. response.Data.Select(w => w.WorkGroupName).OrderBy(w => w)] :
                new List<string>();

            return Json(new { success = true, data = workgroups });
        }

        [HttpGet]
        public async Task<IActionResult> GetGradesByGroups(string workGroup)
        {
            if (string.IsNullOrWhiteSpace(workGroup))
            {
                return Json(new { success = false, message = "Resource Centre is required." });
            }

            var response = await _workGroupGradeService.GetWorkgroupGradesByWorkGroupAsync(workGroup);
            if (!response.Success)
            {
                return Json(new { success = false, message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to load grades." });
            }

            var gradeItems = response.Data != null
                ? response.Data
                    .OrderBy(w => w.WgGrade)
                    .Select(w => new { wgGrade = w.WgGrade, gradeCode = w.GradeCode ?? string.Empty })
                    .ToList<object>()
                : new List<object>();

            return Json(new { success = true, data = gradeItems });
        }

        [HttpGet]
        public async Task<IActionResult> GetGradeStats(string wgGrade)
        {
            if (string.IsNullOrWhiteSpace(wgGrade))
                return Json(new { success = false, message = "WG Grade is required." });

            // Resolve the GradeCode for display in the Summary Grade textbox
            var gradeResponse = await _workGroupGradeService.GetByWgGradeAsync(wgGrade);
            var gradeCode = gradeResponse.Success && gradeResponse.Data != null
                ? gradeResponse.Data.GradeCode ?? string.Empty
                : string.Empty;

            // Sum HrsAvail (AtWork) across all active staff for this grade
            var queryParams = new QueryParameters<string> { Page = 1, PageSize = 10_000 };
            var staffResponse = await _workGroupEmployeeService.GetAllActiveWorkGroupEmployeesAsync(queryParams, wgGrade);
            var totalAtWork = staffResponse.Success && staffResponse.Data != null
                ? staffResponse.Data.Sum(s => s.HrsAvail)
                : 0d;

            return Json(new { success = true, gradeCode, totalAtWork });
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string pactId)
        {
            if (string.IsNullOrWhiteSpace(pactId))
            {
                return Json(new { success = false, message = "PACT ID is required." });
            }

            var response = await _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(pactId);
            if (!response.Success || response.Data == null)
            {
                return NotFound();
            }

            var item = _mapper.Map<SetUpStaffResourcesItem>(response.Data);
            return PartialView("_EditStaffModal", item);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] SetUpStaffResourcesItem model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.PactId))
                return Json(new { success = false, message = "Staff data is required." });

            // Fetch the full existing record so required fields (SpNumber, WorkGroupGrade,
            // PersonStatus, PersonClass, TimeRecorder, StartDate, EndDate, HoursPerWeek)
            // are never lost — only the editable subset is overwritten.
            var existing = await _workGroupEmployeeService.GetWorkGroupEmployeeByIdForStaffAsync(model.PactId);
            if (!existing.Success || existing.Data == null)
                return Json(new { success = false, message = "Staff record not found." });

            var dto = existing.Data;
            dto.Name = model.Name ?? dto.Name;
            dto.HrsPaid = model.HrsPaid;
            dto.Leave = model.Leave;
            dto.SickSpecial = model.SickSpecial;
            dto.HrsAvail = model.HrsPaid - model.Leave - model.SickSpecial;
            dto.MakeAvailable = model.MakeAvailable;

            var response = await _workGroupEmployeeService.UpdateWorkGroupEmployeeForStaffAsync(dto);
            if (response.Success)
                return Json(new { success = true, message = "Staff record updated successfully." });

            return Json(new
            {
                success = false,
                message = response.Errors?.FirstOrDefault()?.Message ?? "Failed to update staff record.",
                errors = (response.Errors ?? new List<ApiErrorDto>())
                    .Select(e => new { field = e.Code ?? string.Empty, message = e.Message ?? "An unexpected error occurred." })
            });
        }

        private static DataGridConfig<SetUpStaffResourcesItem> BuildStaffGridConfig(
            List<SetUpStaffResourcesItem> data,
            PaginationModel pagination,
            Dictionary<string, string>? currentFilters = null) =>
            new()
            {
                GridId = "ssrStaffGrid",
                Title = "Staff",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "PactId",
                AllowAdd = false,
                AllowEdit = true,
                EditFunction = "editSsrStaff",
                AllowDelete = false,
                AllowRowSelection = true,
                RowSelectFunction = "ssrOnStaffRowSelect",
                BindGridUrl = "/FPS/SetUpStaffResources/LoadStaffGrid",
                ExtraFilterMethod = "ssrGetStaffExtraFilters",
                Data = data,
                Columns = GridDataProvider.GetColumnsDefination<SetUpStaffResourcesItem>(),
                Pagination = pagination,
                CurrentFilters = currentFilters ?? new Dictionary<string, string>()
            };

        private async Task<List<SelectListItem>> PopulateResourceCentresAsync()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            if (result.Success && result.Data != null)
            {
                return result.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProfitCentreId,
                        Text = $"{p.ProfitCentreId} - {p.ProfitCentreName}"
                    })
                    .ToList();
            }
            return new List<SelectListItem>();
        }
    }
}
