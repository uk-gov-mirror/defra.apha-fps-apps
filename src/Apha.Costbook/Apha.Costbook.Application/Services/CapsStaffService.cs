using Apha.Costbook.Application.Dtos;
using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Application.Pagination;
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
            if (queryParameters == null)
                throw new ArgumentException("Query parameters must not be null.", nameof(queryParameters));

            var coreParams = _mapper.Map<PaginationParameters<string>>(queryParameters);
            var pagedData = await _repository.GetPaginatedAsync(coreParams);

            return new PaginatedResult<StaffDto>(
                _mapper.Map<List<StaffDto>>(pagedData.Data),
                _mapper.Map<PaginationDto>(pagedData.PaginationData));
        }

        
        public async Task<StaffDto?> GetByMNumberAsync(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(mNumber));

            var entity = await _repository.GetByMNumberAsync(mNumber);
            return entity is null ? null : _mapper.Map<StaffDto>(entity);
        }

        
        public async Task<StaffDto> AddStaffAsync(StaffDto dto)
        {
            if (dto is null)
                throw new ArgumentException("StaffDto must not be null.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Mnumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name must not be null or empty.", nameof(dto));

           
            var exists = await _repository.ExistsAsync(dto.Mnumber);
            if (exists)
                throw new ArgumentException($"A CAPS staff member with MNumber '{dto.Mnumber}' already exists.", nameof(dto));

            var entity = _mapper.Map<Staff>(dto);
            var created = await _repository.AddStaffAsync(entity);
            return _mapper.Map<StaffDto>(created);
        }

        
        public async Task<StaffDto> UpdateStaffAsync(string mNumber, StaffDto dto)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(mNumber));
            if (dto is null)
                throw new ArgumentException("StaffDto must not be null.", nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Name))
                throw new ArgumentException("Name must not be null or empty.", nameof(dto));

            
            var exists = await _repository.ExistsAsync(mNumber);
            if (!exists)
                throw new KeyNotFoundException($"CAPS staff member with MNumber '{mNumber}' was not found.");

            
            dto.Mnumber = mNumber;
            var entity = _mapper.Map<Staff>(dto);
            var updated = await _repository.UpdateStaffAsync(entity);
            return _mapper.Map<StaffDto>(updated);
        }

        
        public async Task DeleteStaffAsync(string mNumber)
        {
            if (string.IsNullOrWhiteSpace(mNumber))
                throw new ArgumentException("MNumber must not be null or empty.", nameof(mNumber));

            var exists = await _repository.ExistsAsync(mNumber);
            if (!exists)
                throw new KeyNotFoundException($"CAPS staff member with MNumber '{mNumber}' was not found.");

            await _repository.DeleteStaffAsync(mNumber);
        }
    }
}
