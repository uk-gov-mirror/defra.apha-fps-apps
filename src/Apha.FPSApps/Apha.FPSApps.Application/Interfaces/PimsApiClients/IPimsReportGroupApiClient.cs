using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsReportGroupApiClient
    {
        Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync();
        Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null);
        Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId);
        Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId);
        Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto);
        Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId);
    }
}
