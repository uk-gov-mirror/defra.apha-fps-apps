/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed.
 *   - Fields derived from the grid column definitions in source/ui/fps/contribution_summary.js
 *     (DataGridComponent columns array) and pageData.rows sample data.
 *   - Response surface extends the request fields with Id, ProfitCentre, and FpsYear
 *     so callers can drive edit/delete actions from the grid row alone.
 *   - Id typed as int to align with the backend entity primary key (see Phase 2 entity plan).
 *
 * PRESERVED:
 *   - All field names match camelCase prototype identifiers verbatim.
 *   - Nullable annotations on read-only output fields follow existing FPS contract conventions
 *     (see WorkgroupGradeRes.cs — nullable int FpsYear on response side).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm Id type (int vs string) once the Phase 2 entity and
 *     repository are in place. The JS prototype uses string composite ids (e.g. "BAC-1");
 *     the backend entity plan specifies int. Align here if the type changes.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for a single ContributionSummary grid row.
    /// Carries the full API output surface required by CRUD (list/get/create/update/delete)
    /// responses, matching the column fields rendered by the DataGridComponent in
    /// contribution_summary.js.
    /// </summary>
    public class ContributionSummaryRes
    {
        // TRANSFORMENGINE: Id — server-assigned row identifier; used by edit/delete actions
        /// <summary>Primary key for this contribution summary row.</summary>
        public int Id { get; set; }

        // TRANSFORMENGINE: Wg — grid column field "wg", header "WG"
        /// <summary>Work group code, e.g. "BAC1".</summary>
        public string Wg { get; set; } = null!;

        // TRANSFORMENGINE: Grade — grid column field "grade", header "Grade"
        /// <summary>Grade code, e.g. "C_BAC1".</summary>
        public string Grade { get; set; } = null!;

        // TRANSFORMENGINE: AvailHrs — grid column field "availHrs", header "Avail Hrs" (formatted as number)
        /// <summary>Available hours for this work group / grade combination.</summary>
        public double AvailHrs { get; set; }

        // TRANSFORMENGINE: ChgRate — grid column field "chgRate", header "Chg Rate" (formatted as £ currency)
        /// <summary>Charge rate (£ per hour).</summary>
        public decimal ChgRate { get; set; }

        // TRANSFORMENGINE: TotalPlanHrs — grid column field "totalPlanHrs", header "PlanHrs" under Total Planned Time group
        /// <summary>Total planned hours.</summary>
        public double TotalPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalFec — grid column field "totalFec", header "FEC" under Total Planned Time group (£ currency)
        /// <summary>Total FEC value (£) for planned time.</summary>
        public decimal TotalFec { get; set; }

        // TRANSFORMENGINE: TotalPctPlanned — grid column field "totalPctPlanned", header "% Planned" under Total Planned Time group
        /// <summary>Total percentage planned (0–100).</summary>
        public int TotalPctPlanned { get; set; }

        // TRANSFORMENGINE: AssuredPlanHrs — grid column field "assuredPlanHrs", header "PlanHrs" under Assured Planned Time group
        /// <summary>Assured planned hours.</summary>
        public double AssuredPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredFec — grid column field "assuredFec", header "FEC" under Assured Planned Time group (£ currency)
        /// <summary>Assured FEC value (£).</summary>
        public decimal AssuredFec { get; set; }

        // TRANSFORMENGINE: AssuredPctPlanned — grid column field "assuredPctPlanned", header "% Planned" under Assured Planned Time group
        /// <summary>Assured percentage planned (0–100).</summary>
        public int AssuredPctPlanned { get; set; }

        // TRANSFORMENGINE: OhRate — grid column field "ohRate", header "OH Rate" under Rate "Efficacy" Checker group (£ currency)
        /// <summary>Overhead rate (£ per hour) used in the Rate Efficacy checker.</summary>
        public decimal OhRate { get; set; }

        // TRANSFORMENGINE: TotalCont — grid column field "totalCont", header "Total Cont" under Rate "Efficacy" Checker group (£ currency)
        /// <summary>Total contribution (£).</summary>
        public decimal TotalCont { get; set; }

        // TRANSFORMENGINE: ProfitCentre — resource-centre discriminator returned with each row
        //   so the frontend can scope further API calls (e.g. GET summary) without losing context.
        /// <summary>Profit centre / resource centre code, e.g. "Bact".</summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: FpsYear — financial year context returned on each row for display and filter binding
        /// <summary>FPS financial year (e.g. 2026).</summary>
        public int? FpsYear { get; set; }
    }
}
