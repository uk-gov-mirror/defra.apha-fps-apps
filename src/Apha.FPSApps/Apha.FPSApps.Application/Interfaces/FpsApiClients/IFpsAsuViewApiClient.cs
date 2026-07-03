/*
 * TRANSFORMENGINE MIGRATION — IFpsAsuViewApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New API client interface created for the AsuView resource family
 *   - GetAsuViewAsync mirrors backend GET /api/v1/animal/asu-view?animalType=X
 *     (QueryParameters<string> + required string animalType matches AnimalController.GetAsuViewAsync)
 *   - GetAnimalTypeLookupAsync mirrors backend GET /api/v1/animal (all animals with DailyRate)
 *     for populating the Animal Type dropdown; reuses AnimalDto already present in frontend
 *
 * PRESERVED:
 *   - Method parameter names and types exactly match backend endpoint signatures
 *     (animalType kept as required string, not nullable, to mirror controller behaviour)
 *   - Return types wrapped in ApiResponseDto<T> per project envelope contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm animalType nullability — controller throws 400 on null/empty;
 *     client should pass a non-null value but the interface leaves enforcement to the caller
 *   - TRANSFORMENGINE TODO: Phase 9 — FpsAsuViewApiClient.cs must implement this interface
 *     calling GET /api/v1/animal/asu-view and GET /api/v1/animal
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    // TRANSFORMENGINE: IFpsAsuViewApiClient — thin HTTP contract for the ASU View resource family.
    // Binds to backend AnimalController endpoints confirmed in Phase 6:
    //   CRUD:   GET /api/v1/animal/asu-view?animalType=X  (paged AsuViewRes)
    //   Lookup: GET /api/v1/animal                         (all AnimalRes for dropdown)
    public interface IFpsAsuViewApiClient
    {
        // TRANSFORMENGINE: mirrors AnimalController.GetAsuViewAsync —
        // GET /api/v1/animal/asu-view?animalType=X&<pagination>
        // animalType is a required business filter (controller rejects null/empty with 400).
        /// <summary>
        /// Returns a paged list of ASU View records filtered by the given animal type.
        /// Maps to GET /api/v1/animal/asu-view on the backend.
        /// </summary>
        /// <param name="query">Pagination, sorting, and search parameters.</param>
        /// <param name="animalType">Required. The animal type to filter by (value of the Animal Type dropdown selection).</param>
        Task<ApiResponseDto<List<AsuViewDto>>> GetAsuViewAsync(QueryParameters<string> query, string animalType);

        // TRANSFORMENGINE: mirrors AnimalController GET /api/v1/animal (all animals, no pagination)
        // used to populate the Animal Type dropdown in fps_asuview.html.
        // Returns AnimalDto list (same type used by IFpsAnimalApiClient.GetAllAnimalsAsync()).
        /// <summary>
        /// Returns all animal master records for populating the Animal Type dropdown.
        /// Maps to GET /api/v1/animal on the backend.
        /// </summary>
        Task<ApiResponseDto<List<AnimalDto>>> GetAnimalTypeLookupAsync();
    }
}
