using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IWorkGroupEmployeeService
    {
        Task<PaginatedResult<WorkGroupEmployeeDto>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<PaginatedResult<WorkGroupEmployeeDto>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade);
        Task<PaginatedResult<WorkGroupEmployeeDto>> GetAllActiveWorkGroupEmployeesAsync(QueryParameters<string> query, string wgGrade);
        Task<WorkGroupEmployeeDto?> GetWorkGroupEmployeeByIdAsync(string pactId);
        
        Task<WorkGroupEmployeeDto> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeDto dto);
        Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);
        Task<WorkGroupEmployeeDto> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeDto dto);
        Task<bool> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
