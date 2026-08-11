
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
public interface IPimsProgramManagerLinkApiClient
{
    Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync();

    Task<ApiResponseDto<List<ProgramLookupDto>>> GetProgramsAsync();

    Task<ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>> GetPagedByManagerAsync(QueryParameters<string> query, string manager);

    Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByProgramAsync(string program);

    Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetByManagerAsync(string manager);

    Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager);

    Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto);

    Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager);
}
}
