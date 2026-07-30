using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    public class AdditionalCostService : IAdditionalCostService
    {
        private readonly IFpsApiClient _fpsClient;

        public AdditionalCostService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalCostsAsync(QueryParameters<string> query, string jobCode)
        {
            return await _fpsClient.FpsAdditionalCost.GetAdditionalCostsAsync(query, jobCode);
        }

        public async Task<ApiResponseDto<decimal>> GetTotalItemCostAsync(string jobCode)
        {
            return await _fpsClient.FpsAdditionalCost.GetTotalItemCostAsync(jobCode);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            return await _fpsClient.FpsAdditionalCost.GetAccountCategoriesAsync();
        }

        public async Task<ApiResponseDto<AdditionalCostDto>> GetByIdAsync(string jobCode, string account, string description)
        {
            return await _fpsClient.FpsAdditionalCost.GetByIdAsync(jobCode, account, description);
        }

        public async Task<ApiResponseDto<AdditionalCostDto>> CreateAdditionalCostAsync(AdditionalCostDto additionalCost)
        {
            return await _fpsClient.FpsAdditionalCost.CreateAdditionalCostAsync(additionalCost);
        }

        public async Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(string jobCode, string account, AdditionalCostDto additionalCost)
        {
            additionalCost.JobCode = jobCode;
            if (string.IsNullOrWhiteSpace(additionalCost.OriginalAccount))
                additionalCost.OriginalAccount = account;
            return await _fpsClient.FpsAdditionalCost.UpdateAdditionalCostAsync(additionalCost);
        }

        public async Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(AdditionalCostDto additionalCost)
        {
            return await _fpsClient.FpsAdditionalCost.DeleteAdditionalCostAsync(additionalCost);
        }
    }
}
