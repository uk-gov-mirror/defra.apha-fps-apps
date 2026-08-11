using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Pagination;

namespace Apha.PACT.Core.Interfaces
{
    public interface ITimeCodeValidRepository
    {
        Task<IEnumerable<TimeCodeValid>> GetByJobCodeAsync(string jobCode, string parentProject);
        Task<List<TimeCodeValid>> GetTimeCodeValidsAsync();
        Task<IEnumerable<TimeCodeValid>> GetTimeCodeValidsByWorkGroupAsync(string workGroup);
        Task<IEnumerable<string>> GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync(string workGroup, string timeCode);
        Task<PagedData<TimeCodeValid>> GetPagedTimeCodesAsync(PaginationParameters<string> query, string? jobCode, string? parentProject);
        Task<PagedData<TimeCodeValid>> GetPagedByProjectAndTestCodeAsync(PaginationParameters<string> query, string parentProject, string testCode);
        Task<TimeCodeValid?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<TimeCodeValid> CreateTimeCodeValidAsync(TimeCodeValid timeCodeValid);
        Task<TimeCodeValid> UpdateTimeCodeValidAsync(TimeCodeValid timeCodeValid);
        Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject);
        Task<IEnumerable<TimeCodeValid>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject);
        Task<bool> DeleteBulkAsync(IEnumerable<(string WorkGroup, string TimeCode)> items, string parentProject);
        Task<IEnumerable<TimeCodeValid>> CopySelectedWorkGroupsAsync(IEnumerable<string> workGroups, string sourceJobCode, string targetJobCode, string parentProject);
        Task<bool> HasRelatedTimeCodeValidRecordsAsync(string jobCode);
    }
}
