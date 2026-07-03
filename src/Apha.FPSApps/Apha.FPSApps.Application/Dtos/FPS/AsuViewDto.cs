/*
 * TRANSFORMENGINE MIGRATION — AsuViewDto.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New DTO created for the AsuView resource family (no prior equivalent existed)
 *   - Mirrors Apha.Common.Contracts.FPS.AsuViewRes field-for-field
 *     (same property names, types, and nullability)
 *   - Resides in Apha.FPSApps.Application.Dtos.FPS namespace (frontend Application layer)
 *
 * PRESERVED:
 *   - All property names exactly match AsuViewRes
 *     (Id, AnimalType, Project, AnimalDays, Cost)
 *   - Nullability: string? for nullable string fields;
 *     value types remain non-nullable (int, double, decimal)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify Cost type (decimal vs double) against the actual DB column
 *     type in mabarchive once the DataAccess layer implementation is finalised in Phase 4
 */

namespace Apha.FPSApps.Application.Dtos.FPS
{
    // TRANSFORMENGINE: AsuViewDto mirrors AsuViewRes — same shape, frontend Application namespace.
    // Used as the return element type for IFpsAsuViewApiClient.GetAsuViewAsync and mapped from
    // AsuViewRes via FpsApiDtoMapper (Phase 10).
    public class AsuViewDto
    {
        /// <summary>
        /// Row identifier. Maps to row.id in fps_asuview.js (used by edit/delete modal operations).
        /// Mirrors AsuViewRes.Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The animal type for this usage record.
        /// Maps to the 'animalType' field in the JS data model and the Animal Type filter dropdown.
        /// Mirrors AsuViewRes.AnimalType.
        /// </summary>
        public string? AnimalType { get; set; }

        /// <summary>
        /// The project code or name for this animal usage record.
        /// Maps to the 'project' column in the ASU grid (fps_asuview.js initializeAsuGrid).
        /// Mirrors AsuViewRes.Project.
        /// </summary>
        public string? Project { get; set; }

        /// <summary>
        /// Number of animal days used by this project.
        /// Maps to the 'animalDays' field; summed for Total Animal Days in updateAsuSummary().
        /// Mirrors AsuViewRes.AnimalDays.
        /// </summary>
        public double AnimalDays { get; set; }

        /// <summary>
        /// Cost associated with the animal days for this project.
        /// Maps to the 'cost' field; formatted via formatMoney() in the grid and summed for Total Cost.
        /// Mirrors AsuViewRes.Cost.
        /// </summary>
        public decimal Cost { get; set; }
    }
}
