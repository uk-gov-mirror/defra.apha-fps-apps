using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAnimalService
    {
        // Animal Master CRUD
        Task<IEnumerable<AnimalDto>> GetAllAnimalsAsync();
        Task<PaginatedResult<AnimalDto>> GetAllAnimalsAsync(QueryParameters<string> query);
        Task<AnimalDto?> GetAnimalByIdAsync(string animalType);
        Task<AnimalDto> AddAnimalAsync(AnimalDto animalDto);
        Task<AnimalDto> UpdateAnimalAsync(AnimalDto animalDto);
        Task<bool> DeleteAnimalAsync(string animalType);

        // Animal Cost (AnimalJob)
        Task<List<AnimalDto>> GetAnimalLookupAsync();
        Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostAsync(QueryParameters<string> query, string jobCode);

        // Animal Costs ASU View (AnimalCosts — frmAnimalCosts)
        Task<PaginatedResult<AnimalCostViewDto>> GetAnimalCostByAnimalTypeAsync(QueryParameters<string> query, string animalType);
        Task<PaginatedResult<AnimalSnapshotViewDto>> GetAnimalSnapshotAsync(QueryParameters<string> query);
        Task<decimal> GetTotalAnimalCostAsync(string jobCode);
        Task<AnimalCostViewDto?> GetAnimalCostViewByIdAsync(int indCounter, string jobCode);
        Task<decimal?> GetAnimalRateByIdAsync(string animalType, string jobCode);
        Task<AnimalRequestDto> AddAnimalCostAsync(AnimalRequestDto animalReq);
        Task<AnimalRequestDto> UpdateAnimalCostAsync(AnimalRequestDto animalReq);
        Task<bool> DeleteAnimalCostAsync(int indCounter);
    }
}

