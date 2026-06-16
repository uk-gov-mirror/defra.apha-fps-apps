/*
 * TRANSFORMENGINE MIGRATION — ContributionSummarySummaryItem.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: summary-box item for the ContributionSummary page (frmTimeSellerPC).
 *   - Maps 1-to-1 to ContributionSummarySummaryDto (9 properties, flat structure).
 *   - Drives the summary boxes rendered below the grid:
 *       • Box 1 — Recovery: TotalBudgetBids, ContributionTarget, TotalToRecover
 *       • Box 2 — Total Planned Time: TotalTimeFeeFromPlanHrs, TotalTimeSurplusShortfall
 *       • Box 3 — Assured Planned Time: AssuredTimeFeeFromPlanHrs, AssuredTimeSurplusShortfall
 *       • Box 4 — Rate Efficacy Checker: RateEfficacyOhRate, RateEfficacyTotalCont
 *   - AutoMapper mapping in FpsViewModelMapper:
 *       CreateMap<ContributionSummarySummaryItem, ContributionSummarySummaryDto>().ReverseMap()
 *     (activated by uncommenting the stub added in Phase 10).
 *
 * PRESERVED:
 *   - All 9 property names match ContributionSummarySummaryDto exactly (PascalCase, no Lk_ prefix).
 *   - Flat structure (no nested objects) — matches the backend Phase 3 / frontend Phase 7 design.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether RateEfficacyOhRate / RateEfficacyTotalCont should
 *     remain flat or be surfaced as a nested sub-item in the Razor view.
 */

namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    /// <summary>
    /// Summary-box item for the ContributionSummary page (frmTimeSellerPC).
    /// Maps to <see cref="Apha.FPSApps.Application.Dtos.FPS.ContributionSummarySummaryDto"/> via AutoMapper.
    /// Drives the four summary panels rendered below the data grid.
    /// </summary>
    public class ContributionSummarySummaryItem
    {
        // TRANSFORMENGINE: TotalBudgetBids — "Total Budget Bids" summary box (renderSummary → cs-total-budget-bids)
        /// <summary>Total budget bids (£) for this profit centre and FPS year.</summary>
        public decimal TotalBudgetBids { get; set; }

        // TRANSFORMENGINE: ContributionTarget — "+ Contribution Target" line in summary box 1
        //   (renderSummary → cs-contribution-target)
        /// <summary>Contribution target set for this profit centre and FPS year (£).</summary>
        public decimal ContributionTarget { get; set; }

        // TRANSFORMENGINE: TotalToRecover — "= Total to Recover" line in summary box 1
        //   (renderSummary → cs-total-to-recover); derived: TotalBudgetBids + ContributionTarget.
        /// <summary>Total amount to recover (£) = TotalBudgetBids + ContributionTarget.</summary>
        public decimal TotalToRecover { get; set; }

        // TRANSFORMENGINE: TotalTimeFeeFromPlanHrs — "Total Time Fee from PlanHrs" in summary box 2
        //   (renderSummary → cs-total-time-fee); SUM(TotalFec) across all rows.
        /// <summary>Sum of TotalFec across all rows (£) — Total Time Fee from Planned Hours.</summary>
        public decimal TotalTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalTimeSurplusShortfall — "Surplus/Shortfall" in summary box 2
        //   (renderSummary → cs-total-surplus); derived: TotalTimeFeeFromPlanHrs - TotalToRecover (may be negative).
        /// <summary>Total time surplus (+) or shortfall (-) (£).</summary>
        public decimal TotalTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: AssuredTimeFeeFromPlanHrs — "Assured Time Only Fee from PlanHrs" in summary box 3
        //   (renderSummary → cs-assured-time-fee); SUM(AssuredFec) across all rows.
        /// <summary>Sum of AssuredFec across all rows (£) — Assured Time Fee from Planned Hours.</summary>
        public decimal AssuredTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredTimeSurplusShortfall — "Surplus/Shortfall" in summary box 3
        //   (renderSummary → cs-assured-surplus); derived: AssuredTimeFeeFromPlanHrs - TotalToRecover (may be negative).
        /// <summary>Assured time surplus (+) or shortfall (-) (£).</summary>
        public decimal AssuredTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: RateEfficacyOhRate — "OH Rate" in Rate Efficacy Checker box 4
        //   (renderSummary → cs-checker-oh-rate); AVG(OhRate) across all rows.
        /// <summary>Average overhead rate across all rows (£) — Rate Efficacy Checker OH Rate.</summary>
        public decimal RateEfficacyOhRate { get; set; }

        // TRANSFORMENGINE: RateEfficacyTotalCont — "Total Cont" in Rate Efficacy Checker box 4
        //   (renderSummary → cs-checker-total-cont and cs-grid-total-cont); SUM(TotalCont) across all rows.
        /// <summary>Sum of TotalCont across all rows (£) — Rate Efficacy Checker Total Cont and grid footer total.</summary>
        public decimal RateEfficacyTotalCont { get; set; }
    }
}
