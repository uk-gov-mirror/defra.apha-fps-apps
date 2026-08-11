using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IProgramManagerLinkService
    {
        Task<List<ProgramManagerLinkDto>> GetAllProgramManagerLinksAsync();

        Task<PaginatedResult<ProgramManagerLinkDto>> GetPagedByManagerAsync(QueryParameters<string> query, string manager);

        Task<List<ProgramManagerLinkDto>> GetByProgramAsync(string program);

        Task<List<ProgramManagerLinkDto>> GetByManagerAsync(string manager);

        Task<ProgramManagerLinkDto?> GetProgramManagerLinkByIdAsync(string program, string manager);

        Task<ProgramManagerLinkDto> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto);

        Task<bool> DeleteProgramManagerLinkAsync(string program, string manager);

        Task<bool> ProgramManagerLinkExistsAsync(string program, string manager);

        Task<List<ProgramLookupDto>> GetProgramsAsync();
    }
}
