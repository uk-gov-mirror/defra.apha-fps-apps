using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FpsApiClients
{
    public interface IFpsAnimalPlanApiClient
    {
        Task<ApiResponseDto<List<AnimalCostViewDto>>> GetAllAnimalCostAsync(QueryParameters<string> query, string jobCode);

        // Animal Costs ASU View (AnimalCosts — frmAnimalCosts)
        Task<ApiResponseDto<List<AnimalCostViewDto>>> GetAnimalCostByAnimalTypeAsync(QueryParameters<string> query, string animalType);

        Task<ApiResponseDto<List<AnimalDto>>> GetAnimalLookupAsync();
        Task<ApiResponseDto<decimal?>> GetAnimalRateAsync(string animalType, string jobCode);
        Task<ApiResponseDto<decimal>> GetTotalAnimalCostAsync(string jobCode);
        Task<ApiResponseDto<AnimalCostViewDto?>> GetAnimalCostViewByIdAsync(int indCounter, string jobCode);
        Task<ApiResponseDto<AnimalRequestDto>> CreateAnimalCostAsync(AnimalRequestDto animalRequest);
        Task<ApiResponseDto<AnimalRequestDto>> UpdateAnimalCostAsync(AnimalRequestDto animalRequest);
        Task<ApiResponseDto<bool>> DeleteAnimalCostAsync(int indCounter);
    }
}
