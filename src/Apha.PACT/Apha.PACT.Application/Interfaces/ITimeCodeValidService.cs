using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Pagination;

namespace Apha.PACT.Application.Interfaces
{
    public interface ITimeCodeValidService
    {
        Task<IEnumerable<TimeCodeValidDto>> GetByJobCodeAsync(string jobCode, string parentProject);
        Task<IEnumerable<TimeCodeValidDto>> GetTimeCodeValidsByWorkGroupAsync(string workGroup);
        Task<IEnumerable<string>> GetTimeCodeValidProjectsByWorkGroupAndTimeCodeAsync(string workGroup, string timeCode);
        Task<IEnumerable<string>> GetAllDistinctTimeCodesAsync();
        Task<IEnumerable<string>> GetAllDistinctProjectsAsync();
        Task<PaginatedResult<TimeCodeValidDto>> GetPagedTimeCodesAsync(QueryParameters<string> query, string? jobCode, string? parentProject);
        Task<PaginatedResult<TimeCodeValidDto>> GetPagedByProjectAndTestCodeAsync(QueryParameters<string> query, string parentProject, string testCode);
        Task<TimeCodeValidDto?> GetTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<TimeCodeValidDto> CreateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid);
        Task<TimeCodeValidDto> UpdateTimeCodeValidAsync(TimeCodeValidDto timeCodeValid);
        Task<bool> DeleteTimeCodeValidAsync(string workGroup, string timeCode, string parentProject);
        Task<bool> DeleteAllByJobCodeAsync(string jobCode, string parentProject);
        Task<IEnumerable<TimeCodeValidDto>> CopyWorkGroupAsync(string sourceJobCode, string targetJobCode, string parentProject);
        Task<bool> DeleteBulkAsync(IEnumerable<(string WorkGroup, string TimeCode)> items, string parentProject);
        Task<IEnumerable<TimeCodeValidDto>> CopySelectedWorkGroupsAsync(IEnumerable<string> workGroups, string sourceJobCode, string targetJobCode, string parentProject);
    }
}
