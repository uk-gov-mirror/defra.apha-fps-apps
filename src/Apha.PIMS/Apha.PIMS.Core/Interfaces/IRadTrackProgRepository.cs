using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IRadTrackProgRepository
    {
        Task<List<RadtrackProg>> GetAllRadTrackProgsAsync();

        Task<PagedData<RadtrackProg>> GetPagedRadTrackProgsAsync(PaginationParameters<string> query);

        Task<RadtrackProg?> GetRadTrackProgByProgramAsync(string program);

        Task<RadtrackProg> AddRadTrackProgAsync(RadtrackProg entity);

        Task<RadtrackProg> UpdateRadTrackProgAsync(RadtrackProg entity);

        Task<bool> DeleteRadTrackProgAsync(string program);

        Task<bool> RadTrackProgExistsAsync(string program);

        // Returns distinct non-null Program values from MY_tlkpProject ordered alphabetically — used to populate the Programme dropdown
        Task<List<string>> GetAllProgramNamesAsync();
    }
}
