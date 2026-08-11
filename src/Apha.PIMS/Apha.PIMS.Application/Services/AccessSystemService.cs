using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    
    public class AccessSystemService : IAccessSystemService
    {
        private readonly IAccessSystemRepository _repository;
        private readonly IMapper _mapper;

        public AccessSystemService(IAccessSystemRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<AccessSystemDto>> GetAllAsync()
        {
            List<AccessSystem> entities = await _repository.GetAllAsync();
            return _mapper.Map<List<AccessSystemDto>>(entities);
        }

        
        public async Task<AccessSystemDto?> GetByIdAsync(int systemid)
        {
            AccessSystem? entity = await _repository.GetByIdAsync(systemid);
            return entity is null ? null : _mapper.Map<AccessSystemDto>(entity);
        }

        public async Task<bool> ExistsAsync(int systemid)
        {
            return await _repository.ExistsAsync(systemid);
        }
    }
}
