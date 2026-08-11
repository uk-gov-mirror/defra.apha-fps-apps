using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsRadTrackProgApiClient
    {
        Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync();

        Task<ApiResponseDto<PaginatedResult<RadTrackProgDto>>> GetPagedRadTrackProgsAsync(QueryParameters<string> query);

        Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByProgramAsync(string program);

        Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto);

        Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto);

        Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program);
        Task<ApiResponseDto<List<string>>> GetAllProgramNamesAsync();
    }
}
