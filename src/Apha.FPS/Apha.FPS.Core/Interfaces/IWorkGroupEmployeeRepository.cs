using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Pagination;

namespace Apha.FPS.Core.Interfaces
{
    public interface IWorkGroupEmployeeRepository
    {
        Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeAsync(PaginationParameters<string> query, string wgGrade);
        Task<PagedData<WorkGroupEmployeeView>> GetWorkGroupEmployeeForStaffAsync(PaginationParameters<string> query, string wgGrade);
        Task<PagedData<WorkGroupEmployeeView>> GetAllActiveWorkGroupEmployeesAsync(PaginationParameters<string> query, string wgGrade);
        Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<WorkGroupEmployeeView?> GetWorkGroupEmployeeByIdForStaffAsync(string pactId);
        Task<WorkGroupEmployee> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity);
        Task<WorkGroupEmployee> UpdateWorkGroupEmployeeAsync(WorkGroupEmployee entity);
        Task<WorkGroupEmployee> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployee entity);
        Task<bool> DeleteWorkGroupEmployeeAsync(string pactId);
        Task<bool> HasAssociatedStaffAsync(string wgGrade);
    }
}
