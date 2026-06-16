/*
 * TRANSFORMENGINE MIGRATION — FpsContributionSummaryApiClientStub.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: compile-time stub so FpsApiClient satisfies IFpsApiClient.FpsContributionSummary
 *     while the real FpsContributionSummaryApiClient is generated in Phase 9.
 *   - All six interface methods throw NotImplementedException — no runtime use is expected
 *     until Phase 9 replaces this stub.
 *
 * PRESERVED:
 *   - Interface contract: all six method signatures from IFpsContributionSummaryApiClient.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO STUB: Replace FpsContributionSummaryApiClientStub with the real
 *     FpsContributionSummaryApiClient implementation in Phase 9.
 *   - Update FpsApiClient.cs to use new FpsContributionSummaryApiClient(http, mapper) instead
 *     of new FpsContributionSummaryApiClientStub() once Phase 9 is complete.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    // TRANSFORMENGINE TODO STUB - replace with real FpsContributionSummaryApiClient in Phase 9
    /// <summary>
    /// Compile-time stub for IFpsContributionSummaryApiClient.
    /// All methods throw <see cref="NotImplementedException"/>.
    /// Replace with <c>FpsContributionSummaryApiClient</c> in Phase 9.
    /// </summary>
    internal sealed class FpsContributionSummaryApiClientStub : IFpsContributionSummaryApiClient
    {
        // TRANSFORMENGINE TODO STUB: implement GetByProfitCentreAsync in Phase 9
        public Task<ApiResponseDto<List<ContributionSummaryDto>>> GetByProfitCentreAsync(
            QueryParameters<string> query, string profitCentre)
            => throw new NotImplementedException(
                "TRANSFORMENGINE STUB: FpsContributionSummaryApiClient not yet implemented — Phase 9 pending.");

        // TRANSFORMENGINE TODO STUB: implement GetSummaryAsync in Phase 9
        public Task<ApiResponseDto<ContributionSummarySummaryDto>> GetSummaryAsync(
            string profitCentre, int? fpsYear = null)
            => throw new NotImplementedException(
                "TRANSFORMENGINE STUB: FpsContributionSummaryApiClient not yet implemented — Phase 9 pending.");

        // TRANSFORMENGINE TODO STUB: implement GetByIdAsync in Phase 9
        public Task<ApiResponseDto<ContributionSummaryDto>> GetByIdAsync(int id)
            => throw new NotImplementedException(
                "TRANSFORMENGINE STUB: FpsContributionSummaryApiClient not yet implemented — Phase 9 pending.");

        // TRANSFORMENGINE TODO STUB: implement CreateAsync in Phase 9
        public Task<ApiResponseDto<ContributionSummaryDto>> CreateAsync(ContributionSummaryDto dto)
            => throw new NotImplementedException(
                "TRANSFORMENGINE STUB: FpsContributionSummaryApiClient not yet implemented — Phase 9 pending.");

        // TRANSFORMENGINE TODO STUB: implement UpdateAsync in Phase 9
        public Task<ApiResponseDto<ContributionSummaryDto>> UpdateAsync(int id, ContributionSummaryDto dto)
            => throw new NotImplementedException(
                "TRANSFORMENGINE STUB: FpsContributionSummaryApiClient not yet implemented — Phase 9 pending.");

        // TRANSFORMENGINE TODO STUB: implement DeleteAsync in Phase 9
        public Task<ApiResponseDto<bool>> DeleteAsync(int id)
            => throw new NotImplementedException(
                "TRANSFORMENGINE STUB: FpsContributionSummaryApiClient not yet implemented — Phase 9 pending.");
    }
}
