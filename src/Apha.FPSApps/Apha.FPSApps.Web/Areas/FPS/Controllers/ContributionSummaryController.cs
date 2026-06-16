/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: ASP.NET Core MVC controller for the Contribution Summary page (frmTimeSellerPC).
 *   - Area: FPS. Authorize Roles: FPSAdmin,FPSUser (matches existing FPS area controllers).
 *   - Injected services: IContributionSummaryService (CRUD) + IProfitCentreService (lookup dropdown).
 *   - Index() — builds full DataGridConfig<ContributionSummaryItem> explicitly; populates
 *     ProfitCentreList dropdown from IProfitCentreService.GetProfitCentresAsync(); populates
 *     SummaryTotals from IContributionSummaryService.GetSummaryAsync() for the default selection.
 *   - LoadContributionSummaryGrid (POST) — AJAX DataGrid reload endpoint; accepts PaginationFilter
 *     and profitCentre query parameter (sourced from the Resource Centre <select> outside the grid).
 *   - Create (GET/POST) — add new contribution row; AllowAdd = false in DataGridConfig (per JS
 *     showAddButton: false), but the standalone cs-add-row-btn button in the HTML prototype still
 *     triggers this endpoint.
 *   - Edit (GET/POST) — edit existing contribution row; AllowEdit = true (onEdit callback present).
 *   - Delete (HttpDelete) — AllowDelete = false in DataGridConfig (no delete button in JS); endpoint
 *     retained as a precaution but not wired to grid delete button.
 *   - GetSummary (GET) — returns updated summary-box totals as JSON after profitCentre change or
 *     grid reload; used by the Razor view's AJAX summary refresh.
 *   - GetProfitCentres (GET) — returns profit centre list as JSON for client-side use if needed.
 *
 * PRESERVED:
 *   - Controller only injects IXxxService interfaces — no repositories or API clients injected directly.
 *   - CRUD service = IContributionSummaryService; Lookup service = IProfitCentreService (kept separate).
 *   - DataGridConfig built explicitly in both Index() and GetContributionSummaryGridConfigAsync() —
 *     never left as new().
 *   - Error response shape matches all other FPS area controllers (success/message/errors).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm that [Authorize(Roles = "FPSAdmin,FPSUser")] is the correct
 *     role set for the ContributionSummary page — other FPS area controllers use "FPSAdmin" only.
 *   - TRANSFORMENGINE TODO: Confirm the default SelectedProfitCentre value ("Bact" in JS prototype)
 *     should be resolved from user session / profile rather than hardcoded as a fallback.
 *   - TRANSFORMENGINE TODO: Verify the GetSummaryAsync fpsYear parameter — currently passed as null
 *     (uses active year server-side) per IContributionSummaryService interface design in Phase 8.
 */

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
    [Authorize(Roles = "FPSAdmin,FPSUser")]
    [AuthorizeForScopes(ScopeKeySection = "FPSApiSettings:Scope")]
    public class ContributionSummaryController : Controller
    {
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: IContributionSummaryService — CRUD service for ContributionSummary rows
        //   (delegates to IFpsContributionSummaryApiClient → api/v1/contributionsummary)
        private readonly IContributionSummaryService _contributionSummaryService;

        // TRANSFORMENGINE: IProfitCentreService — secondary lookup service for the Resource Centre
        //   dropdown (explicit <select id="cs-resource-centre"> outside the grid container).
        //   Kept separate from the CRUD service per CRUD-vs-Lookup layer boundary rule.
        private readonly IProfitCentreService _profitCentreService;

        public ContributionSummaryController(
            IMapper mapper,
            IContributionSummaryService contributionSummaryService,
            IProfitCentreService profitCentreService)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _contributionSummaryService = contributionSummaryService ?? throw new ArgumentNullException(nameof(contributionSummaryService));
            _profitCentreService = profitCentreService ?? throw new ArgumentNullException(nameof(profitCentreService));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var viewModel = new ContributionSummaryViewModel();

            // TRANSFORMENGINE: Populate the Resource Centre dropdown from IProfitCentreService
            //   (lookup flow — separate from CRUD service)
            await PopulateDropdownsAsync(viewModel);

            // TRANSFORMENGINE: Set default selected profit centre from the first available option
            //   (JS prototype default = "Bact"; resolved here from the dropdown list)
            if (string.IsNullOrEmpty(viewModel.SelectedProfitCentre) && viewModel.ProfitCentreList.Any())
            {
                viewModel.SelectedProfitCentre = viewModel.ProfitCentreList.First().Value;
            }

            // TRANSFORMENGINE: Build the full DataGridConfig explicitly — never left as new().
            //   AllowAdd = false: JS DataGridComponent showAddButton: false (standalone cs-add-row-btn
            //   handles add modal outside the grid; Create endpoints are still present for that flow).
            //   AllowEdit = true: JS onEdit callback present.
            //   AllowDelete = false: no delete button or onDelete callback in JS DataGridComponent.
            var defaultRequest = new PaginationFilter<string> { Filter = "{}" };
            viewModel.ContributionSummaryGrid = await GetContributionSummaryGridConfigAsync(
                defaultRequest, viewModel.SelectedProfitCentre);

            // TRANSFORMENGINE: Load summary-box totals for the default profit centre selection
            if (!string.IsNullOrEmpty(viewModel.SelectedProfitCentre))
            {
                var summaryResult = await _contributionSummaryService.GetSummaryAsync(
                    viewModel.SelectedProfitCentre, fpsYear: null);
                if (summaryResult.Success && summaryResult.Data != null)
                {
                    viewModel.SummaryTotals = _mapper.Map<ContributionSummarySummaryItem>(summaryResult.Data);
                }
            }

            return View(viewModel);
        }

        // ── Populate Dropdowns ────────────────────────────────────────────────

        // TRANSFORMENGINE: PopulateDropdownsAsync — uses IProfitCentreService (lookup flow).
        //   ProfitCentreList named [FieldName]List where FieldName = SelectedProfitCentre.
        //   Only present because HTML prototype has explicit <select id="cs-resource-centre">
        //   outside the grid container.
        private async Task PopulateDropdownsAsync(ContributionSummaryViewModel model)
        {
            var result = await _profitCentreService.GetProfitCentresAsync();
            if (result.Success && result.Data != null)
            {
                model.ProfitCentreList = result.Data
                    .Select(pc => new SelectListItem
                    {
                        Value = pc.ProfitCentreId,
                        Text = pc.ProfitCentreId,
                        Selected = string.Equals(model.SelectedProfitCentre, pc.ProfitCentreId,
                            StringComparison.OrdinalIgnoreCase)
                    })
                    .ToList();
            }
        }

        // ── DataGrid AJAX Reload ──────────────────────────────────────────────

        [HttpPost]
        public async Task<IActionResult> LoadContributionSummaryGrid(
            PaginationFilter<string> request,
            string? profitCentre = null)
        {
            if (!ModelState.IsValid)
            {
                return Json(new
                {
                    success = false,
                    message = "Invalid request data",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                });
            }

            var gridConfig = await GetContributionSummaryGridConfigAsync(request, profitCentre);
            return PartialView("_DataGrid", gridConfig);
        }

        private async Task<DataGridConfig<ContributionSummaryItem>> GetContributionSummaryGridConfigAsync(
            PaginationFilter<string> request,
            string? profitCentre)
        {
            var filterDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(
                request.Filter ?? "{}") ?? new Dictionary<string, string>();

            var queryParameters = _mapper.Map<QueryParameters<string>>(request);

            List<ContributionSummaryItem> items = new List<ContributionSummaryItem>();
            PaginationModel paginationModel = new PaginationModel();

            // TRANSFORMENGINE: profitCentre is a required business context parameter on
            //   GetByProfitCentreAsync — sourced from the Resource Centre <select> on the page.
            //   Only call if profitCentre has a real value (per backend parameter compatibility rule).
            if (!string.IsNullOrEmpty(profitCentre))
            {
                var pagedData = await _contributionSummaryService.GetByProfitCentreAsync(
                    queryParameters, profitCentre);

                if (pagedData.Data != null)
                {
                    items = _mapper.Map<List<ContributionSummaryItem>>(pagedData.Data.ToList());
                }

                if (pagedData.Pagination != null)
                {
                    paginationModel = _mapper.Map<PaginationModel>(pagedData.Pagination);
                }
            }

            paginationModel.SortColumn = request.SortBy;
            paginationModel.SortDirection = request.Descending;

            // TRANSFORMENGINE: DataGridConfig built fully here to mirror Index() config exactly.
            //   AllowAdd = false (JS showAddButton: false — standalone button handled outside grid).
            //   AllowEdit = true (JS onEdit callback present).
            //   AllowDelete = false (no delete button/callback in JS DataGridComponent).
            return new DataGridConfig<ContributionSummaryItem>
            {
                GridId             = "contributionSummaryGrid",
                Title              = "Contribution Summary",
                ShowCheckboxColumn = false,
                ShowPagination     = true,
                KeyProperty        = "Id",
                AllowAdd           = false,
                AddFunction        = string.Empty,
                AllowEdit          = true,
                EditFunction       = "editContributionSummary",
                AllowDelete        = false,
                DeleteFunction     = string.Empty,
                ExtraFilterMethod  = "getContributionSummaryExtraFilters",
                BindGridUrl        = "/FPS/ContributionSummary/LoadContributionSummaryGrid",
                Data               = items,
                Columns            = GridDataProvider.GetColumnsDefination<ContributionSummaryItem>(null),
                Pagination         = paginationModel,
                CurrentFilters     = filterDict
            };
        }

        // ── Summary Totals AJAX Endpoint ─────────────────────────────────────

        // TRANSFORMENGINE: GetSummary — returns aggregate summary-box totals as JSON.
        //   Called by the Razor view after a profitCentre change or grid reload to refresh
        //   the four summary panels (cs-total-budget-bids, cs-contribution-target, etc.).
        [HttpGet]
        public async Task<IActionResult> GetSummary(string profitCentre)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                return Json(new { success = false, message = "Profit centre is required" });
            }

            // TRANSFORMENGINE: fpsYear passed as null — uses active year server-side per
            //   IContributionSummaryService.GetSummaryAsync signature (Phase 8 design decision).
            var result = await _contributionSummaryService.GetSummaryAsync(profitCentre, fpsYear: null);

            if (result.Success && result.Data != null)
            {
                var summaryItem = _mapper.Map<ContributionSummarySummaryItem>(result.Data);
                return Json(new { success = true, data = summaryItem });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load summary totals.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── Profit Centres Lookup AJAX Endpoint ───────────────────────────────

        // TRANSFORMENGINE: GetProfitCentres — returns profit centre list as JSON for client-side
        //   use (e.g. initialising the Resource Centre dropdown via AJAX if needed).
        [HttpGet]
        public async Task<IActionResult> GetProfitCentres()
        {
            var result = await _profitCentreService.GetProfitCentresAsync();

            if (result.Success && result.Data != null)
            {
                return Json(new
                {
                    success = true,
                    data = result.Data.Select(pc => new
                    {
                        value = pc.ProfitCentreId,
                        text = pc.ProfitCentreId
                    })
                });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to load profit centres.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // ── CRUD Endpoints ────────────────────────────────────────────────────

        // TRANSFORMENGINE: Create (GET) — returns modal partial for the standalone cs-add-row-btn.
        //   AllowAdd = false in DataGridConfig (per JS showAddButton: false), but the HTML prototype
        //   has a standalone add button outside the grid that still opens this modal.
        [HttpGet]
        public IActionResult Create()
        {
            return PartialView("_AddEditContributionSummary", new ContributionSummaryItem());
        }

        // TRANSFORMENGINE: Create (POST) — persists a new contribution row via CRUD service.
        //   Accepts ContributionSummaryDto from [FromBody] JSON payload posted by the modal form.
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContributionSummaryDto dto)
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

            var result = await _contributionSummaryService.CreateAsync(dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Contribution row added successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to create contribution row.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // TRANSFORMENGINE: Edit (GET) — retrieves a contribution row by Id and returns the modal partial.
        //   AllowEdit = true (JS onEdit callback present).
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "A valid Id is required" });
            }

            var result = await _contributionSummaryService.GetByIdAsync(id);

            if (result.Success && result.Data != null)
            {
                var item = _mapper.Map<ContributionSummaryItem>(result.Data);
                return PartialView("_AddEditContributionSummary", item);
            }

            return Json(new
            {
                success = false,
                message = $"Contribution row with Id {id} not found."
            });
        }

        // TRANSFORMENGINE: Edit (POST) — updates an existing contribution row via CRUD service.
        //   Id from the route matches the DTO body; UpdateAsync uses both (route param + DTO payload).
        [HttpPost]
        public async Task<IActionResult> Edit(int id, [FromBody] ContributionSummaryDto dto)
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

            var result = await _contributionSummaryService.UpdateAsync(id, dto);

            if (result.Success)
            {
                return Json(new { success = true, data = result.Data, message = "Contribution row updated successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to update contribution row.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }

        // TRANSFORMENGINE: Delete (HttpDelete) — AllowDelete = false in DataGridConfig; no delete button
        //   in the JS DataGridComponent. Endpoint retained for completeness but not surface-wired to grid.
        [HttpDelete]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "A valid Id is required" });
            }

            var result = await _contributionSummaryService.DeleteAsync(id);

            if (result.Success)
            {
                return Json(new { success = true, message = "Contribution row deleted successfully" });
            }

            return Json(new
            {
                success = false,
                message = result.Errors?.FirstOrDefault()?.Message ?? "Failed to delete contribution row.",
                errors = (result.Errors ?? new List<ApiErrorDto>()).Select(e => new
                {
                    field = e.Code ?? string.Empty,
                    message = e.Message ?? "An unexpected error occurred."
                })
            });
        }
    }
}
