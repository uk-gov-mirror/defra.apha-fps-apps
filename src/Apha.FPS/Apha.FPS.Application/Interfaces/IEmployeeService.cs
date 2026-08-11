using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Pagination;

namespace Apha.FPS.Application.Interfaces
{
    public interface IEmployeeService
    {        
        Task<PaginatedResult<EmployeeDto>> GetFilteredEmployeesAsync(QueryParameters<string> queryFilter, int filterOption);
        Task<IEnumerable<EmployeeDto>> GetFilteredEmployeesAsync(int filterOption);
        Task<EmployeeDto?> GetEmployeeByIdAsync(string spNumber);
        Task<EmployeeDto> AddEmployeeAsync(EmployeeDto employeeDto);
        Task<EmployeeDto> UpdateEmployeeAsync(EmployeeDto employeeDto);
        Task<bool> DeleteEmployeeAsync(string spNumber);
        Task<IEnumerable<ManagerDto>> GetAllManagersAsync();
        Task<IEnumerable<ManagerDto>> GetAllPactManagersAsync();
        Task<IEnumerable<WorkGroupPersonDto>> GetAllWorkGroupPersonAsync();
        Task<PaginatedResult<PactStaffDto>> GetPagedWorkGroupStaffAsync(QueryParameters<string> queryFilter, string? workGroup = null);
        Task<IEnumerable<PactStaffDto>> GetPactStaffAsync();
        Task<IEnumerable<PactStaffDto>> GetPactWorkGroupStaffAsync(string? workGroup);
    }
}
