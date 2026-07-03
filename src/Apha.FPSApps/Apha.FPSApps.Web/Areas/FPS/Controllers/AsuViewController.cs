/*
 * TRANSFORMENGINE MIGRATION — AsuViewController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New MVC controller created for the ASU Data View page (fps_asuview.html)
 *   - Index(): builds AsuViewViewModel with explicit DataGridConfig and Animal Type dropdown
 *   - LoadAsuViewGrid(): AJAX DataGrid reload endpoint; accepts PaginationFilter<string>
 *     and animalType filter string (from the #asu-animal-type-value hidden input)
 *   - GetTotals(): AJAX endpoint returning TotalAnimalDays and TotalCost for the summary
 *     row, mirroring updateAsuSummary() in fps_asuview.js
 *   - No Create/Edit/Delete endpoints — IAsuViewService exposes no mutating methods;
 *     JS showAddButton: false confirms AllowAdd = false; AllowEdit = false; AllowDelete = false
 *   - Injects IAsuViewService only (CRUD + lookup both via same service interface)
 *   - [Authorize(Roles = "FPSAdmin,FPSUser")] — read-only data view, same as StaffPlanController
 *
 * PRESERVED:
 *   - Layer boundary: only IAsuViewService injected — no API clients or repositories
 *   - Thin controller pattern: no business logic, delegates to service
 *   - Error handling: Json({ success = false, message = ... }) on failure responses
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE RESOLVED (Phase 14): [Authorize(Roles = "FPSAdmin,FPSUser")] confirmed
 *     correct — matches ProgramStaffPlanController and ProjectController in the same FPS area.
 *     Read-only view does not require FPSAdmin-only restriction. [AuthorizeForScopes] present.
 *   - TRANSFORMENGINE RESOLVED (Phase 14): GetTotals endpoint confirmed consumed by
 *     Index.cshtml @section Scripts via AJAX GET (@Url.Action("GetTotals","AsuView",...)).
 *     It is not duplicated in the LoadAsuViewGrid response — totals are a separate AJAX call.
 *   - TRANSFORMENGINE NOTE (Phase 14): GetTotals uses PageSize = int.MaxValue to aggregate
 *     totals server-side. This is behind [Authorize] and for a read-only view; acceptable
 *     for the current data size. If tblAnimals_MAP grows significantly, add a dedicated
 *     totals query in IAsuViewService / IAnimalRepository instead.
 */

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
    // TRANSFORMENGINE: [Area("FPS")] — this is a frontend MVC controller in the FPS area.
    // TRANSFORMENGINE: [Authorize] — read-only view; FPSAdmin and FPSUser roles can access.
    // TRANSFORMENGINE: [AuthorizeForScopes] — MSAL scope enforcement for the FPS backend API.
    [Area("FPS")]
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class AsuViewController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: IAsuViewService — the only injected service.
        // Handles both the CRUD data load (GetAsuViewAsync) and the Animal Type lookup
        // (GetAnimalTypeLookupAsync). No separate lookup service needed because
        // IAsuViewService already exposes both operations.
        // MVC controller → IAsuViewService → IFpsApiClient → FpsAsuViewApiClient → HTTP
        private readonly IAsuViewService _asuViewService;

        public AsuViewController(IMapper mapper, IAsuViewService asuViewService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _asuViewService = asuViewService ?? throw new ArgumentNullException(nameof(asuViewService));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Renders the ASU Data View page with the Animal Type filter dropdown and
        /// an empty DataGrid (populated client-side after the user selects an animal type).
        /// </summary>
        public async Task<IActionResult> Index()
        {
            // TRANSFORMENGINE: Index() builds the full ViewModel — grid config explicitly set,
            // never left as new() per DANGEROUS DEFAULTS note in the skill template.
            var viewModel = new AsuViewViewModel();
            await PopulateDropdownsAsync(viewModel);

            // TRANSFORMENGINE: DataGridConfig explicitly built — AllowAdd/Edit/Delete all false.
            // JS showAddButton: false → AllowAdd = false.
            // IAsuViewService has no create/update/delete methods → AllowEdit = AllowDelete = false.
            // ExtraFilterMethod omitted — filter state managed via the animalType hidden input,
            // not via the DataGrid's built-in ExtraFilterMethod mechanism.
            viewModel.AsuViewGrid = new DataGridConfig<AsuViewItem>
            {
                GridId             = "asuViewGrid",
                Title              = "Animal Type Usage",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                BindGridUrl        = "/FPS/AsuView/LoadAsuViewGrid",
                Data               = new List<AsuViewItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<AsuViewItem>(),
                Pagination         = new PaginationModel()
            };

            return View(viewModel);
        }

        // ── Dropdown Population ───────────────────────────────────────────────

        // TRANSFORMENGINE: PopulateDropdownsAsync — populates AnimalTypeList for the custom
        // Animal Type filter dropdown (#animal-type-dropdown) in fps_asuview.html.
        // Uses GetAnimalTypeLookupAsync (lookup flow) — distinct from GetAsuViewAsync (CRUD flow).
        // Lookup DTO: AnimalDto; Value = AnimalType (code); Text = AnimalType (display text).
        private async Task PopulateDropdownsAsync(AsuViewViewModel model)
        {
            var lookupResult = await _asuViewService.GetAnimalTypeLookupAsync();
            if (lookupResult.Success && lookupResult.Data != null)
            {
                // TRANSFORMENGINE: AnimalDto.AnimalType used for both Value and Text —
                // the JS prototype uses the AnimalType string directly as the selection key.
                // No separate code/description split exists in AnimalDto for this field.
                model.AnimalTypeList = lookupResult.Data
                    .Where(item => !string.IsNullOrWhiteSpace(item.AnimalType))
                    .OrderBy(item => item.AnimalType)
                    .Select(item => new SelectListItem
                    {
                        Value    = item.AnimalType,
                        Text     = item.AnimalType,
                        Selected = string.Equals(
                            model.AnimalType, item.AnimalType,
                            StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }

        // ── DataGrid AJAX Reload ──────────────────────────────────────────────

        /// <summary>
        /// Reloads the ASU View DataGrid partial view.
        /// The <paramref name="animalType"/> parameter is required business context —
        /// it comes from the #asu-animal-type-value hidden input in the Razor view.
        /// </summary>
        /// <param name="request">Pagination, sort, and column-filter parameters.</param>
        /// <param name="animalType">Required. The animal type selected in the filter dropdown.</param>
        [HttpPost]
        public async Task<IActionResult> LoadAsuViewGrid(
            PaginationFilter<string> request, string? animalType = null)
        {
            // TRANSFORMENGINE: ModelState validation — returns 400-style JSON on bad input.
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors  = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            // TRANSFORMENGINE: animalType is a required business parameter for the backend
            // GET /api/v1/animal/asu-view?animalType=X endpoint. When no animal type is
            // selected (page load before user picks one), return an empty grid rather than
            // calling the backend with a null/empty value (backend returns 400 on null/empty).
            if (string.IsNullOrWhiteSpace(animalType))
            {
                var emptyGrid = BuildEmptyGridConfig();
                return PartialView("_DataGrid", emptyGrid);
            }

            var grid = await BuildAsuViewGridAsync(request, animalType);
            return PartialView("_DataGrid", grid);
        }

        // ── Totals Endpoint ───────────────────────────────────────────────────

        /// <summary>
        /// Returns Total Animal Days and Total Cost for the selected animal type.
        /// Called via AJAX by the Razor view to populate the summary row (#asuTotalDays,
        /// #asuTotalCost), mirroring updateAsuSummary() in fps_asuview.js.
        /// </summary>
        /// <param name="animalType">Required. The animal type selected in the filter dropdown.</param>
        [HttpGet]
        public async Task<IActionResult> GetTotals(string? animalType = null)
        {
            // TRANSFORMENGINE: animalType required — return zero totals rather than
            // calling the backend with null/empty (backend enforces 400 on null/empty).
            if (string.IsNullOrWhiteSpace(animalType))
            {
                return Json(new { success = true, totalAnimalDays = 0.0, totalCost = 0.0m });
            }

            // TRANSFORMENGINE: GetAsuViewAsync used with a large-page query to aggregate totals.
            // No dedicated totals endpoint exists on IAsuViewService; all rows are fetched and
            // summed in the controller to mirror updateAsuSummary() behaviour in fps_asuview.js.
            var query = new QueryParameters<string>
            {
                Page     = 1,
                PageSize = int.MaxValue
            };

            var response = await _asuViewService.GetAsuViewAsync(query, animalType);

            if (!response.Success || response.Data == null)
            {
                return Json(new
                {
                    success = false,
                    message = response.Errors?.FirstOrDefault()?.Message
                              ?? "Failed to retrieve totals."
                });
            }

            // TRANSFORMENGINE: sum mirrors updateAsuSummary() in fps_asuview.js:
            //   totalDays = filtered.reduce((sum, item) => sum + Number(item.animalDays), 0)
            //   totalCost = filtered.reduce((sum, item) => sum + Number(item.cost), 0)
            var totalAnimalDays = response.Data.Sum(r => r.AnimalDays);
            var totalCost       = response.Data.Sum(r => r.Cost);

            return Json(new
            {
                success        = true,
                totalAnimalDays,
                totalCost
            });
        }

        // ── Private Helpers ───────────────────────────────────────────────────

        // TRANSFORMENGINE: BuildAsuViewGridAsync — builds the DataGridConfig for a
        // specific animalType filter. Mirrors the pattern used by StaffPlanController.BuildGridAsync().
        private async Task<DataGridConfig<AsuViewItem>> BuildAsuViewGridAsync(
            PaginationFilter<string> request, string animalType)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var query = _mapper.Map<QueryParameters<string>>(request);

            var response = await _asuViewService.GetAsuViewAsync(query, animalType);

            var items      = new List<AsuViewItem>();
            var pagination = new PaginationModel();

            if (response.Success && response.Data != null)
            {
                items = _mapper.Map<List<AsuViewItem>>(response.Data);

                if (response.Pagination != null)
                {
                    pagination.PageNumber   = response.Pagination.PageNumber;
                    pagination.PageSize     = response.Pagination.PageSize;
                    pagination.TotalRecords = response.Pagination.TotalRecords;
                }
            }

            pagination.SortColumn    = request.SortBy;
            pagination.SortDirection = request.Descending;

            return new DataGridConfig<AsuViewItem>
            {
                GridId             = "asuViewGrid",
                Title              = "Animal Type Usage",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                BindGridUrl        = "/FPS/AsuView/LoadAsuViewGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<AsuViewItem>(),
                Pagination         = pagination,
                CurrentFilters     = filterDict
            };
        }

        // TRANSFORMENGINE: BuildEmptyGridConfig — returns a grid with no rows for the
        // initial page load (before the user selects an animal type). Preserves the full
        // config shape so the DataGrid JS component renders correctly.
        private static DataGridConfig<AsuViewItem> BuildEmptyGridConfig()
        {
            return new DataGridConfig<AsuViewItem>
            {
                GridId             = "asuViewGrid",
                Title              = "Animal Type Usage",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AllowEdit          = false,
                AllowDelete        = false,
                BindGridUrl        = "/FPS/AsuView/LoadAsuViewGrid",
                Data               = new List<AsuViewItem>(),
                Columns            = GridDataProvider.GetColumnsDefination<AsuViewItem>(),
                Pagination         = new PaginationModel()
            };
        }
    }
}
