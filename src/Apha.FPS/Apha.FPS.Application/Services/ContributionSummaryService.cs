/*
 * TRANSFORMENGINE MIGRATION — ContributionSummaryService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 3 — Application Layer - DTOs + Service Interfaces + EntityMapper + Services
 * Migrated : 2026-06-16
 *
 * CHANGED:
 *   - New file: service implementation for ContributionSummary business logic.
 *   - Constructor injects IContributionSummaryRepository + IMapper (no direct DbContext).
 *   - Six async methods match IContributionSummaryService exactly.
 *   - GetByProfitCentreAsync maps QueryParameters<string> to Core PaginationParameters<string>
 *     via AutoMapper (identical to GradeService / MonthlyOutputService precedent).
 *   - CreateAsync validates Wg, Grade, ProfitCentre are non-null/non-empty before insert.
 *   - UpdateAsync uses ExistsAsync guard before delegating to repository.
 *   - DeleteAsync uses ExistsAsync guard before delegating to repository.
 *   - GetSummaryAsync delegates to IContributionSummaryRepository.GetSummaryTotalsAsync and
 *     maps the ContributionSummaryTotals aggregate result to ContributionSummarySummaryDto.
 *
 * PRESERVED:
 *   - All business logic extracted from JS summary calculations preserved as service-layer
 *     responsibility (server-side aggregate, not client recomputation).
 *   - No direct DbContext or EF Core usage — repository abstraction boundary maintained.
 *   - Guard patterns consistent with GradeService precedent (ArgumentNullException,
 *     ArgumentException, KeyNotFoundException).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ContributionSummaryTotals is a keyless aggregate returned by the
 *     repository. Confirm AutoMapper has a CreateMap<ContributionSummaryTotals, ContributionSummarySummaryDto>
 *     entry in EntityMapper (added in this phase) before running Phase 5 integration tests.
 *   - TRANSFORMENGINE TODO: fpsYear null-pass-through to repository is intentional — the
 *     repository resolves the active year from IFpsRequestContext when null. Confirm Phase 4
 *     repository implementation handles this consistently.
 */

