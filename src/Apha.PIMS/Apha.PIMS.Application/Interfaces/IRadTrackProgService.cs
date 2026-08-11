using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IRadTrackProgService
    {
        Task<List<RadTrackProgDto>> GetAllRadTrackProgsAsync();

        Task<PaginatedResult<RadTrackProgDto>> GetPagedRadTrackProgsAsync(QueryParameters<string> query);

        Task<RadTrackProgDto?> GetRadTrackProgByProgramAsync(string program);

        Task<RadTrackProgDto> CreateRadTrackProgAsync(RadTrackProgDto dto);

        Task<RadTrackProgDto> UpdateRadTrackProgAsync(RadTrackProgDto dto);

        Task<bool> DeleteRadTrackProgAsync(string program);

        Task<bool> RadTrackProgExistsAsync(string program);

        // Returns distinct non-null Program values from MY_tlkpProject for populating the Programme dropdown
        Task<List<string>> GetAllProgramNamesAsync();
    }
}
