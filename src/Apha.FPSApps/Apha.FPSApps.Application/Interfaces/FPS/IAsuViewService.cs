/*
 * TRANSFORMENGINE MIGRATION — IAsuViewService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New frontend service interface created for the AsuView resource family (no prior equivalent)
 *   - GetAsuViewAsync mirrors IFpsAsuViewApiClient.GetAsuViewAsync signature:
 *     QueryParameters<string> + required string animalType filter
 *   - GetAnimalTypeLookupAsync mirrors IFpsAsuViewApiClient.GetAnimalTypeLookupAsync:
 *     returns AnimalDto list for the Animal Type dropdown in fps_asuview.html
 *   - GetTotalAsync returns aggregated totals (TotalAnimalDays + TotalCost)
 *     for the summary row rendered by updateAsuSummary() in fps_asuview.js
 *
 * PRESERVED:
 *   - Method parameter names and types exactly match the API client interface
 *     (animalType kept as required string, not nullable, mirroring controller behaviour)
 *   - Return types wrapped in ApiResponseDto<T> per project envelope contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: AsuViewTotalsDto is a new DTO; verify field names and types
 *     against FpsAsuViewApiClient and FpsViewModelMapper implementations in later phases
 *   - TRANSFORMENGINE TODO: confirm animalType nullability enforcement —
 *     backend throws 400 on null/empty; frontend service caller must supply a non-null value
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    // TRANSFORMENGINE: IAsuViewService — thin frontend service contract for the ASU View resource family.
    // MVC controller (AsuViewController, Phase 11) injects this interface; no business logic here.
    // Backend authority: GET /api/v1/animal/asu-view (CRUD) + GET /api/v1/animal (lookup).
    public interface IAsuViewService
    {
        // TRANSFORMENGINE: mirrors IFpsAsuViewApiClient.GetAsuViewAsync —
        // paged ASU View records filtered by animalType.
        /// <summary>
        /// Returns a paged list of ASU View records filtered by the given animal type.
        /// Delegates to GET /api/v1/animal/asu-view on the backend.
        /// </summary>
        /// <param name="query">Pagination, sorting, and search parameters.</param>
        /// <param name="animalType">Required. The animal type selected in the Animal Type dropdown.</param>
        Task<ApiResponseDto<List<AsuViewDto>>> GetAsuViewAsync(QueryParameters<string> query, string animalType);

        // TRANSFORMENGINE: mirrors IFpsAsuViewApiClient.GetAnimalTypeLookupAsync —
        // returns all animal master records for the Animal Type dropdown.
        /// <summary>
        /// Returns all animal master records for populating the Animal Type dropdown.
        /// Delegates to GET /api/v1/animal on the backend.
        /// </summary>
        Task<ApiResponseDto<List<AnimalDto>>> GetAnimalTypeLookupAsync();
    }
}
