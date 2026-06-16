/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: frontend service implementation for the ContributionSummary resource (frmTimeSellerPC).
 *   - Implements IContributionSummaryService — thin delegate pattern; zero business logic.
 *   - Injects IFpsApiClient (aggregate) and delegates every method to
 *     _fpsClient.FpsContributionSummary (IFpsContributionSummaryApiClient).
 *   - Six delegate methods, each a single-line return await:
 *       GetByProfitCentreAsync — forwarded to _fpsClient.FpsContributionSummary.GetByProfitCentreAsync
 *       GetSummaryAsync        — forwarded to _fpsClient.FpsContributionSummary.GetSummaryAsync
 *       GetByIdAsync           — forwarded to _fpsClient.FpsContributionSummary.GetByIdAsync
 *       CreateAsync            — forwarded to _fpsClient.FpsContributionSummary.CreateAsync
 *       UpdateAsync            — forwarded to _fpsClient.FpsContributionSummary.UpdateAsync
 *       DeleteAsync            — forwarded to _fpsClient.FpsContributionSummary.DeleteAsync
 *   - _fpsClient is private readonly (Sonar S2933 compliance).
 *   - Constructor null-guards _fpsClient (ArgumentNullException, matching GradeService pattern).
 *
 * PRESERVED:
 *   - Method naming convention matches GradeService and all other FPS frontend service implementations.
 *   - No business logic — this file must remain a pure pass-through layer.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Register IContributionSummaryService / ContributionSummaryService in
 *     Apha.FPSApps.Web Extensions/ServiceCollectionExtension.cs (PENDING in Interface changes log).
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear should remain nullable on GetSummaryAsync
 *     or always be supplied from IFpsRequestContext once context wiring is complete.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Frontend service implementation for the ContributionSummary resource (frmTimeSellerPC).
    /// Thin delegate — all calls forwarded to <see cref="IFpsApiClient.FpsContributionSummary"/> with no business logic.
    /// </summary>
    public class ContributionSummaryService : IContributionSummaryService
    {
        // TRANSFORMENGINE: private readonly _fpsClient — Sonar S2933 compliance; aggregate API client injected via DI
        private readonly IFpsApiClient _fpsClient;

        public ContributionSummaryService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient ?? throw new ArgumentNullException(nameof(fpsClient));
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsContributionSummary.GetByProfitCentreAsync (no logic added)
        public async Task<ApiResponseDto<List<ContributionSummaryDto>>> GetByProfitCentreAsync(
            QueryParameters<string> query,
            string profitCentre)
        {
            return await _fpsClient.FpsContributionSummary.GetByProfitCentreAsync(query, profitCentre);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsContributionSummary.GetSummaryAsync
        public async Task<ApiResponseDto<ContributionSummarySummaryDto>> GetSummaryAsync(
            string profitCentre,
            int? fpsYear = null)
        {
            return await _fpsClient.FpsContributionSummary.GetSummaryAsync(profitCentre, fpsYear);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsContributionSummary.GetByIdAsync
        public async Task<ApiResponseDto<ContributionSummaryDto>> GetByIdAsync(int id)
        {
            return await _fpsClient.FpsContributionSummary.GetByIdAsync(id);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsContributionSummary.CreateAsync
        public async Task<ApiResponseDto<ContributionSummaryDto>> CreateAsync(ContributionSummaryDto dto)
        {
            return await _fpsClient.FpsContributionSummary.CreateAsync(dto);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsContributionSummary.UpdateAsync
        public async Task<ApiResponseDto<ContributionSummaryDto>> UpdateAsync(int id, ContributionSummaryDto dto)
        {
            return await _fpsClient.FpsContributionSummary.UpdateAsync(id, dto);
        }

        // TRANSFORMENGINE: thin delegate — forwards to _fpsClient.FpsContributionSummary.DeleteAsync
        public async Task<ApiResponseDto<bool>> DeleteAsync(int id)
        {
            return await _fpsClient.FpsContributionSummary.DeleteAsync(id);
        }
    }
}
