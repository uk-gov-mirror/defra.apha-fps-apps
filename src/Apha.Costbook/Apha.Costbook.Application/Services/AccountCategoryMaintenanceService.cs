using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    public class AccountCategoryMaintenanceService : IAccountCategoryMaintenanceService
    {
        private readonly IFpsAccountCategoryRepository _repository;
        private readonly IMapper _mapper;

        public AccountCategoryMaintenanceService(IFpsAccountCategoryRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
        
        public async Task<List<AccountCategoryMaintenanceDto>> GetAllForMaintenanceAsync()
        {
            var entities = await _repository.GetAllForMaintenanceAsync();
            return _mapper.Map<List<AccountCategoryMaintenanceDto>>(entities);
        }

        
        public async Task<PaginatedResult<AccountCategoryMaintenanceDto>> GetPaginatedAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetPaginatedAsync(parameters);
            return new PaginatedResult<AccountCategoryMaintenanceDto>(
                _mapper.Map<List<AccountCategoryMaintenanceDto>>(data.Data),
                _mapper.Map<PaginationDto>(data.PaginationData));
        }

        
        public async Task<AccountCategoryMaintenanceDto> UpdateCsg7GroupAsync(string accShortName, string? csg7Group)
        {
           
            if (string.IsNullOrWhiteSpace(accShortName))
                throw new ArgumentException("AccShortName must not be null or empty.", nameof(accShortName));

            
            var existing = await _repository.GetByAccShortNameAsync(accShortName);
            if (existing is null)
                throw new KeyNotFoundException($"Account category with AccShortName '{accShortName}' was not found.");

            
            var updated = await _repository.UpdateCsg7GroupAsync(accShortName, csg7Group);
            if (!updated)
                throw new InvalidOperationException($"Failed to update CSG7 group for account category '{accShortName}'.");

            
            var refreshed = await _repository.GetByAccShortNameAsync(accShortName);
            return _mapper.Map<AccountCategoryMaintenanceDto>(refreshed);
        }
    }
}
