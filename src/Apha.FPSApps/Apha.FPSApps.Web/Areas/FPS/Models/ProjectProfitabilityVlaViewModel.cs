using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Project Profitability VLA page
    /// (<c>frmJobcodeTotalsVLA.html</c> prototype).
    /// Carries filter state, dropdown sources, grid config, and summary totals.
    /// </summary>
    public class ProjectProfitabilityVlaViewModel
    {
        // ── Page-level filter state ───────────────────────────────────────────

        //   (Approved, Completed, Not Approved) in HTML prototype
        /// <summary>Selected project status filter value. Empty string = all statuses.</summary>
        public string SelectedStatus { get; set; } = string.Empty;

        //   IProgramService.GetAllProgramsAsync() (existing /api/v1/program endpoint)
        /// <summary>Selected programme number filter value. Empty string = all programs.</summary>
        public string SelectedProgram { get; set; } = string.Empty;

        //   IProjectService.GetManagersAsync() (existing /api/v1/employee lookup)
        /// <summary>Selected manager name filter value. Empty string = all managers.</summary>
        public string SelectedManager { get; set; } = string.Empty;

        //   IProjectService.GetAllCustomersAsync() (existing /api/v1/customer lookup)
        /// <summary>Selected customer filter value. Empty string = all customers.</summary>
        public string SelectedCustomer { get; set; } = string.Empty;

        // ── Dropdown source lists ─────────────────────────────────────────────

        //   (Approved, Completed, Not Approved) — built in controller PopulateDropdownsAsync
        /// <summary>Static project status dropdown options.</summary>
        public List<SelectListItem> StatusList { get; set; } = new();

        /// <summary>Dynamic program dropdown options.</summary>
        public List<SelectListItem> ProgramList { get; set; } = new();

        /// <summary>Dynamic manager dropdown options.</summary>
        public List<SelectListItem> ManagerList { get; set; } = new();

        /// <summary>Dynamic customer dropdown options.</summary>
        public List<SelectListItem> CustomerList { get; set; } = new();

        // ── DataGrid configuration ────────────────────────────────────────────

        //   AllowAdd/Edit/Delete = false (showAddButton:false; no edit/delete buttons in JS columns).
        //   KeyProperty = "Id" (hidden row discriminator; not a visible grid column).
        /// <summary>
        /// DataGrid configuration for the Project Profitability VLA grid.
        /// Built explicitly in <c>ProjectProfitabilityVlaController.Index()</c>.
        /// </summary>
        public DataGridConfig<ProjectProfitabilityVlaItem> ProfitabilityVlaGrid { get; set; } = new();

        // ── Summary totals (server-side aggregation, mirrors ppf-total-* inputs) ──

        //   inputs. Populated by GetProjectProfitabilityVlaSummary AJAX action.
        //   Nullable decimal? — null when no data is loaded yet.
        /// <summary>Total staff costs across all visible rows.</summary>
        public decimal? TotalStaffCosts { get; set; }

        /// <summary>Total test costs across all visible rows.</summary>
        public decimal? TotalTestCost { get; set; }

        /// <summary>Total animal costs across all visible rows.</summary>
        public decimal? TotalAnimalCosts { get; set; }

        /// <summary>Total additional costs across all visible rows.</summary>
        public decimal? TotalAdditionalCosts { get; set; }

        /// <summary>Total of all cost categories across all visible rows.</summary>
        public decimal? TotalTotalCosts { get; set; }

        /// <summary>Total budget across all visible rows.</summary>
        public decimal? TotalBudget { get; set; }

        /// <summary>Total profit across all visible rows.</summary>
        public decimal? TotalProfit { get; set; }

        /// <summary>Total target profit across all visible rows.</summary>
        public decimal? TotalTargetProfit { get; set; }

        /// <summary>
        /// Total off-target across all visible rows.
        /// Negative value triggers the <c>fps-profit-offtarget</c> CSS class on the summary input.
        /// </summary>
        public decimal? TotalOffTarget { get; set; }
    }
}
