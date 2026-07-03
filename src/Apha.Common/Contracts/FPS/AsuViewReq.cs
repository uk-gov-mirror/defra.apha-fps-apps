/*
 * TRANSFORMENGINE MIGRATION — AsuViewReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New contract created for the ASU View resource family (no prior equivalent existed)
 *   - Derived from the animalTypeFilter hidden input in fps_asuview.html
 *     and the animalType filter logic in fps_asuview.js (getCurrentAnimalType / getFilteredRecords)
 *   - AnimalType maps to the query parameter consumed by GET /api/v1/animal/asu-view
 *
 * PRESERVED:
 *   - Field name and semantics align with the source JS prototype (animalType)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm whether AnimalType should be required ([Required]) or nullable
 *     once the backend controller validation strategy is finalised in Phase 5
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Req contract — writable input fields only (PB-14 rule: no response-only fields)
    public class AsuViewReq
    {
        /// <summary>
        /// The animal type to filter by. Maps to the animalTypeFilter hidden input
        /// (id="asu-animal-type-value") in fps_asuview.html.
        /// </summary>
        public string? AnimalType { get; set; }
    }
}
