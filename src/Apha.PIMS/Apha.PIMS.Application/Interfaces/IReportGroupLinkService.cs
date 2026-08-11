using Apha.PIMS.Application.Dtos;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IReportGroupLinkService
    {
        Task<List<ReportGroupLinkDto>> GetAllReportGroupLinksAsync();

        Task<List<ReportGroupLinkDto>> GetReportGroupLinksByReportIdAsync(int reportId);

        Task<ReportGroupLinkDto?> GetReportGroupLinkByIdAsync(int reportId, int groupId);

        Task<ReportGroupLinkDto> CreateReportGroupLinkAsync(ReportGroupLinkDto dto);

        Task<bool> DeleteReportGroupLinkAsync(int reportId, int groupId);

        Task<bool> ReportGroupLinkExistsAsync(int reportId, int groupId);
    }
}
