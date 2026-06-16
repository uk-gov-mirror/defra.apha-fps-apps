/*
 * TRANSFORMENGINE MIGRATION — ContributionSummarySummaryDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: service-layer DTO for the ContributionSummaryTotals aggregate result.
 *   - All 9 summary fields mirrored from ContributionSummaryTotals (Phase 2 [DONE]) and
 *     ContributionSummarySummaryRes (Phase 1 [DONE]).
 *   - Used by IContributionSummaryService.GetSummaryAsync as the return type for summary-box
 *     totals passed up to the API controller (Phase 5) and eventually to the frontend.
 *   - Field types match ContributionSummaryTotals exactly for zero-friction AutoMapper mapping.
 *
 * PRESERVED:
 *   - All field names match ContributionSummarySummaryRes (Phase 1 [DONE]) and
 *     ContributionSummaryTotals (Phase 2 [DONE]) verbatim.
 *   - Flat structure (no nested objects) mirrors the Phase 1 design decision to avoid
 *     nested DTO leakage.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether RateEfficacyOhRate / RateEfficacyTotalCont should
 *     remain flat on this DTO or be surfaced as a nested sub-DTO in later phases.
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data transfer object for the ContributionSummary aggregate summary-box totals.
    /// Service-layer contract returned by <see cref="Apha.FPS.Application.Interfaces.IContributionSummaryService.GetSummaryAsync"/>.
    /// Field set mirrors <see cref="Apha.FPS.Core.Entities.ContributionSummaryTotals"/> for zero-friction AutoMapper mapping.
    /// </summary>
    public class ContributionSummarySummaryDto
    {
        // TRANSFORMENGINE: TotalBudgetBids — sourced from pc.conttarget; maps to ContributionSummaryTotals.TotalBudgetBids
        /// <summary>Total budget bids (£).</summary>
        public decimal TotalBudgetBids { get; set; }

        // TRANSFORMENGINE: ContributionTarget — target contribution (£); maps to ContributionSummaryTotals.ContributionTarget
        /// <summary>Contribution target set for this profit centre and FPS year (£).</summary>
        public decimal ContributionTarget { get; set; }

        // TRANSFORMENGINE: TotalToRecover — derived: TotalBudgetBids + ContributionTarget;
        //   maps to ContributionSummaryTotals.TotalToRecover
        /// <summary>Total amount to recover (£) = TotalBudgetBids + ContributionTarget.</summary>
        public decimal TotalToRecover { get; set; }

        // TRANSFORMENGINE: TotalTimeFeeFromPlanHrs — SUM(TotalFec) across all rows;
        //   maps to ContributionSummaryTotals.TotalTimeFeeFromPlanHrs
        /// <summary>Sum of TotalFec across all rows (£) — Total Time Fee from Planned Hours.</summary>
        public decimal TotalTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalTimeSurplusShortfall — derived: TotalTimeFeeFromPlanHrs - TotalToRecover (can be negative);
        //   maps to ContributionSummaryTotals.TotalTimeSurplusShortfall
        /// <summary>Total time surplus (+) or shortfall (-) (£).</summary>
        public decimal TotalTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: AssuredTimeFeeFromPlanHrs — SUM(AssuredFec) across all rows;
        //   maps to ContributionSummaryTotals.AssuredTimeFeeFromPlanHrs
        /// <summary>Sum of AssuredFec across all rows (£) — Assured Time Fee from Planned Hours.</summary>
        public decimal AssuredTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredTimeSurplusShortfall — derived: AssuredTimeFeeFromPlanHrs - TotalToRecover (can be negative);
        //   maps to ContributionSummaryTotals.AssuredTimeSurplusShortfall
        /// <summary>Assured time surplus (+) or shortfall (-) (£).</summary>
        public decimal AssuredTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: RateEfficacyOhRate — AVG(OhRate) across all rows;
        //   maps to ContributionSummaryTotals.RateEfficacyOhRate
        /// <summary>Average overhead rate across all rows (£) — Rate Efficacy Checker OH Rate.</summary>
        public decimal RateEfficacyOhRate { get; set; }

        // TRANSFORMENGINE: RateEfficacyTotalCont — SUM(TotalCont) across all rows;
        //   maps to ContributionSummaryTotals.RateEfficacyTotalCont
        /// <summary>Sum of TotalCont across all rows (£) — Rate Efficacy Checker Total Cont and grid footer total.</summary>
        public decimal RateEfficacyTotalCont { get; set; }
    }
}
