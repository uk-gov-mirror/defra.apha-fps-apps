using Apha.Common.Constants;
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
    public class CostBookCapsStaffApiClient : ICostBookCapsStaffApiClient
    {
        private readonly ICostBookHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public CostBookCapsStaffApiClient(ICostBookHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        
        public async Task<ApiResponseDto<List<StaffDto>>> GetPaginatedCapsStaffAsync(QueryParameters<string> query)
        {
            try
            {
                var url = QueryStringHelper.AddQueryString(CostBookApiEndpoints.GetPaginatedCapsStaff, query);
                
                var response = await _http.GetAsync<List<StaffRes>>(url);

                if (response.Success && response.Data != null)
                {
                    var dto = ApiResponseDto<List<StaffDto>>.SuccessResponse(
                        _mapper.Map<List<StaffDto>>(response.Data));
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

                return ApiResponseDto<List<StaffDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors),
                    new ApiMetaDto());
            }
            catch (Exception)
            {
                return ApiResponseDto<List<StaffDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paginated CAPS staff", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<StaffDto>> GetCapsStaffByMNumberAsync(string mNumber)
        {
            try
            {
                var url = string.Format(CostBookApiEndpoints.GetCapsStaffByMNumber, HttpUtility.UrlEncode(mNumber));
                var response = await _http.GetAsync<StaffRes>(url);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<StaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<StaffDto>>(response);
                return ApiResponseDto<StaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<StaffDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<StaffDto>> AddCapsStaffAsync(StaffDto dto)
        {
            try
            {
                var request = _mapper.Map<StaffReq>(dto);
                var response = await _http.PostAsync<StaffReq, StaffRes>(CostBookApiEndpoints.AddCapsStaff, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<StaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<StaffDto>>(response);
                return ApiResponseDto<StaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<StaffDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to add CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<StaffDto>> UpdateCapsStaffAsync(string mNumber, StaffDto dto)
        {
            try
            {
                var request = _mapper.Map<StaffReq>(dto);
                var url = string.Format(CostBookApiEndpoints.UpdateCapsStaff, HttpUtility.UrlEncode(mNumber));
                var response = await _http.PutAsync<StaffReq, StaffRes>(url, request);

                if (response.Success && response.Data != null)
                    return _mapper.Map<ApiResponseDto<StaffDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<StaffDto>>(response);
                return ApiResponseDto<StaffDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<StaffDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        
        public async Task<ApiResponseDto<bool>> DeleteCapsStaffAsync(string mNumber)
        {
            try
            {
                var url = string.Format(CostBookApiEndpoints.DeleteCapsStaff, HttpUtility.UrlEncode(mNumber));
                var response = await _http.DeleteAsync<object>(url);

                if (response.Success)
                    return ApiResponseDto<bool>.SuccessResponse(true);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete CAPS staff member", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