using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for ContributionSummary business logic.
    /// Orchestrates repository calls via <see cref="IContributionSummaryRepository"/> and
    /// enforces the business rules extracted from the frmTimeSellerPC VBA / JS prototype analysis.
    /// No DbContext is used directly — all data access flows through the repository abstraction.
    /// </summary>
    public class ContributionSummaryService : IContributionSummaryService
    {
        private readonly IContributionSummaryRepository _repository;
        private readonly IMapper _mapper;

        public ContributionSummaryService(
            IContributionSummaryRepository repository,
            IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper     = mapper     ?? throw new ArgumentNullException(nameof(mapper));
        }

        // TRANSFORMENGINE: GetByProfitCentreAsync — maps QueryParameters to Core PaginationParameters,
        //   delegates to repository, maps PagedData<ContributionSummary> back to PaginatedResult<ContributionSummaryDto>.
        //   Mirrors renderGrid() / getCurrentReport() resource-centre filter in contribution_summary.js.
        /// <inheritdoc />
        public async Task<PaginatedResult<ContributionSummaryDto>> GetByProfitCentreAsync(
            QueryParameters<string> query,
            string profitCentre)
        {
            ArgumentNullException.ThrowIfNull(query);

            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new ArgumentException("Profit centre code cannot be null or empty.", nameof(profitCentre));
            }

            var paginationParams = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetByProfitCentreAsync(paginationParams, profitCentre);
            return _mapper.Map<PaginatedResult<ContributionSummaryDto>>(pagedData);
        }

        // TRANSFORMENGINE: GetByIdAsync — single-row lookup; null-safe return for 404 mapping in Phase 5 controller.
        /// <inheritdoc />
        public async Task<ContributionSummaryDto?> GetByIdAsync(int id)
        {
            var entity = await _repository.GetByIdAsync(id);
            return entity == null ? null : _mapper.Map<ContributionSummaryDto>(entity);
        }

        // TRANSFORMENGINE: CreateAsync — required-field guards before INSERT; mirrors "add" path of saveCrudRow() in JS.
        //   Wg, Grade, ProfitCentre are mandatory discriminators required to scope the row.
        /// <inheritdoc />
        public async Task<ContributionSummaryDto> CreateAsync(ContributionSummaryDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Guard: Wg is a required non-null discriminator for the WG/Grade grid row
            if (string.IsNullOrWhiteSpace(dto.Wg))
            {
                throw new ArgumentException("Work group code (Wg) is required.", nameof(dto));
            }

            // Guard: Grade is a required non-null discriminator for the WG/Grade grid row
            if (string.IsNullOrWhiteSpace(dto.Grade))
            {
                throw new ArgumentException("Grade code is required.", nameof(dto));
            }

            // Guard: ProfitCentre is the resource-centre scope discriminator; must be provided on CREATE
            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
            {
                throw new ArgumentException("Profit centre code is required.", nameof(dto));
            }

            var entity = _mapper.Map<ContributionSummary>(dto);
            var created = await _repository.CreateAsync(entity);
            return _mapper.Map<ContributionSummaryDto>(created);
        }

        // TRANSFORMENGINE: UpdateAsync — existence guard then UPDATE; mirrors "edit" path of saveCrudRow() in JS.
        //   Uses IContributionSummaryRepository.ExistsAsync for a clean 404 guard before delegating to UpdateAsync.
        /// <inheritdoc />
        public async Task<ContributionSummaryDto> UpdateAsync(int id, ContributionSummaryDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);

            // Guard: Wg is a required non-null discriminator on update
            if (string.IsNullOrWhiteSpace(dto.Wg))
            {
                throw new ArgumentException("Work group code (Wg) is required.", nameof(dto));
            }

            // Guard: Grade is a required non-null discriminator on update
            if (string.IsNullOrWhiteSpace(dto.Grade))
            {
                throw new ArgumentException("Grade code is required.", nameof(dto));
            }

            // Guard: ProfitCentre is required on update to maintain resource-centre scoping
            if (string.IsNullOrWhiteSpace(dto.ProfitCentre))
            {
                throw new ArgumentException("Profit centre code is required.", nameof(dto));
            }

            // Guard: verify the row to update actually exists before issuing UPDATE
            if (!await _repository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"Contribution summary row with Id '{id}' was not found.");
            }

            var entity = _mapper.Map<ContributionSummary>(dto);
            var updated = await _repository.UpdateAsync(id, entity);
            return _mapper.Map<ContributionSummaryDto>(updated);
        }

        // TRANSFORMENGINE: DeleteAsync — existence guard then DELETE; mirrors delete action in contribution_summary.js.
        //   Returns false if repo returns false (not found at delete time, e.g. concurrent delete).
        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            // Guard: verify the row to delete actually exists; throw 404-mappable exception
            if (!await _repository.ExistsAsync(id))
            {
                throw new KeyNotFoundException($"Contribution summary row with Id '{id}' was not found.");
            }

            return await _repository.DeleteAsync(id);
        }

        // TRANSFORMENGINE: GetSummaryAsync — delegates to repository aggregate query, maps to DTO.
        //   Mirrors recomputeSummaryFromRows() + renderSummary() in contribution_summary.js.
        //   Returns null when the repository finds no rows (API layer maps to 204 No Content in Phase 5).
        /// <inheritdoc />
        public async Task<ContributionSummarySummaryDto?> GetSummaryAsync(string profitCentre, int? fpsYear = null)
        {
            if (string.IsNullOrWhiteSpace(profitCentre))
            {
                throw new ArgumentException("Profit centre code cannot be null or empty.", nameof(profitCentre));
            }

            // TRANSFORMENGINE: fpsYear null-pass-through — repository resolves active year from IFpsRequestContext when null.
            //   This allows the API controller to omit fpsYear from the query string and rely on the session context.
            var totals = await _repository.GetSummaryTotalsAsync(profitCentre, fpsYear);
            return totals == null ? null : _mapper.Map<ContributionSummarySummaryDto>(totals);
        }
    }
}
