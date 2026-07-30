/*
 * TRANSFORMENGINE MIGRATION — IWorkgroupMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: Frontend service interface for WorkGroup Maintenance CRUD + lookup operations
 *   - Source form: frmMaintWorkGroup2 (RecordSource: WorkGroup_MAP → fps.workgroup)
 *   - Mirrors IFpsWorkgroupApiClient exactly; 5 CRUD methods + 3 lookup methods
 *   - GetPagedAsync        — paginated grid list (GET api/v1/workgroup/paged)
 *   - GetByWorkGroupNameAsync — single record fetch (GET api/v1/workgroup/{workGroupName})
 *   - CreateAsync         — add-new modal submit (POST api/v1/workgroup)
 *   - UpdateAsync         — edit modal submit (PUT api/v1/workgroup/{workGroupName})
 *   - DeleteAsync         — delete confirm (DELETE api/v1/workgroup/{workGroupName})
 *   - GetProfitCentresAsync  — ResourceCentre dropdown (GET api/v1/workgroup/profitcentres)
 *   - GetOwnersAsync         — Owner dropdown (GET api/v1/workgroup/owners)
 *   - GetCostCentresAsync    — cascading CostCentre dropdown (GET api/v1/workgroup/costcentres?profitCentre=)
 *
 * PRESERVED:
 *   - Method naming convention consistent with IDivisionService, IProfitCentreService, etc.
 *   - All return types wrapped in ApiResponseDto<T> — standard FPSApps response envelope
 *   - Lookup methods use dedicated return types matching IFpsWorkgroupApiClient
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: GetCostCentresAsync returns List<double?> — if labelled projection needed,
 *     coordinate with backend to update the response type before wiring the cascading dropdown
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PACT
{
    /// <summary>
    /// Frontend service interface for WorkGroup Maintenance CRUD and lookup operations.
    /// Mirrors <see cref="Interfaces.FpsApiClients.IFpsWorkgroupApiClient"/>;
    /// all implementations must delegate to the API client without adding business logic.
    /// Migrated from <c>frmMaintWorkGroup2</c>.
    /// </summary>
    public interface IWorkgroupMaintenanceService
    {
        // ── CRUD ────────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: GetPagedAsync — paginated grid list forwarding to GET api/v1/workgroup/paged
        /// <summary>
        /// Returns a paginated list of workgroup maintenance records.
        /// </summary>
        /// <param name="query">Pagination, filter, and sort parameters.</param>
        /// <returns>Paged list of <see cref="WorkGroupDto"/>.</returns>
        Task<ApiResponseDto<List<WorkGroupDto>>> GetPagedAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GetByWorkGroupNameAsync — single record fetch forwarding to GET api/v1/workgroup/{workGroupName}
        /// <summary>
        /// Returns a single workgroup record by its WorkGroupName.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name sourced from grid row selection.</param>
        /// <returns><see cref="WorkGroupDto"/> if found.</returns>
        Task<ApiResponseDto<WorkGroupDto>> GetByWorkGroupNameAsync(string workGroupName);

        // TRANSFORMENGINE: CreateAsync — add-new path forwarding to POST api/v1/workgroup
        /// <summary>
        /// Creates a new workgroup record.
        /// </summary>
        /// <param name="dto">Workgroup data to create.</param>
        /// <returns>Created <see cref="WorkGroupDto"/>.</returns>
        Task<ApiResponseDto<WorkGroupDto>> CreateAsync(WorkGroupDto dto);

        // TRANSFORMENGINE: UpdateAsync — edit path forwarding to PUT api/v1/workgroup/{workGroupName}
        //   workGroupName is the ORIGINAL key (before any rename); dto.WorkGroupName may differ (rename)
        /// <summary>
        /// Updates an existing workgroup identified by <paramref name="workGroupName"/>.
        /// Pass the original WorkGroupName; <paramref name="dto"/>.WorkGroupName may differ to support rename.
        /// </summary>
        /// <param name="workGroupName">Original WorkGroup name (route parameter).</param>
        /// <param name="dto">Updated workgroup data.</param>
        /// <returns>Updated <see cref="WorkGroupDto"/>.</returns>
        Task<ApiResponseDto<WorkGroupDto>> UpdateAsync(string workGroupName, WorkGroupDto dto);

        // TRANSFORMENGINE: DeleteAsync — delete confirm forwarding to DELETE api/v1/workgroup/{workGroupName}
        /// <summary>
        /// Deletes the workgroup with the given WorkGroupName.
        /// </summary>
        /// <param name="workGroupName">WorkGroup name to delete, sourced from grid row.</param>
        /// <returns>True if deletion succeeded.</returns>
        Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName);

        // ── Lookup endpoints (SEPARATE from CRUD resource family) ────────────────

        // TRANSFORMENGINE: GetProfitCentresAsync — ResourceCentre dropdown; GET api/v1/workgroup/profitcentres
        /// <summary>
        /// Returns all available profit centre identifiers for the ResourceCentre dropdown.
        /// </summary>
        /// <returns>List of profit centre identifier strings.</returns>
        Task<ApiResponseDto<List<string>>> GetProfitCentresAsync();

        // TRANSFORMENGINE: GetOwnersAsync — Owner dropdown; GET api/v1/workgroup/owners
        /// <summary>
        /// Returns all owner records for the Owner dropdown.
        /// </summary>
        /// <returns>List of <see cref="OwnerDto"/> records.</returns>
        Task<ApiResponseDto<List<OwnerDto>>> GetOwnersAsync();

        // TRANSFORMENGINE: GetCostCentresAsync — cascading CostCentre dropdown;
        //   GET api/v1/workgroup/costcentres?profitCentre={pc}
        //   profitCentre sourced from modal ProfitCentre change event (confirmed page-sourced)
        /// <summary>
        /// Returns cost centre values for the cascading CostCentre dropdown filtered by profit centre.
        /// </summary>
        /// <param name="profitCentre">Selected profit centre code from the modal dropdown.</param>
        /// <returns>List of cost centre double values for the given profit centre.</returns>
        Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre);
    }
}
