using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class RadTrackProgService : IRadTrackProgService
    {
        private readonly IRadTrackProgRepository _repository;
        private readonly IMapper _mapper;

        public RadTrackProgService(IRadTrackProgRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<RadTrackProgDto>> GetAllRadTrackProgsAsync()
        {
            List<RadtrackProg> entities = await _repository.GetAllRadTrackProgsAsync();
            return _mapper.Map<List<RadTrackProgDto>>(entities);
        }

        public async Task<PaginatedResult<RadTrackProgDto>> GetPagedRadTrackProgsAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedRadTrackProgsAsync(parameters);
            return _mapper.Map<PaginatedResult<RadTrackProgDto>>(pagedData);
        }

        public async Task<RadTrackProgDto?> GetRadTrackProgByProgramAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) throw new ArgumentException("program must not be empty.", nameof(program));

            RadtrackProg? entity = await _repository.GetRadTrackProgByProgramAsync(program);
            return entity is null ? null : _mapper.Map<RadTrackProgDto>(entity);
        }

        
        public async Task<RadTrackProgDto> CreateRadTrackProgAsync(RadTrackProgDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Program)) throw new ArgumentException("Program must not be empty.", nameof(dto));

            // Check if program already exists to prevent duplicate key constraint violation
            bool exists = await _repository.RadTrackProgExistsAsync(dto.Program);
            if (exists)
                throw new InvalidOperationException($"Program '{dto.Program}' already exists. Please use a different program name or update the existing record.");

            RadtrackProg entity = _mapper.Map<RadtrackProg>(dto);
            RadtrackProg created = await _repository.AddRadTrackProgAsync(entity);
            return _mapper.Map<RadTrackProgDto>(created);
        }

        
        public async Task<RadTrackProgDto> UpdateRadTrackProgAsync(RadTrackProgDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Program)) throw new ArgumentException("Program must not be empty.", nameof(dto));

            bool exists = await _repository.RadTrackProgExistsAsync(dto.Program);
            if (!exists)
                throw new KeyNotFoundException($"RadTrackProg with program '{dto.Program}' was not found.");

            RadtrackProg entity = _mapper.Map<RadtrackProg>(dto);
            RadtrackProg updated = await _repository.UpdateRadTrackProgAsync(entity);
            return _mapper.Map<RadTrackProgDto>(updated);
        }

        
        public async Task<bool> DeleteRadTrackProgAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) throw new ArgumentException("program must not be empty.", nameof(program));

            bool exists = await _repository.RadTrackProgExistsAsync(program);
            if (!exists)
                throw new KeyNotFoundException($"RadTrackProg with program '{program}' was not found.");

            return await _repository.DeleteRadTrackProgAsync(program);
        }

        public async Task<bool> RadTrackProgExistsAsync(string program)
        {
            if (string.IsNullOrWhiteSpace(program)) return false;
            return await _repository.RadTrackProgExistsAsync(program);
        }

        // Returns distinct non-null Program values from MY_tlkpProject for populating the Programme dropdown
        public async Task<List<string>> GetAllProgramNamesAsync()
        {
            return await _repository.GetAllProgramNamesAsync();
        }
    }
}
