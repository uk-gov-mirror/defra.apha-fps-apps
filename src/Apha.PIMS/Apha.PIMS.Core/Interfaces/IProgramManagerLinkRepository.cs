using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IProgramManagerLinkRepository
    {
        Task<List<ProgramManagerLink>> GetAllProgramManagerLinksAsync();

        Task<PagedData<ProgramManagerLink>> GetPagedByManagerAsync(PaginationParameters<string> query, string manager);

        Task<List<ProgramManagerLink>> GetByProgramAsync(string program);

        Task<List<ProgramManagerLink>> GetByManagerAsync(string manager);

        Task<ProgramManagerLink?> GetProgramManagerLinkByIdAsync(string program, string manager);

        Task<ProgramManagerLink> AddProgramManagerLinkAsync(ProgramManagerLink entity);

        Task<bool> DeleteProgramManagerLinkAsync(string program, string manager);

        Task<bool> ProgramManagerLinkExistsAsync(string program, string manager);

        // Dropdown: SELECT DISTINCTROW ProgramNo, Max(Year) AS LatestYear FROM MY_tlkpProgram GROUP BY ProgramNo
        Task<List<ProgramLookup>> GetProgramsAsync();
    }
}
