/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: no legacy C# equivalent existed.
 *   - Fields derived from HTML modal form inputs in source/ui/fps/contribution_summary.html
 *     and crudFields array in source/ui/fps/contribution_summary.js.
 *   - Writable input fields only (no Id — Id is route-level on PUT/DELETE).
 *   - AvailHrs / TotalPlanHrs / AssuredPlanHrs typed as double (fractional hours shown in prototype data).
 *   - ChgRate / TotalFec / AssuredFec / OhRate / TotalCont typed as decimal (currency/rate values).
 *   - TotalPctPlanned / AssuredPctPlanned typed as int (0-100 percentage, integer step in HTML).
 *   - ProfitCentre and FpsYear included as context discriminators required by the POST route
 *     (resource-centre dropdown drives the endpoint; year is implicit from the active FPS year).
 *
 * PRESERVED:
 *   - All field names match camelCase prototype identifiers verbatim (Wg, Grade, AvailHrs, etc.).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether FpsYear should be carried in the request body or
 *     resolved server-side from the active year context. Update if resolved server-side.
 */

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for creating or updating a ContributionSummary row.
    /// Contains only writable fields sourced from the CRUD modal form inputs
    /// (contribution_summary.html / contribution_summary.js crudFields array).
    /// </summary>
    public class ContributionSummaryReq
    {
        // TRANSFORMENGINE: Wg — maps to csCrudWg text input (required, string)
        /// <summary>Work group code, e.g. "BAC1".</summary>
        public string Wg { get; set; } = null!;

        // TRANSFORMENGINE: Grade — maps to csCrudGrade text input (required, string)
        /// <summary>Grade code, e.g. "C_BAC1".</summary>
        public string Grade { get; set; } = null!;

        // TRANSFORMENGINE: AvailHrs — maps to csCrudAvailHrs number input (step 0.1, min 0)
        /// <summary>Available hours for this work group / grade combination.</summary>
        public double AvailHrs { get; set; }

        // TRANSFORMENGINE: ChgRate — maps to csCrudChgRate number input (step 0.1, min 0; rendered as £ currency)
        /// <summary>Charge rate (£ per hour).</summary>
        public decimal ChgRate { get; set; }

        // TRANSFORMENGINE: TotalPlanHrs — maps to csCrudTotalPlanHrs number input (step 0.1, min 0)
        /// <summary>Total planned hours (Total Planned Time group).</summary>
        public double TotalPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalFec — maps to csCrudTotalFec number input (step 1, min 0; rendered as £ currency)
        /// <summary>Total FEC value (£) for planned time.</summary>
        public decimal TotalFec { get; set; }

        // TRANSFORMENGINE: TotalPctPlanned — maps to csCrudTotalPctPlanned number input (step 1, min 0, max 100)
        /// <summary>Total percentage planned (0–100).</summary>
        public int TotalPctPlanned { get; set; }

        // TRANSFORMENGINE: AssuredPlanHrs — maps to csCrudAssuredPlanHrs number input (step 0.1, min 0)
        /// <summary>Assured planned hours (Assured Planned Time group).</summary>
        public double AssuredPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredFec — maps to csCrudAssuredFec number input (step 1, min 0; rendered as £ currency)
        /// <summary>Assured FEC value (£).</summary>
        public decimal AssuredFec { get; set; }

        // TRANSFORMENGINE: AssuredPctPlanned — maps to csCrudAssuredPctPlanned number input (step 1, min 0, max 100)
        /// <summary>Assured percentage planned (0–100).</summary>
        public int AssuredPctPlanned { get; set; }

        // TRANSFORMENGINE: OhRate — maps to csCrudOhRate number input (step 0.1, min 0; Rate "Efficacy" Checker column)
        /// <summary>Overhead rate (£ per hour) used in the Rate Efficacy checker.</summary>
        public decimal OhRate { get; set; }

        // TRANSFORMENGINE: TotalCont — maps to csCrudTotalCont number input (step 1, min 0; currency)
        /// <summary>Total contribution (£).</summary>
        public decimal TotalCont { get; set; }

        // TRANSFORMENGINE: ProfitCentre — resource-centre discriminator from cs-resource-centre dropdown;
        //   drives GET filter and is required on POST/PUT to scope the row to the correct profit centre.
        /// <summary>Profit centre / resource centre code, e.g. "Bact".</summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE TODO: Confirm whether FpsYear should remain in the request body or be resolved
        //   server-side from active year context. Present here as writable field pending confirmation.
        /// <summary>FPS financial year (e.g. 2026).</summary>
        public int FpsYear { get; set; }
    }
}
