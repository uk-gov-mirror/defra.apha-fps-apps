/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryController.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 5 — API Layer - Controller + RequestMapper + DI (Steps 8-9)
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: no legacy C# backend equivalent existed.
 *   - MS Access frmTimeSellerPC VBA form logic converted to ASP.NET Core 10 [ApiController].
 *   - Six REST endpoints derived from IContributionSummaryService contract:
 *       GET  api/v1/contributionsummary              -> GetByProfitCentreAsync (paged, filtered by profitCentre)
 *       GET  api/v1/contributionsummary/{id}         -> GetByIdAsync
 *       POST api/v1/contributionsummary              -> CreateAsync
 *       PUT  api/v1/contributionsummary/{id}         -> UpdateAsync
 *       DELETE api/v1/contributionsummary/{id}       -> DeleteAsync
 *       GET  api/v1/contributionsummary/summary      -> GetSummaryAsync (aggregate totals for summary boxes)
 *   - [Authorize] role guard matches all other FPS controllers (API-FPSUser, API-FPSAdmin, API-FPSShared).
 *   - Exception-driven flow: throws ArgumentException / KeyNotFoundException; ExceptionMiddleware maps status codes.
 *   - Request mapping via AutoMapper IMapper (ContributionSummaryReq/Res <-> ContributionSummaryDto).
 *
 * PRESERVED:
 *   - All service method signatures from IContributionSummaryService preserved verbatim.
 *   - Constructor null-guard pattern matches ProfitCentreGradeController and all other FPS controllers.
 *   - Summary sub-route placed before {id} to avoid route ambiguity.
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether fpsYear query param on GetSummaryAsync should default
 *     to IFpsRequestContext.FpsYear or remain as nullable int passed from caller.
 *   - TRANSFORMENGINE TODO: Confirm profitCentre is always required on list/summary endpoints
 *     or whether an unfiltered all-centres view is needed (service currently requires non-null).
 */

using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Apha.FPS.Api.Controllers
{
    /// <summary>
    /// API controller for Contribution Summary maintenance (frmTimeSellerPC).
    /// Provides CRUD operations and aggregate summary-box totals for contribution
    /// summary rows scoped by profit centre and FPS year.
    /// </summary>
    [Authorize(Roles = "API-FPSUser,API-FPSAdmin, API-FPSShared")]
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/contributionsummary")]
    public class ContributionSummaryController : ControllerBase
    {
        private readonly IContributionSummaryService _contributionSummaryService;
        private readonly IMapper _mapper;

        // TRANSFORMENGINE: Constructor null-guards match ProfitCentreGradeController pattern
        public ContributionSummaryController(
            IContributionSummaryService contributionSummaryService,
            IMapper mapper)
        {
            _contributionSummaryService = contributionSummaryService
                ?? throw new ArgumentNullException(nameof(contributionSummaryService));
            _mapper = mapper
                ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Returns a paginated list of contribution summary rows filtered by profit centre.
        /// Mirrors the resource-centre dropdown filter in contribution_summary.js (renderGrid / getCurrentReport).
        /// </summary>
        /// <param name="query">Pagination, sort, and optional search parameters.</param>
        /// <param name="profitCentre">Profit centre / resource centre code to filter by (e.g. "Bact").</param>
        [HttpGet]
        public async Task<IActionResult> GetByProfitCentreAsync(
            [FromQuery] PaginationReq<string> query,
            [FromQuery] string profitCentre)
        {
            // TRANSFORMENGINE: Map PaginationReq -> QueryParameters; call service with profitCentre filter
            var filter = _mapper.Map<QueryParameters<string>>(query);
            var result = await _contributionSummaryService.GetByProfitCentreAsync(filter, profitCentre);
            return Ok(_mapper.Map<PaginationRes<ContributionSummaryRes>>(result));
        }

        /// <summary>
        /// Returns the aggregate summary-box totals for a given profit centre and optional FPS year.
        /// Computes TotalBudgetBids, ContributionTarget, TotalToRecover, time fee / surplus values,
        /// and Rate Efficacy checker values.
        /// Placed before {id} route to avoid route ambiguity.
        /// </summary>
        /// <param name="profitCentre">Profit centre / resource centre code (e.g. "Bact").</param>
        /// <param name="fpsYear">FPS financial year (e.g. 2026). Pass null to use the active year.</param>
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummaryAsync(
            [FromQuery] string profitCentre,
            [FromQuery] int? fpsYear = null)
        {
            // TRANSFORMENGINE: GetSummaryAsync — aggregate summary boxes; mirrors recomputeSummaryFromRows() in JS
            var result = await _contributionSummaryService.GetSummaryAsync(profitCentre, fpsYear);
            if (result is null)
            {
                return Ok(new ContributionSummarySummaryRes());
            }
            return Ok(_mapper.Map<ContributionSummarySummaryRes>(result));
        }

        /// <summary>
        /// Returns a single contribution summary row by its integer primary key.
        /// Used by the edit-modal pre-population flow (openCrudModal "edit" branch in contribution_summary.js).
        /// </summary>
        /// <param name="id">Primary key of the contribution summary row.</param>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            // TRANSFORMENGINE: GetByIdAsync — single row lookup; throws ArgumentException if not found
            var result = await _contributionSummaryService.GetByIdAsync(id);
            if (result is null)
            {
                throw new ArgumentException($"Contribution summary record with id '{id}' not found");
            }
            return Ok(_mapper.Map<ContributionSummaryRes>(result));
        }

        /// <summary>
        /// Creates a new contribution summary row.
        /// Mirrors the "add" path of saveCrudRow() in contribution_summary.js.
        /// Returns the persisted row with its server-assigned Id.
        /// </summary>
        /// <param name="request">Request body containing the new row values.</param>
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] ContributionSummaryReq request)
        {
            // TRANSFORMENGINE: Map ContributionSummaryReq -> ContributionSummaryDto; delegate to service CreateAsync
            var dto = _mapper.Map<ContributionSummaryDto>(request);
            var created = await _contributionSummaryService.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetByIdAsync),
                new { id = created.Id },
                _mapper.Map<ContributionSummaryRes>(created));
        }

        /// <summary>
        /// Updates an existing contribution summary row.
        /// Mirrors the "edit" path of saveCrudRow() in contribution_summary.js.
        /// Returns the updated row.
        /// </summary>
        /// <param name="id">Primary key of the row to update (route parameter takes precedence over body).</param>
        /// <param name="request">Request body containing updated field values.</param>
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] ContributionSummaryReq request)
        {
            // TRANSFORMENGINE: Map ContributionSummaryReq -> ContributionSummaryDto; id from route takes precedence
            var dto = _mapper.Map<ContributionSummaryDto>(request);
            var updated = await _contributionSummaryService.UpdateAsync(id, dto);
            return Ok(_mapper.Map<ContributionSummaryRes>(updated));
        }

        /// <summary>
        /// Deletes a contribution summary row by its integer primary key.
        /// Mirrors the "delete" path in contribution_summary.js.
        /// </summary>
        /// <param name="id">Primary key of the row to delete.</param>
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            // TRANSFORMENGINE: DeleteAsync — service throws KeyNotFoundException if not found
            var deleted = await _contributionSummaryService.DeleteAsync(id);
            if (!deleted)
            {
                throw new ArgumentException($"Contribution summary record with id '{id}' not found");
            }
            return Ok(new { success = true });
        }
    }
}
