using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class WorkGroupEmployeeService : IWorkGroupEmployeeService
    {
        private readonly IFpsApiClient _fpsClient;

        public WorkGroupEmployeeService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeDto>>> GetWorkGroupEmployeeAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeAsync(query, wgGrade);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> GetWorkGroupEmployeeByIdAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeByIdAsync(pactId);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeDto>> UpdateWorkGroupEmployeeAsync(WorkGroupEmployeeDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.UpdateWorkGroupEmployeeAsync(dto);
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetWorkGroupEmployeeForStaffAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeForStaffAsync(query, wgGrade);
        }

        public async Task<ApiResponseDto<List<WorkGroupEmployeeStaffDto>>> GetAllActiveWorkGroupEmployeesAsync(QueryParameters<string> query, string wgGrade)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetAllActiveWorkGroupEmployeesAsync(query, wgGrade);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> GetWorkGroupEmployeeByIdForStaffAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.GetWorkGroupEmployeeByIdForStaffAsync(pactId);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> CreateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.CreateWorkGroupEmployeeForStaffAsync(dto);
        }

        public async Task<ApiResponseDto<WorkGroupEmployeeStaffDto>> UpdateWorkGroupEmployeeForStaffAsync(WorkGroupEmployeeStaffDto dto)
        {
            return await _fpsClient.FpsWorkGroupEmployee.UpdateWorkGroupEmployeeForStaffAsync(dto);
        }

        public async Task<ApiResponseDto<bool>> DeleteWorkGroupEmployeeAsync(string pactId)
        {
            return await _fpsClient.FpsWorkGroupEmployee.DeleteWorkGroupEmployeeAsync(pactId);
        }
    }
}
