using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IReportGroupRepository
    {
        Task<List<ReportGroup>> GetAllReportGroupsAsync();

        Task<PagedData<ReportGroup>> GetPagedReportGroupsAsync(PaginationParameters<string> query, int? reportId = null);

        // Returns all ReportGroup rows that are linked to the given reportid via tblreportgroup_link
        Task<List<ReportGroup>> GetReportGroupsByReportIdAsync(int reportId);

        Task<ReportGroup?> GetReportGroupByIdAsync(int groupId);

        Task<ReportGroup> AddReportGroupAsync(ReportGroup entity);

        Task<ReportGroup> UpdateReportGroupAsync(ReportGroup entity);

        Task<bool> DeleteReportGroupAsync(int groupId);

        Task<bool> ReportGroupExistsAsync(int groupId);

        Task<bool> HasLinkedReportsAsync(int groupId);
    }
}
