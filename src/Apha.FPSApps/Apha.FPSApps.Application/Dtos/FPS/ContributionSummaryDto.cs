/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: frontend-layer DTO for a single ContributionSummary row.
 *   - Mirrors Apha.FPS.Application.Dtos.ContributionSummaryDto (Phase 3 [DONE]) in the FPSApps
 *     application namespace so the frontend application and infrastructure layers never
 *     take a hard dependency on the backend project.
 *   - All 15 properties copied verbatim (identical names, types, and nullability) to allow
 *     zero-conflict AutoMapper mappings in FpsApiDtoMapper (Phase 8).
 *
 * PRESERVED:
 *   - All field names and types match the backend DTO exactly (case-sensitive).
 *   - No Lk_ prefix naming — entity field names used directly.
 *   - Nullable reference type semantics: required reference fields use = null! sentinel;
 *     optional primitives use their default value types.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether ProfitCentre / FpsYear should remain on the DTO
 *     or be resolved server-side via IFpsRequestContext once Phase 5 controller context is wired.
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for a single ContributionSummary row.
    /// Same shape as Apha.FPS.Application.Dtos.ContributionSummaryDto (backend Phase 3).
    /// Used as the service / API-client contract in the FPSApps frontend application layer.
    /// Route: api/v1/contributionsummary.
    /// </summary>
    public class ContributionSummaryDto
    {
        // TRANSFORMENGINE: Id — server-assigned int PK; present on GET/PUT/DELETE responses
        /// <summary>Primary key — auto-increment identity column.</summary>
        public int Id { get; set; }

        // TRANSFORMENGINE: Wg — work group code; mirrors ContributionSummary.Wg
        /// <summary>Work group code, e.g. "BAC1".</summary>
        public string Wg { get; set; } = null!;

        // TRANSFORMENGINE: Grade — grade code; mirrors ContributionSummary.Grade
        /// <summary>Grade code, e.g. "C_BAC1".</summary>
        public string Grade { get; set; } = null!;

        // TRANSFORMENGINE: AvailHrs — available hours (double); mirrors ContributionSummary.AvailHrs
        /// <summary>Available hours for this work group / grade combination.</summary>
        public double AvailHrs { get; set; }

        // TRANSFORMENGINE: ChgRate — charge rate (£/hr, decimal); mirrors ContributionSummary.ChgRate
        /// <summary>Charge rate (£ per hour).</summary>
        public decimal ChgRate { get; set; }

        // TRANSFORMENGINE: TotalPlanHrs — total planned hours (double); mirrors ContributionSummary.TotalPlanHrs
        /// <summary>Total planned hours (all planned, not filtered to assured only).</summary>
        public double TotalPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalFec — total FEC (£, decimal); mirrors ContributionSummary.TotalFec
        /// <summary>Total FEC (£) = TotalPlanHrs × ChgRate.</summary>
        public decimal TotalFec { get; set; }

        // TRANSFORMENGINE: TotalPctPlanned — total % planned (0-100, int); mirrors ContributionSummary.TotalPctPlanned
        /// <summary>Total percentage of available hours that are planned (0–100).</summary>
        public int TotalPctPlanned { get; set; }

        // TRANSFORMENGINE: AssuredPlanHrs — assured planned hours (double); mirrors ContributionSummary.AssuredPlanHrs
        /// <summary>Assured (approved) planned hours for this row.</summary>
        public double AssuredPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredFec — assured FEC (£, decimal); mirrors ContributionSummary.AssuredFec
        /// <summary>Assured FEC (£) = AssuredPlanHrs × ChgRate.</summary>
        public decimal AssuredFec { get; set; }

        // TRANSFORMENGINE: AssuredPctPlanned — assured % planned (0-100, int); mirrors ContributionSummary.AssuredPctPlanned
        /// <summary>Assured percentage planned (0–100).</summary>
        public int AssuredPctPlanned { get; set; }

        // TRANSFORMENGINE: OhRate — overhead rate (£/hr, decimal); mirrors ContributionSummary.OhRate
        /// <summary>Overhead rate (£ per hour) — Rate Efficacy Checker column.</summary>
        public decimal OhRate { get; set; }

        // TRANSFORMENGINE: TotalCont — total contribution (£, decimal); mirrors ContributionSummary.TotalCont
        /// <summary>Total contribution (£) = OhRate × TotalPlanHrs.</summary>
        public decimal TotalCont { get; set; }

        // TRANSFORMENGINE: ProfitCentre — resource centre discriminator; mirrors ContributionSummary.ProfitCentre
        /// <summary>Profit centre / resource centre code, e.g. "Bact".</summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: FpsYear — financial year partition (int); mirrors ContributionSummary.FpsYear
        /// <summary>FPS financial year (e.g. 2026).</summary>
        public int FpsYear { get; set; }
    }
}
