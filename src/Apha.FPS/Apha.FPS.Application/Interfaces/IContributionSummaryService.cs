/*
 * TRANSFORMENGINE MIGRATION — IContributionSummaryService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: service interface for ContributionSummary business logic.
 *   - Six async methods matching the plan spec:
 *       GetByProfitCentreAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync, GetSummaryAsync
 *   - GetByProfitCentreAsync uses QueryParameters<string> consistent with IGradeService
 *     and other paged service interfaces in this project.
 *   - GetSummaryAsync returns ContributionSummarySummaryDto for summary-box totals
 *     (mirrors IContributionSummaryRepository.GetSummaryTotalsAsync contract).
 *
 * PRESERVED:
 *   - Interface stays in the Application layer; no infrastructure or DbContext references.
 *   - All methods are async (Task<T>) per project conventions.
 *   - XML doc comments explain business intent and exception contracts for Phase 5 implementors.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear should be a required parameter on
 *     GetSummaryAsync or continue to be resolved server-side via IFpsRequestContext (Phase 5).
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// Service interface for ContributionSummary business logic.
    /// All methods are async; the interface contains no infrastructure or DbContext references.
    /// Orchestrates repository calls via <see cref="Apha.FPS.Core.Interfaces.IContributionSummaryRepository"/>
    /// and enforces the business rules extracted from the frmTimeSellerPC VBA analysis.
    /// </summary>
    public interface IContributionSummaryService
    {
        // TRANSFORMENGINE: GetByProfitCentreAsync — primary grid query scoped by profitCentre;
        //   maps to GET api/v1/contributionsummary?profitCentre=Bact in Phase 5.
        /// <summary>
        /// Returns a paginated list of contribution summary rows filtered by the given
        /// <paramref name="profitCentre"/> code for the active FPS year.
        /// Mirrors the resource-centre dropdown filter in contribution_summary.js (renderGrid / getCurrentReport).
        /// </summary>
        /// <param name="query">Pagination, sort, and optional search parameters.</param>
        /// <param name="profitCentre">Profit centre / resource centre code to filter by (e.g. "Bact").</param>
        /// <returns>Paginated result of <see cref="ContributionSummaryDto"/> rows.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="profitCentre"/> is null or whitespace.</exception>
        Task<PaginatedResult<ContributionSummaryDto>> GetByProfitCentreAsync(
            QueryParameters<string> query,
            string profitCentre);

        // TRANSFORMENGINE: GetByIdAsync — single-row lookup for CRUD edit/delete modal pre-population;
        //   maps to GET api/v1/contributionsummary/{id} in Phase 5.
        /// <summary>
        /// Returns a single contribution summary row by its integer primary key,
        /// or <c>null</c> if no matching row is found.
        /// Used by the edit-modal pre-population flow (openCrudModal "edit" branch in JS).
        /// </summary>
        /// <param name="id">Primary key of the contribution summary row.</param>
        /// <returns><see cref="ContributionSummaryDto"/> if found; <c>null</c> otherwise.</returns>
        Task<ContributionSummaryDto?> GetByIdAsync(int id);

        // TRANSFORMENGINE: CreateAsync — INSERT guard + delegate; mirrors "add" path of saveCrudRow() in JS;
        //   maps to POST api/v1/contributionsummary in Phase 5.
        /// <summary>
        /// Creates a new contribution summary row after validating the input.
        /// Returns the persisted DTO with its server-assigned <see cref="ContributionSummaryDto.Id"/>.
        /// Mirrors the "add" path of saveCrudRow() in contribution_summary.js.
        /// </summary>
        /// <param name="dto">DTO containing the new row values. Id must be 0.</param>
        /// <returns>Created <see cref="ContributionSummaryDto"/> with its assigned Id.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if required fields (Wg, Grade, ProfitCentre) are null or whitespace.</exception>
        Task<ContributionSummaryDto> CreateAsync(ContributionSummaryDto dto);

        // TRANSFORMENGINE: UpdateAsync — existence guard + update delegate; mirrors "edit" path of saveCrudRow() in JS;
        //   maps to PUT api/v1/contributionsummary/{id} in Phase 5.
        /// <summary>
        /// Updates an existing contribution summary row identified by <paramref name="id"/>.
        /// Returns the updated DTO.
        /// Mirrors the "edit" path of saveCrudRow() in contribution_summary.js.
        /// </summary>
        /// <param name="id">Primary key of the row to update.</param>
        /// <param name="dto">DTO with updated field values. The Id on the DTO is ignored; <paramref name="id"/> takes precedence.</param>
        /// <returns>Updated <see cref="ContributionSummaryDto"/>.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="dto"/> is null.</exception>
        /// <exception cref="System.ArgumentException">Thrown if required fields are null or whitespace.</exception>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no row with the given <paramref name="id"/> exists.</exception>
        Task<ContributionSummaryDto> UpdateAsync(int id, ContributionSummaryDto dto);

        // TRANSFORMENGINE: DeleteAsync — existence guard + delete delegate; mirrors "delete" path in JS;
        //   maps to DELETE api/v1/contributionsummary/{id} in Phase 5.
        /// <summary>
        /// Deletes a contribution summary row by its integer primary key.
        /// Returns <c>true</c> if a row was found and deleted; <c>false</c> if no row matched.
        /// </summary>
        /// <param name="id">Primary key of the row to delete.</param>
        /// <returns><c>true</c> if deleted; <c>false</c> if not found.</returns>
        /// <exception cref="System.Collections.Generic.KeyNotFoundException">Thrown if no row with the given <paramref name="id"/> exists.</exception>
        Task<bool> DeleteAsync(int id);

        // TRANSFORMENGINE: GetSummaryAsync — aggregate summary-box totals;
        //   maps to GET api/v1/contributionsummary/summary?profitCentre=Bact in Phase 5.
        //   Mirrors recomputeSummaryFromRows() / renderSummary() in contribution_summary.js.
        /// <summary>
        /// Returns the aggregate summary-box totals for the given <paramref name="profitCentre"/>
        /// and the active FPS year.
        /// Computes TotalBudgetBids, ContributionTarget, TotalToRecover, TotalTimeFeeFromPlanHrs,
        /// TotalTimeSurplusShortfall, AssuredTimeFeeFromPlanHrs, AssuredTimeSurplusShortfall,
        /// RateEfficacyOhRate, and RateEfficacyTotalCont.
        /// Returns <c>null</c> if no rows exist for the given scope.
        /// </summary>
        /// <param name="profitCentre">Profit centre / resource centre code (e.g. "Bact").</param>
        /// <param name="fpsYear">FPS financial year (e.g. 2026). Pass <c>null</c> to use the active year.</param>
        /// <returns><see cref="ContributionSummarySummaryDto"/> if rows exist; <c>null</c> otherwise.</returns>
        /// <exception cref="System.ArgumentException">Thrown if <paramref name="profitCentre"/> is null or whitespace.</exception>
        Task<ContributionSummarySummaryDto?> GetSummaryAsync(string profitCentre, int? fpsYear = null);
    }
}
