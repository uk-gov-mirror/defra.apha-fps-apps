/*
 * TRANSFORMENGINE MIGRATION — AsuViewRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New contract created for the ASU View resource family (no prior equivalent existed)
 *   - Fields derived from the fps_asuview.js data model (id, animalType, project, animalDays, cost)
 *     and the grid column definitions (Project, Animal Days, Cost)
 *   - AnimalDays maps to the 'animalDays' field used in summary totals (updateAsuSummary)
 *   - Cost maps to the 'cost' field rendered via formatMoney()
 *   - AnimalType carried on each row so clients can group or verify the filter result
 *
 * PRESERVED:
 *   - All field names and types align with the source JS prototype data model
 *   - Id preserved to support edit/delete round-trips (openAsuUsageEditModal uses row.id)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify Cost type (decimal vs double) against the actual DB column
 *     type in mabarchive once the DataAccess layer is implemented in Phase 4
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Res contract — full RecordSource surface for CRUD responses
    public class AsuViewRes
    {
        /// <summary>
        /// Row identifier. Maps to row.id in fps_asuview.js (used by edit/delete operations).
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The animal type for this usage record.
        /// Maps to the 'animalType' field in the JS data model and the AnimalType filter dropdown.
        /// </summary>
        public string? AnimalType { get; set; }

        /// <summary>
        /// The project code or name for this animal usage record.
        /// Maps to the 'project' column in the ASU grid (fps_asuview.js initializeAsuGrid).
        /// </summary>
        public string? Project { get; set; }

        /// <summary>
        /// Number of animal days used by this project.
        /// Maps to the 'animalDays' field; summed for Total Animal Days in updateAsuSummary().
        /// </summary>
        public double AnimalDays { get; set; }

        /// <summary>
        /// Cost associated with the animal days for this project.
        /// Maps to the 'cost' field; formatted via formatMoney() in the grid and summed for Total Cost.
        /// </summary>
        public decimal Cost { get; set; }
    }
}
