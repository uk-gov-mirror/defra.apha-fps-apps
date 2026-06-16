/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: service-layer DTO for the ContributionSummary entity.
 *   - All 15 entity fields mirrored from ContributionSummary.cs (Phase 2 [DONE]).
 *   - DTO used by IContributionSummaryService / ContributionSummaryService as the primary
 *     service-layer contract between the Application layer and the API controller.
 *   - Field types match the entity exactly to enable zero-friction AutoMapper ReverseMap().
 *
 * PRESERVED:
 *   - All field names match ContributionSummaryRes (Phase 1 [DONE]) and ContributionSummary
 *     entity (Phase 2 [DONE]) verbatim to ensure zero-conflict AutoMapper mappings in
 *     EntityMapper (this phase) and RequestMapper (Phase 5).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether ProfitCentre / FpsYear should remain on the DTO
 *     or be resolved server-side via IFpsRequestContext once Phase 5 controller context is wired.
 */

namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data transfer object for a single ContributionSummary row.
    /// Service-layer contract between ContributionSummaryService and the API controller.
    /// Field set mirrors the ContributionSummary entity (Phase 2) for zero-friction AutoMapper mapping.
    /// </summary>
    public class ContributionSummaryDto
    {
        // TRANSFORMENGINE: Id — server-assigned int PK; present on GET/PUT/DELETE responses
        /// <summary>Primary key — auto-increment identity column.</summary>
        public int Id { get; set; }

        // TRANSFORMENGINE: Wg — work group code; maps to ContributionSummary.Wg
        /// <summary>Work group code, e.g. "BAC1".</summary>
        public string Wg { get; set; } = null!;

        // TRANSFORMENGINE: Grade — grade code; maps to ContributionSummary.Grade
        /// <summary>Grade code, e.g. "C_BAC1".</summary>
        public string Grade { get; set; } = null!;

        // TRANSFORMENGINE: AvailHrs — available hours; maps to ContributionSummary.AvailHrs (double)
        /// <summary>Available hours for this work group / grade combination.</summary>
        public double AvailHrs { get; set; }

        // TRANSFORMENGINE: ChgRate — charge rate (£/hr); maps to ContributionSummary.ChgRate (decimal)
        /// <summary>Charge rate (£ per hour).</summary>
        public decimal ChgRate { get; set; }

        // TRANSFORMENGINE: TotalPlanHrs — total planned hours; maps to ContributionSummary.TotalPlanHrs (double)
        /// <summary>Total planned hours (all planned, not filtered to assured only).</summary>
        public double TotalPlanHrs { get; set; }

        // TRANSFORMENGINE: TotalFec — total FEC (£); maps to ContributionSummary.TotalFec (decimal)
        /// <summary>Total FEC (£) = TotalPlanHrs × ChgRate.</summary>
        public decimal TotalFec { get; set; }

        // TRANSFORMENGINE: TotalPctPlanned — total % planned (0-100); maps to ContributionSummary.TotalPctPlanned (int)
        /// <summary>Total percentage of available hours that are planned (0–100).</summary>
        public int TotalPctPlanned { get; set; }

        // TRANSFORMENGINE: AssuredPlanHrs — assured planned hours; maps to ContributionSummary.AssuredPlanHrs (double)
        /// <summary>Assured (approved) planned hours for this row.</summary>
        public double AssuredPlanHrs { get; set; }

        // TRANSFORMENGINE: AssuredFec — assured FEC (£); maps to ContributionSummary.AssuredFec (decimal)
        /// <summary>Assured FEC (£) = AssuredPlanHrs × ChgRate.</summary>
        public decimal AssuredFec { get; set; }

        // TRANSFORMENGINE: AssuredPctPlanned — assured % planned (0-100); maps to ContributionSummary.AssuredPctPlanned (int)
        /// <summary>Assured percentage planned (0–100).</summary>
        public int AssuredPctPlanned { get; set; }

        // TRANSFORMENGINE: OhRate — overhead rate (£/hr); maps to ContributionSummary.OhRate (decimal)
        /// <summary>Overhead rate (£ per hour) — Rate Efficacy Checker column.</summary>
        public decimal OhRate { get; set; }

        // TRANSFORMENGINE: TotalCont — total contribution (£); maps to ContributionSummary.TotalCont (decimal)
        /// <summary>Total contribution (£) = OhRate × TotalPlanHrs.</summary>
        public decimal TotalCont { get; set; }

        // TRANSFORMENGINE: ProfitCentre — resource centre discriminator; maps to ContributionSummary.ProfitCentre
        /// <summary>Profit centre / resource centre code, e.g. "Bact".</summary>
        public string ProfitCentre { get; set; } = null!;

        // TRANSFORMENGINE: FpsYear — financial year partition; maps to ContributionSummary.FpsYear (int)
        /// <summary>FPS financial year (e.g. 2026).</summary>
        public int FpsYear { get; set; }
    }
}
