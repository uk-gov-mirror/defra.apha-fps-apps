using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsReportGroupLinkApiClient
    {
        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync();

        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportId);

        Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportId, int groupId);

        Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto);

        Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportId, int groupId);
    }
}
