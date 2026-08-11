using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    public class SettingService : ISettingService
    {
        private readonly ISettingRepository _repository;
        private readonly IMapper _mapper;

        public SettingService(ISettingRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<SettingDto>> GetAllSettingsAsync()
        {
            List<Settings> entities = await _repository.GetAllSettingsAsync();
            return _mapper.Map<List<SettingDto>>(entities);
        }

        public async Task<List<SettingDto>> GetAllUserUpdateableSettingsAsync()
        {
            List<Settings> entities = await _repository.GetAllUserUpdateableSettingsAsync();
            return _mapper.Map<List<SettingDto>>(entities);
        }

       
        public async Task<SettingDto?> GetSettingByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Setting id is required.", nameof(id));

            Settings? entity = await _repository.GetSettingByIdAsync(id);
            return entity is null ? null : _mapper.Map<SettingDto>(entity);
        }

       
        public async Task<SettingDto> UpdateSettingAsync(SettingDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));
            if (string.IsNullOrWhiteSpace(dto.Id))
                throw new ArgumentException("Setting id is required.", nameof(dto));

            Settings? existing = await _repository.GetSettingByIdAsync(dto.Id);
            if (existing is null)
                throw new KeyNotFoundException($"Setting '{dto.Id}' was not found.");

            // Allow legacy Time-tab maintenance settings to be updated even when
            // not flagged as user-updateable in source data (parity with Costbook/PIMS legacy behavior).
            bool isLegacyTimeSetting =
                string.Equals(existing.Id, "HoursInDay", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(existing.Id, "DaysInYear", StringComparison.OrdinalIgnoreCase);

            
            if (existing.Userupdateable != true && !isLegacyTimeSetting)
                throw new InvalidOperationException(
                    $"Setting '{dto.Id}' is not user-updateable and cannot be modified through this operation.");

            Settings entity = _mapper.Map<Settings>(dto);
            Settings updated = await _repository.UpdateSettingAsync(entity);
            return _mapper.Map<SettingDto>(updated);
        }

        public async Task<bool> SettingExistsAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Setting id is required.", nameof(id));

            return await _repository.SettingExistsAsync(id);
        }
    }
}
