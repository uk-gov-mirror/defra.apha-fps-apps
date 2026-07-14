using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.Costbook;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using System.Web;

namespace Apha.FPSApps.Infrastructure.Integrations.CostBookApis.Clients
{
    public class CostBookMaintenanceApiClient : ICostBookMaintenanceApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        
        private const string SettingsEndpoint = CostBookApiEndpoints.GetMaintenanceSettings;
        private const string AccountCategoriesEndpoint = CostBookApiEndpoints.GetMaintenanceAccountCategories;

        public CostBookMaintenanceApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<MaintenanceSettingsDto>> GetSettingsAsync()
        {
            try
            {
                var response = await _http.GetAsync<MaintenanceSettingsRes>(SettingsEndpoint);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve maintenance settings", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<MaintenanceSettingsDto>> UpdateSettingsAsync(MaintenanceSettingsDto dto)
        {
            try
            {
                var request = _mapper.Map<MaintenanceSettingsReq>(dto);
                var response = await _http.PutAsync<MaintenanceSettingsReq, MaintenanceSettingsRes>(SettingsEndpoint, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<MaintenanceSettingsDto>>(response);
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<MaintenanceSettingsDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update maintenance settings", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetAccountCategoriesAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccountCategoryMaintenanceRes>>(AccountCategoriesEndpoint);

                if (response.Success && response.Data != null)
                    return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(
                        _mapper.Map<List<AccountCategoryMaintenanceDto>>(response.Data));

                return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors),
                    new ApiMetaDto());
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve account categories", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<List<AccountCategoryMaintenanceDto>>> GetPaginatedAccountCategoriesAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(
                    CostBookApiEndpoints.GetPaginatedMaintenanceAccountCategories, query);
                
                var response = await _http.GetAsync<List<AccountCategoryMaintenanceRes>>(url);

                if (response.Success && response.Data != null)
                {
                    var dto = ApiResponseDto<List<AccountCategoryMaintenanceDto>>.SuccessResponse(
                        _mapper.Map<List<AccountCategoryMaintenanceDto>>(response.Data));
                    if (response.Pagination != null)
                    {
                        dto.Pagination = new PaginationDto
                        {
                            PageNumber   = response.Pagination.PageNumber,
                            PageSize     = response.Pagination.PageSize,
                            TotalPages   = response.Pagination.TotalPages,
                            TotalRecords = response.Pagination.TotalRecords
                        };
                    }
                    return dto;
                }

                return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors),
                    new ApiMetaDto());
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccountCategoryMaintenanceDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paginated account categories", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<AccountCategoryMaintenanceDto>> UpdateAccountCategoryAsync(string accShortName, AccountCategoryMaintenanceDto dto)
        {
            try
            {
                var request = _mapper.Map<AccountCategoryMaintenanceReq>(dto);
                var url = string.Format(CostBookApiEndpoints.UpdateMaintenanceAccountCategory, HttpUtility.UrlEncode(accShortName));
                var response = await _http.PutAsync<AccountCategoryMaintenanceReq, AccountCategoryMaintenanceRes>(url, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccountCategoryMaintenanceDto>>(response);
                return ApiResponseDto<AccountCategoryMaintenanceDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccountCategoryMaintenanceDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update account category", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
