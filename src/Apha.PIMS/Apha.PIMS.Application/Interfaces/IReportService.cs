using Apha.PIMS.Application.Dtos;
using Apha.PIMS.Application.Pagination;

namespace Apha.PIMS.Application.Interfaces
{
    public interface IReportService
    {
        Task<List<ReportDto>> GetAllReportsAsync();

        Task<PaginatedResult<ReportDto>> GetPagedReportsAsync(QueryParameters<string> query);

        Task<ReportDto?> GetReportByIdAsync(int id);

        Task<ReportDto> CreateReportAsync(ReportDto dto);

        Task<ReportDto> UpdateReportAsync(ReportDto dto);

        Task<bool> DeleteReportAsync(int id);

        Task<bool> ReportExistsAsync(int id);
    }
}
