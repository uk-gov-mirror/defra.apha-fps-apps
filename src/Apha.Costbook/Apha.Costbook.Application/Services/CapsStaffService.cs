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
    public class CapsStaffService : ICapsStaffService
    {
        private readonly ICapsStaffRepository _repository;
        private readonly IMapper _mapper;

        public CapsStaffService(ICapsStaffRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<StaffDto>> GetAllStaffAsync()
        {
            var entities = await _repository.GetAllStaffAsync();
            return _mapper.Map<List<StaffDto>>(entities);
        }

        public async Task<PaginatedResult<StaffDto>> GetPaginatedAsync(QueryParameters<string> queryParameters)
        {
            var errors = new List<BusinessValidationError>();

            if (queryParameters == null)
                errors.Add(new BusinessValidationError("Query parameters must not be null.", "Query parameters must not be null."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var coreParams = _mapper.Map<PaginationParameters<string>>(queryParameters);
            var pagedData = await _repository.GetPaginatedAsync(coreParams);

            return new PaginatedResult<StaffDto>(
                _mapper.Map<List<StaffDto>>(pagedData.Data),
                _mapper.Map<PaginationDto>(pagedData.PaginationData));
        }

        public async Task<StaffDto?> GetByMNumberAsync(string mNumber)
        {
            var errors = new List<BusinessValidationError>();

            if (string.IsNullOrWhiteSpace(mNumber))
                errors.Add(new BusinessValidationError("MNumber must not be null or empty.", "MNumber must not be null or empty."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var entity = await _repository.GetByMNumberAsync(mNumber);
            return entity is null ? null : _mapper.Map<StaffDto>(entity);
        }

        public async Task<StaffDto> AddStaffAsync(StaffDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto is null)
                errors.Add(new BusinessValidationError("StaffDto must not be null.", "StaffDto must not be null."));

            if (dto is not null && string.IsNullOrWhiteSpace(dto.Mnumber))
                errors.Add(new BusinessValidationError("MNumber must not be null or empty.", "MNumber must not be null or empty."));

            if (dto is not null && string.IsNullOrWhiteSpace(dto.Name))
                errors.Add(new BusinessValidationError("Name must not be null or empty.", "Name must not be null or empty."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            var normalizedMNumber = dto.Mnumber.Trim();

            var exists = await _repository.ExistsAsync(normalizedMNumber);
            if (exists)
                throw new BusinessValidationErrorException(
                    [new BusinessValidationError($"A CAPS staff member with MNumber '{normalizedMNumber}' already exists.", $"A CAPS staff member with MNumber '{normalizedMNumber}' already exists.")]);

            dto.Mnumber = normalizedMNumber;
            var entity = _mapper.Map<Staff>(dto);
            var created = await _repository.AddStaffAsync(entity);
            return _mapper.Map<StaffDto>(created);
        }

        public async Task<StaffDto> UpdateStaffAsync(string mNumber, StaffDto dto)
        {
            var errors = new List<BusinessValidationError>();

            if (dto is null)
                errors.Add(new BusinessValidationError("StaffDto must not be null.", "StaffDto must not be null."));

            if (dto is not null && string.IsNullOrWhiteSpace(dto.Name))
                errors.Add(new BusinessValidationError("Name must not be null or empty.", "Name must not be null or empty."));

            if (errors.Count > 0)
                throw new BusinessValidationErrorException(errors);

            dto.Mnumber = mNumber;
            var entity = _mapper.Map<Staff>(dto);
            var updated = await _repository.UpdateStaffAsync(entity);
            return _mapper.Map<StaffDto>(updated);
        }

        public async Task DeleteStaffAsync(string mNumber)
        {
            await _repository.DeleteStaffAsync(mNumber);
        }
    }
}
