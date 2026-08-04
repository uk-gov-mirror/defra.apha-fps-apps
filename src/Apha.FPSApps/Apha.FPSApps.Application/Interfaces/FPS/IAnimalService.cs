using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IAnimalService
    {
        Task<ApiResponseDto<IEnumerable<AnimalDto>>> GetAllAnimalsAsync();
        Task<ApiResponseDto<List<AnimalDto>>> GetAllAnimalsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<AnimalSnapshotViewDto>>> GetAnimalSnapshotAsync(QueryParameters<string> query);
        Task<ApiResponseDto<AnimalDto?>> GetAnimalByIdAsync(string animalType);
        Task<ApiResponseDto<AnimalDto>> AddAnimalAsync(AnimalDto animalDto);
        Task<ApiResponseDto<AnimalDto>> UpdateAnimalAsync(AnimalDto animalDto);
        Task<ApiResponseDto<bool>> DeleteAnimalAsync(string animalType);
    }
}
