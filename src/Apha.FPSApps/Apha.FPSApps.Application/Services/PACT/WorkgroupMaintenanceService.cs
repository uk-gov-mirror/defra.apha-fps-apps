using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.PACT
{
    /// <summary>
    /// Thin frontend service delegate for WorkGroup Maintenance CRUD and lookup operations.
    /// All method bodies delegate to <see cref="IFpsApiClient.FpsWorkgroupMaintenance"/> without
    /// adding business logic.  Migrated from <c>frmMaintWorkGroup2</c>.
    /// </summary>
    public class WorkgroupMaintenanceService : IWorkgroupMaintenanceService
    {
        // Repointed to PACT API client
        private readonly IPactApiClient _client;

        public WorkgroupMaintenanceService(IPactApiClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        // ── CRUD ────────────────────────────────────────────────────────────────────

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetPagedAsync
        public async Task<ApiResponseDto<List<WorkGroupDto>>> GetPagedAsync(QueryParameters<string> query)
        {
            return await _client.PactWorkGroup.GetPagedAsync(query);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetByWorkGroupNameAsync
        public async Task<ApiResponseDto<WorkGroupDto>> GetByWorkGroupNameAsync(string workGroupName)
        {
            return await _client.PactWorkGroup.GetByWorkGroupNameAsync(workGroupName);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.CreateAsync
        public async Task<ApiResponseDto<WorkGroupDto>> CreateAsync(WorkGroupDto dto)
        {
            return await _client.PactWorkGroup.CreateAsync(dto);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.UpdateAsync
        //   workGroupName is the original key (before any rename); dto.WorkGroupName may differ
        public async Task<ApiResponseDto<WorkGroupDto>> UpdateAsync(string workGroupName, WorkGroupDto dto)
        {
            return await _client.PactWorkGroup.UpdateAsync(workGroupName, dto);
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.DeleteAsync
        public async Task<ApiResponseDto<bool>> DeleteAsync(string workGroupName)
        {
            return await _client.PactWorkGroup.DeleteAsync(workGroupName);
        }

        // ── Lookup endpoints (SEPARATE from CRUD resource family) ────────────────

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetProfitCentresAsync
        public async Task<ApiResponseDto<List<string>>> GetProfitCentresAsync()
        {
            return await _client.PactWorkGroup.GetProfitCentresAsync();
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetOwnersAsync
        public async Task<ApiResponseDto<List<OwnerDto>>> GetOwnersAsync()
        {
            return await _client.PactWorkGroup.GetOwnersAsync();
        }

        // TRANSFORMENGINE: thin delegate → IFpsApiClient.FpsWorkgroupMaintenance.GetCostCentresAsync
        //   profitCentre sourced from modal ProfitCentre change event (confirmed page-sourced)
        public async Task<ApiResponseDto<List<double?>>> GetCostCentresAsync(string profitCentre)
        {
            return await _client.PactWorkGroup.GetCostCentresAsync(profitCentre);
        }
    }
}
