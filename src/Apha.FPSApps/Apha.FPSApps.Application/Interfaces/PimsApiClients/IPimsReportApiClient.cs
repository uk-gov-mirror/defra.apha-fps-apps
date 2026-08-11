using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    public interface IPimsReportApiClient
    {
        Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync();
        Task<ApiResponseDto<PaginatedResult<ReportDto>>> GetPagedReportsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id);
        Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto);
        Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto);
        Task<ApiResponseDto<bool>> DeleteReportAsync(int id);
    }
}
