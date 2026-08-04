using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class AnimalService : IAnimalService
    {
        private readonly IFpsApiClient _fpsApiClient;

        public AnimalService(IFpsApiClient fpsApiClient)
        {
            _fpsApiClient = fpsApiClient ?? throw new ArgumentNullException(nameof(fpsApiClient));
        }

        public async Task<ApiResponseDto<IEnumerable<AnimalDto>>> GetAllAnimalsAsync()
        {
            return await _fpsApiClient.FpsAnimalMaster.GetAllAnimalsAsync();
        }

        public async Task<ApiResponseDto<List<AnimalDto>>> GetAllAnimalsAsync(QueryParameters<string> query)
        {
            return await _fpsApiClient.FpsAnimalMaster.GetAllAnimalsAsync(query);
        }

        public async Task<ApiResponseDto<List<AnimalSnapshotViewDto>>> GetAnimalSnapshotAsync(QueryParameters<string> query)
        {
            return await _fpsApiClient.FpsAnimalMaster.GetAnimalSnapshotAsync(query);
        }

        public async Task<ApiResponseDto<AnimalDto?>> GetAnimalByIdAsync(string animalType)
        {
            return await _fpsApiClient.FpsAnimalMaster.GetAnimalByIdAsync(animalType);
        }

        public async Task<ApiResponseDto<AnimalDto>> AddAnimalAsync(AnimalDto animalDto)
        {
            return await _fpsApiClient.FpsAnimalMaster.AddAnimalAsync(animalDto);
        }

        public async Task<ApiResponseDto<AnimalDto>> UpdateAnimalAsync(AnimalDto animalDto)
        {
            return await _fpsApiClient.FpsAnimalMaster.UpdateAnimalAsync(animalDto);
        }

        public async Task<ApiResponseDto<bool>> DeleteAnimalAsync(string animalType)
        {
            return await _fpsApiClient.FpsAnimalMaster.DeleteAnimalAsync(animalType);
        }
    }
}
