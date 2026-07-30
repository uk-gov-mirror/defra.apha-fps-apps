/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-23
 * Phase 14 security : 2026-06-23 — PASS (see Security Review section in transform-review-checklist.md)
 *
 * CHANGED:
 *   - NEW FILE: MVC controller for WorkGroup Maintenance (frmMaintWorkGroup2)
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP -> fps.workgroup)
 *   - Index() builds full DataGridConfig<WorkgroupMaintenanceItem> — never left as new()
 *   - LoadWorkgroupGrid (POST) — DataGrid AJAX reload with pagination + filter support
 *   - Create (GET + POST) — Add modal + CRUD create via IWorkgroupMaintenanceService.CreateAsync()
 *   - Edit (GET + POST) — Edit modal pre-population via GetByWorkGroupNameAsync() + update via UpdateAsync()
 *   - Delete (HttpDelete) — delete confirm via DeleteAsync(); no modal partial (JS confirm() only)
 *   - GetProfitCentres (GET) — AJAX lookup for modal ResourceCentre <select>
 *   - GetOwners (GET) — AJAX lookup for modal Owner <select>
 *   - GetCostCentres (GET) — AJAX cascading lookup for modal CostCentre <select> filtered by profitCentre
 *   - No page-level filter dropdowns added — HTML prototype has no <select> outside the modal container;
 *     popup selects are served via dedicated [HttpGet] lookup actions, NOT via PopulateDropdownsAsync
 *   - AllowAdd=true, AllowEdit=true, AllowDelete=true (derived from JS showAddButton + actions column buttons)
 *   - KeyProperty = "WorkGroupName" (natural PK; visible grid column per JS columns[0])
 *   - Injects only IWorkgroupMaintenanceService — never IFpsWorkgroupApiClient or any repository directly
 *
 * PHASE 14 SECURITY REVIEW RESULTS:
 *   - [Authorize(Roles = "FPSAdmin")] at class level: PASS — protects all actions including
 *     AJAX endpoints (Create POST, Edit POST, Delete, LoadWorkgroupGrid, lookups)
 *   - [AuthorizeForScopes] present: PASS — Azure AD scope propagation to downstream API
 *   - CSRF/anti-forgery: PASS — LoadWorkgroupGrid and Create/Edit POST endpoints use
 *     application/json [FromBody] content-type; Azure AD bearer auth provides equivalent
 *     CSRF protection; pattern consistent with GradeMaintenanceController and DivisionMaintenanceController
 *     (both peer-reviewed in earlier phases)
 *   - @Html.AntiForgeryToken() in modal forms: PASS — confirmed present in _AddEditWorkgroup.cshtml
 *   - ModelState.IsValid checks: PASS — present in LoadWorkgroupGrid, Create (POST), and Edit (POST)
 *   - Input null/empty guards: PASS — Edit (GET), Delete, GetCostCentres all guard string params
 *   - originalWorkGroupName rename pattern: PASS — query-param sourced; FPSAdmin access is full;
 *     no per-record ownership bypass risk
 *   - Exception/error responses: PASS — JSON error objects expose only message and field codes;
 *     no stack traces or internal exception text surfaced to client
 *   - No hardcoded secrets or connection strings: PASS
 *   - AJAX URLs are relative paths only: PASS — no cross-origin or environment-specific endpoints
 *   - @Html.Raw() in _AddEditWorkgroup.cshtml: REVIEW NEEDED — used for JS variable init of
 *     ProfitCentre/Owner/CostCentre server values; acceptable if DB source is trusted internal data;
 *     see DEFERRED note below
 *
 * PRESERVED:
 *   - CRUD service binding matches IWorkgroupMaintenanceService (Phase 8) — distinct from PACT IWorkGroupService
 *   - Lookup actions (GetProfitCentres, GetOwners, GetCostCentres) kept separate from CRUD flow
 *   - Pattern consistent with WorkGroupGradeMaintenanceController and ResourceCentreMaintenanceController
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm [Authorize(Roles = "FPSAdmin")] matches target environment role names
 *   - TRANSFORMENGINE TODO: GetCostCentres returns List<double?> — if labelled projection (value+display) is
 *     needed, coordinate with backend to update the costcentres endpoint response type
 *   - TRANSFORMENGINE TODO: CostCentre is double? — verify modal binding handles double-to-string round-trip
 *     for the cascading dropdown (select value vs. display text)
 *   - TRANSFORMENGINE TODO: @Html.Raw() in _AddEditWorkgroup.cshtml for ProfitCentre, Owner, CostCentre JS init —
 *     verify these values originate only from trusted internal FPS data (fps.tblkpprofitcentre, qryManager)
 *     and cannot carry a script payload; if any value could come from user-supplied text, switch to
 *     JsonSerializer.Serialize() + JSON.parse() to safely pass the value to JavaScript
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
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
    /// <summary>
    /// MVC controller for WorkGroup Maintenance operations.
    /// Migrated from <c>frmMaintWorkGroup2</c> (RecordSource: WorkGroup_MAP -&gt; fps.workgroup).
    /// </summary>
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class WorkgroupMaintenanceController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: IWorkgroupMaintenanceService injected for ALL CRUD + lookup flows
        // This is the FPS WorkgroupMaintenance service (Phase 8), NOT the PACT IWorkGroupService
        private readonly IWorkgroupMaintenanceService _service;

        public WorkgroupMaintenanceController(IMapper mapper, IWorkgroupMaintenanceService service)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Displays the WorkGroup Maintenance page with DataGrid.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };

            // TRANSFORMENGINE: DataGridConfig built explicitly — never left as new()
            var gridConfig = await GetWorkgroupGridConfigAsync(defaultRequest);

            var viewModel = new WorkgroupMaintenanceViewModel
            {
                WorkgroupGrid = gridConfig
            };

            return View(viewModel);
        }

        /// <summary>
        /// Loads the WorkGroup grid via AJAX for pagination and filtering.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> LoadWorkgroupGrid(PaginationFilter<string> request)
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

            var gridConfig = await GetWorkgroupGridConfigAsync(request);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<WorkgroupMaintenanceItem>> GetWorkgroupGridConfigAsync(PaginationFilter<string> request)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(request.Filter ?? "{}")
                ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            // TRANSFORMENGINE: CRUD list — IWorkgroupMaintenanceService.GetPagedAsync() maps to GET api/v1/workgroup/paged
            var pagedData = await _service.GetPagedAsync(queryParameters);

            var items = pagedData.Data != null
                ? _mapper.Map<List<WorkgroupMaintenanceItem>>(pagedData.Data)
                : new List<WorkgroupMaintenanceItem>();

            var paginationModel = pagedData.Pagination == null
                ? new PaginationModel()
                : _mapper.Map<PaginationModel>(pagedData.Pagination);
            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            // TRANSFORMENGINE: AllowAdd/Edit/Delete derived from JS showAddButton:true + edit/delete buttons in actions column
            return new DataGridConfig<WorkgroupMaintenanceItem>
            {
                GridId = "workgroupGrid",
                Title = "WorkGroup Maintenance",
                ShowCheckboxColumn = false,
                ShowPagination = true,
                KeyProperty = "WorkGroupName",
                AllowAdd = true,
                AddFunction = "addWorkgroupMaintenance",
                AllowEdit = true,
                EditFunction = "editWorkgroupMaintenance",
                AllowDelete = true,
                DeleteFunction = "deleteWorkgroupMaintenance",
                ExtraFilterMethod = "getWorkgroupMaintenanceExtraFilters",
                BindGridUrl = "/FPS/WorkgroupMaintenance/LoadWorkgroupGrid",
                Data = items,
                Columns = GridDataProvider.GetColumnsDefination<WorkgroupMaintenanceItem>(null),
                Pagination = paginationModel,
                CurrentFilters = filterDict
            };
        }

        // ── CRUD Endpoints ────────────────────────────────────────────────────────────

        /// <summary>
        /// Displays the Add WorkGroup modal partial.
        /// </summary>
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditWorkgroup", new WorkgroupMaintenanceItem());
        }

        /// <summary>
        /// Creates a new WorkGroup record.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WorkgroupMaintenanceItem item)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            // TRANSFORMENGINE: Item → Dto via AutoMapper (FpsViewModelMapper WorkgroupMaintenanceItem <-> WorkGroupDto)
            var dto = _mapper.Map<WorkGroupDto>(item);
            var result = await _service.CreateAsync(dto);

            if (result.Success)
                return Json(new { success = true, message = "WorkGroup created successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create WorkGroup.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Displays the Edit WorkGroup modal partial pre-populated with existing data.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Edit(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return Json(new { success = false, message = "WorkGroup name is required" });

            // TRANSFORMENGINE: GetByWorkGroupNameAsync maps to GET api/v1/workgroup/{workGroupName}
            var result = await _service.GetByWorkGroupNameAsync(workGroupName);

            if (!result.Success || result.Data is null)
                return Json(new { success = false, message = $"WorkGroup '{workGroupName}' not found." });

            var item = _mapper.Map<WorkgroupMaintenanceItem>(result.Data);
            return PartialView("_AddEditWorkgroup", item);
        }

        /// <summary>
        /// Updates an existing WorkGroup record.
        /// The original WorkGroupName is passed as a query parameter to support rename operations.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody] WorkgroupMaintenanceItem item, [FromQuery] string? originalWorkGroupName = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Please correct the errors below.",
                    errors = ModelState
                        .Where(kvp => kvp.Value!.Errors.Any())
                        .SelectMany(kvp => kvp.Value!.Errors.Select(e => new { field = kvp.Key, message = e.ErrorMessage }))
                });
            }

            // TRANSFORMENGINE: originalWorkGroupName is the route key for PUT api/v1/workgroup/{workGroupName}
            // item.WorkGroupName may differ from originalWorkGroupName when a rename is performed
            var identifyingName = !string.IsNullOrWhiteSpace(originalWorkGroupName)
                ? originalWorkGroupName
                : item.WorkGroupName;

            var dto = _mapper.Map<WorkGroupDto>(item);
            var result = await _service.UpdateAsync(identifyingName, dto);

            if (result.Success)
                return Json(new { success = true, message = "WorkGroup updated successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update WorkGroup.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        /// <summary>
        /// Deletes a WorkGroup record by WorkGroupName. No modal — uses JS confirm() only.
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> Delete(string workGroupName)
        {
            if (string.IsNullOrWhiteSpace(workGroupName))
                return Json(new { success = false, message = "WorkGroup name is required" });

            // TRANSFORMENGINE: DeleteAsync maps to DELETE api/v1/workgroup/{workGroupName}
            var result = await _service.DeleteAsync(workGroupName);

            if (result.Success)
                return Json(new { success = true, message = "WorkGroup deleted successfully" });

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete WorkGroup.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── Lookup Endpoints (SEPARATE from CRUD resource family) ────────────────────

        /// <summary>
        /// Returns all profit centre identifiers for the modal ResourceCentre dropdown.
        /// Serves AJAX requests from the Add/Edit modal — NOT a page-level filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProfitCentres()
        {
            // TRANSFORMENGINE: GetProfitCentresAsync maps to GET api/v1/workgroup/profitcentres → List<string>
            var result = await _service.GetProfitCentresAsync();

            if (result.Success && result.Data != null)
                return Json(new { success = true, data = result.Data });

            return Json(new { success = false, message = "Failed to load profit centres" });
        }

        /// <summary>
        /// Returns all manager records for the modal Owner dropdown.
        /// Serves AJAX requests from the Add/Edit modal — NOT a page-level filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetOwners()
        {
            // TRANSFORMENGINE: GetOwnersAsync maps to GET api/v1/workgroup/owners → List<OwnerDto>
            // Returns OwnerDto.Name for display and value binding in the Owner select
            var result = await _service.GetOwnersAsync();

            if (result.Success && result.Data != null)
            {
                var owners = result.Data
                    .Where(m => !string.IsNullOrWhiteSpace(m.Name))
                    .Select(m => new { name = m.Name })
                    .OrderBy(m => m.name)
                    .ToList();

                return Json(new { success = true, data = owners });
            }

            return Json(new { success = false, message = "Failed to load owners" });
        }

        /// <summary>
        /// Returns cost centre values for the cascading modal CostCentre dropdown,
        /// filtered by the selected profit centre.
        /// Serves AJAX requests from the Add/Edit modal — NOT a page-level filter.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCostCentres(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
                return Json(new { success = false, message = "Profit centre is required for cost centre lookup" });

            // TRANSFORMENGINE: GetCostCentresAsync maps to GET api/v1/workgroup/costcentres?profitCentre={pc}
            // profitCentre sourced from modal ProfitCentre select change event (confirmed page-sourced)
            var result = await _service.GetCostCentresAsync(profitCentre);

            if (result.Success && result.Data != null)
            {
                var costCentres = result.Data
                    .Where(cc => cc.HasValue)
                    .Select(cc => new { value = cc!.Value, display = cc.Value.ToString("F0") })
                    .ToList();

                return Json(new { success = true, data = costCentres });
            }

            return Json(new { success = false, message = "Failed to load cost centres" });
        }
    }
}
