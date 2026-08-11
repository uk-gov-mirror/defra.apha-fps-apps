using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;

namespace Apha.PIMS.Core.Interfaces
{
    public interface IReportRepository
    {
        Task<List<Report>> GetAllReportsAsync();

        Task<PagedData<Report>> GetPagedReportsAsync(PaginationParameters<string> query);

        Task<Report?> GetReportByIdAsync(int id);

        Task<Report> AddReportAsync(Report entity);

        Task<Report> UpdateReportAsync(Report entity);

        Task<bool> DeleteReportAsync(int id);

        Task<bool> ReportExistsAsync(int id);
    }
}
