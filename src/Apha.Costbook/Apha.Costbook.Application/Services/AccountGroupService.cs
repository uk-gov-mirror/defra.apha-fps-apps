using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
using Apha.Costbook.Application.Validation;
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
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(csg7Group))
                errors.Add(new BusinessValidationError("Csg7Group must not be null or empty.", "Csg7Group must not be null or empty."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var entity = await _repository.GetByCsg7GroupAsync(csg7Group);
            return entity is null ? null : _mapper.Map<AccountGroupDto>(entity);
        }

        public async Task<AccountGroupDto> AddAccountGroupAsync(AccountGroupDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto is null)
                errors.Add(new BusinessValidationError("AccountGroupDto must not be null.", "AccountGroupDto must not be null."));

            if (dto is not null && string.IsNullOrWhiteSpace(dto.Csg7group))
                errors.Add(new BusinessValidationError("Csg7Group must not be null or empty.", "Csg7Group must not be null or empty."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var normalizedCsg7Group = dto.Csg7group.Trim();

            var exists = await _repository.ExistsAsync(normalizedCsg7Group);
            if (exists)
                throw new BusinessValidationErrorException(
                    [new BusinessValidationError($"An AccountGroup with Csg7Group '{normalizedCsg7Group}' already exists.", $"An AccountGroup with Csg7Group '{normalizedCsg7Group}' already exists.")]);

            dto.Csg7group = normalizedCsg7Group;
            var entity = _mapper.Map<AccountGroup>(dto);
            var created = await _repository.AddAccountGroupAsync(entity);
            return _mapper.Map<AccountGroupDto>(created);
        }

        public async Task<AccountGroupDto> UpdateAccountGroupAsync(string csg7Group, AccountGroupDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto is null)
                errors.Add(new BusinessValidationError("AccountGroupDto must not be null.", "AccountGroupDto must not be null."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            dto.Csg7group = csg7Group;
            var entity = _mapper.Map<AccountGroup>(dto);
            var updated = await _repository.UpdateAccountGroupAsync(entity);
            return _mapper.Map<AccountGroupDto>(updated);
        }

        public async Task DeleteAccountGroupAsync(string csg7Group)
        {
            await _repository.DeleteAccountGroupAsync(csg7Group);
        }
    }
}
