using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class AccessLevelService : IAccessLevelService
    {
        private readonly IAccessLevelRepository _repository;
        private readonly IMapper _mapper;

        public AccessLevelService(IAccessLevelRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<AccessLevelDto>> GetAllAsync()
        {
            List<AccessLevel> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessLevelDto>>(entities);
        }

       
        public async Task<List<AccessLevelDto>> GetBySystemIdAsync(int systemid)
        {
            List<AccessLevel> entities = await _repository.GetBySystemIdAsync(systemid);
            return _mapper.Map<List<AccessLevelDto>>(entities);
        }

        
        public async Task<AccessLevelDto?> GetByIdAsync(int systemid, int accesslevelid)
        {
            AccessLevel? entity = await _repository.GetByIdAsync(systemid, accesslevelid);
            return entity is null ? null : _mapper.Map<AccessLevelDto>(entity);
        }

        
        public async Task<AccessLevelDto> CreateAsync(AccessLevelDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool alreadyExists = await _repository.ExistsAsync(dto.SystemId, dto.AccessLevelId);
            if (alreadyExists)
                throw new InvalidOperationException(
                    $"AccessLevel (systemid={dto.SystemId}, accesslevelid={dto.AccessLevelId}) already exists.");

            AccessLevel entity = _mapper.Map<AccessLevel>(dto);
            AccessLevel created = await _repository.AddAsync(entity);
            return _mapper.Map<AccessLevelDto>(created);
        }

        public async Task<AccessLevelDto> UpdateAsync(AccessLevelDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.ExistsAsync(dto.SystemId, dto.AccessLevelId);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessLevel (systemid={dto.SystemId}, accesslevelid={dto.AccessLevelId}) was not found.");

            AccessLevel entity = _mapper.Map<AccessLevel>(dto);
            AccessLevel updated = await _repository.UpdateAsync(entity);
            return _mapper.Map<AccessLevelDto>(updated);
        }

        
        public async Task DeleteAsync(int systemid, int accesslevelid)
        {
            bool exists = await _repository.ExistsAsync(systemid, accesslevelid);
            if (!exists)
                throw new KeyNotFoundException(
                    $"AccessLevel (systemid={systemid}, accesslevelid={accesslevelid}) was not found.");

            await _repository.DeleteAsync(systemid, accesslevelid);
        }

        public async Task<bool> ExistsAsync(int systemid, int accesslevelid)
        {
            return await _repository.ExistsAsync(systemid, accesslevelid);
        }
    }
}
