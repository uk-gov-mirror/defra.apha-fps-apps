/*
 * TRANSFORMENGINE MIGRATION — ContributionSummary.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: no legacy C# entity existed for this form.
 *   - Entity fields derived from:
 *       (a) PostgreSQL view fps.vqryfrmtimesellerpc (source/pgsql/fps/Views/vqryfrmtimesellerpc.sql)
 *           supplying column semantics: profitcentre, wggrade, avhrs, chargerate, ohr, plannedhours,
 *           fec, appfec, contribution, fpsyear
 *       (b) JS CRUD field definitions in source/ui/fps/contribution_summary.js
 *           supplying the full user-facing field set: Wg, Grade, AvailHrs, ChgRate, TotalPlanHrs,
 *           TotalFec, TotalPctPlanned, AssuredPlanHrs, AssuredFec, AssuredPctPlanned, OhRate, TotalCont
 *       (c) Phase 2 plan notes: Id (int PK), ProfitCentre (string), FpsYear (int).
 *   - ContributionSummaryTotals keyless aggregate class added in this file as a companion
 *     result model for the GetSummaryTotalsAsync repository method (no dedicated table —
 *     computed by LINQ aggregate query, must be marked HasNoKey in EF Core mapping).
 *
 * PRESERVED:
 *   - All field names match the ContributionSummaryRes contract (Phase 1, [DONE]) verbatim
 *     to allow zero-friction AutoMapper mappings in Phase 3.
 *   - Nullable semantics aligned with ProfitCentreGrade.cs precedent (same schema).
 *   - ContributionSummaryTotals field names mirror ContributionSummarySummaryRes (Phase 1)
 *     to allow direct mapping in Phase 3.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether tblkpcontributionsummary (or equivalent) exists
 *     in the PostgreSQL schema as a persistent table or whether rows are always derived
 *     from the view (fps.vqryfrmtimesellerpc). The plan treats this as a writable table;
 *     verify DDL before creating the EF Core map in Phase 4.
 *   - TRANSFORMENGINE TODO: Confirm the exact table/column names with the DBA before
 *     finalising the ContributionSummaryMap (Phase 4).
 */

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Represents a single contribution summary row — one WG/Grade combination
    /// for a given ProfitCentre and FPS financial year.
    /// Backed by tblkpcontributionsummary (or the fps.vqryfrmtimesellerpc projection
    /// when writable storage is not present — confirm in Phase 4).
    /// </summary>
    public partial class ContributionSummary
    {
        // TRANSFORMENGINE: Id — server-assigned integer primary key (plan spec: "Id (int)")
        /// <summary>Primary key — auto-increment identity column.</summary>
        public int Id { get; set; }

        // TRANSFORMENGINE: Wg — maps to wgg.workgroup / data-column "wg" in the JS grid
        /// <summary>Work group code, e.g. "BAC1".</summary>
        public string Wg { get; set; } = null!;

        // TRANSFORMENGINE: Grade — maps to wgg.wggrade / data-column "grade" in the JS grid
        /// <summary>Grade code, e.g. "C_BAC1".</summary>
        public string Grade { get; set; } = null!;

        // TRANSFORMENGINE: AvailHrs — maps to sum(we.hrsavail) AS avhrs in the PostgreSQL view
        /// <summary>Available hours for this WG/Grade combination.</summary>
        public double AvailHrs { get; set; }

        // TRANSFORMENGINE: ChgRate — maps to pcg.chargerate in the PostgreSQL view
        /// <summary>Charge rate (£ per hour).</summary>
        public decimal ChgRate { get; set; }

        // TRANSFORMENGINE: TotalPlanHrs — maps to sum(sjh.plannedhours) AS hrs in the PostgreSQL view
        //   (total = all planned hours; not filtered to "assured" only)
        /// <summary>Total planned hours across all staff job hours for this row.</summary>
        public double TotalPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalFec — maps to sum(sjh.plannedhours) * pcg.chargerate AS fec
        //   in the PostgreSQL view; monetary value in £.
        /// <summary>Total FEC (£) = TotalPlanHrs × ChgRate.</summary>
        public decimal TotalFec { get; set; }

        // TRANSFORMENGINE: TotalPctPlanned — no direct aggregate column in the view;
        //   derived from (TotalPlanHrs / AvailHrs * 100) or stored explicitly when rows
        //   are written by the CRUD endpoint. Stored as int per the JS field definition
        //   (type: "number", min: 0, max: 100) and the plan spec.
        /// <summary>Total percentage of available hours that are planned (0–100).</summary>
        public int TotalPctPlanned { get; set; }

        // TRANSFORMENGINE: AssuredPlanHrs — maps to ah.sumofplannedhours AS apphours in the
        //   PostgreSQL view (approved/assured planned hours from vapphours).
        /// <summary>Assured (approved) planned hours for this row.</summary>
        public double AssuredPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredFec — maps to ah.sumofplannedhours * pcg.chargerate AS appfec
        //   in the PostgreSQL view; monetary value in £.
        /// <summary>Assured FEC (£) = AssuredPlanHrs × ChgRate.</summary>
        public decimal AssuredFec { get; set; }

        // TRANSFORMENGINE: AssuredPctPlanned — percentage of available hours covered by assured
        //   plan; stored explicitly when written by CRUD, same semantics as TotalPctPlanned.
        /// <summary>Assured percentage planned (0–100).</summary>
        public int AssuredPctPlanned { get; set; }

        // TRANSFORMENGINE: OhRate — maps to pcg.ohr (overhead rate) in the PostgreSQL view
        /// <summary>Overhead rate (£ per hour) — the Rate "Efficacy" checker column.</summary>
        public decimal OhRate { get; set; }

        // TRANSFORMENGINE: TotalCont — maps to pcg.ohr * sum(sjh.plannedhours) AS contribution
        //   in the PostgreSQL view; monetary value in £.
        /// <summary>Total contribution (£) = OhRate × TotalPlanHrs.</summary>
        public decimal TotalCont { get; set; }

        // TRANSFORMENGINE: ProfitCentre — maps to pcg.profitcentre / sellingpc in the PostgreSQL view;
        //   discriminator key used by GetByProfitCentreAsync to scope the grid per resource centre.
        /// <summary>Profit centre / resource centre code, e.g. "Bact".</summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: FpsYear — maps to we.fpsyear in the PostgreSQL view;
        //   used to partition rows by financial year.
        /// <summary>FPS financial year (e.g. 2026).</summary>
        public int FpsYear { get; set; }
    }

    /// <summary>
    /// Keyless aggregate result model for the ContributionSummary summary-box totals.
    /// Returned by <see cref="Apha.FPS.Core.Interfaces.IContributionSummaryRepository.GetSummaryTotalsAsync"/>.
    /// Must be registered with <c>HasNoKey()</c> in the EF Core configuration (Phase 4).
    /// Field names mirror <see cref="Apha.Common.Contracts.FPS.ContributionSummarySummaryRes"/>
    /// for zero-friction mapping in Phase 3.
    /// </summary>
    public class ContributionSummaryTotals
    {
        // TRANSFORMENGINE: TotalBudgetBids — sourced from pc.conttarget (tblkpprofitcentre)
        //   via the PostgreSQL view; represents external bids that contribute to the recovery target.
        /// <summary>Total budget bids (£).</summary>
        public decimal TotalBudgetBids { get; set; }

        // TRANSFORMENGINE: ContributionTarget — sourced from pc.conttarget in the view;
        //   the target contribution amount set for this profit centre / year.
        /// <summary>Contribution target set for this profit centre and FPS year (£).</summary>
        public decimal ContributionTarget { get; set; }

        // TRANSFORMENGINE: TotalToRecover — computed as TotalBudgetBids + ContributionTarget
        //   (mirrors recomputeSummaryFromRows logic in contribution_summary.js).
        /// <summary>Total amount to recover (£) = TotalBudgetBids + ContributionTarget.</summary>
        public decimal TotalToRecover { get; set; }

        // TRANSFORMENGINE: TotalTimeFeeFromPlanHrs — aggregate SUM(TotalFec) across all rows
        //   for the selected ProfitCentre/FpsYear; mirrors JS totalTimeFeeFromPlanHrs.
        /// <summary>Sum of TotalFec across all rows (£).</summary>
        public decimal TotalTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalTimeSurplusShortfall — derived: TotalTimeFeeFromPlanHrs - TotalToRecover
        //   (can be negative); mirrors JS totalTimeSurplusShortfall.
        /// <summary>Total time surplus (+) or shortfall (-) (£).</summary>
        public decimal TotalTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: AssuredTimeFeeFromPlanHrs — aggregate SUM(AssuredFec) across all rows;
        //   mirrors JS assuredTimeFeeFromPlanHrs.
        /// <summary>Sum of AssuredFec across all rows (£).</summary>
        public decimal AssuredTimeFeeFromPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredTimeSurplusShortfall — derived: AssuredTimeFeeFromPlanHrs - TotalToRecover
        //   (can be negative); mirrors JS assuredTimeSurplusShortfall.
        /// <summary>Assured time surplus (+) or shortfall (-) (£).</summary>
        public decimal AssuredTimeSurplusShortfall { get; set; }

        // TRANSFORMENGINE: RateEfficacyOhRate — AVG(OhRate) across all rows;
        //   mirrors JS rateEfficacy.ohRate = rows.reduce(sum of ohRate) / rows.length.
        /// <summary>Average overhead rate across all rows (£) — Rate Efficacy Checker.</summary>
        public decimal RateEfficacyOhRate { get; set; }

        // TRANSFORMENGINE: RateEfficacyTotalCont — SUM(TotalCont) across all rows;
        //   mirrors JS rateEfficacy.totalCont and the grid footer cs-grid-total-cont cell.
        /// <summary>Sum of TotalCont across all rows (£) — Rate Efficacy Checker and grid footer.</summary>
        public decimal RateEfficacyTotalCont { get; set; }
    }
}
