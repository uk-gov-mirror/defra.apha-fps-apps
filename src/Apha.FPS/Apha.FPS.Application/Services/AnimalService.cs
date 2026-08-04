using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class AnimalService : IAnimalService
    {
        private readonly IAnimalRepository _animalRepository;
        private readonly IMapper _mapper;

        public AnimalService(IAnimalRepository animalRepository, IMapper mapper)
        {
            _animalRepository = animalRepository ?? throw new ArgumentNullException(nameof(animalRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        // Animal Master CRUD

        public async Task<IEnumerable<AnimalDto>> GetAllAnimalsAsync()
        {
            var animals = await _animalRepository.GetAllAnimalsAsync();
            return _mapper.Map<IEnumerable<AnimalDto>>(animals);
        }

        public async Task<PaginatedResult<AnimalDto>> GetAllAnimalsAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var paged = await _animalRepository.GetAllAnimalsAsync(filter);
            return _mapper.Map<PaginatedResult<AnimalDto>>(paged);
        }

        public async Task<AnimalDto?> GetAnimalByIdAsync(string animalType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(animalType);
            var animal = await _animalRepository.GetAnimalByIdAsync(animalType);
            return _mapper.Map<AnimalDto?>(animal);
        }

        public async Task<AnimalDto> AddAnimalAsync(AnimalDto animalDto)
        {
            ArgumentNullException.ThrowIfNull(animalDto);
            if (string.IsNullOrWhiteSpace(animalDto.AnimalType))
                throw new ArgumentException("Animal type is required.");

            var existing = await _animalRepository.GetAnimalByIdAsync(animalDto.AnimalType);
            if (existing != null)
                throw new InvalidOperationException($"Animal '{animalDto.AnimalType}' already exists.");

            var entity = _mapper.Map<Animal>(animalDto);
            var added = await _animalRepository.AddAnimalAsync(entity);
            return _mapper.Map<AnimalDto>(added);
        }

        public async Task<AnimalDto> UpdateAnimalAsync(AnimalDto animalDto)
        {
            ArgumentNullException.ThrowIfNull(animalDto);
            if (string.IsNullOrWhiteSpace(animalDto.AnimalType))
                throw new ArgumentException("Animal type is required.");

            var existing = await _animalRepository.GetAnimalByIdAsync(animalDto.AnimalType)
                ?? throw new KeyNotFoundException($"Animal '{animalDto.AnimalType}' not found.");

            _mapper.Map(animalDto, existing);
            var updated = await _animalRepository.UpdateAnimalAsync(existing);
            return _mapper.Map<AnimalDto>(updated);
        }

        public async Task<bool> DeleteAnimalAsync(string animalType)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(animalType);
            return await _animalRepository.DeleteAnimalAsync(animalType);
        }

        // Animal Cost (AnimalJob)

        public async Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostAsync(QueryParameters<string> query, string jobCode)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var animalCostViews = await _animalRepository.GetAnimalCostAsync(filter, jobCode);
            return _mapper.Map<PaginatedResult<AnimalCostViewDto>>(animalCostViews);
        }

        public async Task<PaginatedResult<AnimalSnapshotViewDto>> GetAnimalSnapshotAsync(QueryParameters<string> query)
        {
            ArgumentNullException.ThrowIfNull(query);
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var animalSnapshots = await _animalRepository.GetAnimalSnapshotAsync(filter);
            return _mapper.Map<PaginatedResult<AnimalSnapshotViewDto>>(animalSnapshots);
        }

        public async Task<List<AnimalDto>> GetAnimalLookupAsync()
        {
            var animalLookup = await _animalRepository.GetAnimalLookup();
            return _mapper.Map<List<AnimalDto>>(animalLookup);
        }

        public async Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode)
        {
            var animalCostViews = await _animalRepository.GetAnimalRateByIdAsync(animalType, jobCode);
            return animalCostViews;
        }
        public async Task<AnimalRequestDto> AddAnimalCostAsync(AnimalRequestDto animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
            ArgumentOutOfRangeException.ThrowIfNegative(animalReq.NumberOfDays);
            ArgumentOutOfRangeException.ThrowIfNegative(animalReq.NumberOfAnimals);

            var mapAnimalReq = _mapper.Map<AnimalRequest>(animalReq);
            var animalRequest = await _animalRepository.AddAnimalCostAsync(mapAnimalReq);
            return _mapper.Map<AnimalRequestDto>(animalRequest);
        }
        public async Task<AnimalRequestDto> UpdateAnimalCostAsync(AnimalRequestDto animalReq)
        {
            ArgumentNullException.ThrowIfNull(animalReq);
            ArgumentOutOfRangeException.ThrowIfNegative(animalReq.NumberOfDays);
            ArgumentOutOfRangeException.ThrowIfNegative(animalReq.NumberOfAnimals);

            var mapAnimalReq = _mapper.Map<AnimalRequest>(animalReq);
            var animalRequest = await _animalRepository.UpdateAnimalCostAsync(mapAnimalReq);
            return _mapper.Map<AnimalRequestDto>(animalRequest);
        }
        public async Task<bool> DeleteAnimalCostAsync(int indCounter)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(indCounter);
            return await _animalRepository.DeleteJobAnimalCostAsync(indCounter);
        }

        public async Task<decimal> GetTotalAnimalCostAsync(string jobCode)
            => await _animalRepository.GetTotalAnimalCostAsync(jobCode);

        // Animal Costs ASU View (AnimalCosts — frmAnimalCosts)
        public async Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostByAnimalTypeAsync(
            QueryParameters<string> query, string animalType)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var paged = await _animalRepository.GetAnimalCostByAnimalTypeAsync(filter, animalType);
            return _mapper.Map<PaginatedResult<AnimalCostViewDto>>(paged);
        }

        public async Task<AnimalCostViewDto?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode)
        {
            var result = await _animalRepository.GetAnimalCostViewByIdAsync(indCounter, jobCode);
            return result == null ? null : _mapper.Map<AnimalCostViewDto>(result);
        }

    }
}
