using Apha.Common.Constants;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients
{
    public class FpsAdditionalCostApiClient : IFpsAdditionalCostApiClient
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;

        public FpsAdditionalCostApiClient(IFpsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<List<AdditionalCostDto>>> GetAdditionalCostsAsync(QueryParameters<string> query, string jobCode)
        {
            var url = QueryStringHelper.AddQueryString(string.Format(FpsApiEndpoints.GetAdditionalCosts, jobCode), query);
            var response = await _http.GetAsync<List<AdditionalCostRes>>(url);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<List<AdditionalCostDto>>>(response);
            return ApiResponseDto<List<AdditionalCostDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<decimal>> GetTotalItemCostAsync(string jobCode)
        {
            var response = await _http.GetAsync<decimal>(string.Format(FpsApiEndpoints.GetTotalItemCost, jobCode));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<decimal>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<decimal>>(response);
            return ApiResponseDto<decimal>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<List<AccountCategoryDto>>> GetAccountCategoriesAsync()
        {
            var response = await _http.GetAsync<List<AccountCategoryRes>>(FpsApiEndpoints.GetAccountCategories);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<List<AccountCategoryDto>>>(response);
            return ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AdditionalCostDto>> GetByIdAsync(string jobCode, string account, string description)
        {
            var response = await _http.GetAsync<AdditionalCostRes>(string.Format(FpsApiEndpoints.GetAdditionalCostById, jobCode, account, description));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
            return ApiResponseDto<AdditionalCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AdditionalCostDto>> CreateAdditionalCostAsync(AdditionalCostDto additionalCost)
        {
            var req = _mapper.Map<AdditionalCostReq>(additionalCost);
            var response = await _http.PostAsync<AdditionalCostReq, AdditionalCostRes>(FpsApiEndpoints.CreateAdditionalCost, req);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
            return ApiResponseDto<AdditionalCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<AdditionalCostDto>> UpdateAdditionalCostAsync(AdditionalCostDto additionalCost)
        {
            var req = _mapper.Map<AdditionalCostReq>(additionalCost);

            if (string.IsNullOrWhiteSpace(additionalCost.OriginalDescription))
                throw new InvalidOperationException("OriginalDescription must be set before calling UpdateAdditionalCostAsync.");

            var response = await _http.PutAsync<AdditionalCostReq, AdditionalCostRes>(FpsApiEndpoints.UpdateAdditionalCost, req);

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<AdditionalCostDto>>(response);
            return ApiResponseDto<AdditionalCostDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }

        public async Task<ApiResponseDto<bool>> DeleteAdditionalCostAsync(AdditionalCostDto additionalCost)
        {
            var response = await _http.DeleteAsync<bool?>(string.Format(FpsApiEndpoints.DeleteAdditionalCost, additionalCost.JobCode, additionalCost.Account, additionalCost.Description));

            if (response.Success)
            {
                return _mapper.Map<ApiResponseDto<bool>>(response);
            }

            var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
            return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
        }
    }
}
