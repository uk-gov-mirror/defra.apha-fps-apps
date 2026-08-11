using Apha.Common.Constants;
using Apha.Common.Contracts.PIMS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Integrations.PIMSApis.Clients
{
    public class PimsAccessUserApiClient : IPimsAccessUserApiClient
    {
        private readonly IPimsHttpExecutor _http;
        private readonly IMapper _mapper;
        private const string InternalCodeError = "INTERNAL_ERROR";

        public PimsAccessUserApiClient(IPimsHttpExecutor http, IMapper mapper)
        {
            _http = http;
            _mapper = mapper;
        }

        public async Task<ApiResponseDto<PaginatedResult<AccessUserDto>>> GetPagedAsync(QueryParameters<string> request)
        {
            try
            {
                string url = QueryStringHelper.AddQueryString(PimsApiEndpoints.GetAccessUsersPaged, request);
                var response = await _http.GetAsync<List<AccessUserRes>>(url);
                if (response.Success)
                {
                    var items        = _mapper.Map<List<AccessUserDto>>(response.Data ?? []);
                    var pageNumber   = response.Pagination?.PageNumber  ?? request.Page;
                    var pageSize     = response.Pagination?.PageSize    ?? request.PageSize;
                    var totalRecords = response.Pagination?.TotalRecords ?? items.Count;
                    var paged = new PaginatedResult<AccessUserDto>(items, totalRecords, pageNumber, pageSize);
                    return ApiResponseDto<PaginatedResult<AccessUserDto>>.SuccessResponse(paged);
                }

                return ApiResponseDto<PaginatedResult<AccessUserDto>>.FailureResponse(
                    _mapper.Map<List<ApiErrorDto>>(response.Errors ?? []),
                    _mapper.Map<ApiMetaDto>(response.Meta));
            }
            catch (Exception)
            {
                return ApiResponseDto<PaginatedResult<AccessUserDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve paged AccessUser data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AccessUserDto>>> GetAllAsync()
        {
            try
            {
                var response = await _http.GetAsync<List<AccessUserRes>>(PimsApiEndpoints.GetAllAccessUsers);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUser data", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<List<AccessUserDto>>> GetBySystemIdAsync(int systemid)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.GetAccessUsersBySystemId, systemid);
                var response = await _http.GetAsync<List<AccessUserRes>>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<List<AccessUserDto>>>(response);
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<List<AccessUserDto>>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUser by system ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<AccessUserDto>> GetByIdAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.GetAccessUserById, systemid, Uri.EscapeDataString(ntlogin));
                var response = await _http.GetAsync<AccessUserRes>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserDto>>(response);
                return ApiResponseDto<AccessUserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to retrieve AccessUser by composite ID", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<AccessUserDto>> CreateAsync(AccessUserDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserReq>(dto);
                var response = await _http.PostAsync<AccessUserReq, AccessUserRes>(PimsApiEndpoints.CreateAccessUser, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserDto>>(response);
                return ApiResponseDto<AccessUserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to create AccessUser", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<AccessUserDto>> UpdateAsync(int systemid, string ntlogin, AccessUserDto dto)
        {
            try
            {
                var request = _mapper.Map<AccessUserReq>(dto);
                var url = string.Format(PimsApiEndpoints.UpdateAccessUser, systemid, Uri.EscapeDataString(ntlogin));
                var response = await _http.PutAsync<AccessUserReq, AccessUserRes>(url, request);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<AccessUserDto>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<AccessUserDto>>(response);
                return ApiResponseDto<AccessUserDto>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<AccessUserDto>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to update AccessUser", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }

        public async Task<ApiResponseDto<bool>> DeleteAsync(int systemid, string ntlogin)
        {
            try
            {
                var url = string.Format(PimsApiEndpoints.DeleteAccessUser, systemid, Uri.EscapeDataString(ntlogin));
                var response = await _http.DeleteAsync<bool>(url);
                if (response.Success)
                    return _mapper.Map<ApiResponseDto<bool>>(response);

                var responseDto = _mapper.Map<ApiResponseDto<bool>>(response);
                return ApiResponseDto<bool>.FailureResponse(responseDto.Errors, responseDto.Meta);
            }
            catch (Exception)
            {
                return ApiResponseDto<bool>.FailureResponse(
                    [new ApiErrorDto { Message = "Failed to delete AccessUser", Code = InternalCodeError }],
                    new ApiMetaDto());
            }
        }
    }
}
