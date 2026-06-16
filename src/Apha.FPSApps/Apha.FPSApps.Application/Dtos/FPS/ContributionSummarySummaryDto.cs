/*
 * TRANSFORMENGINE MIGRATION — ContributionSummarySummaryDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: frontend-layer DTO for the ContributionSummary aggregate summary-box totals.
 *   - Mirrors Apha.FPS.Application.Dtos.ContributionSummarySummaryDto (backend Phase 3 [DONE])
 *     in the FPSApps application namespace so the frontend never takes a hard dependency on
 *     the backend project.
 *   - All 9 summary properties copied verbatim (identical names, types) to allow zero-conflict
 *     AutoMapper mappings in FpsApiDtoMapper (Phase 8).
 *   - Returned by IFpsContributionSummaryApiClient.GetSummaryAsync via
 *     GET api/v1/contributionsummary/summary.
 *
 * PRESERVED:
 *   - All field names and types match the backend DTO exactly (case-sensitive).
 *   - Flat structure (no nested objects) mirrors the Phase 1 / Phase 3 design decision.
 *   - No Lk_ prefix naming — entity field names used directly.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether RateEfficacyOhRate / RateEfficacyTotalCont should
 *     remain flat on this DTO or be surfaced as a nested sub-DTO in later phases.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for the ContributionSummary aggregate summary-box totals.
    /// Same shape as Apha.FPS.Application.Dtos.ContributionSummarySummaryDto (backend Phase 3).
    /// Returned by IFpsContributionSummaryApiClient.GetSummaryAsync via
    /// GET api/v1/contributionsummary/summary.
    /// </summary>
    public class ContributionSummarySummaryDto
    {
        // TRANSFORMENGINE: TotalBudgetBids — total budget bids (£); mirrors ContributionSummaryTotals.TotalBudgetBids
        /// <summary>Total budget bids (£).</summary>
        public decimal TotalBudgetBids { get; set; }

        // TRANSFORMENGINE: ContributionTarget — target contribution (£); mirrors ContributionSummaryTotals.ContributionTarget
        /// <summary>Contribution target set for this profit centre and FPS year (£).</summary>
        public decimal ContributionTarget { get; set; }

        // TRANSFORMENGINE: TotalToRecover — derived: TotalBudgetBids + ContributionTarget;
        //   mirrors ContributionSummaryTotals.TotalToRecover
        /// <summary>Total amount to recover (£) = TotalBudgetBids + ContributionTarget.</summary>
        public decimal TotalToRecover { get; set; }

        // TRANSFORMENGINE: TotalTimeFeeFromPlanHrs — SUM(TotalFec) across all rows;
        //   mirrors ContributionSummaryTotals.TotalTimeFeeFromPlanHrs
        /// <summary>Sum of TotalFec across all rows (£) — Total Time Fee from Planned Hours.</summary>
        public decimal TotalTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalTimeSurplusShortfall — derived: TotalTimeFeeFromPlanHrs - TotalToRecover (can be negative);
        //   mirrors ContributionSummaryTotals.TotalTimeSurplusShortfall
        /// <summary>Total time surplus (+) or shortfall (-) (£).</summary>
        public decimal TotalTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: AssuredTimeFeeFromPlanHrs — SUM(AssuredFec) across all rows;
        //   mirrors ContributionSummaryTotals.AssuredTimeFeeFromPlanHrs
        /// <summary>Sum of AssuredFec across all rows (£) — Assured Time Fee from Planned Hours.</summary>
        public decimal AssuredTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredTimeSurplusShortfall — derived: AssuredTimeFeeFromPlanHrs - TotalToRecover (can be negative);
        //   mirrors ContributionSummaryTotals.AssuredTimeSurplusShortfall
        /// <summary>Assured time surplus (+) or shortfall (-) (£).</summary>
        public decimal AssuredTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: RateEfficacyOhRate — AVG(OhRate) across all rows;
        //   mirrors ContributionSummaryTotals.RateEfficacyOhRate
        /// <summary>Average overhead rate across all rows (£) — Rate Efficacy Checker OH Rate.</summary>
        public decimal RateEfficacyOhRate { get; set; }

        // TRANSFORMENGINE: RateEfficacyTotalCont — SUM(TotalCont) across all rows;
        //   mirrors ContributionSummaryTotals.RateEfficacyTotalCont
        /// <summary>Sum of TotalCont across all rows (£) — Rate Efficacy Checker Total Cont and grid footer total.</summary>
        public decimal RateEfficacyTotalCont { get; set; }
    }
}
