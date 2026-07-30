using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for project-specific component charges (TestRequirementRCCost) CRUD operations.
    /// Enforces business rules extracted from fsubTestequirementRCPrice VBA logic
    /// and fps.tbltestrequirementrccost DDL constraints.
    /// </summary>
    public class TestRequirementRCCostService : ITestRequirementRCCostService
    {
        private readonly ITestRequirementRCCostRepository _repository;
        private readonly IMapper _mapper;

        public TestRequirementRCCostService(ITestRequirementRCCostRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<TestRequirementRCCostDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string testCode)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);

            var paginationParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedByTestCodeAsync(paginationParams, testCode);
            return _mapper.Map<PaginatedResult<TestRequirementRCCostDto>>(pagedData);
        }

        public async Task<IEnumerable<TestRequirementRCCostDto>> GetByTestCodeAsync(string testCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);

            var entities = await _repository.GetByTestCodeAsync(testCode);
            return _mapper.Map<IEnumerable<TestRequirementRCCostDto>>(entities);
        }

        public async Task<TestRequirementRCCostDto?> GetByKeyAsync(string testCode, string buyer, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);

            var entity = await _repository.GetByKeyAsync(testCode, buyer, profitCentre);
            return entity == null ? null : _mapper.Map<TestRequirementRCCostDto>(entity);
        }

        //   Guards: null check, non-empty keys, FpsYear positive, duplicate PK check
        public async Task<TestRequirementRCCostDto> CreateAsync(TestRequirementRCCostDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            var exists = await _repository.ExistsAsync(dto.TestCode, dto.Buyer, dto.ProfitCentre);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestRequirementRCCost entry with TestCode '{dto.TestCode}', Buyer '{dto.Buyer}', ProfitCentre '{dto.ProfitCentre}' already exists for the current FPS year.");

            var entity = _mapper.Map<TestRequirementRCCost>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestRequirementRCCostDto>(created);
        }

        //   Guards: non-empty keys, route-key/body-key consistency, existence check
        public async Task<TestRequirementRCCostDto> UpdateAsync(string testCode, string buyer, string profitCentre, TestRequirementRCCostDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.Buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);

            if (!string.Equals(testCode, dto.TestCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(buyer, dto.Buyer, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(profitCentre, dto.ProfitCentre, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "Route keys (testCode, buyer, profitCentre) must match the DTO body keys.");

            var existing = await _repository.GetByKeyAsync(testCode, buyer, profitCentre);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestRequirementRCCost entry with TestCode '{testCode}', Buyer '{buyer}', ProfitCentre '{profitCentre}' was not found for the current FPS year.");

            var entity = _mapper.Map<TestRequirementRCCost>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestRequirementRCCostDto>(updated);
        }

        public async Task<bool> DeleteAsync(string testCode, string buyer, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(buyer);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);

            return await _repository.DeleteAsync(testCode, buyer, profitCentre);
        }
    }
}
