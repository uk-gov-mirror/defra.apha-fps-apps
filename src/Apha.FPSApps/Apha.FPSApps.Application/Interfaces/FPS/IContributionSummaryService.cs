/*
 * TRANSFORMENGINE MIGRATION — IContributionSummaryService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: frontend service interface for the ContributionSummary resource (frmTimeSellerPC).
 *   - Six method signatures mirror IFpsContributionSummaryApiClient exactly:
 *       GetByProfitCentreAsync — paginated list filtered by profit centre
 *       GetSummaryAsync        — aggregate summary-box totals for a profit centre
 *       GetByIdAsync           — single row by integer primary key
 *       CreateAsync            — create a new contribution summary row
 *       UpdateAsync            — update an existing row (id + dto)
 *       DeleteAsync            — delete a row by integer primary key
 *   - profitCentre is a required business context parameter on GetByProfitCentreAsync
 *     and GetSummaryAsync — sourced from the Resource Centre selector on the page.
 *   - fpsYear on GetSummaryAsync is nullable (optional) matching the API client signature.
 *   - Injected into ContributionSummaryController (FPS area) in the frontend MVC project.
 *
 * PRESERVED:
 *   - Method naming convention matches IGradeService and all other FPS frontend service interfaces.
 *   - Return types wrapped in ApiResponseDto<T> — standard FPSApps response envelope.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear should remain nullable on GetSummaryAsync
 *     or always be supplied from IFpsRequestContext once context wiring is complete.
 *   - TRANSFORMENGINE TODO: Confirm whether an unfiltered (all-centres) overload of
 *     GetByProfitCentreAsync is needed — backend currently requires non-null profitCentre.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    /// <summary>
    /// Frontend service interface for the ContributionSummary resource (frmTimeSellerPC).
    /// Mirrors the six async methods on <see cref="Apha.FPSApps.Application.Interfaces.FpsApiClients.IFpsContributionSummaryApiClient"/>.
    /// Injected into <c>ContributionSummaryController</c> in the FPS area.
    /// </summary>
    public interface IContributionSummaryService
    {
        // TRANSFORMENGINE: paginated list filtered by profitCentre — delegates to IFpsContributionSummaryApiClient.GetByProfitCentreAsync
        /// <summary>
        /// Returns a paginated list of contribution summary rows filtered by profit centre.
        /// </summary>
        /// <param name="query">Pagination, sort, and optional search parameters.</param>
        /// <param name="profitCentre">Profit centre / resource centre code (e.g. "Bact"). Required.</param>
        Task<ApiResponseDto<List<ContributionSummaryDto>>> GetByProfitCentreAsync(
            QueryParameters<string> query,
            string profitCentre);

        // TRANSFORMENGINE: aggregate summary-box totals — delegates to IFpsContributionSummaryApiClient.GetSummaryAsync
        /// <summary>
        /// Returns the aggregate summary-box totals for a given profit centre and optional FPS year.
        /// </summary>
        /// <param name="profitCentre">Profit centre / resource centre code (e.g. "Bact"). Required.</param>
        /// <param name="fpsYear">FPS financial year (e.g. 2026). Null uses the active year server-side.</param>
        Task<ApiResponseDto<ContributionSummarySummaryDto>> GetSummaryAsync(
            string profitCentre,
            int? fpsYear = null);

        // TRANSFORMENGINE: single record by PK — delegates to IFpsContributionSummaryApiClient.GetByIdAsync
        /// <summary>
        /// Returns a single contribution summary row by its integer primary key.
        /// </summary>
        /// <param name="id">Primary key of the contribution summary row.</param>
        Task<ApiResponseDto<ContributionSummaryDto>> GetByIdAsync(int id);

        // TRANSFORMENGINE: create — delegates to IFpsContributionSummaryApiClient.CreateAsync
        /// <summary>
        /// Creates a new contribution summary row.
        /// Returns the persisted row with its server-assigned Id.
        /// </summary>
        /// <param name="dto">DTO containing the new row values.</param>
        Task<ApiResponseDto<ContributionSummaryDto>> CreateAsync(ContributionSummaryDto dto);

        // TRANSFORMENGINE: update — id in signature is route parameter; delegates to IFpsContributionSummaryApiClient.UpdateAsync
        /// <summary>
        /// Updates an existing contribution summary row identified by <paramref name="id"/>.
        /// </summary>
        /// <param name="id">Primary key of the row to update (route parameter).</param>
        /// <param name="dto">DTO containing updated field values.</param>
        Task<ApiResponseDto<ContributionSummaryDto>> UpdateAsync(int id, ContributionSummaryDto dto);

        // TRANSFORMENGINE: delete — delegates to IFpsContributionSummaryApiClient.DeleteAsync
        /// <summary>
        /// Deletes a contribution summary row by its integer primary key.
        /// </summary>
        /// <param name="id">Primary key of the row to delete.</param>
        Task<ApiResponseDto<bool>> DeleteAsync(int id);
    }
}
