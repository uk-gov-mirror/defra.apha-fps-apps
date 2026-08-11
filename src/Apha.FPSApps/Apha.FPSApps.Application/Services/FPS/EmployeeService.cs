using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IFpsApiClient _fpsClient;

        public EmployeeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<EmployeeDto>>> GetFilteredEmployeesAsync(QueryParameters<string> criteria, int filterOption)
        {
            var employees = await _fpsClient.FpsEmployee.GetFilteredEmployeesAsync(criteria, filterOption);
            return employees;
        }

        public async Task<ApiResponseDto<EmployeeDto>> GetEmployeeByIdAsync(string spNumber)
        {
            var employee = await _fpsClient.FpsEmployee.GetEmployeeIdAsync(spNumber);
            return employee;
        }

        public async Task<ApiResponseDto<EmployeeDto>> CreateEmployeeAsync(EmployeeDto employee)
        {
            var result = await _fpsClient.FpsEmployee.CreateEmployeeAsync(employee);
            return result;
        }

        public async Task<ApiResponseDto<EmployeeDto>> UpdateEmployeeAsync(EmployeeDto employee)
        {
            var result = await _fpsClient.FpsEmployee.UpdateEmployeeAsync(employee);
            return result;
        }

        public async Task<ApiResponseDto<bool>> DeleteEmployeeAsync(string spNumber)
        {
            var result = await _fpsClient.FpsEmployee.DeleteEmployeeAsync(spNumber);
            return result;
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllManagersAsync()
        {
            var managers = await _fpsClient.FpsEmployee.GetAllManagerAsync();
            return managers;
        }

        public async Task<ApiResponseDto<List<ManagerDto>>> GetAllPactManagersAsync()
        {
            var managers = await _fpsClient.FpsEmployee.GetAllPactManagerAsync();
            return managers;
        }

        public async Task<ApiResponseDto<List<WorkGroupPersonDto>>> GetAllWorkGroupPersonAsync()
        {
            return await _fpsClient.FpsEmployee.GetAllWorkGroupPersonAsync();
        }

        public async Task<ApiResponseDto<PaginatedResult<PactStaffDto>>> GetWorkGroupStaffAsync(QueryParameters<string> query, string? workGroup = null)
        {
            return await _fpsClient.FpsEmployee.GetWorkGroupStaffAsync(query, workGroup);
        }

        public async Task<ApiResponseDto<List<PactStaffDto>>> GetPactStaffAsync()
        {
            return await _fpsClient.FpsEmployee.GetPactStaffAsync();
        }

        public async Task<ApiResponseDto<List<PactStaffDto>>> GetPactWorkGroupStaffAsync(string? workGroup)
        {
            return await _fpsClient.FpsEmployee.GetPactWorkGroupStaffAsync(workGroup);
        }
    }
}
