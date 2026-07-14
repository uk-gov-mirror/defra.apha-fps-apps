using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Core.Entities;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Core.Pagination;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    
    public class AccountGroupService : IAccountGroupService
    {
        private readonly IAccountGroupRepository _repository;
        private readonly IMapper _mapper;

        public AccountGroupService(IAccountGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

       
        public async Task<List<AccountGroupDto>> GetAllAccountGroupAsync()
        {
            var entities = await _repository.GetAllAccountGroupAsync();
            return _mapper.Map<List<AccountGroupDto>>(entities);
        }

       
        public async Task<PaginatedResult<AccountGroupDto>> GetPaginatedAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var data = await _repository.GetPaginatedAsync(parameters);
            return new PaginatedResult<AccountGroupDto>(
                _mapper.Map<List<AccountGroupDto>>(data.Data),
                _mapper.Map<PaginationDto>(data.PaginationData));
        }

        
        public async Task<AccountGroupDto?> GetByCsg7GroupAsync(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(csg7Group));

            var entity = await _repository.GetByCsg7GroupAsync(csg7Group);
            return entity is null ? null : _mapper.Map<AccountGroupDto>(entity);
        }

        
        public async Task<AccountGroupDto> AddAccountGroupAsync(AccountGroupDto dto)
        {
            if (dto is null)
                throw new ArgumentException("AccountGroupDto must not be null.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Csg7group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(dto));

           
            var exists = await _repository.ExistsAsync(dto.Csg7group);
            if (exists)
                throw new ArgumentException($"An AccountGroup with Csg7Group '{dto.Csg7group}' already exists.", nameof(dto));

            var entity = _mapper.Map<AccountGroup>(dto);
            var created = await _repository.AddAccountGroupAsync(entity);
            return _mapper.Map<AccountGroupDto>(created);
        }

        
        public async Task<AccountGroupDto> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(csg7Group));
            if (dto is null)
                throw new ArgumentException("AccountGroupDto must not be null.", nameof(dto));

            
            var exists = await _repository.ExistsAsync(csg7Group);
            if (!exists)
                throw new KeyNotFoundException($"AccountGroup with Csg7Group '{csg7Group}' was not found.");

            
            dto.Csg7group = csg7Group;
            var entity = _mapper.Map<AccountGroup>(dto);
            var updated = await _repository.UpdateAccountGroupAsync(entity);
            return _mapper.Map<AccountGroupDto>(updated);
        }

        
        public async Task DeleteAccountGroupAsync(string csg7Group)
        {
            if (string.IsNullOrWhiteSpace(csg7Group))
                throw new ArgumentException("Csg7Group must not be null or empty.", nameof(csg7Group));

            var exists = await _repository.ExistsAsync(csg7Group);
            if (!exists)
                throw new KeyNotFoundException($"AccountGroup with Csg7Group '{csg7Group}' was not found.");

            await _repository.DeleteAccountGroupAsync(csg7Group);
        }
    }
}
