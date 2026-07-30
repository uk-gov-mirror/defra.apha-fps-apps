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
    /// Service implementation for component charges per profit centre (TestRCCost) CRUD operations.
    /// Enforces business rules extracted from fsubTestRCPrice VBA logic
    /// and fps.tbltestrccost DDL constraints.
    /// </summary>
    public class TestRCCostService : ITestRCCostService
    {
        private readonly ITestRCCostRepository _repository;
        private readonly IMapper _mapper;

        public TestRCCostService(ITestRCCostRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<PaginatedResult<TestRCCostDto>> GetPagedByTestCodeAsync(QueryParameters<string> query, string testCode)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);

            var paginationParams = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedByTestCodeAsync(paginationParams, testCode);
            return _mapper.Map<PaginatedResult<TestRCCostDto>>(pagedData);
        }

        public async Task<IEnumerable<TestRCCostDto>> GetByTestCodeAsync(string testCode)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);

            var entities = await _repository.GetByTestCodeAsync(testCode);
            return _mapper.Map<IEnumerable<TestRCCostDto>>(entities);
        }

        public async Task<TestRCCostDto?> GetByKeyAsync(string testCode, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);

            var entity = await _repository.GetByKeyAsync(testCode, profitCentre);
            return entity == null ? null : _mapper.Map<TestRCCostDto>(entity);
        }

        //   Guards: null check, non-empty keys, FpsYear positive, duplicate PK check
        public async Task<TestRCCostDto> CreateAsync(TestRCCostDto dto)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);
            if (dto.FpsYear <= 0)
                throw new ArgumentException("FpsYear must be a positive integer.", nameof(dto));

            var exists = await _repository.ExistsAsync(dto.TestCode, dto.ProfitCentre);
            if (exists)
                throw new InvalidOperationException(
                    $"A TestRCCost entry with TestCode '{dto.TestCode}', ProfitCentre '{dto.ProfitCentre}' already exists for the current FPS year.");

            var entity = _mapper.Map<TestRCCost>(dto);
            var created = await _repository.AddAsync(entity);
            return _mapper.Map<TestRCCostDto>(created);
        }

        //   Guards: non-empty keys, route-key/body-key consistency, existence check
        public async Task<TestRCCostDto> UpdateAsync(string testCode, string profitCentre, TestRCCostDto dto)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.TestCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(dto.ProfitCentre);

            if (!string.Equals(testCode, dto.TestCode, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(profitCentre, dto.ProfitCentre, StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException(
                    "Route keys (testCode, profitCentre) must match the DTO body keys.");

            var existing = await _repository.GetByKeyAsync(testCode, profitCentre);
            if (existing == null)
                throw new KeyNotFoundException(
                    $"TestRCCost entry with TestCode '{testCode}', ProfitCentre '{profitCentre}' was not found for the current FPS year.");

            var entity = _mapper.Map<TestRCCost>(dto);
            var updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<TestRCCostDto>(updated);
        }

        public async Task<bool> DeleteAsync(string testCode, string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(testCode);
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);

            return await _repository.DeleteAsync(testCode, profitCentre);
        }
    }
}
