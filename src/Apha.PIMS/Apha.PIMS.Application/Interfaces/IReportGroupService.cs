using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IReportGroupService
    {
        Task<List<ReportGroupDto>> GetAllReportGroupsAsync();

        Task<PaginatedResult<ReportGroupDto>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null);

        // Returns all report groups linked to the given reportid
        Task<List<ReportGroupDto>> GetReportGroupsByReportIdAsync(int reportId);

        Task<ReportGroupDto?> GetReportGroupByIdAsync(int groupId);

        Task<ReportGroupDto> CreateReportGroupAsync(ReportGroupDto dto);

        Task<ReportGroupDto> UpdateReportGroupAsync(ReportGroupDto dto);

        Task<bool> DeleteReportGroupAsync(int groupId);

        Task<bool> ReportGroupExistsAsync(int groupId);
    }
}
