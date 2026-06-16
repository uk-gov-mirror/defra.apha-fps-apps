/*
 * TRANSFORMENGINE MIGRATION — ContributionSummarySummaryRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed.
 *   - Fields derived from pageData.summary and renderSummary() in source/ui/fps/contribution_summary.js,
 *     and from the summary box HTML elements in source/ui/fps/contribution_summary.html.
 *   - Summary box fields mapped:
 *       cs-total-budget-bids       -> TotalBudgetBids
 *       cs-contribution-target     -> ContributionTarget
 *       cs-total-to-recover        -> TotalToRecover
 *       cs-total-time-fee          -> TotalTimeFeeFromPlanHrs
 *       cs-total-surplus           -> TotalTimeSurplusShortfall
 *       cs-assured-time-fee        -> AssuredTimeFeeFromPlanHrs
 *       cs-assured-surplus         -> AssuredTimeSurplusShortfall
 *       cs-checker-oh-rate         -> RateEfficacyOhRate     (nested rateEfficacy.ohRate in JS)
 *       cs-checker-total-cont      -> RateEfficacyTotalCont  (nested rateEfficacy.totalCont in JS)
 *   - Nested rateEfficacy object flattened to top-level properties to avoid nested DTO leakage
 *     and keep contracts simple per Phase 1 rules.
 *   - All monetary values typed as decimal; percentage values typed as int.
 *
 * PRESERVED:
 *   - All semantic names match the JS summary object fields verbatim where possible.
 *   - Computed derivations (TotalTimeSurplusShortfall = TotalTimeFeeFromPlanHrs - TotalToRecover)
 *     are server-calculated and returned ready-to-display; frontend need not recompute.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether RateEfficacyOhRate and RateEfficacyTotalCont
 *     should remain flat on this contract or be surfaced as a nested sub-contract in later phases.
 *     Current flat design avoids extra contract proliferation at Phase 1.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for the ContributionSummary aggregate summary boxes.
    /// Returned by GET api/v1/contributionsummary/summary and consumed by
    /// the three summary panels rendered below the data grid
    /// (Total Budget Bids / Contribution Target / Total to Recover;
    ///  Total Time Fee / Surplus; Assured Time Fee / Surplus;
    ///  Rate Efficacy: OH Rate / Total Cont).
    /// </summary>
    public class ContributionSummarySummaryRes
    {
        // TRANSFORMENGINE: TotalBudgetBids — cs-total-budget-bids (summary base box, first row)
        //   Sourced from summary.totalBudgetBids in JS; typed decimal for monetary consistency.
        /// <summary>Total budget bids (£).</summary>
        public decimal TotalBudgetBids { get; set; }

        // TRANSFORMENGINE: ContributionTarget — cs-contribution-target (summary base box, second row)
        //   Sourced from summary.contributionTarget in JS.
        /// <summary>Contribution target (£).</summary>
        public decimal ContributionTarget { get; set; }

        // TRANSFORMENGINE: TotalToRecover — cs-total-to-recover (summary base box, total row)
        //   Derived as TotalBudgetBids + ContributionTarget on the server; returned pre-computed.
        /// <summary>Total amount to recover (£) = TotalBudgetBids + ContributionTarget.</summary>
        public decimal TotalToRecover { get; set; }

        // TRANSFORMENGINE: TotalTimeFeeFromPlanHrs — cs-total-time-fee (Total Time box, first row)
        //   Sourced from summary.totalTimeFeeFromPlanHrs; sum of all row.totalFec values.
        /// <summary>Total time fee derived from planned hours FEC sum (£).</summary>
        public decimal TotalTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalTimeSurplusShortfall — cs-total-surplus (Total Time box, total row)
        //   Derived as TotalTimeFeeFromPlanHrs - TotalToRecover; can be negative (shortfall).
        /// <summary>Total time surplus (+) or shortfall (-) (£).</summary>
        public decimal TotalTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: AssuredTimeFeeFromPlanHrs — cs-assured-time-fee (Assured Time box, first row)
        //   Sourced from summary.assuredTimeFeeFromPlanHrs; sum of all row.assuredFec values.
        /// <summary>Assured time fee derived from assured planned hours FEC sum (£).</summary>
        public decimal AssuredTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredTimeSurplusShortfall — cs-assured-surplus (Assured Time box, total row)
        //   Derived as AssuredTimeFeeFromPlanHrs - TotalToRecover; can be negative (shortfall).
        /// <summary>Assured time surplus (+) or shortfall (-) (£).</summary>
        public decimal AssuredTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: RateEfficacyOhRate — cs-checker-oh-rate (Rate "Efficacy" Checker box, first row)
        //   Flattened from JS rateEfficacy.ohRate; average OH rate across all rows.
        /// <summary>Average overhead rate across all rows (£), used as the Rate Efficacy checker OH Rate.</summary>
        public decimal RateEfficacyOhRate { get; set; }

        // TRANSFORMENGINE: RateEfficacyTotalCont — cs-checker-total-cont / cs-grid-total-cont
        //   Flattened from JS rateEfficacy.totalCont; sum of all row.totalCont values.
        //   Also drives the grid footer total cell (cs-grid-total-cont).
        /// <summary>Total contribution across all rows (£), used as the Rate Efficacy checker Total Cont
        /// and also displayed in the grid footer total row.</summary>
        public decimal RateEfficacyTotalCont { get; set; }
    }
}
