using Apha.PIMS.Core.Entities;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IReportGroupLinkRepository
    {
        Task<List<ReportGroupLink>> GetAllReportGroupLinksAsync();

        Task<List<ReportGroupLink>> GetReportGroupLinksByReportIdAsync(int reportId);

        Task<ReportGroupLink?> GetReportGroupLinkByIdAsync(int reportId, int groupId);

        Task<ReportGroupLink> AddReportGroupLinkAsync(ReportGroupLink entity);

        Task<bool> DeleteReportGroupLinkAsync(int reportId, int groupId);

        Task<bool> ReportGroupLinkExistsAsync(int reportId, int groupId);
    }
}
