/*
 * TRANSFORMENGINE MIGRATION — AsuViewService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-02
 *
 * CHANGED:
 *   - New frontend service implementation created for the AsuView resource family
 *   - Injects IFpsApiClient (aggregate client) and delegates all calls via _client.FpsAsuView
 *   - GetAsuViewAsync delegates to _client.FpsAsuView.GetAsuViewAsync(query, animalType)
 *   - GetAnimalTypeLookupAsync delegates to _client.FpsAsuView.GetAnimalTypeLookupAsync()
 *   - _client field is private readonly (Sonar S2933 compliance)
 *
 * PRESERVED:
 *   - Thin delegate pattern — zero business logic in this class
 *   - All method signatures exactly match IAsuViewService interface
 *   - No conditional logic, no data transformation (Sonar S4144 intentional — thin delegates)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Phase 9 — FpsAsuViewApiClient must implement IFpsAsuViewApiClient
 *     and be registered on the IFpsApiClient aggregate (FpsAsuView property already present
 *     in IFpsApiClient from Phase 7)
 *   - TRANSFORMENGINE TODO: Phase 10 — register IAsuViewService → AsuViewService in
 *     Apha.FPSApps.Web/Extensions/ServiceCollectionExtension.cs
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    // TRANSFORMENGINE: AsuViewService — thin delegate service for the ASU View resource family.
    // Forwards all calls to IFpsApiClient.FpsAsuView without adding any business logic.
    public class AsuViewService : IAsuViewService
    {
        // TRANSFORMENGINE: _client is private readonly — Sonar S2933 compliance.
        // Aggregate FPS API client; ASU View operations accessed via .FpsAsuView property.
        private readonly IFpsApiClient _client;

        public AsuViewService(IFpsApiClient client)
        {
            _client = client;
        }

        // TRANSFORMENGINE: thin delegate — forwards GetAsuViewAsync to FpsAsuView API client.
        // No business logic: animalType validation is enforced by the backend controller (400 on null/empty).
        /// <inheritdoc />
        public async Task<ApiResponseDto<List<AsuViewDto>>> GetAsuViewAsync(QueryParameters<string> query, string animalType)
        {
            return await _client.FpsAsuView.GetAsuViewAsync(query, animalType);
        }

        // TRANSFORMENGINE: thin delegate — forwards GetAnimalTypeLookupAsync to FpsAsuView API client.
        /// <inheritdoc />
        public async Task<ApiResponseDto<List<AnimalDto>>> GetAnimalTypeLookupAsync()
        {
            return await _client.FpsAsuView.GetAnimalTypeLookupAsync();
        }
    }
}
