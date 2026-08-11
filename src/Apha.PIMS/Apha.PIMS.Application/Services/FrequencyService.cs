using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Interfaces;
using Apha.PIMS.Application.Pagination;
using Apha.PIMS.Application.Validation;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Interfaces;
using Apha.PIMS.Core.Pagination;
using AutoMapper;

namespace Apha.PIMS.Application.Services
{
    
    public class FrequencyService : IFrequencyService
    {
        private readonly IFrequencyRepository _repository;
        private readonly IMapper _mapper;

        public FrequencyService(IFrequencyRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        
        public async Task<List<FrequencyDto>> GetAllFrequenciesAsync()
        {
            List<Frequency> entities = await _repository.GetAllFrequenciesAsync();
            return _mapper.Map<List<FrequencyDto>>(entities);
        }

        
        public async Task<PaginatedResult<FrequencyDto>> GetPagedFrequenciesAsync(QueryParameters<string> query)
        {
            var parameters = _mapper.Map<PaginationParameters<string>>(query);
            var pagedData = await _repository.GetPagedFrequenciesAsync(parameters);
            return _mapper.Map<PaginatedResult<FrequencyDto>>(pagedData);
        }

        
        public async Task<FrequencyDto?> GetFrequencyByIdAsync(int frequencyId)
        {
            Frequency? entity = await _repository.GetFrequencyByIdAsync(frequencyId);
            return entity is null ? null : _mapper.Map<FrequencyDto>(entity);
        }

        
        public async Task<FrequencyDto> CreateFrequencyAsync(FrequencyDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool duplicate = await _repository.FrequencyExistsAsync(dto.FrequencyId);
            if (duplicate)
                throw new BusinessValidationErrorException(
                [
                    new BusinessValidationError(
                        $"A frequency with ID '{dto.FrequencyId}' already exists.",
                        "FREQUENCY_DUPLICATE")
                ]);

            Frequency entity = _mapper.Map<Frequency>(dto);
            Frequency created = await _repository.AddFrequencyAsync(entity);
            return _mapper.Map<FrequencyDto>(created);
        }

        
        public async Task<FrequencyDto> UpdateFrequencyAsync(FrequencyDto dto)
        {
            if (dto is null) throw new ArgumentNullException(nameof(dto));

            bool exists = await _repository.FrequencyExistsAsync(dto.FrequencyId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Frequency with frequencyid {dto.FrequencyId} was not found.",
                        "FREQUENCY_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            Frequency entity = _mapper.Map<Frequency>(dto);
            Frequency updated = await _repository.UpdateFrequencyAsync(entity);
            return _mapper.Map<FrequencyDto>(updated);
        }

        
        public async Task<bool> DeleteFrequencyAsync(int frequencyId)
        {
            bool exists = await _repository.FrequencyExistsAsync(frequencyId);
            if (!exists)
            {
                var errors = new List<BusinessValidationError>
                {
                    new BusinessValidationError(
                        $"Frequency with frequencyid {frequencyId} was not found.",
                        "FREQUENCY_NOT_FOUND")
                };
                throw new BusinessValidationErrorException(errors);
            }

            return await _repository.DeleteFrequencyAsync(frequencyId);
        }

        public async Task<bool> FrequencyExistsAsync(int frequencyId)
        {
            return await _repository.FrequencyExistsAsync(frequencyId);
        }
    }
}
