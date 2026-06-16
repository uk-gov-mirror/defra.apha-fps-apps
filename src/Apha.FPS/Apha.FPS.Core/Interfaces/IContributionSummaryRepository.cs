/*
 * TRANSFORMENGINE MIGRATION — IContributionSummaryRepository.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: no legacy C# repository interface existed for this form.
 *   - Six async methods derived from the plan spec:
 *       GetByProfitCentreAsync, GetByIdAsync, CreateAsync, UpdateAsync, DeleteAsync,
 *       GetSummaryTotalsAsync
 *   - GetByProfitCentreAsync uses PaginationParameters<string> consistent with
 *     IProfitCentreGradeRepository and IProjectRepository precedents in this project.
 *   - GetSummaryTotalsAsync returns ContributionSummaryTotals (keyless aggregate)
 *     derived from the summary-box logic in contribution_summary.js:
 *     recomputeSummaryFromRows() → SUM(TotalFec), SUM(AssuredFec), SUM(TotalCont),
 *     AVG(OhRate), ContributionTarget, TotalBudgetBids, TotalToRecover.
 *   - GetAllProfitCentreCodesAsync added to support the resource-centre dropdown
 *     (pageData.resourceCentres in contribution_summary.js).
 *
 * PRESERVED:
 *   - Core layer remains free of DbContext, EF Core, and infrastructure-specific references.
 *   - Method signatures use async Task<T> throughout per project conventions.
 *   - XML doc comments explain the business intent of each method for Phase 4 implementors.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether ExistsAsync is needed for the INSERT/UPDATE
 *     guard (similar to IProfitCentreGradeRepository.ProfitCentreExistsAsync). Add here
 *     if the Phase 4 repository requires it for trigger-equivalent validation.
 *   - TRANSFORMENGINE TODO: Verify GetSummaryTotalsAsync signature with Phase 3 service
 *     layer — the optional fpsYear parameter may need to be made required once the
 *     IFpsRequestContext integration is confirmed in Phase 3.
 */

using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    /// <summary>
    /// Repository interface for <see cref="ContributionSummary"/> entities.
    /// Exposes CRUD operations and aggregate summary queries required by the
    /// ContributionSummary Application layer (Phase 3).
    /// All methods are async; no infrastructure dependencies are permitted in this interface.
    /// </summary>
    public interface IContributionSummaryRepository
    {
        /// <summary>
        /// Returns a paginated list of contribution summary rows filtered by the
        /// given <paramref name="profitCentre"/> code and the current FPS year,
        /// supporting column sort and optional search via <paramref name="query"/>.
        /// Mirrors the grid data loaded by the resource-centre dropdown in
        /// contribution_summary.js (renderGrid / getCurrentReport).
        /// </summary>
        /// <param name="query">Pagination, sort, and search parameters.</param>
        /// <param name="profitCentre">Profit centre / resource centre code to filter by (e.g. "Bact").</param>
        // TRANSFORMENGINE: GetByProfitCentreAsync — primary grid query, scoped by profitCentre;
        //   maps to GET api/v1/contributionsummary?profitCentre=Bact in Phase 5.
        Task<PagedData<ContributionSummary>> GetByProfitCentreAsync(
            PaginationParameters<string> query,
            string profitCentre);

        /// <summary>
        /// Returns a single contribution summary row by its integer primary key,
        /// or <c>null</c> if no matching row is found.
        /// Used by the edit-modal pre-population (openCrudModal "edit" branch in JS).
        /// </summary>
        /// <param name="id">Primary key of the contribution summary row.</param>
        // TRANSFORMENGINE: GetByIdAsync — single-row lookup for CRUD edit/delete;
        //   maps to GET api/v1/contributionsummary/{id} in Phase 5.
        Task<ContributionSummary?> GetByIdAsync(int id);

        /// <summary>
        /// Inserts a new contribution summary row.
        /// Returns the persisted entity with its server-assigned <see cref="ContributionSummary.Id"/>.
        /// Mirrors the "add" path of saveCrudRow() in contribution_summary.js.
        /// </summary>
        /// <param name="entity">Entity to insert. Id must be 0 (identity column).</param>
        // TRANSFORMENGINE: CreateAsync — INSERT; maps to POST api/v1/contributionsummary in Phase 5.
        Task<ContributionSummary> CreateAsync(ContributionSummary entity);

        /// <summary>
        /// Updates an existing contribution summary row identified by <paramref name="id"/>.
        /// Returns the updated entity.
        /// Mirrors the "edit" path of saveCrudRow() in contribution_summary.js.
        /// </summary>
        /// <param name="id">Primary key of the row to update.</param>
        /// <param name="entity">Updated field values. The Id on the entity is ignored; <paramref name="id"/> takes precedence.</param>
        // TRANSFORMENGINE: UpdateAsync — UPDATE by PK; maps to PUT api/v1/contributionsummary/{id} in Phase 5.
        Task<ContributionSummary> UpdateAsync(int id, ContributionSummary entity);

        /// <summary>
        /// Deletes a contribution summary row by its integer primary key.
        /// Returns <c>true</c> if a row was found and deleted; <c>false</c> if no row matched.
        /// </summary>
        /// <param name="id">Primary key of the row to delete.</param>
        // TRANSFORMENGINE: DeleteAsync — DELETE by PK; maps to DELETE api/v1/contributionsummary/{id} in Phase 5.
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Returns the aggregate summary-box totals for the given <paramref name="profitCentre"/>
        /// and <paramref name="fpsYear"/>.
        /// Computes: SUM(TotalFec), SUM(AssuredFec), SUM(TotalCont), AVG(OhRate),
        /// ContributionTarget (from tblkpprofitcentre.conttarget), TotalBudgetBids,
        /// TotalToRecover, surplus/shortfall values.
        /// Mirrors recomputeSummaryFromRows() in contribution_summary.js and the
        /// summary-box rendering in renderSummary().
        /// Returns <c>null</c> if no rows exist for the given scope.
        /// </summary>
        /// <param name="profitCentre">Profit centre / resource centre code.</param>
        /// <param name="fpsYear">FPS financial year (e.g. 2026). Pass <c>null</c> to use the current active year.</param>
        // TRANSFORMENGINE: GetSummaryTotalsAsync — aggregate query for summary boxes;
        //   maps to GET api/v1/contributionsummary/summary?profitCentre=Bact in Phase 5.
        Task<ContributionSummaryTotals?> GetSummaryTotalsAsync(string profitCentre, int? fpsYear = null);

        /// <summary>
        /// Returns all distinct ProfitCentre codes available for the current FPS year.
        /// Used to populate the resource-centre dropdown (pageData.resourceCentres in
        /// contribution_summary.js / cs-resource-centre select element).
        /// </summary>
        // TRANSFORMENGINE: GetAllProfitCentreCodesAsync — lookup for dropdown;
        //   maps to GET api/v1/contributionsummary/profitcentres in Phase 5.
        Task<List<string>> GetAllProfitCentreCodesAsync();

        /// <summary>
        /// Returns <c>true</c> if a contribution summary row with the given
        /// <paramref name="id"/> exists in the repository.
        /// Used by the Application layer for existence guards before update/delete.
        /// </summary>
        /// <param name="id">Primary key to check.</param>
        // TRANSFORMENGINE: AnyAsync-style existence check — used by service layer guards
        //   before issuing UpdateAsync or DeleteAsync to return 404 cleanly.
        Task<bool> ExistsAsync(int id);
    }
}
