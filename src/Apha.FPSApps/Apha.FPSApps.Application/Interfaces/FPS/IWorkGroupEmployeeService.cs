using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IWorkGroupEmployeeService
    {
        Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId);
        Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto);
        Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetAllActiveWorkGroupEmployeesAsync(QueryParameters<string> query, string wgGrade);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto);
        Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto);
        Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId);
    }
}
