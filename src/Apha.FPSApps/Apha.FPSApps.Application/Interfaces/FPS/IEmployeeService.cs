using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.FPS
{
    public interface IEmployeeService
    {
        Task<ApiResponseDto<List<EmployeeDto>>> GetFilteredEmployeesAsync(QueryParameters<string> criteria, int filterOption);
        Task<ApiResponseDto<EmployeeDto>> GetEmployeeByIdAsync(string spNumber);
        Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employee);
        Task<ApiResponseDto<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto employee);
        Task<ApiResponseDto<bool>> DeleteEmployeeAsync(string spNumber);
        Task<ApiResponseDto<List<ManagerDto>>> GetAllManagersAsync();
        Task<ApiResponseDto<List<ManagerDto>>> GetAllPactManagersAsync();
        Task<ApiResponseDto<List<WorkGroupPersonDto>>> GetAllWorkGroupPersonAsync();
        Task<ApiResponseDto<PaginatedResult<PactStaffDto>>> GetWorkGroupStaffAsync(QueryParameters<string> query, string? workGroup = null);
        Task<ApiResponseDto<List<PactStaffDto>>> GetPactStaffAsync();
        Task<ApiResponseDto<List<PactStaffDto>>> GetPactWorkGroupStaffAsync(string? workGroup);
    }
}
