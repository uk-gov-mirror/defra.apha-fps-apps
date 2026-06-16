/*
 * TRANSFORMENGINE MIGRATION — IFpsContributionSummaryApiClient.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 7 — Frontend DTOs + API Client Interfaces (Steps 10-11)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: frontend API client interface for the ContributionSummary resource.
 *   - Six method signatures derived directly from the six REST endpoints on
 *     ContributionSummaryController (Phase 5 [DONE]):
 *       GET  api/v1/contributionsummary              -> GetByProfitCentreAsync (paged + profitCentre filter)
 *       GET  api/v1/contributionsummary/summary      -> GetSummaryAsync (aggregate summary-box totals)
 *       GET  api/v1/contributionsummary/{id}         -> GetByIdAsync
 *       POST api/v1/contributionsummary              -> CreateAsync
 *       PUT  api/v1/contributionsummary/{id}         -> UpdateAsync
 *       DELETE api/v1/contributionsummary/{id}       -> DeleteAsync
 *   - profitCentre is a required business context parameter on GetByProfitCentreAsync and
 *     GetSummaryAsync — confirmed as satisfiable from the page/route context (profit centre
 *     selector drives the grid in frmTimeSellerPC / contribution_summary.js).
 *   - fpsYear on GetSummaryAsync is nullable (optional) matching the backend signature.
 *   - Return types wrapped in ApiResponseDto<T> — standard FPSApps response envelope.
 *   - Injected into frontend services via IFpsApiClient.FpsContributionSummary (see update
 *     to IFpsApiClient.cs in this same phase).
 *
 * PRESERVED:
 *   - Method naming convention matches IFpsGradeApiClient and all other FPS API client interfaces.
 *   - No Lk_ prefix naming; DTO property names used directly.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear should remain nullable on GetSummaryAsync
 *     or always be supplied from IFpsRequestContext once context wiring is complete (Phase 5 TODO).
 *   - TRANSFORMENGINE TODO: Confirm whether an unfiltered (all-centres) overload of
 *     GetByProfitCentreAsync is needed — backend service currently requires non-null profitCentre.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    /// <summary>
    /// API client interface for the ContributionSummary resource (frmTimeSellerPC).
    /// Mirrors the six REST endpoints on backend ContributionSummaryController
    /// (route: api/v1/contributionsummary).
    /// Injected into frontend services via IFpsApiClient.FpsContributionSummary.
    /// </summary>
    public interface IFpsContributionSummaryApiClient
    {
        // TRANSFORMENGINE: GET api/v1/contributionsummary — paginated list filtered by profitCentre;
        //   maps to ContributionSummaryController.GetByProfitCentreAsync
        /// <summary>
        /// Returns a paginated list of contribution summary rows filtered by profit centre.
        /// Calls GET api/v1/contributionsummary with <paramref name="profitCentre"/> as a
        /// required query parameter.
        /// </summary>
        /// <param name="query">Pagination, sort, and optional search parameters.</param>
        /// <param name="profitCentre">Profit centre / resource centre code (e.g. "Bact"). Required.</param>
        Task<ApiResponseDto<List<ContributionSummaryDto>>> GetByProfitCentreAsync(
            QueryParameters<string> query,
            string profitCentre);

        // TRANSFORMENGINE: GET api/v1/contributionsummary/summary — aggregate totals;
        //   maps to ContributionSummaryController.GetSummaryAsync
        /// <summary>
        /// Returns the aggregate summary-box totals for a given profit centre and optional FPS year.
        /// Calls GET api/v1/contributionsummary/summary.
        /// </summary>
        /// <param name="profitCentre">Profit centre / resource centre code (e.g. "Bact"). Required.</param>
        /// <param name="fpsYear">FPS financial year (e.g. 2026). Null uses the active year server-side.</param>
        Task<ApiResponseDto<ContributionSummarySummaryDto>> GetSummaryAsync(
            string profitCentre,
            int? fpsYear = null);

        // TRANSFORMENGINE: GET api/v1/contributionsummary/{id} — single row by PK;
        //   maps to ContributionSummaryController.GetByIdAsync
        /// <summary>
        /// Returns a single contribution summary row by its integer primary key.
        /// Calls GET api/v1/contributionsummary/{id}.
        /// </summary>
        /// <param name="id">Primary key of the contribution summary row.</param>
        Task<ApiResponseDto<ContributionSummaryDto>> GetByIdAsync(int id);

        // TRANSFORMENGINE: POST api/v1/contributionsummary — create new row;
        //   maps to ContributionSummaryController.CreateAsync
        /// <summary>
        /// Creates a new contribution summary row.
        /// Calls POST api/v1/contributionsummary.
        /// Returns the persisted row with its server-assigned Id.
        /// </summary>
        /// <param name="dto">DTO containing the new row values.</param>
        Task<ApiResponseDto<ContributionSummaryDto>> CreateAsync(ContributionSummaryDto dto);

        // TRANSFORMENGINE: PUT api/v1/contributionsummary/{id} — update existing row;
        //   maps to ContributionSummaryController.UpdateAsync
        /// <summary>
        /// Updates an existing contribution summary row.
        /// Calls PUT api/v1/contributionsummary/{id}.
        /// </summary>
        /// <param name="id">Primary key of the row to update (route parameter).</param>
        /// <param name="dto">DTO containing updated field values.</param>
        Task<ApiResponseDto<ContributionSummaryDto>> UpdateAsync(int id, ContributionSummaryDto dto);

        // TRANSFORMENGINE: DELETE api/v1/contributionsummary/{id} — delete by PK;
        //   maps to ContributionSummaryController.DeleteAsync
        /// <summary>
        /// Deletes a contribution summary row by its integer primary key.
        /// Calls DELETE api/v1/contributionsummary/{id}.
        /// </summary>
        /// <param name="id">Primary key of the row to delete.</param>
        Task<ApiResponseDto<bool>> DeleteAsync(int id);
    }
}
