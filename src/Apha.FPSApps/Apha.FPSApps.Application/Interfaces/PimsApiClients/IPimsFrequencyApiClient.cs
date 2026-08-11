using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsFrequencyApiClient
    {
        Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync();
        Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query);
        Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId);
        Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto);
        Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto);
        Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId);
    }
}
