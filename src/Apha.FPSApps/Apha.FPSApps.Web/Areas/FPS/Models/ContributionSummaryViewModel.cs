/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryViewModel.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: frontend ViewModel for the ContributionSummary page (frmTimeSellerPC).
 *   - DataGridConfig<ContributionSummaryItem> ContributionSummaryGrid — built explicitly in controller;
 *     never left as new().
 *   - List<SelectListItem> ProfitCentreList — sourced from explicit <select id="cs-resource-centre">
 *     outside the grid container in the HTML prototype.
 *   - string SelectedProfitCentre — scalar bound property for the Resource Centre dropdown.
 *   - ContributionSummarySummaryItem SummaryTotals — populated from GetSummaryAsync; drives
 *     the summary boxes below the grid.
 *
 * PRESERVED:
 *   - Dropdown property named ProfitCentreList (FieldName + "List" convention).
 *   - SelectedProfitCentre matches the ProfitCentre field name on ContributionSummaryDto exactly.
 *   - No Lk_ prefix on any property name.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm that SelectedProfitCentre initial default ("Bact" in the JS
 *     prototype) should be resolved from user profile / session context rather than hardcoded.
 */

using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// ViewModel for the Contribution Summary page (frmTimeSellerPC → FPS/ContributionSummary/Index).
    /// Drives the Resource Centre dropdown, the contribution summary data grid, and the summary-box totals.
    /// </summary>
    public class ContributionSummaryViewModel
    {
        // TRANSFORMENGINE: SelectedProfitCentre — bound to the <select id="cs-resource-centre"> dropdown
        //   outside the grid container; used as the profitCentre parameter on grid reload and GetSummaryAsync.
        /// <summary>Currently selected profit centre / resource centre code (e.g. "Bact").</summary>
        public string SelectedProfitCentre { get; set; } = string.Empty;

        // TRANSFORMENGINE: ProfitCentreList — SelectListItem list for the Resource Centre dropdown.
        //   Only present because the HTML prototype has an explicit <select> outside the grid container.
        /// <summary>Dropdown options for the Resource Centre selector.</summary>
        public List<SelectListItem> ProfitCentreList { get; set; } = new List<SelectListItem>();

        // TRANSFORMENGINE: ContributionSummaryGrid — full DataGridConfig built explicitly in
        //   ContributionSummaryController.Index(); never left as new().
        /// <summary>DataGrid configuration for the contribution summary rows grid.</summary>
        public DataGridConfig<ContributionSummaryItem> ContributionSummaryGrid { get; set; } = new DataGridConfig<ContributionSummaryItem>();

        // TRANSFORMENGINE: SummaryTotals — populated from IContributionSummaryService.GetSummaryAsync;
        //   drives the summary boxes (Total Budget Bids, Contribution Target, Total to Recover,
        //   Total Time Fee, Surplus/Shortfall, Assured Time Fee, Assured Surplus/Shortfall,
        //   Rate Efficacy OH Rate, Rate Efficacy Total Cont) below the grid.
        /// <summary>Summary-box aggregate totals for the selected profit centre.</summary>
        public ContributionSummarySummaryItem? SummaryTotals { get; set; }
    }
}
